using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentFrontShadowManager : MonoBehaviour
{
    [SerializeField] protected EnvironmentPhysics _environment;
    [SerializeField] protected GameObject _frontSprite;

    protected static int numShadows = 2;
    [SerializeField] protected Transform _frontShadow; //prefab
    protected List<Transform> shadowPool; //stores inactive objects
    private Dictionary<int, BoxCollider2D> shadowsReceived; // all colliders overlapping the environmentobject
    private Dictionary<int, Transform> frontShadows; //active shadow objects


    void Start ()
    {
        shadowPool = new List<Transform>();
        for (int i = 0; i < numShadows; i++)
        {
            shadowPool.Add(Instantiate(_frontShadow, _frontSprite.transform, false));
            shadowPool[shadowPool.Count - 1].localScale = new Vector3(_environment.GetComponent<BoxCollider2D>().bounds.size.x, _environment.TopHeight - _environment.BottomHeight, 1);
            shadowPool[shadowPool.Count - 1].gameObject.SetActive(false);
        }
        shadowsReceived = new Dictionary<int, BoxCollider2D>();
        frontShadows = new Dictionary<int, Transform>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateFrontShadows();
	}

    /// <summary>
    /// Draws shadows on the front face of the object
    /// </summary>
    private void UpdateFrontShadows()
    {
        float frontY = _environment.GetComponent<BoxCollider2D>().bounds.min.y;
        List<int> instanceIDs = new List<int>(); //objects to remove
        //loop through objects touching - are any overlapping front?
        foreach (KeyValuePair<int, BoxCollider2D> entry in shadowsReceived)
        {
            if (entry.Value.bounds.min.y < frontY && entry.Value.bounds.max.y > frontY) //do the bounds overlap the front?
            {
                if (entry.Value.GetComponent<DynamicPhysics>().GetBottomHeight() + 0.05f > _environment.GetTopHeight()) //is the entity above the top of the object?
                {
                    float bound;
                    if (frontShadows.ContainsKey(entry.Key)) //is this one already being handled?
                    {
                        //do something maybe?
                    }
                    else //make a new one
                    {
                        frontShadows.Add(entry.Key, GetFromPool());
                        Debug.Log("InstanceID in Update : " + entry.Key);
                    }
                    bound = (entry.Value.bounds.min.x - _environment.GetComponent<BoxCollider2D>().bounds.min.x) / (_environment.GetComponent<BoxCollider2D>().bounds.extents.x * 2f);
                    frontShadows[entry.Key].GetComponent<Renderer>().material.SetFloat("_LeftBound", bound);
                    bound = (entry.Value.bounds.max.x - _environment.GetComponent<BoxCollider2D>().bounds.min.x) / (_environment.GetComponent<BoxCollider2D>().bounds.extents.x * 2f);
                    frontShadows[entry.Key].GetComponent<Renderer>().material.SetFloat("_RightBound", bound);
                }
                else //if it aint
                {
                    instanceIDs.Add(entry.Key);
                }
            }
            else //if it aint
            {
                instanceIDs.Add(entry.Key);
            }
        }
        //remove unused shadows
        for (int i = instanceIDs.Count - 1; i >= 0; i--)
        {
            if (frontShadows.ContainsKey(instanceIDs[i]))
            {
                ReturnToPool(frontShadows[instanceIDs[i]]);
                frontShadows.Remove(instanceIDs[i]);
                Debug.Log("executed");
            }
        }
    }
    
    /// <summary>
    /// Gets a shadow object from object pooling (from shadowpool)
    /// </summary>
    Transform GetFromPool()
    {
        Transform obj;
        if (shadowPool.Count > 0) //get from pool
        {
            Debug.Log("Adding...");
            shadowPool[shadowPool.Count - 1].gameObject.SetActive(true);
            obj = shadowPool[shadowPool.Count - 1];
            shadowPool.RemoveAt(shadowPool.Count - 1);
        }
        else //expand pool
        {
            obj = Instantiate(_frontShadow, _frontSprite.transform, false);
            obj.localScale = new Vector3(_environment.GetComponent<BoxCollider2D>().bounds.size.x, _environment.TopHeight - _environment.BottomHeight, 1);
            Debug.Log("Instantiating!");
        }
        return obj;
    }

    /// <summary>
    /// Returns a shadow to the shadowpool, ***does not remove from source***
    /// </summary>
    void ReturnToPool(Transform obj)
    {
        if (!shadowPool.Contains(obj))
        {
            obj.gameObject.SetActive(false);
            shadowPool.Add(obj);
            Debug.Log("Returned");
        }
        else
        {
            Debug.Log("object already in pool");
        }
    }
    

    /// <summary>
    /// This method is called by any entities that cross into this environmentobject's collider
    /// </summary>
    public void AddShadowReceived(int instanceID, BoxCollider2D collider)
    {
        Debug.Log("InstanceID on entry : " + instanceID);

        if (!shadowsReceived.ContainsKey(instanceID))
            shadowsReceived.Add(instanceID, collider);
        else
            Debug.Log("Duplicate add");
    }

    /// <summary>
    /// This method is called by entites as they leave the collider's bounds. Maybe have this be done automatically by envt if the passed-in collider is outside the collier
    /// </summary>
    /// <param name="instanceID"></param>
    public void RemoveShadowReceived(int instanceID)
    {
        Debug.Log("InstanceID on exit : " + instanceID);
        if (frontShadows.ContainsKey(instanceID))
        {
            ReturnToPool(frontShadows[instanceID]);
            frontShadows.Remove(instanceID);
        }
        shadowsReceived.Remove(instanceID);
    }

}
