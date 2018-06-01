using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls a bullet!
/// </summary>
public class BulletHandler : ProjectileHandler
{
    [SerializeField] private ProjectilePhysics _projectilePhysics;
    [SerializeField] private float _bulletSpeed;
    private Vector2 _moveDirection;
    private Weapon _sourceWeapon;


    public Vector2 MoveDirection
    {
        set { _moveDirection = value; }
        get { return _moveDirection; }
    }
    public Weapon SourceWeapon
    {
        set { _sourceWeapon = value; }
        get { return _sourceWeapon;  }
    }


	// Use this for initialization
	void Start ()
    {
        //_projectilePhysics.GetComponent<Rigidbody2D>().MovePosition(new Vector2(1000, 1000));
        _projectilePhysics.ZVelocity = 0.5f;
        //DEBUG
        //_moveDirection = new Vector2(1, 1).normalized;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Debug.Log(_projectilePhysics.GetObjectElevation());
		if (_projectilePhysics.IsCollidingWithEnvironment())
        {
            //GameObject.Destroy(GetComponentInParent<Transform>().parent.gameObject); //TODO : rn it hard destroys bullet, use something like object
            SourceWeapon.ReturnToPool(transform.parent.gameObject.GetInstanceID());
        }
        Vector2 temp = _moveDirection;
        temp.Normalize();
        temp.Set(temp.x * Time.deltaTime * _bulletSpeed, temp.y * Time.deltaTime * _bulletSpeed);


        //_projectilePhysics.MoveWithCollision(temp.x, temp.y);
        
        _projectilePhysics.FreeFall();
        //_projectilePhysics.GetComponent<Rigidbody2D>().MovePosition(_projectilePhysics.GetComponent<Rigidbody2D>().position + temp); //TODO : moveposition should be performed in BulletPhysics, or whatever the new physics system for bullets is
        _projectilePhysics.MoveWithCollision(temp.x, temp.y);
	}


    //Resets the bullet for reuse
    public void ResetBullet()
    {
       
        Start();
    }
}
