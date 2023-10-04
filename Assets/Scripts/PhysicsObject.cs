using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class provides a framework for any "3D" object in the game. 
/// 
/// Since the game I'm working on is using an "artificial" 3D using Unity2D 
/// with a custom 3D implementation, and the illusion of depth, I saw it 
/// prudent to use an abstract superclass to make collision resolution between
/// different objects more accessible.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PhysicsObject : MonoBehaviour
{

    [SerializeField] protected float bottomHeight;
    [SerializeField] protected float topHeight;
    private BoxCollider2D _objectCollider;
    public BoxCollider2D ObjectCollider
    {
        get
        {
            if (!_objectCollider) _objectCollider = GetComponent<BoxCollider2D>();
            return _objectCollider;
        }
    }


	
    public float GetTopHeight()
    {
        return topHeight;
    }

    public float GetBottomHeight()
    {
        return bottomHeight;
    }
	
	
}
