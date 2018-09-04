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
        _physics.TopHeight = _controller.grid[_positionX, _positionY] * _height;
	}
}
