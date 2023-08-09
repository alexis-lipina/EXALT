using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
[RequireComponent(typeof(TriggerVolume))]

public class ExitVolume : MonoBehaviour
{
    [SerializeField] private string _destination;
    [SerializeField] private string _doorName;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (GetComponent<TriggerVolume>().IsTriggered)
        {
            ChangeLevel();
        }
    }

    public void ChangeLevel()
    {
        FadeTransition.Singleton.FadeToScene(_destination, _doorName);
    }

    public void SetTargetLevel(string newDestination)
    {
        _destination = newDestination;
    }
}
