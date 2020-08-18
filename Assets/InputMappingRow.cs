using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class InputMappingRow : MonoBehaviour
{
    [SerializeField] private ControlMenuManager controlMenuManager;
    [SerializeField] private Button Button_Gamepad;
    [SerializeField] private Button Button_KB;
    [SerializeField] private Button Button_Mouse;
    [SerializeField] private string ActionName;
    [SerializeField] private int ActionId;
    [Space(10)]
    [SerializeField] public bool IsAxis;
    [SerializeField] public bool IsPositiveAxis;
    [SerializeField] public string AxisContributionName;

    public void SetButtonSprite_Gamepad(Sprite image)
    {
        Button_Gamepad.GetComponent<Image>().sprite = image;
    }

    public void SetButtonSprite_KB(Sprite image)
    {
        Button_KB.GetComponent<Image>().sprite = image;
    }

    public void SetButtonSprite_Mouse(Sprite image)
    {
        Button_Mouse.GetComponent<Image>().sprite = image;
    }

    public string GetMappingName()
    {
        return ActionName;
    }

    public void SetButtonActive_Gamepad(bool isActive)
    {
        Button_Gamepad.interactable = isActive;
    }

    public void SetButtonActive_KB(bool isActive)
    {
        Button_KB.interactable = isActive;
    }

    public void SetButtonActive_Mouse(bool isActive)
    {
        Button_Mouse.interactable = isActive;
    }
    /*
    public void Gamepad_OnRemapButtonPress()
    {
        controlMenuManager.StartWaitForInputForRemap(this, ControllerType.Joystick);
    }
    public void KBM_OnRemapButtonPress()
    {
        controlMenuManager.StartWaitForInputForRemap(this, ControllerType.Keyboard);
    }*/

    public Button GetGamepadButton()
    {
        return Button_Gamepad;
    }

    public Button GetKBButton()
    {
        return Button_KB;
    }

    public Button GetMouseButton()
    {
        return Button_Mouse;
    }

    public int GetActionId()
    {
        return ActionId;
    }
}
