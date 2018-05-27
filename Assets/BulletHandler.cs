using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls a bullet!
/// </summary>
public class BulletHandler : ProjectileHandler
{
    [SerializeField] private EntityPhysics _entityPhysics;
    [SerializeField] private float _bulletSpeed;
    private Vector2 _moveDirection;

    public Vector2 MoveDirection
    {
        set { _moveDirection = value; }
        get { return _moveDirection; }
    }




	// Use this for initialization
	void Start ()
    {
        _entityPhysics.ZVelocity = 0.5f;
        //DEBUG
        //_moveDirection = new Vector2(1, 1).normalized;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Debug.Log(_entityPhysics.GetObjectElevation());
		if (_entityPhysics.IsCollidingWithEnvironment())
        {
            GameObject.Destroy(GetComponentInParent<Transform>().parent.gameObject); //TODO : rn it hard destroys bullet, use something like object
        }
        Vector2 temp = _moveDirection;
        temp.Normalize();
        temp.Set(temp.x * Time.deltaTime * _bulletSpeed, temp.y * Time.deltaTime * _bulletSpeed);







        //_entityPhysics.MoveWithCollision(temp.x, temp.y);
        
        _entityPhysics.FreeFall();
        _entityPhysics.GetComponent<Rigidbody2D>().MovePosition(_entityPhysics.GetComponent<Rigidbody2D>().position + temp); //TODO : moveposition should be performed in BulletPhysics, or whatever the new physics system for bullets is

	}
}
