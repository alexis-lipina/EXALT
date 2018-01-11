using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    [SerializeField] private GameObject playerCharacterSprite;
    [SerializeField] private GameObject playerPhysicsObject;
    private PlayerColliderScript PlayerCollider;

    private Rigidbody2D playerRigidBody;



    enum PlayerState {IDLE, RUN, JUMP};

    private PlayerState CurrentState;
    private bool UpPressed;
    private bool DownPressed;
    private bool LeftPressed;
    private bool RightPressed;
    private bool JumpPressed;

    private float PlayerElevation;
    private float PlayerHeight;
    private float PlayerRunSpeed;
    private float xInput; 
    private float yInput;   
    private int JumpImpulse;
    private float ZVelocity;

    Dictionary<int, float> TerrainTouched;



	void Start ()
    {
        CurrentState = PlayerState.IDLE;
        PlayerElevation = 0;
        PlayerHeight = 10; //----ARBITRARY VALUE - CHANGE LATER WHEN WORKING ON HITBOXES AND COLLISIONS
        JumpImpulse = 1;
        playerRigidBody = playerPhysicsObject.GetComponent<Rigidbody2D>();
        TerrainTouched = new Dictionary<int, float>();
        TerrainTouched.Add(666, 0);
        PlayerCollider = playerPhysicsObject.GetComponent<PlayerColliderScript>();
       

    }


    void Update ()
    {
        //---------------------------| Manage State Machine |
        switch (CurrentState)
        {
            case (PlayerState.IDLE):
                playerIdle();
                break;
            case (PlayerState.RUN):
                playerRun();
                break;
            case (PlayerState.JUMP):
                playerJump();
                break;
        }
        
        //updateHeight();
        moveCharacterPosition();
        //reset button presses
        if (JumpPressed) JumpPressed = false;
    }

    //================================================================================| STATE METHODS |

    private void playerIdle()
    {
        //do nothing, maybe later have them breathing or getting bored, sitting down

        //------------------------------------------------| STATE CHANGE
        if (Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2) 
        {
            //Debug.Log("IDLE -> RUN");
            CurrentState = PlayerState.RUN;
        }
        if (JumpPressed)
        {
            //Debug.Log("IDLE -> JUMP");
            ZVelocity = JumpImpulse;
            JumpPressed = false;
            CurrentState = PlayerState.JUMP;
        }
    }

    private void playerRun()
    {
        //Debug.Log("Player Running");
        //------------------------------------------------| MOVE
        moveCharacterPositionPhysics();

        //-------| Z Azis Traversal 
        float maxheight = 0;
        foreach(KeyValuePair<int, float> entry in TerrainTouched)
        {
            if (entry.Value > maxheight) maxheight = entry.Value;
        }
        PlayerElevation = maxheight;
        //------------------------------------------------| STATE CHANGE
        //Debug.Log("X:" + xInput + "Y:" + yInput);
        if (Mathf.Abs(xInput) < 0.1 && Mathf.Abs(yInput) < 0.1)
        {
            //Debug.Log("RUN -> IDLE");
            CurrentState = PlayerState.IDLE;
        }
        if (JumpPressed)
        {
            //Debug.Log("RUN -> JUMP");
            ZVelocity = JumpImpulse;
            JumpPressed = false;
            CurrentState = PlayerState.JUMP;
        }
    }

    private void playerJump()
    {
        //Debug.Log("Player Jumping");
        //------------------------------| MOVE
        moveCharacterPositionPhysics();
        PlayerElevation += ZVelocity;
        ZVelocity -= 0.05f;

        //------------------------------| STATE CHANGE
        float maxheight = 0;
        foreach (KeyValuePair<int, float> entry in TerrainTouched)
        {
            if (entry.Value > maxheight) maxheight = entry.Value;
        }

        if (PlayerElevation <= maxheight)
        {
            PlayerElevation = maxheight;
            if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
            {
                //Debug.Log("JUMP -> IDLE");
                CurrentState = PlayerState.IDLE;
            }
            else
            {
                //Debug.Log("JUMP -> RUN");
                CurrentState = PlayerState.RUN;
            }
        }
    }

    //=============| Update Height Method - legacy method that makes more sense to be a part of each player state
    /*
    private void updateHeight()
    {
        float maxTerrainHeight = 0;
        foreach (KeyValuePair<int, float> entry in TerrainTouched)
        {
            if (entry.Value > maxTerrainHeight)
            {
                maxTerrainHeight = entry.Value;
            }
        }
        PlayerElevation = maxTerrainHeight;
    }
    */

    /// <summary>
    /// 
    /// </summary>
    private void moveCharacterPositionPhysics()
    {
        PlayerCollider.MoveWithCollision(xInput * 0.3f, yInput * 0.3f);
        //playerRigidBody.MovePosition(new Vector2(playerRigidBody.position.x + xInput * 0.3f, playerRigidBody.position.y + yInput * 0.3f));
    }

    /// <summary>
    /// Changes position of character image as player moves. 
    /// </summary>
    private void moveCharacterPosition()
    {
        //                           X: Horizontal position                    Y: Vertical position - accounts for height and depth               Z: Depth - order of object draw calls
        Vector3 coords = new Vector3(playerPhysicsObject.transform.position.x, playerPhysicsObject.transform.position.y + 1.4f + PlayerElevation, playerPhysicsObject.transform.position.z);
        playerCharacterSprite.transform.position = coords;
    }

    //================================================================================| SETTERS FOR INPUT |
    public void setUpPressed(bool isPressed)
    {
        UpPressed = isPressed;
    }
    public void setDownPressed(bool isPressed)
    {
        DownPressed = isPressed;
    }
    public void setLeftPressed(bool isPressed)
    {
        LeftPressed = isPressed;
    }
    public void setRightPressed(bool isPressed)
    {
        RightPressed = isPressed;
    }
    public void setJumpPressed(bool isPressed)
    {
        JumpPressed = isPressed;
    }

    public float getPlayerElevation()
    {
        return PlayerElevation;
    }



    public void setXYAnalogInput(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    public void addTerrainTouched(int terrainInstanceID, float height)
    {
        if (TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched already contains ID " + terrainInstanceID);
        }
        TerrainTouched.Add(terrainInstanceID, height);
    }
    public void removeTerrainTouched(int terrainInstanceID)
    {
        if ( !TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched does not contain ID " + terrainInstanceID);
        }
        TerrainTouched.Remove(terrainInstanceID);
    }
}
