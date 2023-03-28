using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceFieldFX : MonoBehaviour
{
    float TimeSinceStart = 0.0f;
    const float WobbleLowScale = 0.9375f;
    const float WobblePeriod = 0.125f;
    [SerializeField] AnimationCurve WobblePattern;
    bool IsShieldDisabled = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TimeSinceStart += Time.deltaTime;

        float normalizedWobbleAmount = (TimeSinceStart % WobblePeriod) / WobblePeriod;

        transform.localScale = new Vector3(Mathf.Lerp(WobbleLowScale, 1.0f, WobblePattern.Evaluate(normalizedWobbleAmount)), 1.0f, 1.0f);        
    }
}
