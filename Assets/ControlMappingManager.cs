using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.UI;
using UnityEditor;

public struct InputMapperDbRow
{
    public InputAction action;
    public AxisRange range;
    public string descriptiveName;
    public InputMappingRow inputMappingRow;
}

/// <summary>
/// This is my Good Version of the remapping UI Manager. Cool, epic, radical, good. Just adjusted the fuck out
/// of it so that it uses my good UI system and not its bad UI system.
/// </summary>
public class ControlMappingManager : MonoBehaviour
{
    public const float MapperTimeout = 4f;
    private const string category = "Default";
    private const string layout = "Default";

    private InputMapper inputMapper = new InputMapper();

    private ControlMenuManager menuManager;

    //public GameObject buttonPrefab;
    //public GameObject textPrefab;
    //public RectTransform fieldGroupTransform;
    //public RectTransform actionMappingListTransform;
    //public Text controllerNameUIText;
    //public Text statusUIText;

    private ControllerType selectedControllerType = ControllerType.Keyboard;
    private int selectedControllerId = 0;
    public List<InputMapperDbRow> rows = new List<InputMapperDbRow>();

    private Player player { get { return ReInput.players.GetPlayer(0); } }
    private ControllerMap controllerMap
    {
        get
        {
            if (controller == null) return null;
            return player.controllers.maps.GetMap(controller.type, controller.id, category, layout);
        }
    }
    private Controller controller { get { return player.controllers.GetController(selectedControllerType, selectedControllerId); } }

    private void OnEnable()
    {
        if (!ReInput.isReady) return; // don't run if Rewired hasn't been initialized

        menuManager = GetComponent<ControlMenuManager>();

        // Ignore Mouse X and Y axes
        inputMapper.options.ignoreMouseXAxis = true;
        inputMapper.options.ignoreMouseYAxis = true;
        inputMapper.options.timeout = MapperTimeout;
        inputMapper.options.allowKeyboardKeysWithModifiers = false;
        inputMapper.options.allowKeyboardModifierKeyAsPrimary = true;
        inputMapper.options.allowAxes = true;

        // Subscribe to events
        ReInput.ControllerConnectedEvent += OnControllerChanged;
        ReInput.ControllerDisconnectedEvent += OnControllerChanged;
        inputMapper.InputMappedEvent += OnInputMapped;
        inputMapper.StoppedEvent += OnStopped;
        inputMapper.options.isElementAllowedCallback = CheckIsConflictAllowed;

        // Create UI elements
        menuManager.InitializeMappings();
        InitializeUI();
    }

    private void OnDisable()
    {
        // Make sure the input mapper is stopped first
        inputMapper.Stop();

        // Unsubscribe from events
        inputMapper.RemoveAllEventListeners();
        ReInput.ControllerConnectedEvent -= OnControllerChanged;
        ReInput.ControllerDisconnectedEvent -= OnControllerChanged;
    }
    /*
    private void RedrawUI()
    {
        //old redraw shit
        
        if (controller == null)
        { // no controller is selected
            ClearUI();
            return;
        }

        // Update joystick name in UI
        //controllerNameUIText.text = controller.name;
        
        // Update each button label with the currently mapped element identifier
        for (int i = 0; i < rows.Count; i++)
        {
            InputMapperDbRow row = rows[i];
            InputAction action = rows[i].action;

            string name = string.Empty;
            int actionElementMapId = -1;

            // Find the first ActionElementMap that maps to this Action and is compatible with this field type
            foreach (var actionElementMap in controllerMap.ElementMapsWithAction(action.id))
            {
                if (actionElementMap.ShowInField(row.range))
                {
                    name = actionElementMap.elementIdentifierName;
                    actionElementMapId = actionElementMap.id;
                    break;
                }
            }

            // Set the label in the field button
            row.text.text = name;

            // Set the field button callback
            row.button.onClick.RemoveAllListeners(); // clear the button event listeners first
            int index = i; // copy variable for closure
            row.button.onClick.AddListener(() => OnInputFieldClicked(index, actionElementMapId));
        }
        

        //Update each row based on current mapping (done in ControlMenuManager)
        //menuManager.UpdateUI();
    }*/

