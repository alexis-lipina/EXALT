using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingBagHandler : EntityHandler
{

    enum PunchingBagState { IDLE, RUN, JUMP };
    private PunchingBagState currentState;

    float xInput;
    float yInput;



    void Start ()
    {
        xInput = 0;
        yInput = 0;
        currentState = PunchingBagState.IDLE;
	}
	
	void Update ()
    {
        ExecuteState();
	}

    public override void setXYAnalogInput(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    protected override void ExecuteState()
    {
        switch ( currentState )
        {
            case (PunchingBagState.IDLE):
                IdleState();
                break;
            case (PunchingBagState.RUN):
                RunState();
                break;
            case (PunchingBagState.JUMP):
                JumpState();
                break;
        }
    }

    //==============================| State Methods |

    private void IdleState()
    {
        //===========| State Switching

        if (Mathf.Abs(xInput) > 0.1 || Mathf.Abs(yInput) > 0.1)
        {
            currentState = PunchingBagState.RUN;
        }

        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        if (EntityPhysics.GetEntityElevation() > maxheight)
        {
            EntityPhysics.ZVelocity = 0;
            currentState = PunchingBagState.JUMP;
        }
        else
        {
            EntityPhysics.SetEntityElevation(maxheight);
        }
    }

    private void RunState()
    {
        //Debug.Log("Running!!!")
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
            currentState = PunchingBagState.JUMP;
        }
        else
        {
            EntityPhysics.SavePosition();
            EntityPhysics.SetEntityElevation(maxheight);
        }
    }
    private void JumpState()
    {
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


}
