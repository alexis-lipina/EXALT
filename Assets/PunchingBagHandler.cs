using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingBagHandler : EntityHandler
{

    enum PunchingBagState { IDLE, RUN, FALL, JUMP, ATTACK, WOUNDED };
    private PunchingBagState currentState;

    float xInput;
    float yInput;
    bool jumpPressed;
    float cooldowntimer;
    bool wasJustHit;

    void Start()
    {
        xInput = 0;
        yInput = 0;
        currentState = PunchingBagState.IDLE;
        jumpPressed = false;
        wasJustHit = false;
    }

    void Update()
    {
        ExecuteState();
        wasJustHit = false;
    }

    public override void setXYAnalogInput(float x, float y)
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
            case PunchingBagState.ATTACK:
                AttackState();
                break;
            case PunchingBagState.WOUNDED:
                WoundedState();
                break;
        }
    }

    //==============================| State Methods |

    private void IdleState()
    {
        //===========| State Switching
        Debug.Log("IDLE!!!");
        if (Mathf.Abs(xInput) > 0.1 || Mathf.Abs(yInput) > 0.1)
        {
            currentState = PunchingBagState.RUN;
        }
        if (wasJustHit)
        {
            cooldowntimer = 1;
            currentState = PunchingBagState.WOUNDED;
        }
        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        if (EntityPhysics.GetEntityElevation() > maxheight)
        {
            EntityPhysics.ZVelocity = 0;
            currentState = PunchingBagState.FALL;
        }
        else
        {
            EntityPhysics.SetEntityElevation(maxheight);
        }
    }

    private void RunState()
    {
        Debug.Log("Running!!!");
        EntityPhysics.MoveCharacterPositionPhysics(xInput, yInput);


        //===========| State Switching

        if (Mathf.Abs(xInput) < 0.1 && Mathf.Abs(yInput) < 0.1)
        {
            EntityPhysics.SavePosition();
            currentState = PunchingBagState.IDLE;
        }
        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        if (EntityPhysics.GetEntityElevation() > maxheight)
        {
            EntityPhysics.ZVelocity = 0;
            currentState = PunchingBagState.FALL;
        }
        if (jumpPressed)
        {
            EntityPhysics.ZVelocity = 1;
            currentState = PunchingBagState.JUMP;
        }
        if (wasJustHit)
        {
            cooldowntimer = 1;
            currentState = PunchingBagState.WOUNDED;
        }
        else
        {
            EntityPhysics.SavePosition();
            EntityPhysics.SetEntityElevation(maxheight);
        }
    }
    private void FallState()
    {
        Debug.Log("Falling!!!");
        EntityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        EntityPhysics.FreeFall();


        //===========| State Switching

        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        if (EntityPhysics.GetEntityElevation() <= maxheight)
        {
            EntityPhysics.SetEntityElevation(maxheight);
            if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
            {
                //EntityPhysics.SavePosition();
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
        Debug.Log("JUMPING!!!");
        EntityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        EntityPhysics.FreeFall();
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
                    currentState = PunchingBagState.IDLE;
                }
                else
                {
                    //Debug.Log("JUMP -> RUN");
                    currentState = PunchingBagState.RUN;
                }
            }
    }

    private void AttackState()
    {

    }

    private void WoundedState()
    {
        EntityPhysics.MoveCharacterPositionPhysics(xInput * 0.3f, yInput * 0.3f);
        cooldowntimer -= Time.deltaTime;
        if (cooldowntimer < 0)
        {
            cooldowntimer = 0;
            currentState = PunchingBagState.RUN;
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
