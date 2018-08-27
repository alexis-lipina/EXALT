using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerElevationTracker : MonoBehaviour
{

    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private EnvironmentPhysics _environmentPhysics;
    [SerializeField]
    private bool _trueIfPlatformFalseIfWall;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_PlayerElevation", _playerPhysics.GetObjectElevation());

        if (_trueIfPlatformFalseIfWall)
        {
            //platform
            gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_PlatformElevation", _environmentPhysics.GetTopHeight());
        }
        else
        {
            //wall
            gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_TopElevation", _environmentPhysics.GetTopHeight());
            gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_BottomElevation", _environmentPhysics.GetBottomHeight());
        }
	}
}
