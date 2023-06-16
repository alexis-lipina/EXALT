using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public struct TimePosition
{
    public float time;
    public Vector3 position;
}

[RequireComponent(typeof(EnvironmentPhysics))]
public class MovingEnvironment : MonoBehaviour
{
    public float CycleDuration = 8.0f;
    public TriggerVolume StandingTrigger; // entities inside this volume are "on" this object and will move with it
    public RestPlatform RestPlatformToListen; // If this is moved by a rest-platform, this is the one that should trigger it. 
    public TimePosition[] keyframes;
    public AnimationCurve XPositionOverTime;
    public AnimationCurve YPositionOverTime;
    public AnimationCurve ZPositionOverTime;

    public AnimationCurve[] TestCurves;
    public UnityEvent OnAnimationComplete;

    float Timer;
    public bool Cycle = true;
    bool isPlaying = false;
    float objectHeight;
    float StartingElevation;
    float animRateScale = 1.0f;

    Vector3 StartingPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (RestPlatformToListen)
        {
            RestPlatformToListen.OnActivated.AddListener(PlayAnim);
        }
        StartingPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        isPlaying = Cycle;
        objectHeight = GetComponent<EnvironmentPhysics>().TopHeight - GetComponent<EnvironmentPhysics>().BottomHeight;
        StartingElevation = GetComponent<EnvironmentPhysics>().BottomHeight;
        
        foreach (TestPlayerElevationTracker tracker in GetComponentsInChildren<TestPlayerElevationTracker>())
        {
            tracker.SetCanChangeElevation(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            float dx = XPositionOverTime.Evaluate(Timer / CycleDuration);
            float dy = YPositionOverTime.Evaluate(Timer / CycleDuration);
            float dz = ZPositionOverTime.Evaluate(Timer / CycleDuration);

            Timer += Time.deltaTime * animRateScale;
            if (Cycle)
            {
                Timer = Timer % CycleDuration;
            }

            dx = XPositionOverTime.Evaluate(Timer / CycleDuration) - dx;
            dy = YPositionOverTime.Evaluate(Timer / CycleDuration) - dy;
            dz = ZPositionOverTime.Evaluate(Timer / CycleDuration) - dz;


            transform.position = StartingPosition + new Vector3(XPositionOverTime.Evaluate(Timer / CycleDuration), YPositionOverTime.Evaluate(Timer / CycleDuration), 0.0f);
            GetComponent<EnvironmentPhysics>().BottomHeight = StartingElevation + ZPositionOverTime.Evaluate(Timer / CycleDuration);
            GetComponent<EnvironmentPhysics>().TopHeight = StartingElevation + ZPositionOverTime.Evaluate(Timer / CycleDuration) + objectHeight;

            GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);
            GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);

            // copied from scalableplatform, forces it to adjust to correct depth. too lazy to integrate in a more optimal way. fuck you
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);

            if (StandingTrigger)
            {
                StandingTrigger.MoveBottom(ZPositionOverTime.Evaluate(Timer / CycleDuration));

                foreach (GameObject obj in StandingTrigger.TouchingObjects)
                {
                    Debug.Log("Overlapping player!");
                    EntityPhysics phys = obj.GetComponent<EntityPhysics>();
                    phys.MoveWithCollision(dx, dy);
                }
            }


            if (!Cycle && Timer > CycleDuration && isPlaying)
            {
                isPlaying = false;
                OnAnimationComplete.Invoke();
            }
        }
    }

    public void PlayAnim()
    {
        isPlaying = true;
        Timer = 0.0f;
    }

    public void SetAnimRate(float NewRate)
    {
        animRateScale = NewRate;
    }
    
    public void SetToElevation(float TargetZ) // immediately change the Z of the lower part of the object to this value.
    {
        float dz = TargetZ - GetComponent<EnvironmentPhysics>().BottomHeight;
        GetComponent<EnvironmentPhysics>().BottomHeight = TargetZ;
        GetComponent<EnvironmentPhysics>().TopHeight = TargetZ + objectHeight;

        GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);
        GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);
    }
}
