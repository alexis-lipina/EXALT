using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls a bullet!
/// </summary>
public class BulletHandler : ProjectileHandler
{
    [SerializeField] protected ProjectilePhysics _projectilePhysics;
    [SerializeField] protected float _bulletSpeed;
    protected Vector2 _moveDirection;
    protected Weapon _sourceWeapon;

    private bool _canBounce;



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
	virtual protected void Start ()
    {
        //_projectilePhysics.GetComponent<Rigidbody2D>().MovePosition(new Vector2(1000, 1000));
        _projectilePhysics.ZVelocity = 0.5f;
        _projectilePhysics.Reset();
        //DEBUG
        //_moveDirection = new Vector2(1, 1).normalized;
    }



    protected virtual void Update ()
    {
		if (_projectilePhysics.IsCollidingWithEnvironment())
        {
            //GameObject.Destroy(GetComponentInParent<Transform>().parent.gameObject); //TODO : rn it hard destroys bullet, use something like object
            //SourceWeapon.ReturnToPool(transform.parent.gameObject.GetInstanceID());
        }
        Vector2 temp = _moveDirection;
        temp.Normalize();
        temp.Set(temp.x * Time.deltaTime * _bulletSpeed, temp.y * Time.deltaTime * _bulletSpeed);
        temp = _projectilePhysics.Bounce(temp); //bounces projectile if need be
        _moveDirection = temp;

        //_projectilePhysics.MoveWithCollision(temp.x, temp.y);
        
        _projectilePhysics.FreeFall();
        //_projectilePhysics.GetComponent<Rigidbody2D>().MovePosition(_projectilePhysics.GetComponent<Rigidbody2D>().position + temp); //TODO : moveposition should be performed in BulletPhysics, or whatever the new physics system for bullets is
        _projectilePhysics.MoveWithCollision(temp.x, temp.y);
        _projectilePhysics.MoveCharacterPositionPhysics(_moveDirection.x, _moveDirection.y);
	}


    //Resets the bullet for reuse
    public void ResetBullet()
    {
        _projectilePhysics.Reset();
        Start();
    }


    //===========================================================| Special Abilities

    /// <summary>
    /// Bounces a projectile off of a wall
    /// </summary>
    protected void Bounce()
    {
        BoxCollider2D bulletcollider = _projectilePhysics.GetComponent<BoxCollider2D>();



        if (!_projectilePhysics.IsCollidingWithEnvironment()) return; //if nothin goin on, exit

    }
}
