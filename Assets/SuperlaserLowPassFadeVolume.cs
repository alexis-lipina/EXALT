using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TriggerVolume))]
public class SuperlaserLowPassFadeVolume : MonoBehaviour
{
    enum ChangeDirection { X, Y, Z, DistanceFromCenter }
    [SerializeField] ChangeDirection AxisToChangeAlong;
    private TriggerVolume ourTrigger;
    Vector3 TriggerSize;
    [SerializeField] private EntityPhysics playerPhysics;
    //[SerializeField] AudioLowPassFilter LowPassFilter;
    [SerializeField] float Cutoff_0;
    [SerializeField] float Cutoff_1;
    [SerializeField] AnimationCurve normalizedCurve;

    public float CurrentValue = 0.0f;

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
            CurrentValue = value;
            MonolithPersistent.GetInstance().GetComponent<AudioLowPassFilter>().cutoffFrequency = Mathf.Lerp(Cutoff_0, Cutoff_1, normalizedCurve.Evaluate(value));
        }
    }
}
