using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentFrontShadowManager : MonoBehaviour
{
    [SerializeField] protected EnvironmentPhysics _environment;
    [SerializeField] protected GameObject _frontSprite;

    protected static int numShadows = 2;
    protected List<Transform> shadowPool; //stores inactive objects
    [SerializeField] protected Transform _frontShadow;
    private Dictionary<int, BoxCollider2D> shadowsReceived;
    private Dictionary<int, Transform> frontShadows;


    void Start ()
    {
        shadowPool = new List<Transform>();
        for (int i = 0; i < numShadows; i++)
        {
            shadowPool.Add(Instantiate(_frontShadow, _frontSprite.transform, false));
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
        List<int> instanceIDs = new List<int>();
        //loop through objects touching - are any overlapping front?
        foreach (KeyValuePair<int, BoxCollider2D> entry in shadowsReceived)
        {
            if (entry.Value.bounds.min.y < frontY && entry.Value.bounds.max.y > frontY) //do the bounds overlap the front?
            {
                if (entry.Value.GetComponent<DynamicPhysics>().GetBottomHeight() + 0.05f > _environment.GetTopHeight()) //is the entity above the top of the object?
                {
                    if (frontShadows.ContainsKey(entry.Value.gameObject.GetInstanceID())) //is this one already being handled?
                    {
                        //Debug.Log("Setting values!");
                        float bound =  (entry.Value.bounds.min.x - _environment.GetComponent<BoxCollider2D>().bounds.min.x) / (_environment.GetComponent<BoxCollider2D>().bounds.extents.x * 2f);
                        frontShadows[entry.Value.gameObject.GetInstanceID()].GetComponent<Renderer>().material.SetFloat("_LeftBound", bound);
                        bound = (entry.Value.bounds.max.x - _environment.GetComponent<BoxCollider2D>().bounds.max.x) / (_environment.GetComponent<BoxCollider2D>().bounds.extents.x * 2f);
                        frontShadows[entry.Value.gameObject.GetInstanceID()].GetComponent<Renderer>().material.SetFloat("_RightBound", bound);
                    }
                    else //make a new one
                    {
                        if (shadowPool.Count > 0)
                        {
                            Debug.Log("Adding...");
                            shadowPool[shadowPool.Count - 1].gameObject.SetActive(true);
                            frontShadows.Add(entry.Value.gameObject.GetInstanceID(), shadowPool[shadowPool.Count - 1]);
                            shadowPool.RemoveAt(shadowPool.Count - 1);
                            float bound = (entry.Value.bounds.min.x - _environment.GetComponent<BoxCollider2D>().bounds.min.x) / (_environment.GetComponent<BoxCollider2D>().bounds.extents.x * 2f);
                            frontShadows[entry.Value.gameObject.GetInstanceID()].GetComponent<Renderer>().material.SetFloat("_LeftBound", bound);
                            bound = (entry.Value.bounds.max.x - _environment.GetComponent<BoxCollider2D>().bounds.max.x) / (_environment.GetComponent<BoxCollider2D>().bounds.extents.x * 2f);
                            frontShadows[entry.Value.gameObject.GetInstanceID()].GetComponent<Renderer>().material.SetFloat("_RightBound", bound);
                        }
                        else
                        {
                            frontShadows.Add(entry.Value.gameObject.GetInstanceID(), Instantiate(_frontShadow, _frontSprite.transform, false));
                        }
                    }
                }
                else //if it aint
                {
                    instanceIDs.Add(entry.Value.gameObject.GetInstanceID());
                }
            }
            else //if it aint
            {
                instanceIDs.Add(entry.Value.gameObject.GetInstanceID());
            }
        }
        //remove unused shadows
        for (int i = instanceIDs.Count - 1; i >= 0; i--)
        {
            if (frontShadows.ContainsKey(instanceIDs[i]))
            {
                shadowPool.Add(frontShadows[instanceIDs[i]]);
                frontShadows[instanceIDs[i]].gameObject.SetActive(false);
                frontShadows.Remove(instanceIDs[i]);
            }
        }
    }



    /// <summary>
    /// This method is called by any entities that cross into this environmentobject's collider
    /// </summary>
    public void AddShadowReceived(int instanceID, BoxCollider2D collider)
    {
        if (!shadowsReceived.ContainsKey(instanceID))
            shadowsReceived.Add(instanceID, collider);
        else
            Debug.Log("Duplicate add");
    }

    /// <summary>
    /// This method is called by entites as they leave the collider's bounds. Maybe have this be done automatically by envt. if the passed-in collider is outside the collier
    /// </summary>
    /// <param name="instanceID"></param>
    public void RemoveShadowReceived(int instanceID)
    {
        shadowsReceived.Remove(instanceID);
    }

}
