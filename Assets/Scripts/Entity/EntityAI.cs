using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class provides a structure/template upon which to build 
/// AI agents who would need to navigate the world 
/// </summary>

public abstract class EntityAI : MonoBehaviour
{
    [SerializeField] protected EntityHandler handler;
    [SerializeField] protected EntityPhysics entityPhysics;
    [SerializeField] public NavigationManager navManager;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
