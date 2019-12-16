using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitlePulse : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        float pulseFloat = (Mathf.Sin(Time.realtimeSinceStartup) + 1) * 0.5f;
        Debug.Log(pulseFloat);
        GetComponent<Image>().material.SetFloat("_Opacity", pulseFloat);
    }
}
