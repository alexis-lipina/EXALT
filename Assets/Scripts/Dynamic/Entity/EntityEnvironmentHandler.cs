using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityEnvironmentHandler : MonoBehaviour {

    [SerializeField] private DynamicPhysics entityCollider;

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Entering!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Blep");

            entityCollider.AddTerrainTouched(other.GetInstanceID(), other.GetComponent<EnvironmentPhysics>());//will be taken care of by EntityPhysics
            if (other.GetComponent<EnvironmentFrontShadowManager>())
                other.GetComponent<EnvironmentFrontShadowManager>().AddShadowReceived(gameObject.GetInstanceID(), entityCollider.GetComponent<BoxCollider2D>());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Exiting!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Hurk");
            entityCollider.RemoveTerrainTouched(other.GetInstanceID()); // EntityPhysics
            if (other.GetComponent<EnvironmentFrontShadowManager>())
                other.GetComponent<EnvironmentFrontShadowManager>().RemoveShadowReceived(gameObject.GetInstanceID());
        }
    }
}
