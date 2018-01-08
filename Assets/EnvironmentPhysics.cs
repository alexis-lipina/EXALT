using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentPhysics : MonoBehaviour {

    [SerializeField] private float height;
    	
	void OnCollisionEnter2D(Collision2D coll)
    {
        //something might happen?
    }

    //============| Getters and Setters

    public float getHeight()
    {
        return height;
    }

    
}
