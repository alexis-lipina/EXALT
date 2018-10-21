using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CursorHandler : MonoBehaviour
{
    [SerializeField] private PlayerHandler _player;
    [SerializeField] private ReticleHandler _reticle;
    [SerializeField]
    private CameraScript _camera;

    private Vector2 _cursorPos;

	// Use this for initialization
	void Start ()
    {
        _cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        _cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = _cursorPos;

        _reticle.UpdateMousePosition(_cursorPos);
        _player.UpdateMousePosition(_cursorPos);
        _camera.UpdateMousePosition(_cursorPos);
	}
}
