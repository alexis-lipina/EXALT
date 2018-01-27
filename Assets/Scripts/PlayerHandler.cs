using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// This class controls player state and contains methods for each state. It also receives input from the InputHandler and acts in accordance with said input.
/// In addition, it handles sprites, shadows, and player height
/// </summary>
public class PlayerHandler : MonoBehaviour
{
    [SerializeField] private GameObject playerCharacterSprite;
    [SerializeField] private GameObject playerPhysicsObject;
    [SerializeField] private GameObject playerEnvironmentHandlerObject;
    [SerializeField] private GameObject FirstShadow;
    [SerializeField] private float playerSpriteZOffset;
    [SerializeField] private GameObject FollowingCamera;

    private EntityColliderScript PlayerCollider;
    private Rigidbody2D playerRigidBody;
    private Animator characterAnimator;


    enum PlayerState {IDLE, RUN, JUMP, LIGHT_STAB, HEAVY_STAB, LIGHT_SWING, HEAVY_SWING};

    const string IDLE_Anim = "Anim_CharacterTest1";
    const string RUN_Anim = "Anim_CharacterTest2";
    const string JUMP_Anim = "Anim_CharacterTest3";
    const string LIGHT_STAB_Anim = "Anim_CharacterTest4";



    private PlayerState CurrentState;
    private PlayerState PreviousState;
    private bool UpPressed;
    private bool DownPressed;
    private bool LeftPressed;
    private bool RightPressed;
    private bool JumpPressed;
    private bool AttackPressed;

    private float PlayerElevation;
    private float PlayerHeight;
    private float PlayerRunSpeed;
    private float xInput; 
    private float yInput;   
    private float JumpImpulse;
    private float ZVelocity;

    Dictionary<int, KeyValuePair<float, float>> TerrainTouched;
    //         ^ instanceID       ^bottom   ^ topheight
    Dictionary<int, KeyValuePair<float, GameObject>> Shadows;
    //          ^ instanceID       ^ height    ^ shadowobject 


	void Start ()
    {
        
        CurrentState = PlayerState.IDLE;
        PlayerElevation = 0;
        PlayerHeight = 3; 
        JumpImpulse = 0.6f;
        playerRigidBody = playerPhysicsObject.GetComponent<Rigidbody2D>();
        TerrainTouched = new Dictionary<int, KeyValuePair<float, float>>();
        TerrainTouched.Add(666, new KeyValuePair<float, float>(0.0f, -20.0f));
        PlayerCollider = playerPhysicsObject.GetComponent<EntityColliderScript>();
        Shadows = new Dictionary<int, KeyValuePair<float, GameObject>>();
        Shadows.Add(FirstShadow.GetInstanceID(), new KeyValuePair<float, GameObject>(0.0f, FirstShadow));
        characterAnimator = playerCharacterSprite.GetComponent<Animator>();

    }


    void Update ()
    {
        //---------------------------| Manage State Machine |
        switch (CurrentState)
        {
            case (PlayerState.IDLE):
                characterAnimator.Play(IDLE_Anim);
                PlayerIdle();
                break;
            case (PlayerState.RUN):
                characterAnimator.Play(RUN_Anim);
                PlayerRun();
                break;
            case (PlayerState.JUMP):
                characterAnimator.Play(JUMP_Anim);
                PlayerJump();
                break;
            case (PlayerState.LIGHT_STAB):
                PlayerLightStab();
                break;
            case (PlayerState.HEAVY_STAB):
                PlayerHeavyStab();
                break;
            case (PlayerState.LIGHT_SWING):
                PlayerLightSwing();
                break;
            case (PlayerState.HEAVY_SWING):
                PlayerHeavySwing();
                break;
        }
        
        //updateHeight();
        moveCharacterPosition();
        //reset button presses
        JumpPressed = false;
        AttackPressed = false;
        PreviousState = CurrentState;
        //FollowingCamera.transform.position = new Vector3(playerCharacterSprite.transform.position.x, playerCharacterSprite.transform.position.y, -100);
    }

    //================================================================================| STATE METHODS |

