using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;


public class PauseTest : MonoBehaviour
{
    private bool _isPaused = false;
	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		if ()
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
	}


}
