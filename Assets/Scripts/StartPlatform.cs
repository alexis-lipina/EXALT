using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(EnvironmentPhysics))] 

/// <summary>
/// This class allows initialization of the players position in a level depending on the level the player was previously in.
/// </summary>
public class StartPlatform : MonoBehaviour
{
    [SerializeField] private string _sourceScene;
    [SerializeField] private string _sourceDoor;
    [SerializeField] private EntityPhysics _playerPhysics;


	// Use this for initialization
	void Start ()
    {                                                       // v- should eval true for normal cases where theres just one unlabeled door
		if (PlayerHandler.PREVIOUS_SCENE == _sourceScene && PlayerHandler.PREVIOUS_SCENE_DOOR == _sourceDoor)
        {
            //move player here
            _playerPhysics.transform.position = transform.position + (Vector3)GetComponent<BoxCollider2D>().offset;
            _playerPhysics.SetElevation(GetComponent<EnvironmentPhysics>().TopHeight + 0.2f);
            _playerPhysics.ZVelocity = 0.0f;
            Debug.Log("TELEPORT!");
            Camera.main.transform.position = transform.position + (Vector3)GetComponent<BoxCollider2D>().offset;
            PlayerHandler.PREVIOUS_SCENE = SceneManager.GetActiveScene().name;
        }
	}
}
