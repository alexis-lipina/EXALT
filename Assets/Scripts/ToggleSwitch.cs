using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toggle switches are attacked to trigger a change in state. Tied to a SwitchPhysics object.
/// </summary>
public class ToggleSwitch : Switch
{
    void Update()
    {
        if (_switchPhysics.IsOn != _switchPhysicsPreviousState) // maybe make it event-driven rather than constantly polling?
        {
            _switchPhysicsPreviousState = _switchPhysics.IsOn;
            IsToggledOn = !IsToggledOn;
        }
    }
}
