using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundCollider : MonoBehaviour
{

    [SerializeField] private string Direction;
    private bool touchingWall;
    private float maxWallHeight;
    private Dictionary<int, float> touchedTerrain;

    void Start()
    {
        touchedTerrain = new Dictionary<int, float>();
        touchedTerrain.Add(0, 0.0f);
        maxWallHeight = 0;
    }


    //=======================================================| HANDLE COLLISIONS
    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Entering!");
        if (other.gameObject.tag == "Environment")
        {
            touchingWall = true;
            touchedTerrain.Add(other.gameObject.GetInstanceID(), other.GetComponent<EnvironmentPhysics>().getHeight());
            updateMaxWallHeight();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Exiting!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Hurk");
            touchedTerrain.Remove(other.gameObject.GetInstanceID());
            updateMaxWallHeight();
        }
    }

    private void updateMaxWallHeight()
    {
        maxWallHeight = 0;
        foreach (KeyValuePair<int, float> entry in touchedTerrain)
        {
            if (entry.Value > maxWallHeight) maxWallHeight = entry.Value;
        }
    }


    //======================| GETTERS AND SETTERS

    public string getDirection()
    {
        return Direction;
    }

    public bool getTouchingWall()
    {
        return touchingWall;
    }

    public float getMaxWallHeight()
    {
        return maxWallHeight;
    }
}
