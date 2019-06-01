using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// for entities tied to an environment object that act as a switch. IsOn toggles every hit.
/// </summary>
public class SwitchPhysics : EntityPhysics
{
    public bool IsOn { get; private set; }
    private bool _hasBeenHitAlreadyThisFrame = false;

    public override void Inflict(int damage)
    {
        Debug.Log("hit!");
        if (_hasBeenHitAlreadyThisFrame) return;
        _hasBeenHitAlreadyThisFrame = true;
        base.Inflict(damage);
        IsOn = !IsOn;
        currentHP = MaxHP;
    }

    protected override void Update()
    {
        _hasBeenHitAlreadyThisFrame = false;
    }
}
