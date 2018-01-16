using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentPhysics : MonoBehaviour {

    [SerializeField] private float topHeight;
    [SerializeField] private float bottomHeight;
    	


	void OnCollisionEnter2D(Collision2D coll)
    {
        //something might happen?
    }



    //===================================| Getters and Setters

    public float getTopHeight()
    {
        return topHeight;
    }
    public float getBottomHeight()
    {
        return bottomHeight;
    }
    /// <summary>
    /// Returns (bottomHeight, topHeight)
    /// </summary>
    /// <returns></returns>
    public KeyValuePair<float, float> getHeightData()
    {
        return new KeyValuePair<float, float>(bottomHeight, topHeight);
    }

    
}
