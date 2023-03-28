using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Vector2Event : UnityEvent<Vector2> { }

public class RipplePylon : MonoBehaviour
{
    [SerializeField] AnimationCurve HeightCurveOnHit;
    [SerializeField] float TimeToRing = 1.0f;
    [SerializeField] float TimeToReset = 4.0f;
    [SerializeField] ToggleSwitch hittableSwitch;
    public Vector2Event OnDongEvent;

    MovingEnvironment movingEnvt;
    float RestElevation;

    // Start is called before the first frame update
    void Start()
    {
        movingEnvt = GetComponent<MovingEnvironment>();

        RestElevation = GetComponent<EnvironmentPhysics>().BottomHeight;
    }
    
    public void PlayOnHit()
    {
        StartCoroutine(PlayFireAnimation());
    }

    IEnumerator PlayFireAnimation()
    {
        hittableSwitch.LockSwitch();
        float timer = 0.0f;
        while (timer < TimeToRing)
        {
            timer += Time.deltaTime;
            movingEnvt.SetToElevation(HeightCurveOnHit.Evaluate(timer / TimeToRing));
            yield return new WaitForEndOfFrame();
        }
        OnDongEvent.Invoke(GetComponent<BoxCollider2D>().bounds.center);
        float LowElevation = GetComponent<EnvironmentPhysics>().BottomHeight;

        timer = 0.0f;
        while (timer < TimeToReset)
        {
            timer += Time.deltaTime;
            movingEnvt.SetToElevation(Mathf.Lerp(LowElevation, RestElevation, timer / TimeToReset));
            yield return new WaitForEndOfFrame();
        }

        hittableSwitch.UnlockSwitch();
    }
}
