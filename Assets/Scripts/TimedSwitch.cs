using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedSwitch : Switch
{
    [SerializeField] private float _durationActive = 1f;
    private float _timer;


    void Update()
    {
        if (IsToggledOn) //tick down
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                _timer = _durationActive;
                IsToggledOn = false;
            }
        }

        if (_switchPhysics.IsOn != _switchPhysicsPreviousState) // maybe make it event-driven rather than constantly polling?
        {
            IsToggledOn = true;
            _timer = _durationActive;
            _switchPhysicsPreviousState = _switchPhysics.IsOn;
            Debug.Log("bloop");
        }
    }
}
