using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This is purely the data representation of all accessibility options. This is the object that is serialized and deserialized to persist set options
/// </summary>
[Serializable]
public class AccessibilityOptionsData 
{
    public bool IsFlashingEnabled = true;
    public float ScreenshakeAmount = 1.0f;
    public bool LowHPVignette = true;
    public float UIScale = 1.0f;
    public int CustomizationOptionIndex = 0;
    public bool IsBlinkInDirectionOfMotion = true;
}
