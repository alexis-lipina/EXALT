using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/** Input Handler Script 
 *  This script receives input from the keyboard, and sends that input to the PlayerHandler
 *  @author Mark Lipina
*/

public class InputHandler : MonoBehaviour
{

    [SerializeField] private GameObject playerHandlerObject;
    private PlayerHandler playerHandler;


    void Start()
    {
        playerHandler = playerHandlerObject.GetComponent<PlayerHandler>();
    }

    void Update()
    {
        //Get Input
        float x = Input.GetAxisRaw("Horizontal"); // GetAxis for smooth, GetAxisRaw for snappy
        float y = Input.GetAxisRaw("Vertical");
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetAxisRaw("XBox One - A Button") > 0)
        {
            playerHandler.setJumpPressed(true);
        }
        //send input data
        playerHandler.setXYAnalogInput(x, y);
       
    }
}
