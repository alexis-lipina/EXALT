using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

/// <summary>
/// **Does not extend EntityHandler**
/// 
/// This class controls the reticle, an object that will most likely be controlled explicitly by the player. It will change depending on the weapon being used. The reticle should almost always be at the same height as the player.
/// </summary>
public class ReticleHandler : MonoBehaviour
{
    private const float HEIGHT_TOLERANCE = 0.6f;
    //[SerializeField] private InputHandler _inputHandler;
    //[SerializeField] private GameObject _reticleSprite;
    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private EntityPhysics _entityPhysics; //reticle
    [SerializeField] private Vector2 _maxMoveSpeed;
    [SerializeField] private float _maxReticleDistance;
    [SerializeField] private bool _isUsingCursor;

    private float _timeSinceMoved = 0f;
    private const float _timeTillFade = 4f;
    private bool _isFaded = false;


    private Vector2 _reticleDimensions;
    private Vector2 _tempRightAnalogDirection;
    private Player controller;
    private Vector2 _cursorWorldPos = Vector2.zero;

    public bool IsUsingMouse
    {
        get { return _isUsingCursor; }
        set { _isUsingCursor = value; }
    }


    void Awake()
    {
        _reticleDimensions = _entityPhysics.GetComponent<BoxCollider2D>().size;
        controller = ReInput.players.GetPlayer(0);
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
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

        //Get direction in which to go
        if (_isUsingCursor)
        {
            _tempRightAnalogDirection = new Vector2(_cursorWorldPos.x, _cursorWorldPos.y - _playerPhysics.GetBottomHeight() /* - 0.5f */ ) - (Vector2)_playerPhysics.GetComponent<Transform>().position;
            _tempRightAnalogDirection *= 0.05f;
        }
        else
        {
            if (controller.GetAxis2DRaw("LookHorizontal", "LookVertical").magnitude <= 0.2)
            {
                //_tempRightAnalogDirection = _tempRightAnalogDirection.normalized;
                if (controller.GetAxis2DRaw("MoveHorizontal", "MoveVertical").magnitude >= 0.2)
                {
                    _tempRightAnalogDirection = controller.GetAxis2DRaw("MoveHorizontal", "MoveVertical");
                    _timeSinceMoved = 0f;
                }
                else
                {
                    _tempRightAnalogDirection = _tempRightAnalogDirection.normalized * 0.2f;
                    _timeSinceMoved += Time.fixedDeltaTime;
                }
            }
            else
            {
                _timeSinceMoved = 0f;
                _tempRightAnalogDirection = controller.GetAxis2DRaw("LookHorizontal", "LookVertical");
            }
        }

        if ( !_isFaded && _timeSinceMoved > _timeTillFade )
        {
            StartCoroutine(FadeOut());
            _isFaded = true;
        }
        if ( _isFaded && _timeSinceMoved == 0 )
        {
            StartCoroutine(FadeIn());
            _isFaded = false;
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
                if (hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight() > _entityPhysics.GetBottomHeight() + HEIGHT_TOLERANCE && hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetBottomHeight() < _entityPhysics.GetBottomHeight() + HEIGHT_TOLERANCE) // if the height of the terrain object is greater than the altitude of the player
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

    /// <summary>
    /// Sent the position of the mouse in world, needs to factor in height and stuff itself
    /// </summary>
    /// <param name="position"></param>
    public void UpdateMousePosition(Vector2 position)
    {
        _isUsingCursor = true;
        _cursorWorldPos = position;
    }

    private IEnumerator FadeOut()
    {
        float opacity = 1f;
        for (int i = 0; i < 10; i++)
        {
            _entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, opacity);
            SetOpacityOfShadows(_entityPhysics, opacity);

            opacity -= 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        SetOpacityOfShadows(_entityPhysics, 0);
        _entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);

    }

    private IEnumerator FadeIn()
    {
        float opacity = 0f;
        for (int i = 0; i < 10; i++)
        {
            _entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, opacity);
            SetOpacityOfShadows(_entityPhysics, opacity);
            opacity += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        _entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        SetOpacityOfShadows(_entityPhysics, 1);

    }

    private void SetOpacityOfShadows(EntityPhysics physics, float opacity)
    {
        var sprites = physics.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.material.SetFloat("_Opacity", opacity);
        }
    }

}
