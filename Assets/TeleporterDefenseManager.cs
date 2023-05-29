using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TeleporterDefenseManager : MonoBehaviour
{
    public MovingEnvironment[] ChargingPillars;
    public float ChargeDuration = 30.0f;
    public float PillarHeight = 24f;
    bool IsDefending = false;
    float ChargeTimer = 0.0f;
    float PillarStartElevation = 0.0f;
    public UnityEvent OnTeleporterCharged;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDefending)
        {
            ChargeTimer += Time.deltaTime;

        }
    }

    public void BeginTeleporterDefense()
    {
        StartCoroutine(DefenseCoroutine());
    }


    IEnumerator DefenseCoroutine()
    {
        ChargeTimer = 0.0f;

        // TODO : do we want this to actually only change in response to killing enemies? or maybe its driven by both?
        while (ChargeTimer < ChargeDuration)
        {
            ChargeTimer += Time.deltaTime; 
            foreach (MovingEnvironment phys in ChargingPillars)
            {
                phys.SetToElevation((1 - ChargeTimer / ChargeDuration) * PillarHeight * -1.0f);
            }
            yield return new WaitForEndOfFrame();
        }
        OnTeleporterCharged.Invoke();
    }
}
