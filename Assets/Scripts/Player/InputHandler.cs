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

    private Vector2 rightAnalog;
    public Vector2 RightAnalog
    {
        get { return rightAnalog; }
    }
    

    void Start()
    {
        playerHandler = playerHandlerObject.GetComponent<PlayerHandler>();
    }

    void Update()
    {
        //Get Input
        float x = Input.GetAxisRaw("Horizontal"); // GetAxis for smooth, GetAxisRaw for snappy
        float y = Input.GetAxisRaw("Vertical");

        float rightX = Input.GetAxisRaw("XBox One - Right Analog X");
        float rightY = Input.GetAxisRaw("XBox One - Right Analog Y");
        rightAnalog = new Vector2(rightX, rightY);
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetAxisRaw("XBox One - A Button") > 0)
        {
            playerHandler.SetJumpPressed(true);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetAxisRaw("XBox One - X Button") > 0)
        {
            playerHandler.SetAttackPressed(true);
        }

        //send input data
        playerHandler.SetXYAnalogInput(x, y);
        if (Input.GetAxisRaw("XBox One - Menu Button") > 0 || Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
       
    }
}