    private void PlayerIdle()
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
        if (AttackPressed)
        {
            Debug.Log("IDLE -> ATTACK");
            CurrentState = PlayerState.LIGHT_STAB;
        }
    }

    private void PlayerRun()
    {
        //Debug.Log("Player Running");
        //------------------------------------------------| MOVE
        moveCharacterPositionPhysics();

        //-------| Z Azis Traversal 
        float maxheight = 0;
        foreach(KeyValuePair<int, KeyValuePair<float, float>> entry in TerrainTouched)
        {
            if (entry.Value.Value > maxheight && PlayerHeight + PlayerElevation > entry.Value.Value) maxheight = entry.Value.Value;
        }
        if (PlayerElevation > maxheight)
        {
            ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            PlayerElevation = maxheight;
        }
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

    private void PlayerJump()
    {
        //Debug.Log("Player Jumping");
        //------------------------------| MOVE
        
        moveCharacterPositionPhysics();
        
        PlayerElevation += ZVelocity;
        
        ZVelocity -= 0.03f;

        //------------------------------| STATE CHANGE
        float maxheight = 0;
        foreach (KeyValuePair<int, KeyValuePair<float, float>> entry in TerrainTouched)
        {
            if (entry.Value.Value > maxheight && PlayerHeight + PlayerElevation > entry.Value.Value) maxheight = entry.Value.Value; // landing on ground
            if (entry.Value.Key < PlayerHeight + PlayerElevation && PlayerElevation < entry.Value.Key && ZVelocity > 0) ZVelocity = 0; // hit head on ceiling
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

    private void PlayerLightStab()
    {
        //todo - test area for collision, if coll
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(new Vector2(PlayerCollider.transform.position.x + 2, PlayerCollider.transform.position.y), new Vector2(4, 4), 0);
        foreach(Collider2D hit in hitobjects)
        {
            if (hit.tag == "Enemy")
            {
                hit.gameObject.GetComponent<HealthManager>().Inflict(1.0f);
            }
        }
        CurrentState = PlayerState.RUN;
    }
    private void PlayerHeavyStab()
    {
        //todo
    }
    private void PlayerLightSwing()
    {
        //todo
    }
    private void PlayerHeavySwing()
    {
        //todo
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
        PlayerCollider.MoveWithCollision(xInput * 15f * Time.deltaTime, yInput * 15f * Time.deltaTime);
        //playerRigidBody.MovePosition(new Vector2(playerRigidBody.position.x + xInput * 0.3f, playerRigidBody.position.y + yInput * 0.3f));
    }

    /// <summary>
    /// Changes position of character image as player moves. 
    /// </summary>
    private void moveCharacterPosition()
    {
        //                           X: Horizontal position                    Y: Vertical position - accounts for height and depth               Z: Depth - order of object draw calls
        Vector3 coords = new Vector3(playerPhysicsObject.transform.position.x, playerPhysicsObject.transform.position.y + playerSpriteZOffset + PlayerElevation, playerPhysicsObject.transform.position.y + playerEnvironmentHandlerObject.GetComponent<BoxCollider2D>().offset.y - playerEnvironmentHandlerObject.GetComponent<BoxCollider2D>().size.y / 2 + 0.4f);
        playerCharacterSprite.transform.position = coords;
        //playerCharacterSprite.transform.position = new Vector3(playerCharacterSprite.transform.position.x, playerCharacterSprite.transform.position.y, playerPhysicsObject.transform.position.y + playerPhysicsObject.GetComponent<BoxCollider2D>().offset.y + playerPhysicsObject.GetComponent<BoxCollider2D>().size.y / 2);
        //Vector2 tempvect = new Vector2(xInput, yInput);
        


        //move shadows
        foreach (KeyValuePair<int, KeyValuePair<float, GameObject>> entry in Shadows)
        {
            entry.Value.Value.transform.position = new Vector3(playerPhysicsObject.transform.position.x, playerPhysicsObject.transform.position.y + entry.Value.Key, playerPhysicsObject.transform.position.y + playerEnvironmentHandlerObject.GetComponent<BoxCollider2D>().offset.y - playerEnvironmentHandlerObject.GetComponent<BoxCollider2D>().size.y / 2 + 0.4f);
        }
    }
    //================================================================================| ANIMATOR CONTROLLER HANDLING | 


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
    public void setAttackPressed(bool isPressed)
    {
        AttackPressed = isPressed;
    }

    public float getPlayerElevation()
    {
        return PlayerElevation;
    }
    public float getPlayerHeight()
    {
        return PlayerHeight;
    }



    public void setXYAnalogInput(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    public void addTerrainTouched(int terrainInstanceID, float bottomHeight, float topHeight)
    {
        if (TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched already contains ID " + terrainInstanceID);
        }
        else
        {
            TerrainTouched.Add(terrainInstanceID, new KeyValuePair<float, float>(bottomHeight, topHeight));
            Shadows.Add(terrainInstanceID, new KeyValuePair<float, GameObject>(topHeight, Instantiate(FirstShadow, this.transform.parent)));
            Shadows[terrainInstanceID].Value.transform.position = new Vector3(playerPhysicsObject.transform.position.x, playerPhysicsObject.transform.position.y + topHeight, topHeight);
        }
    }
    public void removeTerrainTouched(int terrainInstanceID)
    {
        if ( !TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched does not contain ID " + terrainInstanceID);
        }
        TerrainTouched.Remove(terrainInstanceID);
        Destroy(Shadows[terrainInstanceID].Value);
        Shadows.Remove(terrainInstanceID);
    }
}
