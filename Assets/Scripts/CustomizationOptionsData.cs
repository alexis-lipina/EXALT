using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Contains all customization options
/// </summary>
[Serializable]
public class CustomizationOptionsDataArray
{
    public CustomizationOptionsDataElement[] array;
}

[Serializable]
public class CustomizationOptionsDataElement
{
    public string PaletteNickname;
    public float[] ColorA;
    public float[] ColorB;
    public float[] ColorC;
    public float[] ColorD;
    public float[] ColorE;
    public float[] ColorF;
    public float[] ColorG;
}