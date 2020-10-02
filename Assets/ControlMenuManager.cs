using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rewired;
using Rewired.UI.ControlMapper;

struct AxisInfo
{
    bool IsPositive;
    int ElementId;

    public AxisInfo(bool isPositive, int elementId)
    {
        IsPositive = isPositive;
        ElementId = elementId;
    }
    public static bool operator ==(AxisInfo val1, AxisInfo val2)
    {
        return val1.IsPositive == val2.IsPositive && val1.ElementId == val2.ElementId;
    }
    public static bool operator !=(AxisInfo val1, AxisInfo val2)
    {
        return !(val1.IsPositive == val2.IsPositive && val1.ElementId == val2.ElementId);
    }
}


///This class manages the UI/frontend for control mapping. See ControlMappingManager for the backend.
public class ControlMenuManager : MonoBehaviour
{
    public static Rewired.Player player;

    [SerializeField] private string InputSpritesheet_ResourceName;
    [SerializeField] private Sprite DefaultInputIcon;
    [SerializeField] private Sprite ErrorInputIcon;
    [Space(10)]
    [SerializeField] private Sprite ControllerIcon_Keyboard;
    [SerializeField] private Sprite ControllerIcon_Mouse;
    [SerializeField] private Sprite ControllerIcon_Gamepad;
    [Space(10)]
    [SerializeField] private GameObject WaitForInput_Panel;
    [SerializeField] private GameObject WaitForInput_ControllerIcon;
    [SerializeField] private GameObject WaitForInput_ProgressBar;
    [Space(10)]
    [SerializeField] private ScrollRect scrollRect;
    private static Dictionary<string, Sprite> InputStringToImageMapping; //maps Exalt input string name for input to sprite, built on load
    private static Dictionary<int, string> GamepadIdToStringMapping; //maps GamepadTemplate id to Exalt input string name, built on load
    private static Dictionary<AxisInfo, string> GamepadAxisToStringMapping; //maps GamepadTemplate id to Exalt input string name, built on load
    private static Dictionary<int, string> KeyboardIdToStringMapping; //maps GamepadTemplate id to Exalt input string name, built on load
    private static Dictionary<int, string> MouseIdToStringMapping; //maps GamepadTemplate id to Exalt input string name, built on load
    private static Dictionary<AxisInfo, string> MouseAxisToStringMapping; //maps GamepadTemplate id to Exalt input string name, built on load
    public List<InputMappingRow> MappingRows;

    private List<Button> Buttons_KBM;
    private List<Button> Buttons_Gamepad;

    private bool Buttons_KBM_IsEnabled = true;
    private bool Buttons_Gamepad_IsEnabled = true;
    private bool IsWaitingForInput = false;
    private GameObject LastSelectedGameObject;

    //private List<InputMapper> currentRunningInputMappers;

    private ControlMappingManager backend;

    #region Dumb Weird Delegate Stuff
    private bool isInputMapperFinishedMapping = false;
    private bool GetInputMapperStatus()
    {
        return isInputMapperFinishedMapping;
    }
    private void SetInputMapperStatusToDone()
    {
        isInputMapperFinishedMapping = true;
    }
    #endregion

    /// <summary>
    /// On closing the control menu, Source_Menu will be set active
    /// </summary>
    public GameObject Source_Menu;

    void OnEnable()
    {
        StartCoroutine(FocusElement(MappingRows[0].GetKBButton().gameObject));
    }

