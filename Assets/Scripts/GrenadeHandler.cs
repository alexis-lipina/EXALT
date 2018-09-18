using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls a Grenade!
/// </summary>
public class GrenadeHandler : ProjectileHandler
{
    [SerializeField] private ProjectilePhysics _grenadePhysics;
    [SerializeField] private float _bulletSpeed;
    private Vector2 _moveDirection;

    public Vector2 MoveDirection
    {
        set { _moveDirection = value; }
        get { return _moveDirection; }
    }




    // Use this for initialization
    void Start()
    {
        //DEBUG
        //_moveDirection = new Vector2(1, 1).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(_entityPhysics.GetObjectElevation());
        
        Vector2 temp = _moveDirection;
        //temp.Normalize();
        _grenadePhysics.MoveWithCollision(temp.x, temp.y);
        //_entityPhysics.MoveWithCollision(temp.x, temp.y);

        //_entityPhysics.GetComponent<Rigidbody2D>().MovePosition(_entityPhysics.GetComponent<Rigidbody2D>().position + temp); //TODO : moveposition should be performed in BulletPhysics, or whatever the new physics system for bullets is

    }

}
