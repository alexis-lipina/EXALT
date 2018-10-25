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
        _usingMouse = false;
        SwitchToGamepad(new InputActionEventData());
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (_usingMouse)
        {
            _cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = _cursorPos;

            _reticle.UpdateMousePosition(_cursorPos);
            _player.UpdateMousePosition(_cursorPos);
            _camera.UpdateMousePosition(_cursorPos);
        }
	}
    
    private void OnEnable()
    {
        ReInput.players.Players[0].AddInputEventDelegate(SwitchToGamepad,
            UpdateLoopType.Update,
            InputActionEventType.ButtonJustPressed, ReInput.mapping.GetActionId("GamepadSwitch"));
        ReInput.players.Players[0].AddInputEventDelegate(SwitchToMouse,
            UpdateLoopType.Update,
            InputActionEventType.ButtonJustPressed, ReInput.mapping.GetActionId("MouseSwitch"));

        

    }

    private void OnDisable()
    {
        ReInput.players.Players[0].RemoveInputEventDelegate(SwitchToGamepad,
            UpdateLoopType.Update,
            InputActionEventType.ButtonJustPressed, ReInput.mapping.GetActionId("GamepadSwitch"));
        ReInput.players.Players[0].RemoveInputEventDelegate(SwitchToMouse,
            UpdateLoopType.Update,
            InputActionEventType.ButtonJustPressed, ReInput.mapping.GetActionId("MouseSwitch"));
    }


    private void SwitchToGamepad(InputActionEventData data)
    {
        _usingMouse = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        Debug.Log("GAMEPAD");
        _player.IsUsingMouse = false;
        _reticle.IsUsingMouse = false;
        _camera.IsUsingMouse = false;
    }

    private void SwitchToMouse(InputActionEventData data)
    {
        _usingMouse = true;
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        Debug.Log("MOUSE");
        _player.IsUsingMouse = true;
        _reticle.IsUsingMouse = true;
        _camera.IsUsingMouse = true;
    }
    


}