    public void InitializeMappings()
    {
        if (backend) return;

        backend = gameObject.GetComponent<ControlMappingManager>();

        #region Setup GamepadIdToStringMapping table
        if (GamepadIdToStringMapping == null)
        {
            GamepadIdToStringMapping = new Dictionary<int, string>();
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_actionTopRow1, "FACEBUTTON_LEFT");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_actionTopRow2, "FACEBUTTON_UP");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_actionBottomRow1, "FACEBUTTON_DOWN");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_actionBottomRow2, "FACEBUTTON_RIGHT");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_center1, "CENTER_L");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_center2, "CENTER_R");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_dPadDown, "DPAD_DOWN");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_dPadUp, "DPAD_UP");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_dPadRight, "DPAD_RIGHT");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_dPadLeft, "DPAD_LEFT");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_leftBumper, "BUMPER_L");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_rightBumper, "BUMPER_R");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_leftTrigger, "TRIGGER_L");
            GamepadIdToStringMapping.Add(GamepadTemplate.elementId_rightTrigger, "TRIGGER_R");
        }
        if (GamepadAxisToStringMapping == null)
        {
            GamepadAxisToStringMapping = new Dictionary<AxisInfo, string>();
            GamepadAxisToStringMapping.Add(new AxisInfo(true,  GamepadTemplate.elementId_leftStickX),   "LS_Right");
            GamepadAxisToStringMapping.Add(new AxisInfo(false, GamepadTemplate.elementId_leftStickX),   "LS_Left");
            GamepadAxisToStringMapping.Add(new AxisInfo(true,  GamepadTemplate.elementId_leftStickY),   "LS_Up");
            GamepadAxisToStringMapping.Add(new AxisInfo(false, GamepadTemplate.elementId_leftStickY),   "LS_Down");
            GamepadAxisToStringMapping.Add(new AxisInfo(true,  GamepadTemplate.elementId_rightStickX),  "RS_Right");
            GamepadAxisToStringMapping.Add(new AxisInfo(false, GamepadTemplate.elementId_rightStickX),  "RS_Left");
            GamepadAxisToStringMapping.Add(new AxisInfo(true,  GamepadTemplate.elementId_rightStickY),  "RS_Up");
            GamepadAxisToStringMapping.Add(new AxisInfo(false, GamepadTemplate.elementId_rightStickY),  "RS_Down");
        }
        if (KeyboardIdToStringMapping == null)
        {
            KeyboardIdToStringMapping = new Dictionary<int, string>();
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha1,           "KEY_1");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha2,           "KEY_2");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha3,           "KEY_3");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha4,           "KEY_4");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha5,           "KEY_5");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha6,           "KEY_6");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha7,           "KEY_7");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha8,           "KEY_8");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha9,           "KEY_9");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Alpha0,           "KEY_0");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.A,                "KEY_A");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.B,                "KEY_B");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.C,                "KEY_C");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.D,                "KEY_D");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.E,                "KEY_E");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F,                "KEY_F");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.G,                "KEY_G");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.H,                "KEY_H");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.I,                "KEY_I");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.J,                "KEY_J");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.K,                "KEY_K");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.L,                "KEY_L");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.M,                "KEY_M");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.N,                "KEY_N");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.O,                "KEY_O");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.P,                "KEY_P");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Q,                "KEY_Q");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.R,                "KEY_R");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.S,                "KEY_S");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.T,                "KEY_T");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.U,                "KEY_U");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.V,                "KEY_V");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.W,                "KEY_W");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.X,                "KEY_X");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Y,                "KEY_Y");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Z,                "KEY_Z");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Space,            "KEY_Space");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Comma,            "KEY_Comma");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Period,           "KEY_Period");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Slash,            "KEY_ForwardSlash");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Semicolon,        "KEY_Semicolon");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.BackQuote,        "KEY_Backtick");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Minus,            "KEY_Dash");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Equals,           "KEY_Equals");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.LeftBracket,      "KEY_LeftBracket");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.RightBracket,     "KEY_RightBracket");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Backslash,        "KEY_BackSlash");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Quote,            "KEY_Apostrophe");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.LeftArrow,        "KEY_LEFT");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.RightArrow,       "KEY_RIGHT");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.UpArrow,          "KEY_UP");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.DownArrow,        "KEY_DOWN");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Return,           "KEY_Enter");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.LeftShift,        "KEY_Shift");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.LeftControl,      "KEY_Ctrl");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Tab,              "KEY_Tab");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.Escape,           "KEY_Esc");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F1,               "KEY_F1");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F2,               "KEY_F2");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F3,               "KEY_F3");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F4,               "KEY_F4");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F5,               "KEY_F5");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F6,               "KEY_F6");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F7,               "KEY_F7");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F8,               "KEY_F8");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F9,               "KEY_F9");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F10,              "KEY_F10");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F11,              "KEY_F11");
            KeyboardIdToStringMapping.Add((int)KeyboardKeyCode.F12,              "KEY_F12");
        }
        if (MouseIdToStringMapping == null)
        {
            MouseIdToStringMapping = new Dictionary<int, string>();
            MouseIdToStringMapping.Add((int)MouseInputElement.Button0              , "MOUSE_LEFT");
            MouseIdToStringMapping.Add((int)MouseInputElement.Button1              , "MOUSE_RIGHT");
            MouseIdToStringMapping.Add((int)MouseInputElement.Button2              , "MOUSE_MIDDLE");
            MouseIdToStringMapping.Add((int)MouseInputElement.Button3              , "MOUSE_4");
            MouseIdToStringMapping.Add((int)MouseInputElement.Button4              , "MOUSE_5");
            MouseIdToStringMapping.Add((int)MouseInputElement.Button5              , "MOUSE_6");
        }
        if (MouseAxisToStringMapping == null)
        {
            MouseAxisToStringMapping = new Dictionary<AxisInfo, string>();
            MouseAxisToStringMapping.Add(new AxisInfo(true,  (int)MouseInputElement.AxisX),  "MOUSE_MOVERIGHT");
            MouseAxisToStringMapping.Add(new AxisInfo(false, (int)MouseInputElement.AxisX),  "MOUSE_MOVELEFT");
            MouseAxisToStringMapping.Add(new AxisInfo(true,  (int)MouseInputElement.AxisY),  "MOUSE_MOVEUP");
            MouseAxisToStringMapping.Add(new AxisInfo(false, (int)MouseInputElement.AxisY),  "MOUSE_MOVEDOWN");
        }
            #endregion

            Sprite[] sprites = Resources.LoadAll<Sprite>(InputSpritesheet_ResourceName);
        InputStringToImageMapping = new Dictionary<string, Sprite>();
        foreach (Sprite sprite in sprites)
        {
            //_inputMapping.Add(sprite.name, sprite);
            if (GamepadIdToStringMapping.ContainsValue(sprite.name) || KeyboardIdToStringMapping.ContainsValue(sprite.name) || MouseIdToStringMapping.ContainsValue(sprite.name) || GamepadAxisToStringMapping.ContainsValue(sprite.name) || MouseAxisToStringMapping.ContainsValue(sprite.name) || sprite.name == "UNKNOWN")
            {
                //Debug.Log("Input sprite added : " + sprite.name);
                InputStringToImageMapping.Add(sprite.name, sprite);
            }
            else
            {
                Debug.LogWarning("Sprite " + sprite.name + " does not have corresponding entry in any input table");
            }
        }
        // PLAN FOR "press button to remap" : when an input mapping button is pressed, calls a function on ControlMenuManager that sends its info - sets all other buttons to non-interactable, focus shifts to nowhere, should probably be a pulsing square 
        // outline on where the new input will be assigned (do this with an overlay, darkens the whole screen except the focused element and puts a "press button to assign"/"push axis to assign"), then when an input is received, remap it on the backend, 
        // set all buttons to interactable (honestly could just use a canvas group that parents over the buttons?) and refocuses to the focused button. Rather than a button toggling on and off or something, I think if it just fires a oneoff function w
        // a coroutine that just waits for input or something and just adds a UI overlay would be good. Animating the selected row/input would be good too, with a pulse
    }


    // Start is called before the first frame update
    void Start()
    {
        backend = GetComponent<ControlMappingManager>();
        if (player == null) player = ReInput.players.GetPlayer(0);

        Debug.Log(ReInput.mapping.JoystickLayouts.Count);
        //ReInput.mapping.JoystickLayouts[0];
        //ReInput.mapping.GetControllerTemplateMapInstance();

        //try joystick mapping shit (THIS GOOD)
        /*
        if (player.controllers.joystickCount > 0)
        {
            Debug.Log(player.controllers.Joysticks[0].Templates[0].typeGuid);
            List<ControllerTemplateElementTarget> targets = new List<ControllerTemplateElementTarget>();
            player.controllers.Joysticks[0].Templates[0].GetElementTargets(player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Joystick, "Melee", false), targets);
            Debug.Log(targets[0].element.id);
        }*/


        //RefreshGamepadMappings();
        //RefreshKbmMappings();
    }

    void Update()
    {
        //press menu or back to return
        if (player.GetButtonDown("UICancel") || player.GetButtonDown("Pause"))
        {
            Source_Menu.SetActive(true);
            gameObject.SetActive(false);
        }

        if (player.controllers.joystickCount == 0 && Buttons_Gamepad_IsEnabled) // disable the joystick column of buttons
        {
            Debug.Log("disable joysticks!");
            foreach(var row in MappingRows)
            {
                row.SetButtonActive_Gamepad(false);
            }
            Buttons_Gamepad_IsEnabled = false;
        }
        else if (player.controllers.joystickCount > 0 && !Buttons_Gamepad_IsEnabled)
        {
            Debug.Log("Enable joysticks!");
            foreach (var row in MappingRows)
            {
                row.SetButtonActive_Gamepad(true);
            }
            Buttons_Gamepad_IsEnabled = true;
        }
        if (LastSelectedGameObject != EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject)
        {
            SnapTo(EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>());
            LastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        }
    }

    //Probably call this to "refresh" the UI with the correct icons for the mappings
    void RefreshGamepadMappings()
    {
        if (player.controllers.joystickCount == 0) return;

        var mappings = player.controllers.maps.GetMaps(ControllerType.Joystick, 0)[0];
        foreach (var row in MappingRows)
        {
            if (mappings.ContainsAction(row.GetMappingName()))
            {
                //var actionElementMapping = player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Joystick, row.GetMappingName(), false);
                List<ActionElementMap> actionElementMaps = new List<ActionElementMap>();
                player.controllers.maps.GetElementMapsWithAction(ControllerType.Joystick, row.GetMappingName(), false, actionElementMaps);

                if (actionElementMaps.Count == 1)
                {
                    List<ControllerTemplateElementTarget> targets = new List<ControllerTemplateElementTarget>();
                    player.controllers.Joysticks[0].Templates[0].GetElementTargets(actionElementMaps[0], targets);
                    var action = ReInput.mapping.GetAction(actionElementMaps[0].actionId);

                    if (GamepadIdToStringMapping.ContainsKey(targets[0].element.id))
                    {
                        row.SetButtonSprite_Gamepad(InputStringToImageMapping[GamepadIdToStringMapping[targets[0].element.id]]);
                    }
                }
                else if (actionElementMaps.Count > 1)
                {
                    foreach (var aem in actionElementMaps)
                    {
                        //else if (GamepadAxisToStringMapping.ContainsKey(new AxisInfo(targets[0].axisRange == AxisRange.Positive, targets[0].element.id)))
                        if (aem.axisContribution == Pole.Positive && row.IsPositiveAxis && GamepadAxisToStringMapping.ContainsKey(new AxisInfo(true, aem.elementIdentifierId)))
                        {
                            Debug.Log("POSITIVE");
                            row.SetButtonSprite_Gamepad(InputStringToImageMapping[GamepadAxisToStringMapping[new AxisInfo(true, aem.elementIdentifierId)]]);
                        }
                        //else if (GamepadAxisToStringMapping.ContainsKey(new AxisInfo(targets[0].axisRange == AxisRange.Negative, targets[0].element.id)))
                        else if (aem.axisContribution == Pole.Negative && !row.IsPositiveAxis && GamepadAxisToStringMapping.ContainsKey(new AxisInfo(false, aem.elementIdentifierId)))
                        {
                            Debug.Log("NEGATIVE");
                            row.SetButtonSprite_Gamepad(InputStringToImageMapping[GamepadAxisToStringMapping[new AxisInfo(false, aem.elementIdentifierId)]]);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Uh oh! Trouble finding icon!");
                    row.SetButtonSprite_Gamepad(ErrorInputIcon);
                }
            }
            else
            {
                Debug.LogError("Action \"" + row.GetMappingName() + "\" does not exist!");
            }
        }      
    }

    private void OnInputMapped(InputMapper.InputMappedEventData data)
    {
        Debug.Log("REMAPPED ACTION : " + data.actionElementMap.actionDescriptiveName + "    REMAPPED BUTTON : " + data.actionElementMap.elementIdentifierName);
        //probably eventually do something with the data idk
        SetInputMapperStatusToDone();
    }

    private IEnumerator FocusElement(GameObject newTarget)
    {
        EventSystem.current.SetSelectedGameObject(null, new BaseEventData(EventSystem.current));
        yield return null;
        EventSystem.current.SetSelectedGameObject(newTarget, new BaseEventData(EventSystem.current));
    }

    public void UpdateUI()
    {
        //called by controlmappingmanager when things need to change
        //RefreshGamepadMappings();
        //RefreshKbmMappings();

        if (player == null) player = ReInput.players.GetPlayer(0);

        var keyboardmap = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0);
        var mousemap = player.controllers.maps.GetMaps(ControllerType.Mouse, 0)[0];
        var joystickmap = player.controllers.maps.GetMaps(ControllerType.Joystick, 0).Count > 0 ? player.controllers.maps.GetMaps(ControllerType.Joystick, 0)[0] : null;

        foreach (var row in backend.rows)
        {
            bool registeredKB = false;
            bool registeredMouse = false;
            bool registeredGamepad = false;

            foreach (var actionelementmap in keyboardmap.ElementMapsWithAction(row.action.id))
            {
                if (actionelementmap.ShowInField(row.range))
                {
                    if (KeyboardIdToStringMapping.ContainsKey((int)actionelementmap.keyboardKeyCode))
                    {
                        row.inputMappingRow.SetButtonSprite_KB(InputStringToImageMapping[KeyboardIdToStringMapping[(int)actionelementmap.keyboardKeyCode]]);// expand this out for debugging
                    }
                    else
                    {
                        row.inputMappingRow.SetButtonSprite_KB(InputStringToImageMapping["UNKNOWN"]);
                    }
                    registeredKB = true;
                }
            }
            foreach (var actionelementmap in mousemap.ElementMapsWithAction(row.action.id)) 
            {
                if (actionelementmap.ShowInField(row.range))
                {
                    if (MouseIdToStringMapping.ContainsKey(actionelementmap.elementIdentifierId))
                    {
                        row.inputMappingRow.SetButtonSprite_Mouse(InputStringToImageMapping[MouseIdToStringMapping[actionelementmap.elementIdentifierId]]);
                        registeredMouse = true;
                    }
                    else if (MouseAxisToStringMapping.ContainsKey(new AxisInfo(true, actionelementmap.elementIdentifierId)) || MouseAxisToStringMapping.ContainsKey(new AxisInfo(false, actionelementmap.elementIdentifierId)))
                    {
                        var mousePole = actionelementmap.axisContribution == Pole.Positive && actionelementmap.axisRange == AxisRange.Positive || actionelementmap.axisContribution == Pole.Negative && actionelementmap.axisRange == AxisRange.Negative ? 
                            MouseAxisToStringMapping[new AxisInfo(row.range == AxisRange.Positive, actionelementmap.elementIdentifierId)] : 
                            MouseAxisToStringMapping[new AxisInfo(row.range == AxisRange.Negative, actionelementmap.elementIdentifierId)];
                        row.inputMappingRow.SetButtonSprite_Mouse(InputStringToImageMapping[mousePole]);
                        registeredMouse = true;
                    }
                    else
                    {
                        row.inputMappingRow.SetButtonSprite_Mouse(InputStringToImageMapping["UNKNOWN"]);
                        registeredMouse = true;
                    }
                }
            }
            if (joystickmap != null)
            {
                foreach (var actionelementmap in joystickmap.ElementMapsWithAction(row.action.id))
                {
                    if (actionelementmap.ShowInField(row.range))
                    {
                        List<ControllerTemplateElementTarget> targets = new List<ControllerTemplateElementTarget>();
                        player.controllers.Joysticks[0].Templates[0].GetElementTargets(actionelementmap, targets);
                        var a = targets[0].element.id;

                        if (GamepadIdToStringMapping.ContainsKey(a)) //is hardware-button
                        {
                            var b = GamepadIdToStringMapping.ContainsKey(a) ? GamepadIdToStringMapping[a] : GamepadAxisToStringMapping[new AxisInfo(row.range == AxisRange.Positive, a)];
                            var c = InputStringToImageMapping[b];
                            row.inputMappingRow.SetButtonSprite_Gamepad(c);
                            registeredGamepad = true;
                        }
                        else if (GamepadAxisToStringMapping.ContainsKey(new AxisInfo(true, a)) || GamepadAxisToStringMapping.ContainsKey(new AxisInfo(false, a))) //is hardware-axis (joystick)
                        {
                            var b = actionelementmap.axisContribution == Pole.Positive && actionelementmap.axisRange == AxisRange.Positive || actionelementmap.axisContribution == Pole.Negative && actionelementmap.axisRange == AxisRange.Negative ? GamepadAxisToStringMapping[new AxisInfo(row.range == AxisRange.Positive, a)] : GamepadAxisToStringMapping[new AxisInfo(row.range == AxisRange.Negative, a)];
                            var c = InputStringToImageMapping[b];
                            row.inputMappingRow.SetButtonSprite_Gamepad(c);
                            registeredGamepad = true;
                        }
                        else
                        {
                            row.inputMappingRow.SetButtonSprite_Gamepad(InputStringToImageMapping["UNKNOWN"]);
                            registeredGamepad = true;
                        }
                    }
                }
            }

            if (!registeredKB && !registeredMouse && !row.inputMappingRow.IsGamepadOnly)
            {
                row.inputMappingRow.SetButtonSprite_KB(ErrorInputIcon);
                row.inputMappingRow.SetButtonSprite_Mouse(ErrorInputIcon);
            }
            else if (!registeredKB && !row.inputMappingRow.IsGamepadOnly)
            {
                row.inputMappingRow.SetButtonSprite_KB(DefaultInputIcon);
            }
            else if (!registeredMouse && !row.inputMappingRow.IsGamepadOnly)
            {
                row.inputMappingRow.SetButtonSprite_Mouse(DefaultInputIcon);
            }

            if (!registeredGamepad)
            {
                row.inputMappingRow.SetButtonSprite_Gamepad(ErrorInputIcon);
            }
        }
    }

    public void StartWaitingForInputUI(ControllerType controllerType)
    {
        StartCoroutine(WaitForInput(controllerType));
    }

    public void StopWaitingForInputUI()
    {
        if (IsWaitingForInput)
        {
            IsWaitingForInput = false;
        }
        else
        {
            Debug.LogError("Warning! Called StopWaitForInput while no input was active!");
        }
    }

    private IEnumerator WaitForInput(ControllerType type)
    {
        if (IsWaitingForInput)
        {
            Debug.LogError("This should NEVER HAPPEN! Started WaitForInput while another WaitForInput is already active!");
        }

        IsWaitingForInput = true;
        WaitForInput_Panel.SetActive(true);
        player.controllers.maps.SetMapsEnabled(false, "UIControls");
        GameObject currentUIElement = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);

        switch (type)
        {
            case ControllerType.Keyboard:
                WaitForInput_ControllerIcon.GetComponent<Image>().sprite = ControllerIcon_Keyboard;
                break;
            case ControllerType.Mouse:
                WaitForInput_ControllerIcon.GetComponent<Image>().sprite = ControllerIcon_Mouse;
                break;
            case ControllerType.Joystick:
                WaitForInput_ControllerIcon.GetComponent<Image>().sprite = ControllerIcon_Gamepad;
                break;
        }
        float mappingTimer = ControlMappingManager.MapperTimeout;
        WaitForInput_ProgressBar.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        Debug.Log("Mapping Timer (Start) : " + mappingTimer);

        while (/*mappingTimer > 0 &&*/ IsWaitingForInput)
        {
            yield return null;
            mappingTimer -= Time.unscaledDeltaTime;
            WaitForInput_ProgressBar.GetComponent<RectTransform>().localScale = new Vector3(mappingTimer / ControlMappingManager.MapperTimeout, 1, 1);
        }
        Debug.Log("Mapping Timer (End) : " + mappingTimer);

        yield return new WaitForSecondsRealtime(0.1f);
        
        IsWaitingForInput = false;
        WaitForInput_Panel.SetActive(false);
        player.controllers.maps.SetMapsEnabled(true, "UIControls");
        EventSystem.current.SetSelectedGameObject(currentUIElement);
        WaitForInput_ProgressBar.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

    }

    void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        StartCoroutine(InterpTo((Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
            - (Vector2)scrollRect.transform.InverseTransformPoint(target.parent.position) + new Vector2(0, scrollRect.viewport.rect.height * -0.5f)));
        return;
    }

    private IEnumerator InterpTo(Vector2 end)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return null;
            scrollRect.content.anchoredPosition = Vector2.Lerp(scrollRect.content.anchoredPosition, end, 0.5f);
        }
    }

    public Sprite GetSpriteForAction(string _actionName)
    {
        if (player == null)
        {
            InitializeMappings();
            player = ReInput.players.GetPlayer(0);
            backend.InitializeUI();
        }

        var lastController = player.controllers.GetLastActiveController();

        var keyboardmap = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0);
        var mousemap = player.controllers.maps.GetMaps(ControllerType.Mouse, 0)[0];
        var joystickmap = player.controllers.maps.GetMaps(ControllerType.Joystick, 0).Count > 0 ? player.controllers.maps.GetMaps(ControllerType.Joystick, 0)[0] : null;

        UpdateUI();

        switch (lastController.type)
        {
            case ControllerType.Joystick:
                foreach (var row in MappingRows)
                {
                    if (row.GetMappingName() == _actionName)
                    {
                        return row.GetButtonSprite_Gamepad();
                    }
                }
                break;
            default:
                //kbm
                foreach (var row in MappingRows)
                {
                    string rowMappingName = row.GetMappingName();
                    if (rowMappingName == _actionName && row.GetButtonSprite_Mouse() != DefaultInputIcon && row.GetButtonSprite_Mouse() != ErrorInputIcon)
                    {
                        return row.GetButtonSprite_Mouse();
                    }
                    else if (rowMappingName == _actionName && row.GetButtonSprite_Keyboard() != DefaultInputIcon && row.GetButtonSprite_Keyboard() != ErrorInputIcon)
                    {

                        return row.GetButtonSprite_Keyboard();
                    }
                }
                break;
        }
        Debug.LogError("Error - could not find sprite for action " + _actionName + " and controller type " + lastController.type);

        return ErrorInputIcon;
    }
}
