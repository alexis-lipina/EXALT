using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// **Does not extend EntityHandler**
/// 
/// This class controls the reticle, an object that will most likely be controlled explicitly by the player. It will change depending on the weapon being used. The reticle should almost always be at the same height as the player.
/// </summary>
public class ReticleHandler : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    //[SerializeField] private GameObject _reticleSprite;
    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private EntityPhysics _entityPhysics;
    [SerializeField] private Vector2 _maxMoveSpeed;
    [SerializeField] private float _maxReticleDistance;
    private Vector2 _reticleDimensions;

    private Vector2 _tempRightAnalogDirection;


    void Awake()
    {
        _reticleDimensions = _entityPhysics.GetComponent<BoxCollider2D>().size;
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        _entityPhysics.ZVelocity = 0;
        _entityPhysics.SetObjectElevation(_playerPhysics.GetObjectElevation());

        UpdateReticle();
        
        
        


        //LEGACY RETICLE POSITIONING CODE
        /*
        if (_inputHandler.RightAnalog.magnitude > 0.1) // dead zone
        {
            MoveToPoint(_playerPhysics.GetComponent<Rigidbody2D>().position + _inputHandler.RightAnalog * _maxReticleDistance);
        }
        else MoveToPoint(_playerPhysics.GetComponent<Rigidbody2D>().position);
        

        MoveToPoint(_playerPhysics.GetComponent<Rigidbody2D>().position + _tempRightAnalogDirection * _maxReticleDistance);

        //Debug.Log("ReticlePos" + _entityPhysics.GetComponent<Rigidbody2D>().position);
        */
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


    /// <summary>
    /// Updates the reticles position - TODO - If player should be able to shoot over low cover, change how this is done
    /// </summary>
    private void UpdateReticle()
    {
        //Vector2 reticlevector = _inputHandler.RightAnalog;
        float shortestDistance = _maxReticleDistance; //the distance to the first obstruction


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
        if (reticlevector.magnitude == 0) //get vector reticle should be drawn along, and distance of it
        {
            reticlevector = _inputHandler.LeftAnalog;
            if (reticlevector.magnitude == 0)
            {
                //idk what to do here, maybe a private field just for the cases where this happens?
            }
        }
        */
        RaycastHit2D[] impendingCollisions = Physics2D.BoxCastAll(_playerPhysics.transform.position, _entityPhysics.GetComponent<BoxCollider2D>().size, 0f, _tempRightAnalogDirection, distance: _maxReticleDistance); //btw, boxcastall is necessary cuz its gonna "collide" with stuff below the player too
        
        foreach (RaycastHit2D hit in impendingCollisions)
        {
            if (hit.transform.gameObject.tag == "Environment")
            {
                if (hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight() > _entityPhysics.GetBottomHeight()) // if the height of the terrain object is greater than the altitude of the player
                {
                    if (hit.distance < shortestDistance) 
                    {
                        shortestDistance = hit.distance;
                    }
                }
            }
        }

        //_tempRightAnalogDirection.Normalize();
        float truncatedMagnitude = _tempRightAnalogDirection.magnitude;
        if (truncatedMagnitude > 1)
        {
            truncatedMagnitude = 1;
            _tempRightAnalogDirection.Normalize();
            _tempRightAnalogDirection.Set(_tempRightAnalogDirection.x * truncatedMagnitude, _tempRightAnalogDirection.y * truncatedMagnitude);
        }
        _tempRightAnalogDirection.Set(_tempRightAnalogDirection.x * shortestDistance, _tempRightAnalogDirection.y * shortestDistance);
        //_entityPhysics.GetComponent<Rigidbody2D>().MovePosition(_playerPhysics.GetComponent<Rigidbody2D>().position + _tempRightAnalogDirection);//TODO : Rigidbody2D.position or Transform.position?
        _entityPhysics.GetComponent<Transform>().SetPositionAndRotation(_playerPhysics.GetComponent<Rigidbody2D>().position + _tempRightAnalogDirection, Quaternion.identity);
        /*
        if (shortestDistance == _maxReticleDistance)
        {
            _tempRightAnalogDirection.Normalize();
            _tempRightAnalogDirection.Set(_tempRightAnalogDirection.x * shortestDistance, _tempRightAnalogDirection.y * shortestDistance);
        }
        else
        {
            //reticlevector.Set(reticlevector.x * shortestDistance/reticlevector.magnitude, reticlevector.y * shortestDistance / reticlevector.magnitude); 
            _tempRightAnalogDirection.Set(_tempRightAnalogDirection.x * shortestDistance / _tempRightAnalogDirection.magnitude, _tempRightAnalogDirection.y * shortestDistance / _tempRightAnalogDirection.magnitude);
            _entityPhysics.GetComponent<Rigidbody2D>().MovePosition(_playerPhysics.GetComponent<Rigidbody2D>().position + _tempRightAnalogDirection);//TODO : Rigidbody2D.position or Transform.position?
        }
        */


    }

}
