using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityEnvironmentHandler : MonoBehaviour {

    [SerializeField] private GameObject entityHandlerObject;
    private EntityHandler entityHandler;


    void Start ()
    {
        entityHandler = entityHandlerObject.GetComponent<EntityHandler>();
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
            entityHandler.addTerrainTouched(other.GetInstanceID(), other.GetComponent<EnvironmentPhysics>());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Exiting!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Hurk");
            entityHandler.removeTerrainTouched(other.GetInstanceID());
        }
    }
}
