using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updates shader
/// </summary>
public class EnvironmentShaderUpdater : MonoBehaviour
{
    [SerializeField] private Material _TopMaterial;
    [SerializeField] private Material _FrontMaterial;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        //TODO : Lerp player height
        /*
        _TopMaterial.SetFloat("_PlayerElevation", GetComponent<EntityPhysics>().GetBottomHeight());
        _FrontMaterial.SetFloat("_PlayerElevation", GetComponent<EntityPhysics>().GetBottomHeight());
        */
	}
}
