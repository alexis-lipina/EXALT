using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.EventSystems;

public class AccessibilityMenu : MonoBehaviour
{
    [SerializeField] private Slider Screenshake_Slider;
    [SerializeField] private ToggleButton IsFlashingEnabled_Button;
    [SerializeField] private ToggleButton LowHPVignette_Button;
    //[SerializeField] private Slider UIScale_Slider;
    [SerializeField] private GameObject _defaultOption;

    public GameObject SourceUIMenu;


    void Start()
    {
        IsFlashingEnabled_Button.SetCurrentValue(AccessibilityOptionsSingleton.GetInstance().IsFlashingEnabled);
        Screenshake_Slider.value = AccessibilityOptionsSingleton.GetInstance().ScreenshakeAmount;
    }

    public void SaveCurrentState()
    {
        AccessibilityOptionsSingleton.GetInstance().IsFlashingEnabled = IsFlashingEnabled_Button.GetCurrentValue();
        AccessibilityOptionsSingleton.GetInstance().ScreenshakeAmount = Screenshake_Slider.value;
        //AccessibilityOptionsSingleton.GetInstance().UIScale = UIScale_Slider.value;
        AccessibilityOptionsSingleton.GetInstance().SaveCurrentOptions();
    }

    void OnButtonPress(InputActionEventData data)
    {
        if (data.actionName == "UICancel" || data.actionName == "Pause")
        {
            AccessibilityOptionsSingleton.GetInstance().SaveCurrentOptions();
            SourceUIMenu.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void OnUIScaleChanged()
    {
        //transform.root.GetComponent<CanvasScaler>().scaleFactor = UIScale_Slider.value;
        //AccessibilityOptionsSingleton.GetInstance().UIScale = UIScale_Slider.value;
        AccessibilityOptionsSingleton.GetInstance().SaveCurrentOptions();
    }

    private void OnEnable()
    {
        ReInput.players.GetPlayer(0).AddInputEventDelegate(OnButtonPress, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
        EventSystem.current.SetSelectedGameObject(_defaultOption);
    }
    private void OnDisable()
    {
        SaveCurrentState();
        ReInput.players.GetPlayer(0).RemoveInputEventDelegate(OnButtonPress);
    }
}
