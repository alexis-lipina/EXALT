using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
[RequireComponent(typeof(TriggerVolume))]

public class ExitVolume : MonoBehaviour
{
    [SerializeField] private string _destination;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (GetComponent<TriggerVolume>().IsTriggered)
        {
            SceneManager.LoadScene(_destination);
            Debug.Log("Load!");

        }
    }
}
