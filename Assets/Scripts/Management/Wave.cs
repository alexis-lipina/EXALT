using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Wave
{
    public int TotalMelee = 0; //total number of units deployed over the course of the wave
    public int TotalRanged = 0;
    public int MaxMelee = 0; //max units that can be alive at any given time 
    public int MaxRanged = 0;
}
