using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// This class controls player state and contains methods for each state. It also receives input from the InputHandler and acts in accordance with said input.
/// In addition, it handles sprites, shadows, and player height
/// </summary>
public class PlayerHandler : EntityHandler
{
    
    [SerializeField] private GameObject characterSprite;
    [SerializeField] private GameObject FollowingCamera;
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

    
    private float PlayerRunSpeed;
    private float xInput; 
    private float yInput;   
    private float JumpImpulse;

    

	void Start ()
    {
        
        CurrentState = PlayerState.IDLE;
        
        JumpImpulse = 0.6f;
        //playerRigidBody = PhysicsObject.GetComponent<Rigidbody2D>();
        
        //TerrainTouched.Add(666, new KeyValuePair<float, float>(0.0f, -20.0f));
        //Shadows.Add(FirstShadow.GetInstanceID(), new KeyValuePair<float, GameObject>(0.0f, FirstShadow));
        characterAnimator = characterSprite.GetComponent<Animator>();


}


    void Update ()
    {
        //---------------------------| Manage State Machine |
        this.ExecuteState();
        //updateHeight();
        //moveCharacterPosition();
        //reset button presses
        JumpPressed = false;
        AttackPressed = false;
        PreviousState = CurrentState;
        //FollowingCamera.transform.position = new Vector3(playerCharacterSprite.transform.position.x, playerCharacterSprite.transform.position.y, -100);
    }

    protected override void ExecuteState()
    {
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
    }

    //================================================================================| STATE METHODS |

    private void PlayerIdle()
    {
        //do nothing, maybe later have them breathing or getting bored, sitting down
        //Debug.Log("Player Idle");
        //------------------------------------------------| STATE CHANGE
        if (Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2) 
        {
            //Debug.Log("IDLE -> RUN");
            CurrentState = PlayerState.RUN;
        }
        if (JumpPressed)
        {
            //Debug.Log("IDLE -> JUMP");
            EntityPhysics.ZVelocity = JumpImpulse;
            JumpPressed = false;
            CurrentState = PlayerState.JUMP;
        }

        if (AttackPressed)
        {
            Debug.Log("IDLE -> ATTACK");
            CurrentState = PlayerState.LIGHT_STAB;
        }

        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        if (EntityPhysics.GetEntityElevation() > maxheight)
        {
            EntityPhysics.ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            EntityPhysics.SetEntityElevation(maxheight);
        }
        
    }

    private void PlayerRun()
    {
        //Debug.Log("Player Running");
        //------------------------------------------------| MOVE
        EntityPhysics.MoveCharacterPositionPhysics(xInput, yInput);

        //-------| Z Azis Traversal 
        // handles falling if player is above ground
        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        if (EntityPhysics.GetEntityElevation() > maxheight)
        {
            EntityPhysics.ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            EntityPhysics.SetEntityElevation(maxheight);
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
            EnvironmentPhysics temp = null;
            //save position and terrain standing on
            /*
            foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
            {
                if (entry.Value.getTopHeight() == PlayerElevation)
                {
                    temp = entry.Value;
                }
            }
            if (temp != null)
            {
                lasttouched = new KeyValuePair<Vector2, EnvironmentPhysics>(EntityPhysics.GetComponent<Rigidbody2D>().position, temp);
            }
            */
            EntityPhysics.SavePosition();
            //Debug.Log("RUN -> JUMP");
            EntityPhysics.ZVelocity = JumpImpulse;
            JumpPressed = false;
            CurrentState = PlayerState.JUMP;
        }
        if (CurrentState == PlayerState.RUN)
        {
            EntityPhysics.SavePosition();
        }
    }

    private void PlayerJump()
    {
        //Debug.Log("Player Jumping");
        //------------------------------| MOVE
        
        EntityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        EntityPhysics.FreeFall();
        /*
        EntityPhysics.SetEntityElevation(EntityPhysics.GetEntityElevation() + EntityPhysics.ZVelocity);
        
        EntityPhysics.ZVelocity -= 0.03f;
        */
        //------------------------------| STATE CHANGE

        //Check for foot collision

        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        //EntityPhysics.CheckHitHeadOnCeiling();
        if (EntityPhysics.TestFeetCollision())


        if (EntityPhysics.GetEntityElevation() <= maxheight)
        {
            EntityPhysics.SetEntityElevation(maxheight);
            if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
            {
                EntityPhysics.SavePosition();
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
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(new Vector2(EntityPhysics.transform.position.x + 2, EntityPhysics.transform.position.y), new Vector2(4, 4), 0);
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



    public override void setXYAnalogInput(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    
    
   

    

    
}