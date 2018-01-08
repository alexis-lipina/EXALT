using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class handles player interaction with environment - namely, terrain traversal. An area of terrain has a 2D Box Collider associated with it,
/// which defines the area, and the terrain object stores the height of the terrain surface. Also, within the PlayerHandler is stored a HashMap which
/// stores each terrain object it is currently above. When a player's collider enters a new terrain collider, a new entry is added to this HashMap, 
/// with the terrain object's InstanceID as the key, and the terrain object's height as the value. When the player collider exits a terrain collider,
/// the entry in the HashMap with the terrain object's InstanceID as the key is removed.
/// 
/// Thus, the terrain objects above which the player is standing are stored. These are referenced for physics calculations and interactions, such as 
/// what height a player will land from a jump at, whether they just walked off a cliff, etc.
/// </summary>
public class PlayerColliderScript : MonoBehaviour
{

    [SerializeField] private GameObject playerHandlerObject;
    private PlayerHandler playerHandler;

    void Start()
    {
        playerHandler = playerHandlerObject.GetComponent<PlayerHandler>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Entering!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Blep");
            playerHandler.addTerrainTouched(other.GetInstanceID(), other.GetComponent<EnvironmentPhysics>().getHeight());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Exiting!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Hurk");
            playerHandler.removeTerrainTouched(other.GetInstanceID());
        }
    }


}
