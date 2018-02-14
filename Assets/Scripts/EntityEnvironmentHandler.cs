using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityEnvironmentHandler : MonoBehaviour {

    [SerializeField] private GameObject entityHandlerObject;
    [SerializeField] private EntityColliderScript entityCollider;

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Entering!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Blep");

            entityCollider.AddTerrainTouched(other.GetInstanceID(), other.GetComponent<EnvironmentPhysics>());//will be taken care of by EntityPhysics
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Exiting!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Hurk");
            entityCollider.RemoveTerrainTouched(other.GetInstanceID());//EntityPhysics
        }
    }
}
