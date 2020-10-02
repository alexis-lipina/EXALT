using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainOptionsMenu : MonoBehaviour
{
    [SerializeField] private ControlMenuManager _controlMenuManager;
    [SerializeField] private AccessibilityMenu accessibilityMenu;
    [SerializeField] private CustomizationMenu customizationMenu;
    [Space(10)]
    public GameObject SourceMenu;
    [SerializeField] private Button _defaultButton;

    public void OnControlsPressed()
    {
        Debug.Log("Loading controls");
        _controlMenuManager.Source_Menu = gameObject;
        _controlMenuManager.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnAccessibilityPressed()
    {
        Debug.Log("Loading accessibility");
        accessibilityMenu.SourceUIMenu = gameObject;
        accessibilityMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnCustomizationPressed()
    {
        Debug.Log("Loading customization");
        customizationMenu.SourceUIMenu = gameObject;
        customizationMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnBackToMainMenuPressed()
    {
        gameObject.SetActive(false);
        SourceMenu.SetActive(true);
    }


    public void OnButtonPress(InputActionEventData data)
    {
        if (data.actionName == "UICancel"  || data.actionName == "Pause")
        {
            AccessibilityOptionsSingleton.GetInstance().SaveCurrentOptions();
            SourceMenu.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void OnEnable()
    {
        ReInput.players.GetPlayer(0).AddInputEventDelegate(OnButtonPress, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
        EventSystem.current.SetSelectedGameObject(_defaultButton.gameObject);
    }

    private void OnDisable()
    {
        ReInput.players.GetPlayer(0).RemoveInputEventDelegate(OnButtonPress);
    }
}
