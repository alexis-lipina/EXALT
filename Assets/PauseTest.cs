using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;


public class PauseTest : MonoBehaviour
{
    private bool _isPaused = false;
    private Player _controller;


	// Use this for initialization
	void Start ()
    {
        _controller = ReInput.players.GetPlayer(0);
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (_controller.GetButtonDown("Pause"))
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
