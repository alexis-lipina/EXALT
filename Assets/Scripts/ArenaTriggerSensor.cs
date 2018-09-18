using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaTriggerSensor : MonoBehaviour
{

    private bool willBeeline;

    public bool WillBeeLine
    {
        get { return willBeeline; }
    }

	// Use this for initialization
	void Awake () {
        willBeeline = false;
	}
	
	

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "BeeLineTrigger")
        {
            Debug.Log("wooop");
            willBeeline = true;
        }
    }
}
