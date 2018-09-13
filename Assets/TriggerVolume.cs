using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

/// <summary>
/// Pseudo-3D version of a trigger collider - provides publicly visible, privately modifiable
/// data on whether certain entities are within the volume, and what specific entities they are.
/// Intended to be used for traps, doors, puzzles and cinematic triggers.
/// </summary>
public class TriggerVolume : PhysicsObject
{


    [SerializeField] private float _startTopHeight;
    [SerializeField] private float _startBottomHeight;
    [SerializeField] private bool _isTriggeredByPlayer;
    [SerializeField] private bool _isTriggeredByFriend;
    [SerializeField] private bool _isTriggeredByEnemy;

    private float _height;
    private bool _isTriggered;
    public bool IsTriggered
    {
        get { return _isTriggered; }
    }


    private List<GameObject> _objectsInAirspace; //all object currently in contact with BoxCollider2D but which are not necessarily touching the virtual 3D collider
    [SerializeField] private List<GameObject> _touchingObjects;
    public List<GameObject> TouchingObjects
    {
        get { return _touchingObjects; }
    }

    void Awake()
    {
        _objectsInAirspace = new List<GameObject>();
        _touchingObjects = new List<GameObject>();
        _isTriggered = false;
        bottomHeight = _startBottomHeight;
        topHeight = _startTopHeight;
    }
    


    void Update()
    {
        if (_touchingObjects.Count > 0)
        {
            _isTriggered = true;
        }
        else
        {
            _isTriggered = false;
        }


        //because OnTriggerStay is a broken piece of garbo...
        foreach (GameObject other in _objectsInAirspace)
        {
            if (!_touchingObjects.Contains(other) && IsVerticalCollision(other.GetComponent<EntityPhysics>())) //check if enters virtual collider
            {
                _touchingObjects.Add(other.gameObject);
                //Debug.Log("Entered!");
            }
            else if (_touchingObjects.Contains(other) && !IsVerticalCollision(other.GetComponent<EntityPhysics>())) //check if exits virtual collider
            {
                _touchingObjects.Remove(other.gameObject);
                //Debug.Log("Exited!");
            }
        }
        if (_touchingObjects.Count > 0)
        {
            for (int i = _touchingObjects.Count-1; i >= 0; i--)
            {
                if (!_objectsInAirspace.Contains(_touchingObjects[i]))
                {
                    _touchingObjects.RemoveAt(i);
                }
            }
        }
    }







    // COLLISION RESOLUTION

    void OnTriggerEnter2D(Collider2D other)
    {
        /*
        if (other.gameObject.GetComponent<EntityPhysics>() != null)
        {
            if (IsVerticalCollision(other.GetComponent<EntityPhysics>()))
            {
                if (_isTriggeredByFriend && other.gameObject.tag == "Friend")
                {
                    Debug.Log("Triggered by Friend");
                    _touchingObjects.Add(other.gameObject);
                }
                if (_isTriggeredByEnemy && other.gameObject.tag == "Enemy")
                {
                    Debug.Log("Triggered by Enemy");
                    _touchingObjects.Add(other.gameObject);

                }
            }
        }
        */

        if (other.gameObject.GetComponent<EntityPhysics>() != null)
        {
            if (_isTriggeredByFriend && other.gameObject.tag == "Friend")
            {
                _objectsInAirspace.Add(other.gameObject);
            }
            if (_isTriggeredByEnemy && other.gameObject.tag == "Enemy")
            {
                _objectsInAirspace.Add(other.gameObject);
            }
            
        }


    }

    //necessary because an entity could enter the virtual 3D collider while having already been inside the 2D collider
    /*
    void OnTriggerStay2D(Collider2D other)
    {
        
        if (other.GetComponent<EntityPhysics>() != null)
        {
            if (_touchingObjects.Contains(other.gameObject))
            {
                if (!IsVerticalCollision(other.GetComponent<EntityPhysics>())) //check if other just left the collider
                {
                    _touchingObjects.Remove(other.gameObject);
                    Debug.Log("Someone Left");
                }
            }
            else
            {
                if (IsVerticalCollision(other.GetComponent<EntityPhysics>())) //check if other just entered the collider
                {
                    if (_isTriggeredByFriend && other.gameObject.tag == "Friend")
                    {
                        Debug.Log("Triggered by Friend");
                        _touchingObjects.Add(other.gameObject);
                    }
                    if (_isTriggeredByEnemy && other.gameObject.tag == "Enemy")
                    {
                        Debug.Log("Triggered by Enemy");
                        _touchingObjects.Add(other.gameObject);

                    }
                }
            }
        }
    }
    */

    void OnTriggerExit2D(Collider2D other)
    {
        if (_objectsInAirspace.Contains(other.gameObject))
        {
            //Debug.Log("Exited!");
            _objectsInAirspace.Remove(other.gameObject);
        }
    }



    /// <summary>
    /// Modifies the bottom and top of collider to reflect new position
    /// </summary>
    /// <param name="newBase"></param>
    public void MoveBottom(float newbottom)
    {
        bottomHeight = newbottom;
        topHeight = newbottom + _height;
    }
    /// <summary>
    /// Checks the virtual z-axis for overlap
    /// </summary>
    /// <returns></returns>
    protected bool IsVerticalCollision(EntityPhysics other)
    {
        return (other.GetBottomHeight() < topHeight && other.GetTopHeight() > bottomHeight);
    }
}
