using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriggerVolume))]
public class CameraSizeChangeVolume : MonoBehaviour
{
    enum ChangeDirection { X, Y, Z, DistanceFromCenter}
    [SerializeField] ChangeDirection AxisToChangeAlong;
    [SerializeField] AnimationCurve SizeChange; // normalized X along size of trigger, Y : 1 = normal camera size
    [SerializeField] EntityPhysics playerPhysics;
    [SerializeField] CameraScript camera;
    [SerializeField] float LerpRate = 0.5f;
    TriggerVolume ourTrigger;
    Vector3 TriggerSize;
    float DampedSizeValue = 1.0f;

    const float DefaultCameraSize = 16.875f;
    float CurrentVelocity = 0.0f;
    bool wasTracking = false;

    // Start is called before the first frame update
    void Start()
    {
        ourTrigger = GetComponent<TriggerVolume>();
        TriggerSize = new Vector3(
            GetComponent<BoxCollider2D>().size.x,
            GetComponent<BoxCollider2D>().size.y,
            ourTrigger.GetTopHeight() - ourTrigger.GetBottomHeight()
            );
    }

    // Update is called once per frame
    void Update()
    {
        if (ourTrigger.IsTriggered)
        {
            float value = 1.0f;
            switch (AxisToChangeAlong)
            {
                case ChangeDirection.X:
                    value = (playerPhysics.transform.position.x - transform.position.x) / TriggerSize.x + 0.5f;
                    break;
                case ChangeDirection.Y:
                    value = (playerPhysics.transform.position.y - transform.position.y) / TriggerSize.y + 0.5f;
                    break;
                case ChangeDirection.Z:
                    value = (playerPhysics.GetBottomHeight() - ourTrigger.GetBottomHeight()) / TriggerSize.z;
                    //Debug.Log("VALUE : " + value);
                    break;
                case ChangeDirection.DistanceFromCenter:
                    Vector2 playerpos = new Vector2(playerPhysics.transform.position.x, playerPhysics.transform.position.y);
                    Vector2 triggerpos = new Vector2(transform.position.x, transform.position.y);
                    value = 1 - Mathf.Clamp((playerpos - triggerpos).magnitude / Mathf.Min(TriggerSize.x, TriggerSize.y), 0, 1) * 2;
                    break;
            }

            if (wasTracking == false) // force damped to be correct when player initially enters
            {
                wasTracking = true;
                DampedSizeValue = value;
            }
            else
            {
                DampedSizeValue = Mathf.SmoothDamp(DampedSizeValue, value, ref CurrentVelocity, LerpRate, 2.0f);
            }

            camera.GetComponent<Camera>().orthographicSize = DefaultCameraSize * SizeChange.Evaluate(DampedSizeValue);
        }
        else
        {
            wasTracking = false;
        }
    }
}
