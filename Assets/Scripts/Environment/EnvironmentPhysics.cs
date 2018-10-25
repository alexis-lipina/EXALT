using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class describes all static objects in the environment that may be traversed over. Class also supports AI navigation, with
/// each EnvironmentPhysics object using an adjacency list to help construct a nav graph.
/// </summary>
public class EnvironmentPhysics : PhysicsObject
{
    [SerializeField] protected float environmentBottomHeight; //for initialization only
    [SerializeField] protected float environmentTopHeight; //for initialization only
    [SerializeField] protected GameObject playerSprite;
    [SerializeField] protected GameObject parent;
    [SerializeField] protected GameObject[] neighbors;
    [SerializeField] protected bool isTransparentOnOcclude;
    [SerializeField] protected bool isSavePoint = true; //whether the object can be relied on as a teleport location (does it move? does it activate/deactivate?)

    private Dictionary<int, BoxCollider2D> shadowsReceived;
    private Dictionary<int, Transform> frontShadows;

    public float TopHeight
    {
        set { topHeight = value; }
        get { return topHeight; }
    }
    public float BottomHeight
    {
        set { bottomHeight = value; }
        get { return bottomHeight; }
    }

    protected List<NavEdge> neighborEdges;

    public bool IsSavePoint
    {
        get { return isSavePoint; }
    }

    void Awake()
    {
        bottomHeight = environmentBottomHeight;
        topHeight = environmentTopHeight;
        neighborEdges = new List<NavEdge>();
        shadowsReceived = new Dictionary<int, BoxCollider2D>();
        frontShadows = new Dictionary<int, Transform>();
    }

    void Start()
    {
        //Debug.Log("POSITION BEFORE:" + parent.transform.position.z);
        parent.transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, parent.transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);
        //Debug.Log("POSITION OF ME NOW:" + parent.transform.position.z);
        foreach(GameObject neighbor in neighbors)
        {
            //Debug.Log("Neighbor visited in " + gameObject.name);
            NavEdge temp = new NavEdge();
            temp.EnvironmentObject = neighbor.GetComponent<EnvironmentPhysics>();
            temp.HeightDifference = neighbor.GetComponent<EnvironmentPhysics>().GetTopHeight() - topHeight;
            temp.Distance = Vector2.Distance(neighbor.transform.position, gameObject.transform.position); //change when using elevated objects, take into account collider offset
            neighborEdges.Add(temp);
        }
       // Debug.Log("navedge number:" + neighborEdges.Count);
    }

    void Update()
    {
        /*
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
        */
    }

    /// <summary>
    /// Draws shadows on the front face of the object
    /// </summary>
    private void UpdateFrontShadows()
    {
        float frontY = gameObject.GetComponent<BoxCollider2D>().bounds.min.y;
        List<int> instanceIDs = new List<int>();
        //loop through objects touching - are any overlapping front?
        foreach (KeyValuePair<int, BoxCollider2D> entry in shadowsReceived)
        {
            if (entry.Value.bounds.min.y < frontY &&  entry.Value.bounds.max.y > frontY) //do the bounds overlap the front?
            {
                if (entry.Value.GetComponent<DynamicPhysics>().GetBottomHeight() < topHeight) //is the entity above the top of the object?
                {
                    instanceIDs.Add(entry.Value.gameObject.GetInstanceID());
                    if (frontShadows.ContainsKey(instanceIDs[instanceIDs.Count-1])) //is this one already being handled?
                    {
                        float bound = entry.Value.bounds.min.x - GetComponent<BoxCollider2D>().bounds.min.x;
                        frontShadows[entry.Value.gameObject.GetInstanceID()].GetComponent<Renderer>().material.SetFloat("_LeftBound", bound);
                        bound = entry.Value.bounds.max.x - GetComponent<BoxCollider2D>().bounds.max.x;
                        frontShadows[entry.Value.gameObject.GetInstanceID()].GetComponent<Renderer>().material.SetFloat("_RightBound", bound);
                    }
                    else //make a new one
                    {

                    }
                }
            }
        }
        //for each overlapping front, get those which are above
        // determine if the frontShadows needs to be changed at all

    }
    
    /// <summary>
    /// This method is called by any entities that cross into this environmentobject's collider
    /// </summary>
    public void AddShadowReceived(int instanceID, BoxCollider2D collider)
    {
        shadowsReceived.Add(instanceID, collider);
    }

    /// <summary>
    /// This method is called by entites as they leave the collider's bounds. Maybe have this be done automatically by envt. if the passed-in collider is outside the collier
    /// </summary>
    /// <param name="instanceID"></param>
    public void RemoveShadowsReceived(int instanceID)
    {
        shadowsReceived.Remove(instanceID);
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
