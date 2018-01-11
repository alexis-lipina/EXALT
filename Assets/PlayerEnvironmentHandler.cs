using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnvironmentHandler : MonoBehaviour {

    [SerializeField] private GameObject playerHandlerObject;
    private PlayerHandler playerHandler;


    void Start ()
    {
        playerHandler = playerHandlerObject.GetComponent<PlayerHandler>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
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
