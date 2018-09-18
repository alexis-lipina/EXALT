using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    [SerializeField] private PixelRotate _rotateScript;

    private float angle;
	// Use this for initialization
	void Start ()
    {
        angle = 0;
        _rotateScript.SetRotate(45);
	}
	
	// Update is called once per frame
	void Update ()
    {
        angle += Time.deltaTime;
        _rotateScript.SetRotate(angle);
	}
}
