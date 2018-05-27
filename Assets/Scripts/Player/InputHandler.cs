using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
/** Input Handler Script 
 *  This script receives input from the keyboard, and sends that input to the PlayerHandler
 *  @author Mark Lipina
*/
public class InputHandler : MonoBehaviour
{

    [SerializeField] private GameObject playerHandlerObject;
    private PlayerHandler playerHandler;
    

    //-----------------------| Input Fields and Properties
    private Vector2 rightAnalog;
    public Vector2 RightAnalog
    {
        get { return rightAnalog; }
    }

    private Vector2 leftAnalog;
    public Vector2 LeftAnalog
    {
        get { return leftAnalog; }
    }
    private float _rightTrigger; //degree to which trigger is pressed
    public float RightTrigger
    {
        get { return _rightTrigger; }
    }
    //DPad
    private float _DPadNorth;
    private float _DPadSouth;
    private float _DPadEast;
    private float _DPadWest;

    public float DPadNorth
    {
        get { return _DPadNorth; }
    }
    public float DPadSouth
    {
        get { return _DPadSouth; }
    }
    public float DPadEast
    {
        get { return _DPadEast; }
    }
    public float DPadWest
    {
        get { return _DPadWest; }
    }

    //Bumpers
    private float _rightBumper;
    public float RightBumper
    {
        get { return _rightBumper; }
    }



    void Start()
    {
        playerHandler = playerHandlerObject.GetComponent<PlayerHandler>();
    }

    void Update()
    {
        //Get Input

        //Movement
        float x = Input.GetAxisRaw("Horizontal"); // GetAxis for smooth, GetAxisRaw for snappy
        float y = Input.GetAxisRaw("Vertical");
        leftAnalog = new Vector2(x, y);

        //DPad
        if (Input.GetAxisRaw("XBox One - DPad Y") > 0.2)
        {
            _DPadNorth = Input.GetAxisRaw("XBox One - DPad Y");
            _DPadSouth = 0;
        }
        else if (Input.GetAxisRaw("XBox One - DPad Y") < -0.2)
        {

            _DPadSouth = -Input.GetAxisRaw("XBox One - DPad Y");
            _DPadNorth = 0;
        }
        else
        {
            _DPadNorth = 0;
            _DPadSouth = 0;
        }

        if (Input.GetAxisRaw("XBox One - DPad X") > 0.2)
        {

            _DPadEast = Input.GetAxisRaw("XBox One - DPad X");
            _DPadWest = 0;
        }
        else if (Input.GetAxisRaw("XBox One - DPad X") < -0.2)
        {
            _DPadWest = -Input.GetAxisRaw("XBox One - DPad X");
            _DPadEast = 0;
        }
        else
        {
            _DPadWest = _DPadEast = 0;
        }

        //Aiming
        float rightX = Input.GetAxisRaw("XBox One - Right Analog X");
        float rightY = Input.GetAxisRaw("XBox One - Right Analog Y");
        rightAnalog = new Vector2(rightX, rightY);

        //Firing
        _rightTrigger = Input.GetAxisRaw("XBox One - Right Trigger");
        //if (_rightTrigger > 0.2) Debug.Log("Trigger press");
        //Debug.Log(_rightTrigger);

        //Grenade Lob
        _rightBumper = Input.GetAxisRaw("XBox One - Right Bumper");


        if (Input.GetKeyDown(KeyCode.Space) || Input.GetAxisRaw("XBox One - A Button") > 0)
        {
            playerHandler.SetJumpPressed(true);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetAxisRaw("XBox One - X Button") > 0)
        {
            playerHandler.SetAttackPressed(true);
        }

        //send input data (TODO - Make this something other classes access rather than this class sends)
        playerHandler.SetXYAnalogInput(x, y);
        if (Input.GetAxisRaw("XBox One - Menu Button") > 0 || Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
        }
       
    }
}
