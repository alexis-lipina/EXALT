using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls a bullet!
/// </summary>
public class BulletHandler : ProjectileHandler
{
    [SerializeField] protected ProjectilePhysics _projectilePhysics;
    [SerializeField] public AudioClip SpawnSFX;
    [SerializeField] protected float _bulletSpeed;
    protected Vector2 _moveDirection;
    protected Weapon _sourceWeapon;

    private bool _canBounce;
    private bool _hasBeenHit = false;



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

    public bool HasBeenHit
    {
        get { return _hasBeenHit; }
    }

    public virtual void OnPlayerDeflect()
    {
        // do something unique
        _hasBeenHit = true;
    }

	// Use this for initialization
	virtual protected void Start ()
    {
        //_projectilePhysics.GetComponent<Rigidbody2D>().MovePosition(new Vector2(1000, 1000));
        //_projectilePhysics.ZVelocity = 0.5f;
        _projectilePhysics.Reset();
        _projectilePhysics.Velocity = _moveDirection * _bulletSpeed;
        //DEBUG
        //_moveDirection = new Vector2(1, 1).normalized;
    }
    
    //Resets the bullet for reuse
    public void ResetBullet()
    {
        _projectilePhysics.Reset();
        /*Start();
        _projectilePhysics.Velocity = MoveDirection;*/
    }

    private void OnEnable()
    {
        _projectilePhysics.Velocity = MoveDirection;
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
