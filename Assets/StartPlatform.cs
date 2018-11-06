using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(EnvironmentPhysics))] 

/// <summary>
/// This class allows initialization of the players position in a level depending on the level the player was previously in.
/// </summary>
public class StartPlatform : MonoBehaviour
{
    [SerializeField] private string _sourceScene;
    [SerializeField] private EntityPhysics _playerPhysics;


	// Use this for initialization
	void Start ()
    {
		if (PlayerHandler.PREVIOUS_SCENE == _sourceScene)
        {
            //move player here
            _playerPhysics.transform.position = transform.position;
            _playerPhysics.SetElevation(GetComponent<EnvironmentPhysics>().TopHeight);
            Debug.Log("TELEPORT!");
            Camera.main.transform.position = transform.position;
        }
	}
	
}
