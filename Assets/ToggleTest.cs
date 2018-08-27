using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleTest : MonoBehaviour
{

    [SerializeField] private TriggerVolume _trigger;
    private bool _isActive;


	// Use this for initialization
	void Start ()
    {
        _isActive = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (_trigger.IsTriggered && !_isActive)
        {
            _isActive = true;
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
            gameObject.GetComponent<EnvironmentPhysics>().enabled = true;
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            Debug.Log("<color=green>Activated</color>");

        }
        else if (!_trigger.IsTriggered && _isActive)
        {
            _isActive = false;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<EnvironmentPhysics>().enabled = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            Debug.Log("<color=red>Deactivated</color>");
        }
    }
}
