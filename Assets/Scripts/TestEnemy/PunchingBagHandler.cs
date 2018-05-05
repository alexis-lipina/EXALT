using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingBagHandler : EntityHandler
{

    enum PunchingBagState { IDLE, RUN, FALL, JUMP };
    private PunchingBagState currentState;
    [SerializeField] private bool isCompanion;


    [SerializeField] private Animator spriteAnimator;
    const string IDLE_EAST_Anim = "Anim_GoldieIdleEast";
    const string IDLE_WEST_Anim = "Anim_GoldieIdleWest";
    const string RUN_EAST_Anim = "Anim_GoldieJogEast";
    const string RUN_WEST_Anim = "Anim_GoldieJogWest";
    

    float xInput;
    float yInput;
    bool jumpPressed;
    bool _tempFacingEast;
    float cooldowntimer;
    bool wasJustHit;

   

    void Start()
    {
        xInput = 0;
        yInput = 0;
        currentState = PunchingBagState.IDLE;
        jumpPressed = false;
        wasJustHit = false;
        _tempFacingEast = true;
    }

    void Update()
    {
        ExecuteState();
        wasJustHit = false;
    }

    public override void SetXYAnalogInput(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    protected override void ExecuteState()
    {
        switch (currentState)
        {
            case (PunchingBagState.IDLE):
                IdleState();
                break;
            case (PunchingBagState.RUN):
                RunState();
                break;
            case (PunchingBagState.FALL):
                FallState();
                break;
            case (PunchingBagState.JUMP):
                JumpState();
                break;
        }
    }

    //==============================| State Methods |

    private void IdleState()
    {
        //Draw
        //Draw
        if (_tempFacingEast)
        {
            spriteAnimator.Play(IDLE_EAST_Anim);
        }
        else
        {
            spriteAnimator.Play(IDLE_WEST_Anim);
        }
        //===========| State Switching
        //Debug.Log("IDLE!!!");
        entityPhysics.MoveCharacterPositionPhysics(0, 0);
        if (Mathf.Abs(xInput) > 0.1 || Mathf.Abs(yInput) > 0.1)
        {
            currentState = PunchingBagState.RUN;
        }
        if (wasJustHit)
        {
            cooldowntimer = 1;
            //currentState = PunchingBagState.WOUNDED;
        }
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetEntityElevation() > maxheight)
        {
            entityPhysics.ZVelocity = 0;
            currentState = PunchingBagState.FALL;
            //Debug.Log("Falling");
        }
        else
        {
            entityPhysics.SetEntityElevation(maxheight);
        }
    }

    private void RunState()
    {
        //Draw
        if (xInput > 0)
        {
            spriteAnimator.Play(RUN_EAST_Anim);
            _tempFacingEast = true;
        }
        else if (xInput < 0)
        {
            spriteAnimator.Play(RUN_WEST_Anim);
            _tempFacingEast = false;
        }
        else
        {
            if (_tempFacingEast)
            {
                spriteAnimator.Play(IDLE_EAST_Anim);
            }
            else
            {
                spriteAnimator.Play(IDLE_WEST_Anim);
            }
        }
        //Debug.Log("Running!!! : " + entityPhysics.GetBottomHeight());
        entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);

        //===========| State Switching

        if (Mathf.Abs(xInput) < 0.1 && Mathf.Abs(yInput) < 0.1)
        {
            entityPhysics.SavePosition();
            currentState = PunchingBagState.IDLE;
        }
        
        if (jumpPressed)
        {
            entityPhysics.ZVelocity = 0.5f;
            currentState = PunchingBagState.JUMP;
        }
        if (wasJustHit)
        {
            cooldowntimer = 1;
            //currentState = PunchingBagState.WOUNDED;
        }
        //fall
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetEntityElevation() > maxheight)
        {
            entityPhysics.ZVelocity = 0;
            currentState = PunchingBagState.FALL;
        }
        else
        {
            entityPhysics.SavePosition();
            entityPhysics.SetEntityElevation(maxheight);
        }
    }
    private void FallState()
    {
        //Debug.Log("Falling: " + entityPhysics.GetBottomHeight());
        entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        entityPhysics.FreeFall();


        //===========| State Switching

        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetEntityElevation() <= maxheight)
        {
            entityPhysics.SetEntityElevation(maxheight);
            if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
            {
                //entityPhysics.SavePosition();
                //Debug.Log("JUMP -> IDLE");
                currentState = PunchingBagState.IDLE;
            }
            else
            {
                //Debug.Log("JUMP -> RUN");
                currentState = PunchingBagState.RUN;
            }
        }

    }

    private void JumpState()
    {
        //Debug.Log("JUMPING!!!");
        entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        entityPhysics.FreeFall();
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        //entityPhysics.CheckHitHeadOnCeiling();
        if (entityPhysics.TestFeetCollision())


            if (entityPhysics.GetEntityElevation() <= maxheight)
            {
                entityPhysics.SetEntityElevation(maxheight);
                if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
                {
                    entityPhysics.SavePosition();
                    //Debug.Log("JUMP -> IDLE");
                    currentState = PunchingBagState.IDLE;
                }
                else
                {
                    //Debug.Log("JUMP -> RUN");
                    currentState = PunchingBagState.RUN;
                }
            }
    }

    

    public void SetJumpPressed(bool value)
    {
        jumpPressed = value;
    }
    public override void JustGotHit()
    {
        cooldowntimer = 1.0f;
        wasJustHit = true;
    }

    

}
