using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Provides support for things happening when a certain entity is within a certain worldspace volume. 
/// </summary>
public class VolumeTrigger : MonoBehaviour
{
    [SerializeField] private DynamicPhysics _objectToScanFor;
    [SerializeField] private SpriteRenderer[] VisibleWhenInside;
    [SerializeField] private SpriteRenderer[] InvisibleWhenInside;


	void Start ()
    {
		
	}
	
    


	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Friend") //When player enters the trigger
        {
            //activate occlusion magic
            for (int i = 0; i < VisibleWhenInside.Length; i++)
            {
                VisibleWhenInside[i].enabled = true;
            }
            //Debug.Log("Yeet"); 
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Friend")
        {
            for (int i = 0; i < VisibleWhenInside.Length; i++)
            {
                VisibleWhenInside[i].enabled = false;
            }
        }
    }
}
