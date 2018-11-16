using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonolithUpdater : MonoBehaviour
{
    private EnvironmentDynamicPhysics _physics;
    private float _lowestelevation;
    private float _highestelevation;
    private float _totalheight;

    [SerializeField] private int position_x;
    [SerializeField] private int position_y;
	// Use this for initialization
	void Start ()
    {
        _physics = GetComponent<EnvironmentDynamicPhysics>();
        _lowestelevation = 0;
        _highestelevation = 5;
        _totalheight = _physics.TopHeight - _physics.BottomHeight;
	}
	
	// Update is called once per frame
	void Update ()
    {/*
        float delta = _physics.BottomHeight;
        _physics.BottomHeight = Mathf.Lerp(_lowestelevation, _highestelevation, MonolithManager.array[position_x, position_y] * 0.5f + 1);
        delta -= _physics.BottomHeight;
        _physics.TopHeight = _physics.BottomHeight + _totalheight;
        
        gameObject.GetComponent<Transform>().position = new Vector3(gameObject.GetComponent<Transform>().position.x, gameObject.GetComponent<Transform>().position.y + delta, gameObject.GetComponent<Transform>().position.z);
        gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(gameObject.GetComponent<BoxCollider2D>().offset.x, gameObject.GetComponent<BoxCollider2D>().offset.y + delta);
        _physics.BottomHeight += delta;
        _physics.TopHeight += delta;*/


        float newElevation = MonolithManager.array[position_x, position_y] * _totalheight;
        float delta = newElevation - _physics.TopHeight;


        _physics.TopHeight = newElevation; //change physics parameters
        _physics.BottomHeight = newElevation - _totalheight; //change physics parameters

        //old code - messes with boxcollider position and shadowcasting
        gameObject.GetComponent<Transform>().position = new Vector3(gameObject.GetComponent<Transform>().position.x, gameObject.GetComponent<Transform>().position.y + delta, gameObject.GetComponent<Transform>().position.z);

        //_top.GetComponent<>
        gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(gameObject.GetComponent<BoxCollider2D>().offset.x, gameObject.GetComponent<BoxCollider2D>().offset.y - delta);
    }
}


