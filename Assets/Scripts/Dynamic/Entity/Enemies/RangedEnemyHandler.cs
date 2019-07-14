using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyHandler : EntityHandler
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private bool isCompanion;


    enum TestEnemyState { IDLE, RUN, FALL, JUMP, READY, SWING, ATTACK, FLINCH };
    private TestEnemyState currentState;

    const string IDLE_EAST_Anim = "RangedBot_IdleEast";
    const string IDLE_WEST_Anim = "RangedBot_IdleWest";
    const string RUN_EAST_Anim = "RangedBot_RunRight";
    const string RUN_WEST_Anim = "RangedBot_RunLeft";
    const string JUMP_EAST_Anim = "RangedBot_JumpEast";
    const string JUMP_WEST_Anim = "RangedBot_JumpWest";
    const string FALL_EAST_Anim = "RangedBot_FallEast";
    const string FALL_WEST_Anim = "RangedBot_FallWest";

    const string READY_NORTH_Anim = "RangedBot_ReadyEast";
    const string READY_SOUTH_Anim = "RangedBot_ReadyWest";
    const string READY_EAST_Anim = "RangedBot_ReadyEast";
    const string READY_WEST_Anim = "RangedBot_ReadyWest";

    const string ATTACK_NORTH_Anim = "RangedBot_AttackEast";
    const string ATTACK_SOUTH_Anim = "RangedBot_AttackWest";
    const string ATTACK_EAST_Anim = "RangedBot_AttackEast";
    const string ATTACK_WEST_Anim = "RangedBot_AttackWest";

    const string SWING_NORTH_Anim = "RangedBot_SwingEast";
    const string SWING_SOUTH_Anim = "RangedBot_SwingWest";
    const string SWING_EAST_Anim = "RangedBot_SwingEast";
    const string SWING_WEST_Anim = "RangedBot_SwingWest";

    const string FLINCH_Anim = "Anim_Flinch";

    const float WINDUP_DURATION = 0.33f; //duration of the windup before the swing
    const float FOLLOWTHROUGH_DURATION = 0.33f; //duration of the follow through after the swing






    private enum TempTexDirection
    {
        EAST = 0,
        NORTH = 1,
        WEST = 2,
        SOUTH = 3
    }
    private TempTexDirection tempDirection;
    private const float AttackMovementSpeed = 0.2f;
    private const float JumpImpulse = 40f;

    private Weapon _enemyProjectileLauncher;

    private float attackCoolDown;
    float xInput;
    float yInput;
    bool jumpPressed;

    bool attackPressed; //temporary, probably
    bool hasSwung;

    bool wasJustHit;
    float stateTimer;

    private EnemySpawner _spawner;


    void Awake()
    {
        /*if (isCompanion)
        {
            this.entityPhysics.GetComponent<Rigidbody2D>().MovePosition(TemporaryPersistentDataScript.getDestinationPosition());
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
        hasSwung = false;
        tempDirection = TempTexDirection.EAST;
        currentPrimes = new List<ElementType>();
        _enemyProjectileLauncher = (Weapon)ScriptableObject.CreateInstance("RangedBotLauncher");
        _enemyProjectileLauncher.PopulateBulletPool();
    }

    void Update()
    {
        ExecuteState();
        wasJustHit = false;
        jumpPressed = false;
        attackPressed = false;
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
            case (TestEnemyState.IDLE):
                IdleState();
                break;
            case (TestEnemyState.RUN):
                RunState();
                break;
            case (TestEnemyState.FALL):
                FallState();
                break;
            case (TestEnemyState.JUMP):
                JumpState();
                break;
            case TestEnemyState.READY:
                ReadyState();
                break;
            case TestEnemyState.ATTACK:
                AttackState();
                break;
            case TestEnemyState.SWING:
                SwingState();
                break;
            case TestEnemyState.FLINCH:
                FlinchState();
                break;
        }
    }

    //==============================| State Methods |

    private void IdleState()
    {

        //===========| Draw
        if (tempDirection == TempTexDirection.EAST)
        {
            characterAnimator.Play(IDLE_EAST_Anim);
        }
        else
        {
            characterAnimator.Play(IDLE_WEST_Anim);
        }

        //===========| Physics
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        entityPhysics.SnapToFloor();
        //entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        //===========| State Switching


        if (Mathf.Abs(xInput) > 0.1 || Mathf.Abs(yInput) > 0.1)
        {
            currentState = TestEnemyState.RUN;
        }
        if (wasJustHit)
        {
            //stateTimer = 1;
            //currentState = TestEnemyState.WOUNDED;
        }
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight)
        {
            entityPhysics.ZVelocity = 0;
            currentState = TestEnemyState.FALL;
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }
    }

    private void RunState()
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

        //===========| Draw
        if (xInput > 0)
        {
            characterAnimator.Play(RUN_EAST_Anim);
        }
        else
        {
            characterAnimator.Play(RUN_WEST_Anim);
        }

        //entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        Vector2 movementVector = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
        entityPhysics.MoveCharacterPositionPhysics(movementVector.x, movementVector.y);



        //===========| State Switching

        if (Mathf.Abs(xInput) < 0.1 && Mathf.Abs(yInput) < 0.1)
        {
            entityPhysics.SavePosition();
            currentState = TestEnemyState.IDLE;
        }

        if (jumpPressed)
        {
            entityPhysics.ZVelocity = JumpImpulse;
            currentState = TestEnemyState.JUMP;
        }

        //fall
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (attackPressed)
        {
            stateTimer = 0;
            currentState = TestEnemyState.READY;
        }
        if (entityPhysics.GetObjectElevation() > maxheight)
        {
            entityPhysics.ZVelocity = 0;
            currentState = TestEnemyState.FALL;
        }

        else
        {
            entityPhysics.SavePosition();
            entityPhysics.SetObjectElevation(maxheight);
        }
    }
    private void FallState()
    {
        //==========| Draw
        if (entityPhysics.ZVelocity < 0)
        {
            if (xInput > 0)
            {
                characterAnimator.Play(FALL_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(FALL_WEST_Anim);
            }
        }
        else
        {
            if (xInput > 0)
            {
                characterAnimator.Play(JUMP_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(JUMP_WEST_Anim);
            }
        }

        //Debug.Log("Falling!!!");
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        entityPhysics.FreeFall();


        //===========| State Switching

        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() <= maxheight)
        {
            entityPhysics.SetObjectElevation(maxheight);
            if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
            {
                //entityPhysics.SavePosition();
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
        //==========| Draw
        if (entityPhysics.ZVelocity < 0)
        {
            if (xInput > 0)
            {
                characterAnimator.Play(FALL_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(FALL_WEST_Anim);
            }
        }
        else
        {
            if (xInput > 0)
            {
                characterAnimator.Play(JUMP_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(JUMP_WEST_Anim);
            }
        }

        //Debug.Log("JUMPING!!!");
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        Debug.Log("FORCES : " + velocityAfterForces);
        entityPhysics.FreeFall();
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        //entityPhysics.CheckHitHeadOnCeiling();
        if (entityPhysics.TestFeetCollision())


            if (entityPhysics.GetObjectElevation() <= maxheight)
            {
                entityPhysics.SetObjectElevation(maxheight);
                if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
                {
                    entityPhysics.SavePosition();
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
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(Vector2.zero);
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);

        //========| Draw
        switch (tempDirection)
        {
            case TempTexDirection.EAST:
                characterAnimator.Play(ATTACK_EAST_Anim);
                break;
            case TempTexDirection.WEST:
                characterAnimator.Play(ATTACK_WEST_Anim);
                break;
            case TempTexDirection.NORTH:
                characterAnimator.Play(ATTACK_NORTH_Anim);
                break;
            case TempTexDirection.SOUTH:
                characterAnimator.Play(ATTACK_SOUTH_Anim);
                break;
        }

        Vector2 swingboxpos = Vector2.zero;
        switch (tempDirection)
        {
            case TempTexDirection.EAST:
                swingboxpos = new Vector2(entityPhysics.transform.position.x + 2, entityPhysics.transform.position.y);
                entityPhysics.MoveCharacterPositionPhysics(AttackMovementSpeed, 0);
                break;
            case TempTexDirection.WEST:
                swingboxpos = new Vector2(entityPhysics.transform.position.x - 2, entityPhysics.transform.position.y);
                entityPhysics.MoveCharacterPositionPhysics(-AttackMovementSpeed, 0);
                break;
            case TempTexDirection.NORTH:
                swingboxpos = new Vector2(entityPhysics.transform.position.x, entityPhysics.transform.position.y + 2);
                entityPhysics.MoveCharacterPositionPhysics(0, AttackMovementSpeed);
                break;
            case TempTexDirection.SOUTH:
                swingboxpos = new Vector2(entityPhysics.transform.position.x, entityPhysics.transform.position.y - 2);
                entityPhysics.MoveCharacterPositionPhysics(0, -AttackMovementSpeed);
                break;
        }
        //todo - test area for collision, if coll
        if (!hasSwung)
        {
            
            //fire bullet
            GameObject bullet = _enemyProjectileLauncher.FireBullet(new Vector2(xInput, yInput));
            bullet.GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(entityPhysics.GetObjectElevation() + 2f);
            bullet.GetComponentInChildren<ProjectilePhysics>().GetComponent<Rigidbody2D>().position = (entityPhysics.GetComponent<Rigidbody2D>().position);
            hasSwung = true;
        }
        stateTimer += Time.deltaTime;
        if (stateTimer > 0.05)
        {
            stateTimer = 0;
            currentState = TestEnemyState.SWING;
            hasSwung = false;
        }


    }
    //telegraph about to swing, called in AttackState()
    private void ReadyState()
    {

        //========| Draw
        switch (tempDirection)
        {
            case TempTexDirection.EAST:
                characterAnimator.Play(READY_EAST_Anim);
                break;
            case TempTexDirection.WEST:
                characterAnimator.Play(READY_WEST_Anim);
                break;
            case TempTexDirection.NORTH:
                characterAnimator.Play(READY_NORTH_Anim);
                break;
            case TempTexDirection.SOUTH:
                characterAnimator.Play(READY_SOUTH_Anim);
                break;
        }
        //Physics
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(Vector2.zero);
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        stateTimer += Time.deltaTime;
        if (stateTimer < 0.4) //if 500 ms have passed
        {
            //do nothing
        }
        else
        {
            stateTimer = 0;
            currentState = TestEnemyState.ATTACK;
        }
    }

    //flash attack
    private void SwingState()
    {
        //========| Draw
        switch (tempDirection)
        {
            case TempTexDirection.EAST:
                characterAnimator.Play(SWING_EAST_Anim);
                break;
            case TempTexDirection.WEST:
                characterAnimator.Play(SWING_WEST_Anim);
                break;
            case TempTexDirection.NORTH:
                characterAnimator.Play(SWING_NORTH_Anim);
                break;
            case TempTexDirection.SOUTH:
                characterAnimator.Play(SWING_SOUTH_Anim);
                break;
        }
        //Physics
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(Vector2.zero);
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);

        stateTimer += Time.deltaTime;
        if (stateTimer < 0.5) //if 500 ms have passed
        {
            //do nothing
        }
        else
        {
            stateTimer = 0;
            currentState = TestEnemyState.RUN;
        }
    }


    private void FlinchState()
    {
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(Vector2.zero);
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        characterAnimator.Play(FLINCH_Anim);
    }

    public void SetAttackPressed(bool value)
    {
        attackPressed = value;
    }

    public void SetJumpPressed(bool value)
    {
        jumpPressed = value;
    }
    public override void JustGotHit()
    {
        //stateTimer = 1.0f;
        wasJustHit = true;
    }





    public override void OnDeath()
    {
        if (_isDetonating) return;
        //Destroy(gameObject.transform.parent.gameObject);
        StartCoroutine(PlayDeathAnim());
    }

    private IEnumerator PlayDeathAnim()
    {
        currentState = TestEnemyState.FLINCH;
        yield return new WaitForSeconds(0.25f);
        //destroy VFX
        if (_zapPrimeVfx != null) Destroy(_zapPrimeVfx);
        if (_voidPrimeVfx != null) Destroy(_voidPrimeVfx);
        if (_firePrimeVfx != null) Destroy(_firePrimeVfx);
        Destroy(transform.parent.gameObject);
    }
}
