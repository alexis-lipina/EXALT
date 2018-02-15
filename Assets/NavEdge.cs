using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavEdge {

    private float distance;
    private float heightDifference;
    private EnvironmentPhysics environmentObject;
    private Vector2 position;



    public EnvironmentPhysics EnvironmentObject
    {
        get { return environmentObject; }
        set {
            environmentObject = value;
            position = value.gameObject.transform.position;
        }
    }
    
    public float HeightDifference
    {
        get { return heightDifference; }
        set { heightDifference = value;  }
    }
    public float Distance
    {
        get { return distance; }
        set { distance = value; }
    }

    public Vector2 Position
    {
        get { return position; }
    }
}
