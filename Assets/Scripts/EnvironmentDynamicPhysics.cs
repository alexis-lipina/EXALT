using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class allows for EnvironmentPhysics objects which might move, specifically in this implementation changing the Z-position of the object.
/// </summary>
public class EnvironmentDynamicPhysics : EnvironmentPhysics {

    public float TopHeight
    {
        set { topHeight = value; }
        get { return topHeight; }
    }
    public float BottomHeight
    {
        set { bottomHeight = value; }
        get { return bottomHeight;  }
    }
}
