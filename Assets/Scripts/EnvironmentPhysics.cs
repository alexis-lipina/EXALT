using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentPhysics : PhysicsObject
{
    [SerializeField] private GameObject playerSprite;
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject[] neighbors;
    [SerializeField] private bool isTransparentOnOcclude;

    private List<NavEdge> neighborEdges;

    void Start()
    {
        //Debug.Log("POSITION BEFORE:" + parent.transform.position.z);
        parent.transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, parent.transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);
        //Debug.Log("POSITION OF ME NOW:" + parent.transform.position.z);
        neighborEdges = new List<NavEdge>();
        foreach(GameObject neighbor in neighbors)
        {
            //Debug.Log("Neighbor visited in " + gameObject.name);
            NavEdge temp = new NavEdge();
            temp.EnvironmentObject = neighbor.GetComponent<EnvironmentPhysics>();
            temp.HeightDifference = neighbor.GetComponent<EnvironmentPhysics>().getTopHeight() - topHeight;
            temp.Distance = Vector2.Distance(neighbor.transform.position, gameObject.transform.position); //change when using elevated objects, take into account collider offset
            neighborEdges.Add(temp);
        }
       // Debug.Log("navedge number:" + neighborEdges.Count);
    }

    void Update()
    {
        if (isTransparentOnOcclude) //if object can become transparent when player behind
        {
            if (playerSprite.GetComponent<Transform>().position.z > gameObject.GetComponent<Transform>().position.z) //if player is behind
            {
                Debug.Log("Clear!");
                Color temp = gameObject.GetComponent<SpriteRenderer>().color;
                temp = new Color(temp.r, temp.g, temp.b, 0.5f);
                gameObject.GetComponent<SpriteRenderer>().color = temp;
            }
            else
            {
                Color temp = gameObject.GetComponent<SpriteRenderer>().color;
                temp = new Color(temp.r, temp.g, temp.b, 1f);
                gameObject.GetComponent<SpriteRenderer>().color = temp;
            }
            
        }
        else
        {
            Color temp = gameObject.GetComponent<SpriteRenderer>().color;
            temp = new Color(temp.r, temp.g, temp.b, 1f);
            gameObject.GetComponent<SpriteRenderer>().color = temp;
        }
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

    public List<NavEdge> getNavEdges()
    {
        return neighborEdges;
    }

    
}
