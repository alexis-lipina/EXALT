using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicEnvironmentFollow : MonoBehaviour
{
    [SerializeField] private EntityPhysics _destination;
    [SerializeField] private EnvironmentPhysics _environment;
    [SerializeField] private GameObject _sprites;
    [SerializeField] private float _verticalSpriteOffset;
    private float _height;
	// Use this for initialization
	void Start () {
        _height = _environment.TopHeight - _environment.BottomHeight;
	}
	
	// Update is called once per frame
	void Update ()
    {
        _environment.transform.position = new Vector3(_destination.transform.position.x, _destination.transform.position.y, _destination.transform.position.y + _environment.GetComponent<BoxCollider2D>().size.y / 2);
        _environment.BottomHeight = _destination.GetBottomHeight();
        _environment.TopHeight = _environment.BottomHeight + _height;
        _sprites.transform.position = new Vector3(_destination.transform.position.x, _destination.transform.position.y + _verticalSpriteOffset + _environment.BottomHeight, gameObject.transform.position.y + _destination.GetComponent<BoxCollider2D>().offset.y - _destination.GetComponent<BoxCollider2D>().size.y / 2);
        
	}
}
