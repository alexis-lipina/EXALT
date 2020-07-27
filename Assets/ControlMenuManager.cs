using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rewired;

struct ControlMappingRow
{
    string rewiredActionName;
    Image currentIcon;

}

public class ControlMenuManager : MonoBehaviour
{
    public static Rewired.Player player;

    [SerializeField] private string InputSpritesheet_ResourceName;
    [SerializeField] private Sprite DefaultInputIcon;
    private static Dictionary<string, Sprite> InputStringToImageMapping; //maps Exalt input string name for input to sprite, built on load
    private static Dictionary<int, string> InputIdToStringMapping; //maps GamepadTemplate id to Exalt input string name, built on load
    [SerializeField] private List<InputMappingRow> MappingRows;

    private List<Button> Buttons_KBM;
    private List<Button> Buttons_Gamepad;

    private bool Buttons_KBM_IsEnabled = true;
    private bool Buttons_Gamepad_IsEnabled = true;

    private InputMapper inputMapper;

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
        StartCoroutine(FocusElement(MappingRows[0].GetKBMButton().gameObject));
    }

    void Awake()
    {
        #region Setup InputIDToStringMapping table
        if (InputIdToStringMapping == null)
        {
            InputIdToStringMapping = new Dictionary<int, string>();
            InputIdToStringMapping.Add(GamepadTemplate.elementId_actionTopRow1     , "FACEBUTTON_LEFT");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_actionTopRow2     , "FACEBUTTON_UP");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_actionBottomRow1  , "FACEBUTTON_DOWN");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_actionBottomRow2  , "FACEBUTTON_RIGHT");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_center1           , "CENTER_L");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_center2           , "CENTER_R");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_dPadDown          , "DPAD_DOWN");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_dPadUp            , "DPAD_UP");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_dPadRight         , "DPAD_RIGHT");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_dPadLeft          , "DPAD_LEFT");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_leftBumper        , "BUMPER_L");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_rightBumper       , "BUMPER_R");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_leftTrigger       , "TRIGGER_L");
            InputIdToStringMapping.Add(GamepadTemplate.elementId_rightTrigger      , "TRIGGER_R");
        }
        #endregion

        Sprite[] sprites = Resources.LoadAll<Sprite>(InputSpritesheet_ResourceName);
        InputStringToImageMapping = new Dictionary<string, Sprite>();
        foreach (Sprite sprite in sprites)
        {
            //_inputMapping.Add(sprite.name, sprite);
            if (InputIdToStringMapping.ContainsValue(sprite.name))
            {
                //Debug.Log("Input sprite added : " + sprite.name);
                InputStringToImageMapping.Add(sprite.name, sprite);
            }
            else
            {
                Debug.LogWarning("Sprite " + sprite.name + " does not have corresponding entry in _inputEncoder!");
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


        //GetGamepadMappings();
        RefreshGamepadMappings();
    }

    // Update is called once per frame
    void Update()
    {
        //press menu or back to return
        if (player.GetButton("UICancel") || player.GetButton("Pause"))
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
        }
        else if (player.controllers.joystickCount > 0 && !Buttons_Gamepad_IsEnabled)
        {
            Debug.Log("Enable joysticks!");
            foreach (var row in MappingRows)
            {
                row.SetButtonActive_Gamepad(true);
            }
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
                List<ControllerTemplateElementTarget> targets = new List<ControllerTemplateElementTarget>();
                player.controllers.Joysticks[0].Templates[0].GetElementTargets(player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Joystick, row.GetMappingName() , false), targets);
                //Debug.Log("Mapping Name : " + row.GetMappingName());
                //Debug.Log("Element ID : " + targets[0].element.id);
                //Debug.Log("EXALT Input Name : " + InputIdToStringMapping[targets[0].element.id]);
                row.SetButtonSprite_Gamepad(InputStringToImageMapping[InputIdToStringMapping[targets[0].element.id]]);
                /*
                Debug.Log("epic!");
                var elementmaps = mappings.GetElementMapsWithAction(row.GetMappingName());
                if (elementmaps.Length == 1)
                {
                    Debug.Log(elementmaps[0].elementIdentifierName);
                    Debug.Log(elementmaps[0].elementIdentifierId);
                    // issue with this is it isn't the "generic" button, its specific to the gamepad
                    // https://guavaman.com/projects/rewired/docs/ControllerTemplates.html
                }
                else
                {
                    Debug.LogError("Multiple element mappings for action \"" + row.GetMappingName() + "\"");
                }
                */
            }
            else
            {
                Debug.LogError("Action \"" + row.GetMappingName() + "\" does not exist!");
            }
        }

        

        //ReInput.mapping.JoystickLayouts
        /*

        Debug.Log("epic");
        Debug.Log(player.controllers.joystickCount);
        //Debug.Log(player.controllers.maps.GetMaps(ControllerType.Joystick, ));
        foreach (var controllerMap in player.controllers.maps.GetMaps(player.controllers.Joysticks[0]))
        {
            Debug.Log("whoa");
            foreach (var actionElementMap in controllerMap.AllMaps)
            {
                InputAction action = ReInput.mapping.GetAction(actionElementMap.actionId);
                Debug.Log(actionElementMap.elementType + " " + actionElementMap.elementIndex + " is bound to " + (action != null ? action.name : "nothing"));
            }
        }
        //ReInput.mapping.JoystickLayouts[0];
        //InputLayout thing = ReInput.mapping.MapLayouts(ControllerType.Joystick)[0];
        //ReInput.mapping.GetJoystickMapInstance();
        
        //ReInput.controllers.joystick
        //foreach action that exists, find action in joystick mapping
        //player.controllers.Joysticks[0].
        //foreach ()
        //{

        //}

        */
    }

    public void StartWaitForInputForRemap(InputMappingRow row, ControllerType controllerType)
    {
        StartCoroutine(WaitForInputForRemap(row, controllerType));
        //SetGamepadMapping(row, controllerType);
    }

    private IEnumerator WaitForInputForRemap(InputMappingRow mappingRow, ControllerType controllerType)
    {
        //show wait for key press UI
        Color oldColor = mappingRow.GetComponent<Image>().color;
        mappingRow.GetComponent<Image>().color = new Color(0f, 1f, 1f, 1f);
        ReInput.players.GetPlayer(0).controllers.maps.SetAllMapsEnabled(false);
        /*
        //have a loop where you loop with a yield till the frame input is received and then detect where the input came from (only accept from gamepad if its a gamepad mapping for example)
        ControllerPollingInfo pollingInfo;
        bool mappingSuccessfullyAssigned = false;
        do
        {
            yield return null;
            pollingInfo = player.controllers.Joysticks[0].PollForFirstButtonDown();
            if (pollingInfo.success)
            {
                //pollingInfo.elementIdentifier.elementType == ControllerElementType.Axis; // TODO : this is super useful for separating implementation for axis mappings vs button mappings
                if (pollingInfo.controllerType == controllerType)
                {
                    Debug.Log("WOOP! Got it : " + pollingInfo.elementIdentifierName);
                    List<ControllerTemplateElementTarget> targets = new List<ControllerTemplateElementTarget>();
                    player.controllers.Joysticks[0].Templates[0].GetElementTargets(player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Joystick, mappingRow.GetMappingName(), false), targets);

                    //reassign mapping
                    SetGamepadMapping();

                    mappingSuccessfullyAssigned = true;
                }
            }
        }
        while (!mappingSuccessfullyAssigned);
        */

        //==============================| Setup input mapper

        var something = player.controllers.maps.GetMaps(ControllerType.Joystick, player.controllers.Joysticks[0].id)[0].ElementMapsWithAction(mappingRow.GetActionId());
        ActionElementMap mapToReplace = new ActionElementMap();
        int numMapsWithAction = 0;
        foreach (var thing in something)
        {
            mapToReplace = thing;
            numMapsWithAction++;
        }
        if (numMapsWithAction == 0)
        {
            Debug.LogError("Alert! No maps with the action of ID \"" + mappingRow.GetActionId() + "\" found!");
        }
        if (numMapsWithAction > 1)
        {
            Debug.LogError("Alert! Multiple maps with action ID \"" + mappingRow.GetActionId() + "\" found!");
        }

        //Setup for input mapper
        InputMapper.Context context = new InputMapper.Context()
        {
            actionId = mappingRow.GetActionId(),
            controllerMap = player.controllers.maps.GetMaps(ControllerType.Joystick, player.controllers.Joysticks[0].id)[0],
            actionRange = AxisRange.Positive,
            actionElementMapToReplace = mapToReplace
        };

        inputMapper = new InputMapper();

        inputMapper.options.timeout = 5f;
        inputMapper.options.ignoreMouseXAxis = true;
        inputMapper.options.ignoreMouseYAxis = true;
        inputMapper.options.defaultActionWhenConflictFound = InputMapper.ConflictResponse.Replace; //might want this replaced if it sucks

        inputMapper.InputMappedEvent += OnInputMapped;

        inputMapper.Start(context);

        //=================================| End input mapper setup


        yield return new WaitUntil(GetInputMapperStatus);



        mappingRow.GetComponent<Image>().color = oldColor;
        ReInput.players.GetPlayer(0).controllers.maps.SetAllMapsEnabled(true);
        RefreshGamepadMappings();
    }



    void SetGamepadMapping(InputMappingRow row, ControllerType controllerType)
    {
        //show wait for key press UI
        Color oldColor = row.GetComponent<Image>().color;
        row.GetComponent<Image>().color = new Color(0f, 1f, 1f, 1f);
        #region might throw away
        //visit here for more map control : https://guavaman.com/projects/rewired/docs/api-reference/html/Methods_T_Rewired_Player_ControllerHelper_MapHelper.htm
        //player.controllers.maps.RemoveMap()

        //todo : do we want to remove action maps that use the same button?

        /*
        //remove maps that use that action
        List<ActionElementMap> currentMaps = new List<ActionElementMap>();
        player.controllers.maps.GetButtonMapsWithAction("", true, currentMaps);
        foreach (ActionElementMap map in currentMaps)
        {
            if (map.controllerMap.controllerType == ControllerType.Joystick)
            {
                Debug.Log(map.)
            }
        }


        //add new mapping
        
        var controllerMaps = player.controllers.maps.GetMaps(ControllerType.Joystick, player.controllers.Joysticks[0].id);
        foreach (ControllerMap map in controllerMaps)
        {
            Debug.Log(map.id);
            ElementAssignment assignment = new ElementAssignment();
            assignment.actionId = row
            map.CreateElementMap()
            //map.CreateElementMap()
            //map.ButtonMaps.Add()
        }*/
        #endregion

        var something = player.controllers.maps.GetMaps(ControllerType.Joystick, player.controllers.Joysticks[0].id)[0].ElementMapsWithAction(row.GetActionId());
        ActionElementMap mapToReplace = new ActionElementMap();
        int numMapsWithAction = 0;
        foreach (var thing in something)
        {
            mapToReplace = thing;
            numMapsWithAction++;
        }
        if (numMapsWithAction == 0)
        {
            Debug.LogError("Alert! No maps with the action of ID \"" + row.GetActionId() + "\" found!");
            return;
        }
        if (numMapsWithAction > 1)
        {
            Debug.LogError("Alert! Multiple maps with action ID \"" + row.GetActionId() + "\" found!");
            return;
        }

        //Setup for input mapper
        InputMapper.Context context = new InputMapper.Context()
        {
            actionId = row.GetActionId(),
            controllerMap = player.controllers.maps.GetMaps(ControllerType.Joystick, player.controllers.Joysticks[0].id)[0],
            actionRange = AxisRange.Positive,
            actionElementMapToReplace = mapToReplace
        };

        inputMapper = new InputMapper();

        inputMapper.options.timeout = 5f;
        inputMapper.options.ignoreMouseXAxis = true;
        inputMapper.options.ignoreMouseYAxis = true;
        inputMapper.options.defaultActionWhenConflictFound = InputMapper.ConflictResponse.Replace; //might want this replaced if it sucks

        inputMapper.InputMappedEvent += OnInputMapped;

        inputMapper.Start(context);

        //player.controllers.maps.AddMap(ControllerType.Joystick, 0, ma);            
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
}
