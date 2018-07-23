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


    private bool _isTriggered;
    public bool IsTriggered
    {
        get { return _isTriggered; }
    }

    private List<GameObject> _touchingObjects;
    public List<GameObject> TouchingObjects
    {
        get { return _touchingObjects; }
    }

    void Awake()
    {
        _isTriggered = false;
        bottomHeight = _startBottomHeight;
        topHeight = _startTopHeight;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isTriggeredByFriend)
        {
            if (other.gameObject.tag == "Friend")
            {
                Debug.Log("Friend entered volume");
            }
        }
        if (_isTriggeredByEnemy)
        {

        }

    }
}
