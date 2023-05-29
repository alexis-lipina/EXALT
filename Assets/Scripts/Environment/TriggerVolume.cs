using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public UnityEvent On3DOverlapEvent;


    private float _height;
    private bool _isTriggered;
    public bool IsTriggered
    {
        get { return _isTriggered; }
    }

    // attachment
    private EnvironmentPhysics attachParent; // if null, is not attached
    private Vector3 attachParentPriorPosition;


    private List<GameObject> _objectsInAirspace; //all object currently in contact with BoxCollider2D but which are not necessarily touching the virtual 3D collider
    [SerializeField] private List<GameObject> _touchingObjects;
    public List<GameObject> TouchingObjects
    {
        get { return _touchingObjects; }
    }

    // other triggers that add to the effective volume of this trigger
    public List<TriggerVolume> ChildTriggers;

    void Awake()
    {
        _objectsInAirspace = new List<GameObject>();
        _touchingObjects = new List<GameObject>();
        _isTriggered = false;
        bottomHeight = _startBottomHeight;
        topHeight = _startTopHeight;
        _height = topHeight - bottomHeight;
    }

    private void Start()
    {
        Collider2D[] colliders = new Collider2D[8];
        ContactFilter2D cf = new ContactFilter2D();
        int NumColliders = GetComponent<BoxCollider2D>().OverlapCollider(cf, colliders);
        for (int i = 0; i < NumColliders; i++)
        {
            //OnTriggerEnter2D(colliders[i]);
        }
    }

    void Update()
    {
        if (attachParent)
        {
            MoveBottom( bottomHeight + attachParent.BottomHeight - attachParentPriorPosition.z);
            attachParentPriorPosition = new Vector3(attachParent.transform.position.x, attachParent.transform.position.y, attachParent.BottomHeight);
            //Debug.Log("New trigger bottom height = " + bottomHeight);
        }

        if (_touchingObjects.Count > 0)
        {
            _isTriggered = true;
        }
        else
        {
            _isTriggered = false;
        }

        foreach (TriggerVolume tv in ChildTriggers)
        {
            if (tv.IsTriggered) { _isTriggered = true; }
        }


        //because OnTriggerStay is a broken piece of garbo...
        foreach (GameObject other in _objectsInAirspace)
        {
            if (!_touchingObjects.Contains(other) && IsVerticalCollision(other.GetComponent<EntityPhysics>())) //check if enters virtual collider
            {
                _touchingObjects.Add(other.gameObject);
                Debug.Log("Entered!");
                On3DOverlapEvent.Invoke();
            }
            else if (_touchingObjects.Contains(other) && !IsVerticalCollision(other.GetComponent<EntityPhysics>())) //check if exits virtual collider
            {
                _touchingObjects.Remove(other.gameObject);
                Debug.Log("Exited!");
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

    // forces trigger to change position as the target object changes position
    public void AttachTriggerToEnvironment(EnvironmentPhysics newAttachParent)
    {
        attachParent = newAttachParent;
        attachParentPriorPosition = new Vector3(attachParent.transform.position.x, attachParent.transform.position.y, attachParent.BottomHeight);
    }
}
