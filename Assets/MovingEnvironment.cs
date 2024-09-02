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
    public bool bIsRestPlatformPotentiometer = false; // if true the rest platforms charge amount controls state, if false the rest platform fires our animation when it's fully charged
    private float restPlatformPreviousCharge;
    public TimePosition[] keyframes;
    public AnimationCurve XPositionOverTime;
    public AnimationCurve YPositionOverTime;
    public AnimationCurve ZPositionOverTime;

    public AnimationCurve[] TestCurves;
    public List<MovingEnvironment> SynchronizedEnvironment; // these are used to move other stuff in concert with this.
    public UnityEvent OnAnimationComplete;
    private bool bUsesOnAnimationComplete = true;

    float Timer;
    public bool Cycle = true;
    bool isPlaying = false;
    float objectHeight;
    float StartingElevation;
    float animRateScale = 1.0f;

    Vector3 StartingPosition;

    public EnvironmentPhysics environmentPhysics;

    void Awake()
    {
        environmentPhysics = GetComponent<EnvironmentPhysics>();
    }

    // Start is called before the first frame update
    void Start()
    {
        objectHeight = GetComponent<EnvironmentPhysics>().TopHeight - GetComponent<EnvironmentPhysics>().BottomHeight;
        StartingElevation = GetComponent<EnvironmentPhysics>().BottomHeight;
        StartingPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        if (RestPlatformToListen)
        {
            //RestPlatformToListen.OnActivated.AddListener(PlayAnim);
            restPlatformPreviousCharge = RestPlatformToListen.CurrentChargeAmount;
        }
        if (Cycle)
        {
            isPlaying = Cycle; // in case we play at start
        }
        foreach (TestPlayerElevationTracker tracker in GetComponentsInChildren<TestPlayerElevationTracker>())
        {
            tracker.SetCanChangeElevation(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bIsRestPlatformPotentiometer && RestPlatformToListen)
        {
            Update_RestPlatformPotentiometer();
            return;
        }
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

            environmentPhysics.TopSprite.gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);
            environmentPhysics.FrontSprite.gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);

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


            if (!Cycle && Timer > CycleDuration && isPlaying && bUsesOnAnimationComplete)
            {
                isPlaying = false;
                OnAnimationComplete.Invoke();
            }
        }
    }

    void Update_RestPlatformPotentiometer()
    {
        if (RestPlatformToListen.CurrentChargeAmount == restPlatformPreviousCharge) return;


        float dx = XPositionOverTime.Evaluate(restPlatformPreviousCharge);
        float dy = YPositionOverTime.Evaluate(restPlatformPreviousCharge);
        float dz = ZPositionOverTime.Evaluate(restPlatformPreviousCharge);

        dx = XPositionOverTime.Evaluate(RestPlatformToListen.CurrentChargeAmount) - dx;
        dy = YPositionOverTime.Evaluate(RestPlatformToListen.CurrentChargeAmount) - dy;
        dz = ZPositionOverTime.Evaluate(RestPlatformToListen.CurrentChargeAmount) - dz;



        transform.position = StartingPosition + new Vector3(XPositionOverTime.Evaluate(RestPlatformToListen.CurrentChargeAmount), YPositionOverTime.Evaluate(RestPlatformToListen.CurrentChargeAmount), 0.0f);
        GetComponent<EnvironmentPhysics>().BottomHeight = StartingElevation + ZPositionOverTime.Evaluate(RestPlatformToListen.CurrentChargeAmount);
        GetComponent<EnvironmentPhysics>().TopHeight = StartingElevation + ZPositionOverTime.Evaluate(RestPlatformToListen.CurrentChargeAmount) + objectHeight;

        environmentPhysics.TopSprite.gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);
        environmentPhysics.FrontSprite.gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);

        // copied from scalableplatform, forces it to adjust to correct depth. too lazy to integrate in a more optimal way. fuck you
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);

        if (StandingTrigger)
        {
            StandingTrigger.MoveBottom(ZPositionOverTime.Evaluate(RestPlatformToListen.CurrentChargeAmount));

            foreach (GameObject obj in StandingTrigger.TouchingObjects)
            {
                Debug.Log("Overlapping player!");
                EntityPhysics phys = obj.GetComponent<EntityPhysics>();
                phys.MoveWithCollision(dx, dy);
            }
        }


        if (RestPlatformToListen.CurrentChargeAmount == 1 && restPlatformPreviousCharge != 1 && bUsesOnAnimationComplete)
        {
            OnAnimationComplete.Invoke();
        }
        restPlatformPreviousCharge = RestPlatformToListen.CurrentChargeAmount;
    }

    public void PlayAnim()
    {
        isPlaying = true;
        Timer = 0.0f;

        foreach (MovingEnvironment envt in SynchronizedEnvironment)
        {
            envt.PlayAnim();
        }
    }

    public void SetAnimRate(float NewRate)
    {
        animRateScale = NewRate;
        foreach (var asdf in SynchronizedEnvironment)
        {
            asdf.SetAnimRate(NewRate);
        }
    }
    
    public void SetToElevation(float TargetZ, bool IsElevationForTop = false) // immediately change the Z of the lower part of the object to this value.
    {
        if (objectHeight == 0)
        {
            objectHeight = GetComponent<EnvironmentPhysics>().TopHeight - GetComponent<EnvironmentPhysics>().BottomHeight;
        }

        if (IsElevationForTop)
        {
            TargetZ -= objectHeight;
        }

        float dz = TargetZ - GetComponent<EnvironmentPhysics>().BottomHeight;
        GetComponent<EnvironmentPhysics>().BottomHeight = TargetZ;
        GetComponent<EnvironmentPhysics>().TopHeight = TargetZ + objectHeight;

        environmentPhysics.TopSprite.gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);
        environmentPhysics.FrontSprite.gameObject.transform.localPosition += new Vector3(0.0f, dz, 0.0f);
    }

    public void AddElevationOffset(float offset)
    {
        GetComponent<EnvironmentPhysics>().BottomHeight += offset;
        GetComponent<EnvironmentPhysics>().TopHeight += offset;

        environmentPhysics.TopSprite.gameObject.transform.localPosition += new Vector3(0.0f, offset, 0.0f);
        environmentPhysics.FrontSprite.gameObject.transform.localPosition += new Vector3(0.0f, offset, 0.0f);
    }

    public void SetUsesOnAnimationComplete(bool bUses)
    {
        bUsesOnAnimationComplete = bUses;
    }
}
