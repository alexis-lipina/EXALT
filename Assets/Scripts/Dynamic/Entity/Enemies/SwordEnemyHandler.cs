﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordEnemyHandler : EntityHandler
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private Animator fxAnimator;
    [SerializeField] private bool isCompanion;
    [SerializeField] private SpriteRenderer shieldSprite;

    [SerializeField] private SpriteRenderer bloodSplatterSprite;
    [SerializeField] private Transform _ichorHeartPrefab;


    enum TestEnemyState { IDLE, RUN, FALL, JUMP, READY, SWING, ATTACK, FLINCH, SPAWN, SHIELDBREAK, DEATH, FROZEN, SHATTER, STAGGER };
    private TestEnemyState currentState;

    const string IDLE_EAST_Anim = "SwordEnemy_IdleEast";
    const string IDLE_WEST_Anim = "SwordEnemy_IdleWest";
    const string RUN_EAST_Anim = "SwordEnemy_RunEast";
    const string RUN_WEST_Anim = "SwordEnemy_RunWest";
    const string JUMP_EAST_Anim = "SwordEnemy_JumpEast";
    const string JUMP_WEST_Anim = "SwordEnemy_JumpWest";
    const string FALL_EAST_Anim = "SwordEnemy_FallEast";
    const string FALL_WEST_Anim = "SwordEnemy_FallWest";

    const string READY_NORTH_Anim = "SwordEnemy_ReadyNorth";
    const string READY_SOUTH_Anim = "SwordEnemy_ReadySouth";
    const string READY_EAST_Anim = "SwordEnemy_ReadyEast";
    const string READY_WEST_Anim = "SwordEnemy_ReadyWest";

    const string ATTACK_NORTH_Anim = "SwordEnemy_AttackNorth";
    const string ATTACK_SOUTH_Anim = "SwordEnemy_AttackSouth";
    const string ATTACK_EAST_Anim = "SwordEnemy_AttackEast";
    const string ATTACK_WEST_Anim = "SwordEnemy_AttackWest";

    const string SWING_NORTH_Anim = "SwordEnemy_SlashNorth";
    const string SWING_SOUTH_Anim = "SwordEnemy_SlashSouth";
    const string SWING_EAST_Anim = "SwordEnemy_SlashEast";
    const string SWING_WEST_Anim = "SwordEnemy_SlashWest";

    const string SHIELDBREAK_FIRE = "SwordEnemy_ShieldBreak_Fire";
    const string SHIELDBREAK_VOID = "SwordEnemy_ShieldBreak_Void";
    const string SHIELDBREAK_ZAP = "SwordEnemy_ShieldBreak_Zap";

    const string FLINCH_Anim = "Anim_Flinch";

    const string SPAWN_Anim = "SwordEnemy_Spawn";

    const string DEATH_WEST_Anim = "SwordEnemy_DeathWest";
    const string DEATH_EAST_Anim = "SwordEnemy_DeathEast";

    const string DEATH_FALL_Anim = "DeathBeam";

    const string DEATH_SHATTER_Anim = "DeathShatter";

    const string STAGGER_WEST_Anim = "SwordEnemy_StaggerWest";
    const string STAGGER_EAST_Anim = "SwordEnemy_StaggerEast";

    const float WINDUP_DURATION = 0.33f; //duration of the windup before the swing
    const float FOLLOWTHROUGH_DURATION = 0.33f; //duration of the follow through after the swing

    const float SPAWN_DURATION = 0.66f;
    const float SHIELDBREAK_DURATION = 0.66f;
    const float DEATH_DURATION = 1.3f;
    const float DEATH_FALL_DURATION = 0.666f;
    const float STAGGER_DURATION = 1.0f;



    private enum TempTexDirection
    {
        EAST=0,
        NORTH=1,
        WEST=2,
        SOUTH=3
    }
    private TempTexDirection tempDirection;
    private const float AttackMovementSpeed = 0.2f;
    private const float JumpImpulse = 30f;

    private float attackCoolDown;
    float xInput;
    float yInput;
    bool jumpPressed;

    bool attackPressed; //temporary, probably
    bool hasSwung;

    bool wasJustHit;
    float stateTimer = 0.0f;
    private ElementType shieldToBreak = ElementType.NONE;

    private EnemySpawner _spawner;
    private bool fallDeath = false;

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
        currentState = TestEnemyState.SPAWN;
        stateTimer = 0.0f;
        jumpPressed = false;
        wasJustHit = false;
        hasSwung = false;
        tempDirection = TempTexDirection.EAST;
        currentPrimes = new List<ElementType>();
        bloodSplatterSprite.gameObject.SetActive(false);
    }

    void Update()
    {
        ExecuteState();
        wasJustHit = false;
        jumpPressed = false;
        attackPressed = false;
        fxAnimator.GetComponent<SpriteRenderer>().material.SetFloat("_FireAmount", entityPhysics.GetNormalizedBurnTimer());
        fxAnimator.GetComponent<SpriteRenderer>().material.SetFloat("_IchorFreezeBreak", entityPhysics.GetNormalizedFreezeTimer());

        //Debug.Log("Enemy Elevation : " + GetEntityPhysics().GetBottomHeight());
    }

    public override void SetXYAnalogInput(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    protected override void ExecuteState()
    {
        //Debug.Log("Enemy Current Z value is " + entityPhysics.GetObjectElevation()) ;

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
            case TestEnemyState.SPAWN:
                SpawnState();
                break;
            case TestEnemyState.SHIELDBREAK:
                ShieldBreakState();
                break;
            case TestEnemyState.DEATH:
                DeathState();
                break;
            case TestEnemyState.FROZEN:
                FrozenState();
                break;
            case TestEnemyState.SHATTER:
                ShatterState();
                break;
            case TestEnemyState.STAGGER:
                StaggerState();
                break;
            default:
                Debug.LogError("State " + currentState + "not implemented in state machine!");
                break;
        }

        // there exists a special case for enemy death occurs during detonation
        if (!_isDetonating && entityPhysics.GetCurrentHealth() <= 0)
        {
            OnDeath();
        }
    }

    //==============================| State Methods |

    private void IdleState()
    {

        //===========| Draw
        if (tempDirection == TempTexDirection.EAST)
        {
            characterAnimator.Play(IDLE_EAST_Anim);
            fxAnimator.Play(IDLE_EAST_Anim);
        }
        else
        {
            characterAnimator.Play(IDLE_WEST_Anim);
            fxAnimator.Play(IDLE_WEST_Anim);
        }

        //===========| Physics
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2 (xInput, yInput));
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
        if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
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
            fxAnimator.Play(RUN_EAST_Anim);
        }
        else
        {
            characterAnimator.Play(RUN_WEST_Anim);
            fxAnimator.Play(RUN_WEST_Anim);
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
        if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
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
                fxAnimator.Play(FALL_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(FALL_WEST_Anim);
                fxAnimator.Play(FALL_WEST_Anim);
            }
        }
        else
        {
            if (xInput > 0)
            {
                characterAnimator.Play(JUMP_EAST_Anim);
                fxAnimator.Play(JUMP_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(JUMP_WEST_Anim);
                fxAnimator.Play(JUMP_WEST_Anim);
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
                fxAnimator.Play(FALL_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(FALL_WEST_Anim);
                fxAnimator.Play(FALL_WEST_Anim);
            }
        }
        else
        {
            if (xInput > 0)
            {
                characterAnimator.Play(JUMP_EAST_Anim);
                fxAnimator.Play(JUMP_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(JUMP_WEST_Anim);
                fxAnimator.Play(JUMP_WEST_Anim);
            }
        }

        //Debug.Log("JUMPING!!!");
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput) * 1.8f); // multiplier is for jump lateral movement to be fast to allow gap crossing
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
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
        //free fall
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
        {
            entityPhysics.FreeFall();
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }

        //========| Draw
        switch (tempDirection)
        {
            case TempTexDirection.EAST:
                characterAnimator.Play(ATTACK_EAST_Anim);
                fxAnimator.Play(ATTACK_EAST_Anim);
                break;
            case TempTexDirection.WEST:
                characterAnimator.Play(ATTACK_WEST_Anim);
                fxAnimator.Play(ATTACK_WEST_Anim);
                break;
            case TempTexDirection.NORTH:
                characterAnimator.Play(ATTACK_NORTH_Anim);
                fxAnimator.Play(ATTACK_NORTH_Anim);
                break;
            case TempTexDirection.SOUTH:
                characterAnimator.Play(ATTACK_SOUTH_Anim);
                fxAnimator.Play(ATTACK_SOUTH_Anim);
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
            Collider2D[] hitobjects = Physics2D.OverlapBoxAll(swingboxpos, new Vector2(4, 3), 0);
            foreach (Collider2D hit in hitobjects)
            {
                EntityPhysics hitEntity = hit.gameObject.GetComponent<EntityPhysics>();
                if (hit.tag == "Friend" && hitEntity.GetObjectHeight() + hitEntity.GetObjectElevation() > entityPhysics.GetObjectElevation() && hitEntity.GetObjectElevation() < entityPhysics.GetObjectElevation() + entityPhysics.GetObjectHeight())
                {
                    hit.gameObject.GetComponent<EntityPhysics>().Inflict(1, force:(hitEntity.transform.position - entityPhysics.transform.position).normalized);
                    Debug.Log("Hit player!");
                }
            }
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
        if (stateTimer == 0f)
        {
            switch (tempDirection)
            {
                case TempTexDirection.EAST:
                    characterAnimator.Play(READY_EAST_Anim);
                    fxAnimator.Play(READY_EAST_Anim);
                    BigFlareVFX.DeployFromPool(entityPhysics.ObjectSprite.transform.position + new Vector3(-0.75f, -1.0625f, -1f));
                    break;
                case TempTexDirection.WEST:
                    characterAnimator.Play(READY_WEST_Anim);
                    fxAnimator.Play(READY_WEST_Anim);
                    BigFlareVFX.DeployFromPool(entityPhysics.ObjectSprite.transform.position + new Vector3(1.125f, -1.0625f, -1f));
                    break;
                case TempTexDirection.NORTH:
                    characterAnimator.Play(READY_NORTH_Anim);
                    fxAnimator.Play(READY_NORTH_Anim);
                    BigFlareVFX.DeployFromPool(entityPhysics.ObjectSprite.transform.position + new Vector3(2.25f, -0.5f, -1f));
                    break;
                case TempTexDirection.SOUTH:
                    characterAnimator.Play(READY_SOUTH_Anim);
                    fxAnimator.Play(READY_SOUTH_Anim);
                    BigFlareVFX.DeployFromPool(entityPhysics.ObjectSprite.transform.position + new Vector3(-1.75f, -0.75f, -1f));
                    break;
            }
        }
        
        //Physics
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(Vector2.zero);
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
        {
            entityPhysics.FreeFall();
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }

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
                fxAnimator.Play(SWING_EAST_Anim);
                break;
            case TempTexDirection.WEST:
                characterAnimator.Play(SWING_WEST_Anim);
                fxAnimator.Play(SWING_WEST_Anim);
                break;
            case TempTexDirection.NORTH:
                characterAnimator.Play(SWING_NORTH_Anim);
                fxAnimator.Play(SWING_NORTH_Anim);
                break;
            case TempTexDirection.SOUTH:
                characterAnimator.Play(SWING_SOUTH_Anim);
                fxAnimator.Play(SWING_SOUTH_Anim);
                break;
        }
        //Physics
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(Vector2.zero);
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
        {
            entityPhysics.FreeFall();
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }

        stateTimer += Time.deltaTime;
        if (stateTimer < 0.500) //if 500 ms have passed
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
        fxAnimator.Play(FLINCH_Anim);
    }

    private void SpawnState()
    {
        //Draw
        characterAnimator.Play(SPAWN_Anim);
        fxAnimator.Play(SPAWN_Anim);

        //Physics
        //Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(0, 0));
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        entityPhysics.SnapToFloor();

        //fall
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
        {
            entityPhysics.FreeFall();
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }

        //state transitions
        stateTimer += Time.deltaTime;

        if (stateTimer >= SPAWN_DURATION)
        {
            currentState = TestEnemyState.IDLE;
        }
    }

    private void ShieldBreakState()
    {
        //Draw
        switch (shieldToBreak)
        {
            case ElementType.VOID:
                characterAnimator.Play(SHIELDBREAK_VOID);
                fxAnimator.Play(SHIELDBREAK_VOID);
                break;
            case ElementType.FIRE:
                characterAnimator.Play(SHIELDBREAK_FIRE);
                fxAnimator.Play(SHIELDBREAK_FIRE);
                break;
            case ElementType.ZAP:
                characterAnimator.Play(SHIELDBREAK_ZAP);
                fxAnimator.Play(SHIELDBREAK_ZAP);
                break;
        }


        //Physics
        //Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(0, 0));
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        entityPhysics.SnapToFloor();

        //state transitions
        stateTimer += Time.deltaTime;

        if (stateTimer >= SHIELDBREAK_DURATION)
        {
            stateTimer = 0;
            shieldToBreak = ElementType.NONE;
            currentState = TestEnemyState.IDLE;
        }
    }

    private void DeathState()
    {
        //Debug.LogError("NOT IMPLEMENTED");
        
        //Draw
        if (entityPhysics.FellOutOfBounds)
        {
            characterAnimator.Play(DEATH_FALL_Anim);
            fxAnimator.Play(DEATH_FALL_Anim);
        }
        else
        {
            if (DeathVector.x > 0)
            {
                characterAnimator.Play(DEATH_EAST_Anim);
                fxAnimator.Play(DEATH_EAST_Anim);
            }
            else
            {
                characterAnimator.Play(DEATH_WEST_Anim);
                fxAnimator.Play(DEATH_WEST_Anim);
            }
            //Physics
            //Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
            Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(DeathVector);
            entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
            entityPhysics.SnapToFloor(); //TODO - Maybe have death animation be a two stage "fall" and "land" anim

            //fall
            float maxheight = entityPhysics.GetMaxTerrainHeightBelow();

            if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
            {
                //entityPhysics.ZVelocity = 0;
                entityPhysics.FreeFall();
            }
            else
            {
                entityPhysics.SetObjectElevation(maxheight);
            }
            DeathVector *= 0.95f;
        }
    }
    private void ShatterState()
    {
        characterAnimator.Play(DEATH_SHATTER_Anim);
        fxAnimator.Play(DEATH_SHATTER_Anim);
        //Physics
        //Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
        //Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(DeathVector);
        //entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        //entityPhysics.SnapToFloor(); //TODO - Maybe have death animation be a two stage "fall" and "land" anim
        //DeathVector *= 0.5f;
    }

    public override void Stagger()
    {
        if (currentState == TestEnemyState.FROZEN) return;
        if (currentState == TestEnemyState.DEATH) return;
        stateTimer = STAGGER_DURATION;
        currentState = TestEnemyState.STAGGER;

        //Vector2 tempDir = new Vector2(xInput, yInput).normalized;
        Vector2 tempDir = DeathVector;

        if (tempDir.x < 0.0f)
        {
            tempDirection = TempTexDirection.EAST;
            characterAnimator.Play(STAGGER_EAST_Anim, 0, 0.0f);
            fxAnimator.Play(STAGGER_EAST_Anim, 0, 0.0f);
        }
        else
        {
            tempDirection = TempTexDirection.WEST;
            characterAnimator.Play(STAGGER_WEST_Anim, 0, 0.0f);
            fxAnimator.Play(STAGGER_WEST_Anim, 0, 0.0f);
        }
    }

    private void StaggerState()
    {
        Vector2 movementVector = entityPhysics.MoveAvoidEntities(new Vector2(0, 0));
        entityPhysics.MoveCharacterPositionPhysics(movementVector.x, movementVector.y);

        //fall
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();

        if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
        {
            //entityPhysics.ZVelocity = 0;
            entityPhysics.FreeFall();
        }
        else
        {
            entityPhysics.SavePosition();
            entityPhysics.SetObjectElevation(maxheight);
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer < 0)
        {
            currentState = TestEnemyState.RUN;
        }
    }

    public void SetAttackPressed(bool value)
    {
        attackPressed = value;
    }
    
    public void SetJumpPressed(bool value)
    {
        jumpPressed = value;
    }
    public override void JustGotHit(Vector2 hitDirection)
    {
        //stateTimer = 1.0f;
        wasJustHit = true;
        DeathVector = hitDirection;
    }

    //==========================| SHIELD MANAGEMENT
    public override void ActivateShield(ElementType elementToMakeShield)
    {
        base.ActivateShield(elementToMakeShield);
        if (elementToMakeShield == ElementType.NONE)
        {
            Debug.LogWarning("ActivateShield called with NONE ElementType!");
            return;
        }
        shieldSprite.enabled = true;
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 1.0f);
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", GetElementColor(elementToMakeShield));
    }

    public override void BreakShield()
    {
        shieldToBreak = _shieldType;
        base.BreakShield();
        //TODO : dramatic thing must happen
        //shieldSprite.enabled = false;
        Debug.Log("SHIELD MACHINE BROKE");
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 0.0f);

        //force state transition when shield breaks
        stateTimer = 0f;
        currentState = TestEnemyState.SHIELDBREAK;
    }


    public override void UpdateIchorCorrupt()
    {
        float amt = 0.0f;
        switch (entityPhysics.IchorCorruptionAmount)
        {
            case 0:
                amt = 0.0f;
                break;
            case 1:
                amt = 0.4f;
                break;
            case 2:
                amt = 0.9f;
                break;
            default:
                amt = 1.0f;
                break;
        }
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CrystallizationAmount", amt);
    }


    public override void Freeze()
    {
        if (currentState == TestEnemyState.DEATH) return;
        currentState = TestEnemyState.FROZEN;
        fxAnimator.GetComponent<SpriteRenderer>().material.SetFloat("_CrystallizationAmount", 1.0f);
        characterAnimator.speed = 0;
        fxAnimator.speed = 0;
    }

    public override void Unfreeze()
    {
        currentState = TestEnemyState.IDLE; // TODO = break-out anim?
        fxAnimator.GetComponent<SpriteRenderer>().material.SetFloat("_CrystallizationAmount", 0.0f);
        characterAnimator.speed = 1.0f;
        fxAnimator.speed = 1.0f;
    }

    private void FrozenState()
    {
        //===========| Physics
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(Vector2.zero);
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        entityPhysics.SnapToFloor();
        //fall
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();

        if (entityPhysics.GetObjectElevation() > maxheight + 0.1)
        {
            //entityPhysics.ZVelocity = 0;
            entityPhysics.FreeFall();
        }
        else
        {
            entityPhysics.SavePosition();
            entityPhysics.SetObjectElevation(maxheight);
        }
    }

    public override void Flinch()
    {
        if (currentState == TestEnemyState.READY)
        {
            stateTimer = 0.01f;
        }
    }

    //==========================| DEATH MANAGEMENT

    public override void OnDeath()
    {
        if (_isDetonating || entityPhysics.IsDead) return;
        //Destroy(gameObject.transform.parent.gameObject);
        if (entityPhysics.IsFrozen) // frozen
        {
            Transform heart = Instantiate(_ichorHeartPrefab);
            ProjectilePhysics heartPhys = heart.GetComponentInChildren<ProjectilePhysics>();
            heartPhys.transform.position = entityPhysics.transform.position;
            heartPhys.SetObjectElevation(entityPhysics.GetObjectElevation() + 1.0f);
            entityPhysics.Unfreeze();
            StartCoroutine(PlayDeathAnim(isShatter: true));
        }
        else
        {
            StartCoroutine(PlayDeathAnim(isShatter: false));

        }
        entityPhysics.IsDead = true;
    }

    private IEnumerator PlayDeathAnim(bool isShatter)
    {
        currentState = isShatter ? TestEnemyState.SHATTER : TestEnemyState.DEATH;
        if (GetEntityPhysics().FellOutOfBounds)
        {
            Camera.main.GetComponent<CameraScript>().Jolt(2.0f, Vector2.down);
            yield return new WaitForSeconds(0.05f);
            ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.75f, 0.125f);
            yield return new WaitForSeconds(DEATH_FALL_DURATION - 0.05f);
        }
        else
        {
            if (!isShatter) StartCoroutine(PlayBloodSplatter());
            yield return new WaitForSeconds(DEATH_DURATION);
        }
        //destroy VFX
        if (_zapPrimeVfx != null) Destroy(_zapPrimeVfx);
        if (_voidPrimeVfx != null) Destroy(_voidPrimeVfx);
        if (_firePrimeVfx != null) Destroy(_firePrimeVfx);
        if (_ichorPrimeVfx != null) Destroy(_ichorPrimeVfx);


        if (entityPhysics._spawner)
        {
            Debug.Log("Returning melee enemy to pool");
            entityPhysics._spawner.ReturnToPool(transform.parent.gameObject.GetInstanceID());
            if (!entityPhysics._spawner.UseObjectPooling)
            {
                Destroy(transform.parent.gameObject);
            }
        }
        else
        {
            Debug.Log("Destroying enemy");
            Destroy(transform.parent.gameObject);
        }
    }
    
    private IEnumerator PlayBloodSplatter()
    {
        Debug.Log("SPLAT");
        bloodSplatterSprite.gameObject.SetActive(true);
        bloodSplatterSprite.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, DeathVector));
        bloodSplatterSprite.GetComponent<Animator>().Play("SplatterShort");
        yield return new WaitForSeconds(0.5f);
        bloodSplatterSprite.enabled = false;
    }
}
