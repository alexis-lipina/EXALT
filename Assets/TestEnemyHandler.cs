using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemyHandler : EntityHandler
{
    [SerializeField] private SpriteRenderer characterSprite;


    enum TestEnemyState { IDLE, RUN, FALL, JUMP, READY, SWING, ATTACK, WOUNDED };
    private TestEnemyState currentState;
    [SerializeField] private bool isCompanion;
    [SerializeField] private Sprite[] tempAttackSprite;
    [SerializeField] private Sprite[] tempReadySprite;
    [SerializeField] private Sprite[] tempSwingSprite;
    [SerializeField] private Sprite[] tempRunSprite;
    [SerializeField] private Sprite[] tempIdleSprite;
    [SerializeField] private Sprite[] tempRecoilSprite;  

    


    private enum TempTexDirection
    {
        EAST=0,
        NORTH=1,
        WEST=2,
        SOUTH=3
    }
    private TempTexDirection tempDirection;


    float xInput;
    float yInput;
    bool jumpPressed;
    float cooldowntimer;
    bool wasJustHit;

    void Awake()
    {
        /*if (isCompanion)
        {
            this.EntityPhysics.GetComponent<Rigidbody2D>().MovePosition(TemporaryPersistentDataScript.getDestinationPosition());
        }
        */
    }

    void Start()
    {
        xInput = 0;
        yInput = 0;
        currentState = TestEnemyState.IDLE;
        jumpPressed = false;
        wasJustHit = false;
        tempDirection = TempTexDirection.EAST;
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
        Vector2 tempDir = new Vector2(xInput, yInput).normalized;
        if (tempDir.x > Math.Abs(tempDir.y))
        {
            tempDirection = TempTexDirection.EAST;
        }
        else if (-tempDir.x > Math.Abs(tempDir.y))
        {
            tempDirection = TempTexDirection.WEST;
        }
        else if (tempDir.y > 0) { tempDirection = TempTexDirection.NORTH; }
        else if (tempDir.y < 0) { tempDirection = TempTexDirection.SOUTH; }


        switch (currentState)
        {
            case (TestEnemyState.IDLE):
                characterSprite.sprite = tempIdleSprite[(int)tempDirection];
                IdleState();
                break;
            case (TestEnemyState.RUN):
                characterSprite.sprite = tempRunSprite[(int)tempDirection];
                RunState();
                break;
            case (TestEnemyState.FALL):
                characterSprite.sprite = tempRunSprite[(int)tempDirection];
                FallState();
                break;
            case (TestEnemyState.JUMP):
                characterSprite.sprite = tempRunSprite[(int)tempDirection];
                JumpState();
                break;
            case TestEnemyState.ATTACK:
                characterSprite.sprite = tempAttackSprite[(int)tempDirection];
                AttackState();
                break;
            case TestEnemyState.WOUNDED:
                characterSprite.sprite = tempRecoilSprite[(int)tempDirection];
                WoundedState();
                break;
        }
    }

    //==============================| State Methods |

    private void IdleState()
    {
        //===========| State Switching
        //Debug.Log("IDLE!!!");
        if (Mathf.Abs(xInput) > 0.1 || Mathf.Abs(yInput) > 0.1)
        {
            currentState = TestEnemyState.RUN;
        }
        if (wasJustHit)
        {
            cooldowntimer = 1;
            currentState = TestEnemyState.WOUNDED;
        }
        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        if (EntityPhysics.GetEntityElevation() > maxheight)
        {
            EntityPhysics.ZVelocity = 0;
            currentState = TestEnemyState.FALL;
        }
        else
        {
            EntityPhysics.SetEntityElevation(maxheight);
        }
    }

    private void RunState()
    {
        //Debug.Log("Running!!!");
        EntityPhysics.MoveCharacterPositionPhysics(xInput, yInput);


        //===========| State Switching

        if (Mathf.Abs(xInput) < 0.1 && Mathf.Abs(yInput) < 0.1)
        {
            EntityPhysics.SavePosition();
            currentState = TestEnemyState.IDLE;
        }
        float maxheight = EntityPhysics.GetMaxTerrainHeightBelow();
        if (EntityPhysics.GetEntityElevation() > maxheight)
        {
            EntityPhysics.ZVelocity = 0;
            currentState = TestEnemyState.FALL;
        }
        if (jumpPressed)
        {
            EntityPhysics.ZVelocity = 1;
            currentState = TestEnemyState.JUMP;
        }
        if (wasJustHit)
        {
            cooldowntimer = 1;
            currentState = TestEnemyState.WOUNDED;
        }
        else
        {
            EntityPhysics.SavePosition();
            EntityPhysics.SetEntityElevation(maxheight);
        }
    }
    private void FallState()
    {
        //Debug.Log("Falling!!!");
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
                currentState = TestEnemyState.IDLE;
            }
            else
            {
                //Debug.Log("JUMP -> RUN");
                currentState = TestEnemyState.RUN;
            }
        }

    }

    private void JumpState()
    {
        //Debug.Log("JUMPING!!!");
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
                    currentState = TestEnemyState.IDLE;
                }
                else
                {
                    //Debug.Log("JUMP -> RUN");
                    currentState = TestEnemyState.RUN;
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
            currentState = TestEnemyState.RUN;
        }
    }

    /*
    private void DrawSprite()
    {
        switch (currentState)
        {
            case (TestEnemyState.IDLE):
                characterSprite.sprite = tempIdleSprite[(int)tempDirection];
                break;
            case (TestEnemyState.RUN):
                characterSprite.sprite = tempRunSprite[(int)tempDirection];
                RunState();
                break;
            case (TestEnemyState.FALL):
                characterSprite.sprite = tempIdleSprite[(int)tempDirection];
                FallState();
                break;
            case (TestEnemyState.JUMP):
                JumpState();
                break;
            case TestEnemyState.ATTACK:
                AttackState();
                break;
            case TestEnemyState.WOUNDED:
                WoundedState();
                break;
        }
    }
    */

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
