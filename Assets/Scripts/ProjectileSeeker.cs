using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSeeker : MonoBehaviour
{
    public List<EntityPhysics> trackedTargets;

	void OnEnable()
    {
        trackedTargets = new List<EntityPhysics>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<EntityPhysics>())
        {
            trackedTargets.Add(other.GetComponent<EntityPhysics>());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        trackedTargets.Remove(other.GetComponent<EntityPhysics>());
    }

}
