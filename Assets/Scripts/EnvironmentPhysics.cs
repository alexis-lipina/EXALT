using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentPhysics : MonoBehaviour {

    [SerializeField] private float topHeight;
    [SerializeField] private float bottomHeight;
    [SerializeField] private GameObject parent;
    [SerializeField] private float heightReferenceOffset;
    [SerializeField] private GameObject[] neighbors;
    	
    void Start()
    {
        //Debug.Log("POSITION BEFORE:" + parent.transform.position.z);
        parent.transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, parent.transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);
        //Debug.Log("POSITION OF ME NOW:" + parent.transform.position.z);
    }



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

    public List<EnvironmentPhysics> getNeighbors()
    {
        List<EnvironmentPhysics> temp = new List<EnvironmentPhysics>();
        foreach( GameObject obj in neighbors )
        {
            temp.Add(obj.GetComponent<EnvironmentPhysics>());
        }
        return temp;
    }

    
}