    /*
    private void ClearUI()
    {

        // Clear the controller name
        if (selectedControllerType == ControllerType.Joystick) controllerNameUIText.text = "No joysticks attached";
        else controllerNameUIText.text = string.Empty;

        // Clear button labels
        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].text.text = string.Empty;
        }
    }*/

    
    private void InitializeUI()
    {
        // Create Action fields and input field buttons
        foreach (var action in ReInput.mapping.Actions)
        {
            if (!action.userAssignable) continue;

            if (action.type == InputActionType.Axis)
            {
                // Create a full range, one positive, and one negative field for Axis-type Actions
                //CreateUIRow(action, AxisRange.Full, action.descriptiveName); // I dont want "full range" ones
                CreateDbRow(action, AxisRange.Positive, !string.IsNullOrEmpty(action.positiveDescriptiveName) ? action.positiveDescriptiveName : action.descriptiveName + " +");
                CreateDbRow(action, AxisRange.Negative, !string.IsNullOrEmpty(action.negativeDescriptiveName) ? action.negativeDescriptiveName : action.descriptiveName + " -");
            }
            else if (action.type == InputActionType.Button)
            {
                // Just create one positive field for Button-type Actions
                CreateDbRow(action, AxisRange.Positive, action.descriptiveName);
            }
        }
        //initialize each row
        for (int i = 0; i < rows.Count; i++)
        {
            InputMapperDbRow row = rows[i];
            InputAction action = rows[i].action;

            string name = string.Empty;
            /*
            int actionElementMapId = -1;

            // Find the first ActionElementMap that maps to this Action and is compatible with this field type
            
            foreach (var actionElementMap in controllerMap.ElementMapsWithAction(action.id))
            {
                if (actionElementMap.ShowInField(row.range))
                {
                    name = actionElementMap.elementIdentifierName;
                    actionElementMapId = actionElementMap.id;
                    break;
                }
            }*/

            // Set the field button callback
            row.inputMappingRow.GetGamepadButton().onClick.RemoveAllListeners();
            row.inputMappingRow.GetKBButton().onClick.RemoveAllListeners();
            row.inputMappingRow.GetMouseButton().onClick.RemoveAllListeners();

            int index = i; // copy variable for closure

            // setup keyboard button
            bool keyboardLinked = false;
            selectedControllerType = ControllerType.Keyboard;
            foreach (var actionElementMap in controllerMap.ElementMapsWithAction(action.id))
            {
                if (actionElementMap.ShowInField(row.range))
                {
                    if (keyboardLinked) Debug.LogError("Duplicate mapping! This probably shouldnt happen???");
                    name = actionElementMap.elementIdentifierName;
                    row.inputMappingRow.GetKBButton().onClick.AddListener(() => OnInputFieldClicked(index, actionElementMap.id, ControllerType.Keyboard));
                    keyboardLinked = true;
                    break;
                }
            }
            if (!keyboardLinked)
            {
                row.inputMappingRow.GetKBButton().onClick.AddListener(() => OnInputFieldClicked(index, -1, ControllerType.Keyboard));
            }

            // setup mouse button
            bool mouseLinked = false;
            selectedControllerType = ControllerType.Mouse;
            foreach (var actionElementMap in controllerMap.ElementMapsWithAction(action.id))
            {
                if (actionElementMap.ShowInField(row.range))
                {
                    mouseLinked = true;
                    name = actionElementMap.elementIdentifierName;
                    row.inputMappingRow.GetMouseButton().onClick.AddListener(() => OnInputFieldClicked(index, actionElementMap.id, ControllerType.Mouse));
                    break;
                }
            }
            if (!mouseLinked)
            {
                row.inputMappingRow.GetMouseButton().onClick.AddListener(() => OnInputFieldClicked(index, -1, ControllerType.Mouse));
            }

            // setup keyboard button
            bool gamepadLinked = false;
            selectedControllerType = ControllerType.Joystick;
            foreach (var actionElementMap in controllerMap.ElementMapsWithAction(action.id))
            {
                if (actionElementMap.ShowInField(row.range))
                {
                    gamepadLinked = true;
                    name = actionElementMap.elementIdentifierName;
                    row.inputMappingRow.GetGamepadButton().onClick.AddListener(() => OnInputFieldClicked(index, actionElementMap.id, ControllerType.Joystick));
                    break;
                }
            }
            if (!gamepadLinked)
            {
                row.inputMappingRow.GetGamepadButton().onClick.AddListener(() => OnInputFieldClicked(index, -1, ControllerType.Joystick));
            }

            // Set the label in the field button
            //row.text.text = name;
        }

        menuManager.UpdateUI();
    }

