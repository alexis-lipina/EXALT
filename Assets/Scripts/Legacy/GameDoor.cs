using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDoor : MonoBehaviour {

    [SerializeField] private string destinationScene;
    [SerializeField] private Vector2 destinationPoint;

	
	void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "MovementCollisionBox" || other.gameObject.name == "PlayerCollider")
        {
            TemporaryPersistentDataScript.setDestinationPosition(destinationPoint);
            SceneManager.LoadScene(destinationScene, LoadSceneMode.Single);
        }
    }
}
