using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemyHandler : EntityHandler
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private AudioSource _primeAudioSource;
    [SerializeField] private bool isCompanion;


    enum TestEnemyState { IDLE, RUN, FALL, JUMP, READY, SWING, ATTACK, FLINCH };
    private TestEnemyState currentState;

    const string IDLE_EAST_Anim = "Anim_EnemyIdleEast";
    const string IDLE_WEST_Anim = "Anim_EnemyIdleWest";
    const string RUN_EAST_Anim = "Anim_EnemyRunEast";
    const string RUN_WEST_Anim = "Anim_EnemyRunWest";
    const string JUMP_EAST_Anim = "Anim_EnemyJumpEast";
    const string JUMP_WEST_Anim = "Anim_EnemyJumpWest";
    const string FALL_EAST_Anim = "Anim_EnemyFallEast";
    const string FALL_WEST_Anim = "Anim_EnemyFallWest";

    const string READY_NORTH_Anim = "Anim_EnemyReadyNorth";
    const string READY_SOUTH_Anim = "Anim_EnemyReadySouth";
    const string READY_EAST_Anim = "Anim_EnemyReadyEast";
    const string READY_WEST_Anim = "Anim_EnemyReadyWest";

    const string ATTACK_NORTH_Anim = "Anim_EnemyAttackNorth";
    const string ATTACK_SOUTH_Anim = "Anim_EnemyAttackSouth";
    const string ATTACK_EAST_Anim = "Anim_EnemyAttackEast";
    const string ATTACK_WEST_Anim = "Anim_EnemyAttackWest";

    const string SWING_NORTH_Anim = "Anim_EnemySlashNorth";
    const string SWING_SOUTH_Anim = "Anim_EnemySlashSouth";
    const string SWING_EAST_Anim = "Anim_EnemySlashEast";
    const string SWING_WEST_Anim = "Anim_EnemySlashWest";

    const string FLINCH_Anim = "Anim_Flinch";

    

    private bool _isPrimed_Void = false;
    private GameObject _voidPrimeVfx;
    private bool _isPrimed_Zap = false;
    private GameObject _zapPrimeVfx;
    private bool _isPrimed_Fire = false;
    private GameObject _firePrimeVfx;
    private List<ElementType> currentPrimes;

    public void PrimeEnemy(ElementType type)
    {
        if (_isPrimed_Void && type == ElementType.VOID || _isPrimed_Fire && type == ElementType.FIRE || _isPrimed_Zap && type == ElementType.ZAP) return;
        _primeAudioSource.Play();
        switch (type)
        {
            case ElementType.FIRE:
                _isPrimed_Fire = true;
                //_firePrimeVfx = Instantiate(Resources.Load("Prefabs/VFX/PrimedParticles_Fire", typeof(GameObject)) as GameObject, entityPhysics.ObjectSprite.transform);
                _firePrimeVfx = FireDetonationHandler.DeployFromPool(entityPhysics);
                currentPrimes.Add(type);
                return;
            case ElementType.VOID:
                _isPrimed_Void = true;
                //_voidPrimeVfx = Instantiate(Resources.Load("Prefabs/VFX/PrimedParticles_Void", typeof(GameObject)) as GameObject, entityPhysics.ObjectSprite.transform);
                _voidPrimeVfx = VoidDetonationHandler.DeployFromPool(entityPhysics);
                currentPrimes.Add(type);
                return;
            case ElementType.ZAP:
                _isPrimed_Zap = true;
                //_zapPrimeVfx = Instantiate(Resources.Load("Prefabs/VFX/PrimedParticles_Zap", typeof(GameObject)) as GameObject, entityPhysics.ObjectSprite.transform);
                _zapPrimeVfx = ZapDetonationHandler.DeployFromPool(entityPhysics);
                currentPrimes.Add(type);
                return;
        }
    }


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
                    hit.gameObject.GetComponent<EntityPhysics>().Inflict(1);
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
        if (stateTimer < 0.5) //if 500 ms have passed
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

    public void PerformDetonations(ElementType elementOfAttack)
    {
        if (!(_isPrimed_Fire || _isPrimed_Void || _isPrimed_Zap ) || elementOfAttack == ElementType.NONE) return;

        if (!currentPrimes.Contains(elementOfAttack))
        {
            PrimeEnemy(elementOfAttack);
        }

        List<ElementType> detonations = new List<ElementType>();

        foreach (ElementType element in currentPrimes)
        {
            switch (element)
            {
                case ElementType.FIRE:
                    Debug.Log("Fire Detonation");
                    _isPrimed_Fire = false;
                    //Destroy(_firePrimeVfx);
                    detonations.Add(ElementType.FIRE);
                    break;
                case ElementType.ZAP:
                    Debug.Log("Zap Detonation");
                    _isPrimed_Zap = false;
                    //Destroy(_zapPrimeVfx);
                    detonations.Add(ElementType.ZAP);
                    break;
                case ElementType.VOID:
                    Debug.Log("Void Detonation");
                    _isPrimed_Void = false;
                    //Destroy(_voidPrimeVfx);
                    detonations.Add(ElementType.VOID);
                    break;
            }
        }
        currentPrimes = new List<ElementType>();

        StartCoroutine(ExecuteDetonations(detonations));


        Debug.Log("Blam!");
        //ScreenFlash.InstanceOfScreenFlash.PlayHitPause(0.15f);
        
    }

    IEnumerator ExecuteDetonations(List<ElementType> detonations)
    {
        currentState = TestEnemyState.FLINCH;
        yield return new WaitForSeconds(0.1f);
        foreach (ElementType element in detonations)
        {
            switch (element)
            {
                case ElementType.FIRE:
                    //FireDetonationHandler.DeployFromPool(entityPhysics);
                    _firePrimeVfx.GetComponent<FireDetonationHandler>().Detonate();
                    _firePrimeVfx = null;
                    break;
                case ElementType.VOID:
                    //VoidDetonationHandler.DeployFromPool(entityPhysics);
                    _voidPrimeVfx.GetComponent<VoidDetonationHandler>().Detonate();
                    _voidPrimeVfx = null;
                    break;
                case ElementType.ZAP:
                    _zapPrimeVfx.GetComponent<ZapDetonationHandler>().Detonate();
                    _zapPrimeVfx = null;
                    break;
            }
            yield return new WaitForSeconds(0.15f);
        }
        currentState = TestEnemyState.RUN;
        if (entityPhysics.GetCurrentHealth() <= 0) OnDeath();
    }

    public override void OnDeath()
    {
        if (currentState == TestEnemyState.FLINCH) return;
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