    /// <summary>
    /// Creates a row in the list that exposes information about the action mapping
    /// </summary>
    private void CreateDbRow(InputAction action, AxisRange actionRange, string label)
    {
        var newRow = new InputMapperDbRow();
        newRow.action = action;
        newRow.range = actionRange;
        newRow.descriptiveName = label;

        foreach (InputMappingRow mappingRow in menuManager.MappingRows)
        {
            if (mappingRow.GetMappingName() == label)
            {
                newRow.inputMappingRow = mappingRow;
                rows.Add(newRow);
            }
        }
        //Debug.LogError("Shit! Failed to create DB Row - cant find mapping row in ControlMenuManager for Mapping Name : " + label);
    }

    /*
    private void CreateUIRow(InputAction action, AxisRange actionRange, string label)
    {
        // Create the Action label
        GameObject labelGo = Object.Instantiate<GameObject>(textPrefab);
        labelGo.transform.SetParent(actionGroupTransform);
        labelGo.transform.SetAsLastSibling();
        labelGo.GetComponent<Text>().text = label;

        // Create the input field button
        GameObject buttonGo = Object.Instantiate<GameObject>(buttonPrefab);
        buttonGo.transform.SetParent(fieldGroupTransform);
        buttonGo.transform.SetAsLastSibling();

        // Add the row to the rows list
        rows.Add(
            new Row()
            {
                action = action,
                actionRange = actionRange,
                button = buttonGo.GetComponent<Button>(),
                text = buttonGo.GetComponentInChildren<Text>()
            }
        );
    }
    */
    private void SetSelectedController(ControllerType controllerType)
    {
        bool changed = false;

        // Check if the controller type changed
        if (controllerType != selectedControllerType)
        { // controller type changed
            selectedControllerType = controllerType;
            changed = true;
        }

        // Check if the controller id changed
        int origId = selectedControllerId;
        if (selectedControllerType == ControllerType.Joystick)
        {
            if (player.controllers.joystickCount > 0) selectedControllerId = player.controllers.Joysticks[0].id;
            else selectedControllerId = -1;
        }
        else
        {
            selectedControllerId = 0;
        }
        if (selectedControllerId != origId) changed = true;

        // If the controller changed, stop the input mapper and update the UI
        if (changed)
        {
            inputMapper.Stop();
            menuManager.UpdateUI();
        }
    }

    // Event Handlers

    // Called by the controller UI Buttons when pressed
    public void OnControllerSelected(int controllerType)
    {
        SetSelectedController((ControllerType)controllerType);
    }

    // Called by the input field UI Button when pressed
    private void OnInputFieldClicked(int index, int actionElementMapToReplaceId, ControllerType buttonControllerType)
    {
        SetSelectedController(buttonControllerType);

        if (index < 0 || index >= rows.Count) return; // index out of range
        if (controller == null) return; // there is no Controller selected


        var aemtrid = controllerMap.GetElementMap(actionElementMapToReplaceId);

        if (aemtrid == null)
        {
            Debug.Log("Whooop");
        }

        // Begin listening for input
        inputMapper.Start(
            new InputMapper.Context()
            {
                actionId = rows[index].action.id,
                controllerMap = controllerMap,
                actionRange = rows[index].range,
                actionElementMapToReplace = controllerMap.GetElementMap(actionElementMapToReplaceId),
            }
        );
        menuManager.StartWaitingForInputUI(buttonControllerType);
    }

    private void OnControllerChanged(ControllerStatusChangedEventArgs args)
    {
        SetSelectedController(selectedControllerType);
    }

    private void OnInputMapped(InputMapper.InputMappedEventData data)
    {
        menuManager.UpdateUI();
    }

    private void OnStopped(InputMapper.StoppedEventData data)
    {
        menuManager.StopWaitingForInputUI();
    }

    private bool CheckIsConflictAllowed(ControllerPollingInfo data)
    {
        if (data.elementIdentifierName == "ESC")
        {
            return false;
        }
        if (data.controllerType == ControllerType.Joystick)
        {
            var map = player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Joystick, "Pause", false);
            if (map.elementIdentifierId == data.elementIdentifierId)
            {
                return false;
            }
        }
        return true;
    }

    // A small class to store information about the input field buttons
    private class Row
    {
        public InputAction action;
        public AxisRange actionRange;
        public Button button;
        public Text text;
    }
}