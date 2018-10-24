using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CursorHandler : MonoBehaviour
{
    [SerializeField] private PlayerHandler _player;
    [SerializeField] private ReticleHandler _reticle;
    [SerializeField] private CameraScript _camera;

    private Vector2 _cursorPos;
    [SerializeField] private bool _usingMouse;

	// Use this for initialization
	void Start ()
    {
        _cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        //if during joystick mouse moves or clicks, change to mouse. if during mouse, joystick moves, change to joystick

        //if currently using joystickmouse moves, change to mouse controls
        if (!_usingMouse && _cursorPos != (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition))
        {
            _usingMouse = true;
        }
        


        if (_usingMouse)
        {

        }
        else
        {

        }


        _cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = _cursorPos;

        _reticle.UpdateMousePosition(_cursorPos);
        _player.UpdateMousePosition(_cursorPos);
        _camera.UpdateMousePosition(_cursorPos);
	}
    /*
    private void OnEnable()
    {
        ReInput.players.Players[0].AddInputEventDelegate(SwitchToGamepad,
            UpdateLoopType.Update,
            InputActionEventType.ButtonJustPressed,
            ReInput.mapping.GetMouseLayout(0).);
    }

    private void OnDisable()
    {
        ReInput.eve
    }


    private void SwitchToGamepad()
    {

    }

    private void SwitchToMouse()
    {

    }
    */


}
