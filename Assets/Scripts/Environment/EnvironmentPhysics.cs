using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class describes all static objects in the environment that may be traversed over. Class also supports AI navigation, with
/// each EnvironmentPhysics object using an adjacency list to help construct a nav graph.
/// </summary>
public class EnvironmentPhysics : PhysicsObject
{
    [SerializeField] public float environmentBottomHeight; //for initialization only
    [SerializeField] public float environmentTopHeight; //for initialization only
    [SerializeField] protected GameObject[] neighbors;
    [SerializeField] protected bool isTransparentOnOcclude;
    [SerializeField] protected float _opacityHeightTolerance = 1f;
    [SerializeField] protected bool isSavePoint = true; //whether the object can be relied on as a teleport location (does it move? does it activate/deactivate?)
    private float _opacity = 1;

    public static EntityPhysics _playerPhysics;
    public static GameObject _playerSprite;

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
        
    }

    void Start()
    {
        //Debug.Log("POSITION BEFORE:" + parent.transform.position.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);
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
        if (isTransparentOnOcclude)
        {

            float desiredOpacity = gameObject.GetComponent<Transform>().position.z - _playerSprite.GetComponent<Transform>().position.z + 5f;

            //less than 0 if player is within x-bounds, greater than 0 otherwise
            float distanceFromPlayerToLeftOrRightBound = Mathf.Abs((GetComponent<BoxCollider2D>().bounds.center - _playerSprite.GetComponent<Transform>().position).x) - GetComponent<BoxCollider2D>().bounds.extents.x;
            desiredOpacity = Mathf.Max(desiredOpacity, distanceFromPlayerToLeftOrRightBound);

            desiredOpacity *= 0.1f;

            desiredOpacity = Mathf.Clamp(desiredOpacity, 0f, 1.0f);
            if (_playerPhysics.GetBottomHeight() + _opacityHeightTolerance > TopHeight) desiredOpacity = 1f;
            _opacity = Mathf.Lerp(_opacity, desiredOpacity, 0.1f);

            GetComponentsInChildren<SpriteRenderer>()[0].material.SetFloat("_Opacity", _opacity);
            GetComponentsInChildren<SpriteRenderer>()[1].material.SetFloat("_Opacity", _opacity);

            /*
            if (playerSprite.GetComponent<Transform>().position.z > gameObject.GetComponent<Transform>().position.z)
            {
                //distance
                float opacity = gameObject.GetComponent<Transform>().position.z - playerSprite.GetComponent<Transform>().position.z + 3f;
                opacity *= 0.1f;
                GetComponentsInChildren<SpriteRenderer>()[0].material.SetFloat("_Opacity", opacity);
                GetComponentsInChildren<SpriteRenderer>()[1].material.SetFloat("_Opacity", opacity);
            }
            else
            {
                GetComponentsInChildren<SpriteRenderer>()[0].material.SetFloat("_Opacity", 1f);
                GetComponentsInChildren<SpriteRenderer>()[1].material.SetFloat("_Opacity", 1f);
            }*/
        }
        else
        {
            
        }
        
        
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
