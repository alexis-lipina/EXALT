using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Just controls the positions of the reticle, pretty much. NOT an extension of EntityHandler
public class ReticleHandler : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    //[SerializeField] private GameObject _reticleSprite;
    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private EntityPhysics _entityPhysics;
    [SerializeField] private Vector2 _maxMoveSpeed;
    [SerializeField] private float _maxReticleDistance;

    private Vector2 _tempRightAnalogDirection;

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        _entityPhysics.ZVelocity = 0;
        _entityPhysics.SetEntityElevation(_playerPhysics.GetEntityElevation());

        if (_inputHandler.RightAnalog.magnitude <= 0.2)
        {
            //_tempRightAnalogDirection = _tempRightAnalogDirection.normalized;
            if (_inputHandler.LeftAnalog.magnitude >= 0.2)
            {
                _tempRightAnalogDirection = _inputHandler.LeftAnalog;
            }
            else
            {
                _tempRightAnalogDirection = _tempRightAnalogDirection.normalized * 0.2f;
            }
        }
        else
        {

            _tempRightAnalogDirection = _inputHandler.RightAnalog;
        }


        /*
        if (_inputHandler.RightAnalog.magnitude > 0.1) // dead zone
        {
            MoveToPoint(_playerPhysics.GetComponent<Rigidbody2D>().position + _inputHandler.RightAnalog * _maxReticleDistance);
        }
        else MoveToPoint(_playerPhysics.GetComponent<Rigidbody2D>().position);
        */

        MoveToPoint(_playerPhysics.GetComponent<Rigidbody2D>().position + _tempRightAnalogDirection * _maxReticleDistance);

        //Debug.Log("ReticlePos" + _entityPhysics.GetComponent<Rigidbody2D>().position);
	}



    //Moves reticle to coordinates it should be, snappy
    private void MoveToPoint(Vector2 destination)
    {
        if (_entityPhysics.GetComponent<Rigidbody2D>().position != destination)
        {
            Vector2 velocity = (destination - _entityPhysics.GetComponent<Rigidbody2D>().position);
            _entityPhysics.MoveWithCollision(velocity.x, velocity.y);
        }
    }

}
