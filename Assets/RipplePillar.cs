using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipplePillar : MonoBehaviour
{
    [SerializeField] private int _positionX;
    [SerializeField] private int _positionY;
    [SerializeField] private RippleController _controller;
    [SerializeField] private EnvironmentDynamicPhysics _physics;
    private float _height;


	// Use this for initialization
	void Start () {
        _height = _physics.TopHeight - _physics.BottomHeight;	
	}
	
	// Update is called once per frame
	void Update ()
    {
        
        float newElevation = _controller.grid[_positionX, _positionY] * _height;
        float delta = newElevation - _physics.TopHeight;


        _physics.TopHeight = newElevation; //change physics parameters
        _physics.BottomHeight = newElevation - _height; //change physics parameters
        gameObject.GetComponent<Transform>().position = new Vector3(gameObject.GetComponent<Transform>().position.x, gameObject.GetComponent<Transform>().position.y + delta, gameObject.GetComponent<Transform>().position.z);
        gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(gameObject.GetComponent<BoxCollider2D>().offset.x, gameObject.GetComponent<BoxCollider2D>().offset.y - delta);
    }
}
