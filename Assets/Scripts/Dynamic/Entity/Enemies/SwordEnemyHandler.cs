﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordEnemyHandler : EntityHandler
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private bool isCompanion;
    [SerializeField] private SpriteRenderer shieldSprite;

    [SerializeField] private Sprite shieldSpriteImage_Void;
    [SerializeField] private Sprite shieldSpriteImage_Zap;
    [SerializeField] private Sprite shieldSpriteImage_Fire;


    enum TestEnemyState { IDLE, RUN, FALL, JUMP, READY, SWING, ATTACK, FLINCH, SPAWN };
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

    const string FLINCH_Anim = "Anim_Flinch";

    const string SPAWN_Anim = "SwordEnemy_Spawn";

    const float WINDUP_DURATION = 0.33f; //duration of the windup before the swing
    const float FOLLOWTHROUGH_DURATION = 0.33f; //duration of the follow through after the swing

    const float SPAWN_DURATION = 0.63f;
    
    


    private enum TempTexDirection
    {
        EAST=0,
        NORTH=1,
        WEST=2,
        SOUTH=3
    }
    private TempTexDirection tempDirection;
    private const float AttackMovementSpeed = 0.2f;
    private const float JumpImpulse = 40f;

    private float attackCoolDown;
    float xInput;
    float yInput;
    bool jumpPressed;

    bool attackPressed; //temporary, probably
    bool hasSwung;

    bool wasJustHit;
    float stateTimer = 0.0f;

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
        currentState = TestEnemyState.SPAWN;
        stateTimer = 0.0f;
        jumpPressed = false;
        wasJustHit = false;
        hasSwung = false;
        tempDirection = TempTexDirection.EAST;
        currentPrimes = new List<ElementType>();
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
            case TestEnemyState.SPAWN:
                SpawnState();
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
        }
        else
        {
            characterAnimator.Play(IDLE_WEST_Anim);
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
                    BigFlareVFX.DeployFromPool(entityPhysics.ObjectSprite.transform.position + new Vector3(-0.75f, -1.0625f, -1f));
                    break;
                case TempTexDirection.WEST:
                    characterAnimator.Play(READY_WEST_Anim);
                    BigFlareVFX.DeployFromPool(entityPhysics.ObjectSprite.transform.position + new Vector3(1.125f, -1.0625f, -1f));
                    break;
                case TempTexDirection.NORTH:
                    characterAnimator.Play(READY_NORTH_Anim);
                    BigFlareVFX.DeployFromPool(entityPhysics.ObjectSprite.transform.position + new Vector3(2.25f, -0.5f, -1f));
                    break;
                case TempTexDirection.SOUTH:
                    characterAnimator.Play(READY_SOUTH_Anim);
                    BigFlareVFX.DeployFromPool(entityPhysics.ObjectSprite.transform.position + new Vector3(-1.75f, -0.75f, -1f));
                    break;
            }
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
    }

    private void SpawnState()
    {
        //Draw
        characterAnimator.Play(SPAWN_Anim);

        //Physics
        //Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(xInput, yInput));
        Vector2 velocityAfterForces = entityPhysics.MoveAvoidEntities(new Vector2(0, 0));
        entityPhysics.MoveCharacterPositionPhysics(velocityAfterForces.x, velocityAfterForces.y);
        entityPhysics.SnapToFloor();

        //state transitions
        stateTimer += Time.deltaTime;

        if (stateTimer >= SPAWN_DURATION)
        {
            currentState = TestEnemyState.IDLE;
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
    }

    //==========================| SHIELD MANAGEMENT
    public override void ActivateShield(ElementType elementToMakeShield)
    {
        base.ActivateShield(elementToMakeShield);
        switch (elementToMakeShield)
        {
            case ElementType.FIRE:
                shieldSprite.enabled = true;
                //shieldSprite.sprite = shieldSpriteImage_Fire;
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 1.0f);
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(1f, 0.5f, 0f, 1f));
                break;
            case ElementType.VOID:
                shieldSprite.enabled = true;
                //shieldSprite.sprite = shieldSpriteImage_Void;
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 1.0f);
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0.5f, 0f, 1f, 1f));
                break;
            case ElementType.ZAP:
                shieldSprite.enabled = true;
                //shieldSprite.sprite = shieldSpriteImage_Zap;
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 1.0f);
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0, 1, 0.5f, 1));
                break;
            case ElementType.NONE:
                Debug.LogWarning("Attempted to assign NONE type shield to enemy!");
                break;
        }
    }

    public override void BreakShield()
    {
        base.BreakShield();
        //TODO : dramatic thing must happen
        //shieldSprite.enabled = false;
        Debug.Log("SHIELD MACHINE BROKE");
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 0.0f);
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


        if (entityPhysics._spawner)
        {
            Debug.Log("Returning melee enemy to pool");
            entityPhysics._spawner.ReturnToPool(transform.parent.gameObject.GetInstanceID());
        }
        else
        {
            Debug.Log("Destroying enemy");
            Destroy(transform.parent.gameObject);
        }
    }
}
