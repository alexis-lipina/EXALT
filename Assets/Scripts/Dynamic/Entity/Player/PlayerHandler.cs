using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Rewired;

/// <summary>
/// This class controls player state and contains methods for each state. It also receives input from the InputHandler and acts in accordance with said input.
/// In addition, it handles sprites, shadows, and player height
/// </summary>
public class PlayerHandler : EntityHandler
{
    //[SerializeField] private InputHandler _inputHandler;
    [SerializeField] private GameObject characterSprite;
    [SerializeField] private GameObject FollowingCamera;
    [SerializeField] private GameObject LightMeleeSprite;
    [SerializeField] private GameObject HeavyMeleeSprite;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private CursorHandler _cursor;
    [SerializeField] private bool _isUsingCursor; //TEMPORARY



    private Animator characterAnimator;
    private PlayerInventory inventory;


    [SerializeField] private UIHealthBar _healthBar;

    enum PlayerState {IDLE, RUN, JUMP, LIGHT_MELEE, HEAVY_MELEE, CHARGE, BURST};

    const string IDLE_EAST_Anim = "New_IdleEast";
    const string IDLE_WEST_Anim = "New_IdleWest";
    const string IDLE_NORTH_Anim = "New_IdleNorth";
    const string IDLE_SOUTH_Anim = "New_IdleSouth";
    const string RUN_EAST_Anim = "New_RunEast";
    const string RUN_WEST_Anim = "New_RunWest";
    const string RUN_SOUTH_Anim = "New_RunSouth";
    const string RUN_NORTH_Anim = "New_RunNorth";
    const string WALK_EAST_Anim = "New_WalkEast";
    const string WALK_WEST_Anim = "New_WalkWest";
    const string WALK_SOUTH_Anim = "New_WalkSouth";
    const string WALK_NORTH_Anim = "New_WalkNorth";
    const string JUMP_EAST_Anim = "Anim_PlayerJumpEast";
    const string JUMP_WEST_Anim = "Anim_PlayerJumpWest";
    const string FALL_EAST_Anim = "Anim_PlayerFallEast";
    const string FALL_WEST_Anim = "Anim_PlayerFallWest";
    const string SWING_NORTH_Anim = "Anim_PlayerSwingNorth";
    const string SWING_SOUTH_Anim = "Anim_PlayerSwingSouth";
    const string SWING_EAST_Anim = "Anim_PlayerSwingEast";
    const string SWING_WEST_Anim = "Anim_PlayerSwingWest";


    private const float AttackMovementSpeed = 0.6f;

    private Weapon _equippedWeapon;

    private PlayerState CurrentState;
    private PlayerState PreviousState;
    private FaceDirection currentFaceDirection;

    
    private bool hasSwung;

    //=================| NEW COMBAT STUFF
    //state times
    private const float time_lightMelee = 0.25f; //duration of state
    //private const float time_to_combo = 0.2f;
    private bool _hasHitAttackAgain = false; //used for combo chaining 
    private bool _readyForThirdHit = false; //true during second attack, if player hits x again changes to heavy attack
    private float _lengthOfLightMeleeAnimation;

    private Vector2 lightmelee_hitbox = new Vector2(5, 4);
    private Vector2 thrustDirection;
    private Vector2 aimDirection; // direction AND magnitude of "right stick", used for attack direction, camera, never a 0 vector
    private float _timeToComboReady = 0.17f; //higher means more generous
    private bool _hitComboBeforeReady;

    private const float time_heavyMelee = 0.3f;
    private Vector2 heavymelee_hitbox = new Vector2(8, 4);
    private float _lengthOfHeavyMeleeAnimation;

    //Charge Stuff
    private const float time_Charge = 1.45f; //total time before player is charged up
    private const float time_ChargeLight = .5f;
    private const float time_ChargeMedium = .5f;
    private const float time_ChargeTransition = 0.25f; //how long is the entire transition animation, anyway?

    private float _burstDuration = .75f;
    private const float time_burstHit = 0.4f;
    private Vector2 _burstArea = new Vector2(16f, 12f);
    private bool _hasFlashed = false;

    private Vector2 _cursorWorldPos;

    private float PlayerRunSpeed;
    private float xInput; 
    private float yInput;   
    private float StateTimer;
    private bool isFlipped;
    private List<int> hitEnemies;

    private Player controller;


    //=====================| JUMP/FALL FIELDS
    [SerializeField] private float JumpImpulse = 30f;
    [SerializeField] private float JumpHeldGravity;
    [SerializeField] private float JumpFallGravity;
    [SerializeField] private float FallGravity;



    public bool IsUsingMouse
    {
        get { return _isUsingCursor; }
        set { _isUsingCursor = value; }
    }

    void Awake()
    {
        controller = ReInput.players.GetPlayer(0);
        //this.entityPhysics.GetComponent<Rigidbody2D>().MovePosition(TemporaryPersistentDataScript.getDestinationPosition());
        inventory = gameObject.GetComponent<PlayerInventory>();
        
    }

	void Start ()
    {
        aimDirection = Vector2.right;
        //SwapWeapon("NORTH"); //Debug
        //Debug.Log(_equippedWeapon);
        CurrentState = PlayerState.IDLE;
        StateTimer = 0;
        //playerRigidBody = PhysicsObject.GetComponent<Rigidbody2D>();
        
        //TerrainTouched.Add(666, new KeyValuePair<float, float>(0.0f, -20.0f));
        characterAnimator = characterSprite.GetComponent<Animator>();
        hasSwung = false;
        hitEnemies = new List<int>();
        _lengthOfLightMeleeAnimation = LightMeleeSprite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        _lengthOfHeavyMeleeAnimation = HeavyMeleeSprite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;

        SwapWeapon("SOUTH");
    }


    void Update ()
    {
        xInput = controller.GetAxisRaw("MoveHorizontal");
        yInput = controller.GetAxisRaw("MoveVertical");
        if (entityPhysics.GetCurrentHealth() <= 1) { OnDeath(); }
        //---------------------------| Manage State Machine |
        this.ExecuteState();
        //updateHeight();
        //moveCharacterPosition();
        //reset button presses
        PreviousState = CurrentState;
        //FollowingCamera.transform.position = new Vector3(playerCharacterSprite.transform.position.x, playerCharacterSprite.transform.position.y, -100);


        //Swapping weapons - still unsure if I want to keep ranged combat...?
        /*
        if (_inputHandler.DPadNorth > 0)
        {
            SwapWeapon("NORTH");
        }
        else if (_inputHandler.DPadSouth > 0)
        {
            SwapWeapon("SOUTH");
        }
        else if (_inputHandler.DPadWest > 0)
        {
            SwapWeapon("WEST");
        }
        else if (_inputHandler.DPadEast > 0)
        {
            SwapWeapon("EAST");
        }
        */


        //TODO : Temporary gun testing
        if (controller.GetButton("RangedAttack"))
        {
            if (_equippedWeapon.CanFireBullet())
            {
                FireBullet();
            }
        }
        /*
        if (_inputHandler.RightBumper > 0.2)
        {
            ThrowGrenade();
        }
        */
       
    }

    protected override void ExecuteState()
    {
        switch (CurrentState)
        {
            case (PlayerState.IDLE):
                PlayerIdle();
                break;
            case (PlayerState.RUN):
                PlayerRun();
                break;
            case (PlayerState.JUMP):
                //characterAnimator.Play(JUMP_Anim);
                PlayerJump();
                break;
            case (PlayerState.LIGHT_MELEE):
                if (isFlipped)
                {
                    FlipCharacterSprite();
                    isFlipped = false;
                }
                PlayerLightMelee();
                break;
            case (PlayerState.HEAVY_MELEE):
                if (isFlipped)
                {
                    FlipCharacterSprite();
                    isFlipped = false;
                }
                PlayerHeavyMelee();
                break;
            case (PlayerState.CHARGE):
                PlayerCharge();
                break;
            case (PlayerState.BURST):
                PlayerBurst();
                break;
        }
    }
    
    private void FlipCharacterSprite()
    {
        Vector3 theScale = characterSprite.transform.localScale;
        theScale.x *= -1;
        characterSprite.transform.localScale = theScale;
    }

    //================================================================================| STATE METHODS 
    #region State Methods
    private void PlayerIdle()
    {
        //Draw
        switch (currentFaceDirection)
        {
            case FaceDirection.NORTH:
                characterAnimator.Play(IDLE_NORTH_Anim);
                break;
            case FaceDirection.SOUTH:
                characterAnimator.Play(IDLE_SOUTH_Anim);
                break;
            case FaceDirection.EAST:
                characterAnimator.Play(IDLE_EAST_Anim);
                break;
            case FaceDirection.WEST:
                characterAnimator.Play(IDLE_WEST_Anim);
                break;
        }
        
        
        //do nothing, maybe later have them breathing or getting bored, sitting down
        entityPhysics.MoveCharacterPositionPhysics(0, 0);
        entityPhysics.SnapToFloor();
        //Debug.Log("Player Idle");


        // track aimDirection vector
        UpdateAimDirection();
        
        LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * lightmelee_hitbox.x / 2.0f, characterSprite.transform.position.y + aimDirection.normalized.y * lightmelee_hitbox.x/2.0f, characterSprite.transform.position.z + aimDirection.normalized.y), Quaternion.identity);
        LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
        HeavyMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * heavymelee_hitbox.x / 2.0f, characterSprite.transform.position.y + aimDirection.normalized.y * heavymelee_hitbox.x / 2.0f, characterSprite.transform.position.z + aimDirection.normalized.y), Quaternion.identity);
        HeavyMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));

        //------------------------------------------------| STATE CHANGE
        if (Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2) 
        {
            //Debug.Log("IDLE -> RUN");
            CurrentState = PlayerState.RUN;
        }
        if (controller.GetButtonDown("Jump"))
        {
            //Debug.Log("IDLE -> JUMP");
            entityPhysics.ZVelocity = JumpImpulse;
            CurrentState = PlayerState.JUMP;
        }

        if (controller.GetButtonDown("Melee"))
        {
            hasSwung = false;
            //Debug.Log("IDLE -> ATTACK");
            StateTimer = time_lightMelee;
            CurrentState = PlayerState.LIGHT_MELEE;
        }
        
        if (controller.GetButton("Charge"))
        {
            StateTimer = time_Charge;
            CurrentState = PlayerState.CHARGE;
        }
        

        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight) //override other states to trigger fall
        {
            entityPhysics.ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }
        
    }

    private void PlayerRun()
    {
        //Face Direction Determination
        Vector2 direction = controller.GetAxis2DRaw("MoveHorizontal", "MoveVertical");
        if (direction.sqrMagnitude > 1) direction.Normalize(); //prevents going too fast on KB
        if (direction.sqrMagnitude > 0.01f)
        {
            if (Vector2.Angle(new Vector2(1, 0), direction) < 60)
            {
                currentFaceDirection = FaceDirection.EAST;
            }
            else if (Vector2.Angle(new Vector2(0, 1), direction) < 30)
            {
                currentFaceDirection = FaceDirection.NORTH;
            }
            else if (Vector2.Angle(new Vector2(0, -1), direction) < 30)
            {
                currentFaceDirection = FaceDirection.SOUTH;
            }
            else if (Vector2.Angle(new Vector2(-1, 0), direction) < 60)
            {
                currentFaceDirection = FaceDirection.WEST;
            }
        }
        

        //===============================================| DRAW
        if (direction.sqrMagnitude < 0.35)//walk
        {
            switch(currentFaceDirection)
            {
                case FaceDirection.EAST:
                    characterAnimator.Play(WALK_EAST_Anim);
                    break;
                case FaceDirection.WEST:
                    characterAnimator.Play(WALK_WEST_Anim);
                    break;
                case FaceDirection.NORTH:
                    characterAnimator.Play(WALK_NORTH_Anim);
                    break;
                case FaceDirection.SOUTH:
                    characterAnimator.Play(WALK_SOUTH_Anim);
                    break;
            }

        }
        else
        {
            switch (currentFaceDirection)
            {
                case FaceDirection.EAST:
                    characterAnimator.Play(RUN_EAST_Anim);
                    break;
                case FaceDirection.WEST:
                    characterAnimator.Play(RUN_WEST_Anim);
                    break;
                case FaceDirection.NORTH:
                    characterAnimator.Play(RUN_NORTH_Anim);
                    break;
                case FaceDirection.SOUTH:
                    characterAnimator.Play(RUN_SOUTH_Anim);
                    break;
            }
        }


        // track aimDirection vector
        UpdateAimDirection();
        
        LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * lightmelee_hitbox.x / 2.0f, characterSprite.transform.position.y + aimDirection.normalized.y * lightmelee_hitbox.x / 2.0f, characterSprite.transform.position.z + aimDirection.normalized.y), Quaternion.identity);
        LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
        /*
        LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.x, characterSprite.transform.position.y + aimDirection.y, characterSprite.transform.position.z + aimDirection.y), Quaternion.identity);
        LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
        */
        HeavyMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * heavymelee_hitbox.x / 2.0f, characterSprite.transform.position.y + aimDirection.normalized.y * heavymelee_hitbox.x / 2.0f, characterSprite.transform.position.z + aimDirection.normalized.y), Quaternion.identity);
        HeavyMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));

        //Debug.Log("Player Running");
        //------------------------------------------------| MOVE

        Vector2 vec = entityPhysics.MoveAvoidEntities(direction);
        entityPhysics.MoveCharacterPositionPhysics(vec.x, vec.y);
        entityPhysics.SnapToFloor();
        //face direction determination
        
        
        //-------| Z Azis Traversal 
        // handles falling if player is above ground
        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight)
        {
            entityPhysics.ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }
        //------------------------------------------------| STATE CHANGE
        //Debug.Log("X:" + xInput + "Y:" + yInput);
        if (Mathf.Abs(xInput) < 0.1 && Mathf.Abs(yInput) < 0.1)
        {
            //Debug.Log("RUN -> IDLE");
            CurrentState = PlayerState.IDLE;
        }
        if (controller.GetButtonDown("Jump"))
        {
            entityPhysics.SavePosition();
            //Debug.Log("RUN -> JUMP");
            entityPhysics.ZVelocity = JumpImpulse;
            CurrentState = PlayerState.JUMP;
        }
        if (controller.GetButtonDown("Melee"))
        {
            //Debug.Log("RUN -> ATTACK");
            StateTimer = time_lightMelee;
            CurrentState = PlayerState.LIGHT_MELEE;
        }
        
        if (controller.GetButton("Charge"))
        {
            CurrentState = PlayerState.CHARGE;
            StateTimer = time_Charge;
        }
        

        if (CurrentState == PlayerState.RUN)
        {
            entityPhysics.SavePosition();
        }
    }

    private void PlayerJump()
    {
        //Debug.Log("Player Jumping");
        //Facing Determination

        Vector2 direction = new Vector2(xInput, yInput);
        if (direction.sqrMagnitude > 1) direction.Normalize(); //prevents going too fast on KB
        if (direction.sqrMagnitude > 0.01f)
        {
            if (Vector2.Angle(new Vector2(1, 0), direction) < 45)
            {
                currentFaceDirection = FaceDirection.EAST;
            }
            else if (Vector2.Angle(new Vector2(0, 1), direction) < 45)
            {
                currentFaceDirection = FaceDirection.NORTH;
            }
            else if (Vector2.Angle(new Vector2(0, -1), direction) < 45)
            {
                currentFaceDirection = FaceDirection.SOUTH;
            }
            else if (Vector2.Angle(new Vector2(-1, 0), direction) < 45)
            {
                currentFaceDirection = FaceDirection.WEST;
            }
        }

        //=======================| DRAW

        if (entityPhysics.ZVelocity > 0 && currentFaceDirection == FaceDirection.EAST)
        {
            characterAnimator.Play(JUMP_EAST_Anim);
        }
        else if (entityPhysics.ZVelocity < 0 && currentFaceDirection == FaceDirection.EAST)
        {
            characterAnimator.Play(FALL_EAST_Anim);
        }
        else if (entityPhysics.ZVelocity > 0)
        {
            characterAnimator.Play(JUMP_WEST_Anim);
        }
        else if (entityPhysics.ZVelocity < 0)
        {
            characterAnimator.Play(FALL_WEST_Anim);
        }
        //------------------------------| MOVE
        Vector2 vec = entityPhysics.MoveAvoidEntities(direction);
        entityPhysics.MoveCharacterPositionPhysics(vec.x, vec.y);
        //entityPhysics.MoveCharacterPositionPhysics(xInput, yInput);
        if (entityPhysics.ZVelocity < 0)
        {
            entityPhysics.Gravity = FallGravity;
        }
        else
        {
            if (controller.GetButton("Jump"))
            {
                entityPhysics.Gravity = JumpHeldGravity;
            }
            else
            {
                entityPhysics.Gravity = JumpFallGravity;
            }
        }
        entityPhysics.FreeFall();
        
        //------------------------------| STATE CHANGE

        //Check for foot collision

        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        //EntityPhysics.CheckHitHeadOnCeiling();
        //if (entityPhysics.TestFeetCollision())


        if (entityPhysics.GetObjectElevation() <= maxheight)
        {
            entityPhysics.SetObjectElevation(maxheight);
            if (Mathf.Abs(xInput) < 0.1 || Mathf.Abs(yInput) < 0.1)
            {
                entityPhysics.SavePosition();
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
    

    
    /// <summary>
    /// This is the player's basic close-range attack. Can be chained for a swipe-swipe-jab combo, and supports aiming in any direction
    /// </summary>
    private void PlayerLightMelee()
    {
        //Functionality to be done at very beginning
        if (StateTimer == time_lightMelee)
        {
            //Play SFX
            _audioSource.Play();

            _hitComboBeforeReady = false;
            StartCoroutine(PlayLightAttack(_readyForThirdHit));

            thrustDirection = aimDirection;

            //Debug.DrawRay(entityPhysics.transform.position, thrustDirection*5.0f, Color.cyan, 0.2f);


            Vector2 hitboxpos = (Vector2)entityPhysics.transform.position + thrustDirection * (lightmelee_hitbox.x / 2.0f);
            Collider2D[] hitobjects = Physics2D.OverlapBoxAll(hitboxpos, lightmelee_hitbox, Vector2.SignedAngle(Vector2.right, thrustDirection));
            Debug.DrawLine(hitboxpos, entityPhysics.transform.position, Color.cyan, 0.2f);
            foreach (Collider2D obj in hitobjects)
            {
                if ((obj.GetComponent<EntityPhysics>() && obj.tag == "Enemy"))
                {
                    if (obj.GetComponent<EntityPhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && obj.GetComponent<EntityPhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                    {
                        //FollowingCamera.GetComponent<CameraScript>().Jolt(0.2f, aimDirection);
                        FollowingCamera.GetComponent<CameraScript>().Shake(0.3f, 10, 0.01f);

                        Debug.Log("Owch!");
                        obj.GetComponent<EntityPhysics>().Inflict(0.1f, aimDirection.normalized, 1.0f);
                    }
                    
                }
            }
            //------------------------| MOVE
            Vector2 vec = entityPhysics.MoveAvoidEntities(thrustDirection);
            entityPhysics.MoveCharacterPositionPhysics(vec.x * AttackMovementSpeed * 5.0f, vec.y * AttackMovementSpeed * 5.0f);

            #region Draw Player
            //Draw Player
            if (!_readyForThirdHit)
            {
                if (thrustDirection.x <= 0)
                {
                    if (thrustDirection.y >= 0)
                    {
                        characterAnimator.Play("Anim_Swing_Right_NW");
                    }
                    else
                    {
                        characterAnimator.Play("Anim_Swing_Right_SW");
                    }
                }
                else
                {
                    //Swing East
                    if (thrustDirection.y >= 0)
                    {
                        characterAnimator.Play("Anim_Swing_Right_NE");
                    }
                    else
                    {
                        characterAnimator.Play("Anim_Swing_Right_SE");
                    }
                }
            }
            else
            {
                if (thrustDirection.x <= 0)
                {
                    if (thrustDirection.y >= 0)
                    {
                        characterAnimator.Play("Anim_Swing_Left_NW");
                    }
                    else
                    {
                        characterAnimator.Play("Anim_Swing_Left_SW");
                    }
                }
                else
                {
                    //Swing East
                    if (thrustDirection.y >= 0)
                    {
                        characterAnimator.Play("Anim_Swing_Left_NE");
                    }
                    else
                    {
                        characterAnimator.Play("Anim_Swing_Left_SE");
                    }
                }
                #endregion 
            }
        }

        // Old slidey movement
        //entityPhysics.MoveCharacterPositionPhysics(thrustDirection.x * AttackMovementSpeed, thrustDirection.y * AttackMovementSpeed);

        //Button press check for combo chaining
        if (controller.GetButtonDown("Melee"))
        {
            if (StateTimer > _timeToComboReady) //penalize player for hitting button too fast
            {
                _hitComboBeforeReady = true;
            }
            else
            {
                _hasHitAttackAgain = true;
                Debug.Log("Woo!");
            }
        }

        //State Switching

        StateTimer -= Time.deltaTime;

        if (StateTimer < 0)
        {
            LightMeleeSprite.GetComponent<SpriteRenderer>().flipX = false;

            if (_hasHitAttackAgain && _readyForThirdHit && !_hitComboBeforeReady)
            {
               
                CurrentState = PlayerState.HEAVY_MELEE;
                StateTimer = time_heavyMelee;
                hitEnemies.Clear();
                _readyForThirdHit = false;
                _hasHitAttackAgain = false;

                //allow adjusting direction of motion/attack
                UpdateAimDirection();

                LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * lightmelee_hitbox.x / 2.0f, characterSprite.transform.position.y + aimDirection.normalized.y * lightmelee_hitbox.x / 2.0f, characterSprite.transform.position.z + aimDirection.normalized.y), Quaternion.identity);
                LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
                HeavyMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * heavymelee_hitbox.x / 2.0f, characterSprite.transform.position.y + aimDirection.normalized.y * heavymelee_hitbox.x / 2.0f, characterSprite.transform.position.z + aimDirection.normalized.y), Quaternion.identity);
                HeavyMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
            }
            else if (_hasHitAttackAgain && !_hitComboBeforeReady)
            {
                _readyForThirdHit = true;
                CurrentState = PlayerState.LIGHT_MELEE;
                StateTimer = time_lightMelee;
                _hasHitAttackAgain = false;
                //allow adjusting direction of motion/attack
                UpdateAimDirection();

                LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * lightmelee_hitbox.x / 2.0f, characterSprite.transform.position.y + aimDirection.normalized.y * lightmelee_hitbox.x / 2.0f, characterSprite.transform.position.z + aimDirection.normalized.y), Quaternion.identity);
                LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
                HeavyMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * heavymelee_hitbox.x / 2.0f, characterSprite.transform.position.y + aimDirection.normalized.y * heavymelee_hitbox.x / 2.0f, characterSprite.transform.position.z + aimDirection.normalized.y), Quaternion.identity);
                HeavyMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
            }
            else
            {
                LightMeleeSprite.GetComponent<SpriteRenderer>().enabled = false;
                _hasHitAttackAgain = false;
                CurrentState = PlayerState.RUN;
                hitEnemies.Clear();
                _readyForThirdHit = false;
            }
        }
    }

    private void PlayerHeavyMelee()
    {
        if (StateTimer == time_heavyMelee)
        {
            _audioSource.Play();

            StartCoroutine(PlayHeavyAttack(false));

            thrustDirection = aimDirection;

            //Debug.DrawRay(entityPhysics.transform.position, thrustDirection*5.0f, Color.cyan, 0.2f);


            Vector2 hitboxpos = (Vector2)entityPhysics.transform.position + thrustDirection * (heavymelee_hitbox.x / 2.0f);
            Collider2D[] hitobjects = Physics2D.OverlapBoxAll(hitboxpos, heavymelee_hitbox, Vector2.SignedAngle(Vector2.right, thrustDirection));
            Debug.DrawLine(hitboxpos, entityPhysics.transform.position, Color.cyan, 0.2f);
            foreach (Collider2D obj in hitobjects)
            {
                if (obj.GetComponent<EntityPhysics>() && obj.tag == "Enemy")
                {
                    if (obj.GetComponent<EntityPhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && obj.GetComponent<EntityPhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                    {
                        //FollowingCamera.GetComponent<CameraScript>().Jolt(0.2f, aimDirection);
                        FollowingCamera.GetComponent<CameraScript>().Shake(0.5f, 10, 0.01f);

                        Debug.Log("Owch!");
                        obj.GetComponent<EntityPhysics>().Inflict(0.1f, aimDirection.normalized, 3.0f);
                    }
                }
            }

            //Draw Player
            if (thrustDirection.x <= 0)
            {
                if (thrustDirection.y >= 0)
                {
                    characterAnimator.Play("Anim_Swing_Right_NW");
                }
                else
                {
                    characterAnimator.Play("Anim_Swing_Right_SW");
                }
            }
            else
            {
                //Swing East
                if (thrustDirection.y >= 0)
                {
                    characterAnimator.Play("Anim_Swing_Right_NE");
                }
                else
                {
                    characterAnimator.Play("Anim_Swing_Right_SE");
                }
            }

            //Step movement
            entityPhysics.MoveCharacterPositionPhysics(thrustDirection.x * AttackMovementSpeed * 10.0f, thrustDirection.y * AttackMovementSpeed * 10.0f);
        }

        //Move in direction of swipe

        //entityPhysics.MoveCharacterPositionPhysics(thrustDirection.x * AttackMovementSpeed, thrustDirection.y * AttackMovementSpeed);

        StateTimer -= Time.deltaTime;

        if (StateTimer < 0)
        {
            HeavyMeleeSprite.GetComponent<SpriteRenderer>().enabled = false;
            CurrentState = PlayerState.RUN;
            hitEnemies.Clear();
            _readyForThirdHit = false;
        }
    }
    
    #endregion
    
    private void PlayerCharge()
    {
        StateTimer -= Time.deltaTime;
        Debug.Log(StateTimer);
        if (StateTimer < 0)
        {
            characterAnimator.Play("New_ChargeFinal");
        }
        else if (StateTimer < time_Charge - time_ChargeLight - time_ChargeMedium) //play transition
        {
            characterAnimator.Play("New_ChargeTransition");
        }
        else if (StateTimer < time_Charge - time_ChargeLight) //play medium
        {
            characterAnimator.Play("New_ChargeMedium");
        }
        else //play small
        {
            characterAnimator.Play("New_ChargeSmall");
        }



        Debug.Log("Charging...!!!");


        //State Switching
        if ( !controller.GetButton("Charge") )
        {
            CurrentState = PlayerState.IDLE;
        }
        else if (StateTimer < 0.1 && controller.GetButtonDown("Melee"))
        {
            StateTimer = _burstDuration;
            CurrentState = PlayerState.BURST;
        }
    }
    
    
    private void PlayerBurst()
    {
        characterAnimator.Play("New_Unleash");
        if (StateTimer < time_burstHit && !_hasFlashed)
        {
            _hasFlashed = true;
            Vector2 cornerSouthWest = entityPhysics.transform.position;

            Collider2D[] hitEntities = Physics2D.OverlapAreaAll((Vector2)entityPhysics.transform.position - _burstArea/2.0f, (Vector2)entityPhysics.transform.position + _burstArea / 2.0f);
            for (int i = 0; i < hitEntities.Length; i++)
            {
                
                if (hitEntities[i].GetComponent<EntityPhysics>() && hitEntities[i].tag == "Enemy")
                {
                    //FollowingCamera.GetComponent<CameraScript>().Jolt(0.2f, aimDirection);
                    FollowingCamera.GetComponent<CameraScript>().Shake(1.0f, 10, 0.01f);

                    //Debug.Log("Owch!");
                    Vector2 displacementOfEnemy = hitEntities[i].transform.position - entityPhysics.transform.position;
                    displacementOfEnemy = (displacementOfEnemy.normalized * 10.0f) / (displacementOfEnemy.magnitude + 1);
                    hitEntities[i].GetComponent<EntityPhysics>().Inflict(1f, displacementOfEnemy, displacementOfEnemy.magnitude);
                }
            }
        }

        if (StateTimer < time_burstHit)
        {

        }

        //tick
        StateTimer -= Time.deltaTime;
        if (StateTimer <= 0)
        {
            CurrentState = PlayerState.RUN;
            _hasFlashed = false;
        }
    }
    
    //================================================================================| FIRE BULLETS

    /// <summary>
    /// Fires a bullet
    /// </summary>
    private void FireBullet()
    {
        Vector2 _tempRightAnalogDirection = aimDirection;
        /*
        Vector2 _tempRightAnalogDirection = Vector2.zero;
        if (_inputHandler.RightAnalog.magnitude <= 0.2)
        {
            //_tempRightAnalogDirection = _tempRightAnalogDirection.normalized;
            if (_inputHandler.LeftAnalog.magnitude >= 0.2)
            {
                _tempRightAnalogDirection = _inputHandler.LeftAnalog;
            }
            else
            {
                _tempRightAnalogDirection = _tempRightAnalogDirection.normalized * 0.2f;
            }
        }
        else
        {
            _tempRightAnalogDirection = _inputHandler.RightAnalog;
        }
        */

        GameObject tempBullet = _equippedWeapon.FireBullet(_tempRightAnalogDirection);
        //tempBullet.GetComponentInChildren<EntityPhysics>().NavManager = entityPhysics.NavManager;
        tempBullet.GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(entityPhysics.GetObjectElevation() + 2.0f);
        tempBullet.GetComponentInChildren<ProjectilePhysics>().GetComponent<Rigidbody2D>().position = (entityPhysics.GetComponent<Rigidbody2D>().position);
    }
    
    /// <summary>
    /// Swaps weapon with one from your inventory given a d-pad direction
    /// </summary>
    /// <param name="cardinal"></param>
    private void SwapWeapon(string cardinal)
    {
        Weapon temp = inventory.GetWeapon(cardinal);
        if (temp) //not null
        {
            Debug.Log("Equipping " + temp);
            _equippedWeapon = inventory.GetWeapon(cardinal);
            _equippedWeapon.PopulateBulletPool();
        }
    }
    /*
    /// <summary>
    /// Throws a grenade in the direction of aim.
    /// </summary>
    private void ThrowGrenade()
    {
        GameObject tempBullet = Instantiate(Resources.Load("Prefabs/Bullets/TestGrenade")) as GameObject;
        tempBullet.GetComponentInChildren<GrenadeHandler>().MoveDirection = Vector2.right;
        tempBullet.SetActive(true);
        tempBullet.GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(entityPhysics.GetObjectElevation());
        tempBullet.GetComponentInChildren<ProjectilePhysics>().GetComponent<Transform>().position = (entityPhysics.GetComponent<Rigidbody2D>().position);
    }

    

    */

    public override void SetXYAnalogInput(float x, float y)
    {
        //xInput = x;
        //yInput = y;
    }

    public override void JustGotHit()
    {
        Debug.Log("Player: Ow!");
        FollowingCamera.GetComponent<CameraScript>().Shake(1f, 6, 0.01f);
        _healthBar.UpdateBar((int)entityPhysics.GetCurrentHealth());
    }


    override protected void OnDeath()
    {
        Debug.Log("<color=pink>HEY!</color>");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Misc Animations
    IEnumerator PlayLightAttack(bool flip)
    {
        LightMeleeSprite.GetComponent<SpriteRenderer>().enabled = true;
        LightMeleeSprite.GetComponent<SpriteRenderer>().flipX = flip;
        LightMeleeSprite.GetComponent<Animator>().PlayInFixedTime(0);

        //LightMeleeSprite.GetComponent<Animator>().Play("LightMeleeSwing");
        yield return new WaitForSeconds(_lengthOfLightMeleeAnimation);
        LightMeleeSprite.GetComponent<SpriteRenderer>().enabled = false;
    }

    IEnumerator PlayHeavyAttack(bool flip)
    {
        Debug.Log("POW!");
        HeavyMeleeSprite.GetComponent<SpriteRenderer>().enabled = true;
        HeavyMeleeSprite.GetComponent<SpriteRenderer>().flipX = flip;
        HeavyMeleeSprite.GetComponent<Animator>().PlayInFixedTime(0);

        //HeavyMeleeSprite.GetComponent<Animator>().Play("HeavyMeleeSwing!!!!!");
        yield return new WaitForSeconds(_lengthOfHeavyMeleeAnimation);
        HeavyMeleeSprite.GetComponent<SpriteRenderer>().enabled = false;
    }

    /// <summary>
    /// Handles aim direction, toggles between using mouse & keyboard and gamepad
    /// </summary>
    private void UpdateAimDirection()
    {
        if (_isUsingCursor)
        {
            //do cursor input
            aimDirection = new Vector2(_cursorWorldPos.x, _cursorWorldPos.y - entityPhysics.GetBottomHeight() - 1f) - (Vector2)entityPhysics.GetComponent<Transform>().position;
            aimDirection.Normalize();
        }
        else
        {
            if (controller.GetAxis2DRaw("LookHorizontal", "LookVertical").magnitude >= 0.1f)
            {
                //Debug.Log("Changing aim!");
                aimDirection = controller.GetAxis2DRaw("LookHorizontal", "LookVertical");
            }
            else if (controller.GetAxis2DRaw("MoveHorizontal", "MoveVertical").magnitude >= 0.1f)
            {
                aimDirection = controller.GetAxis2DRaw("MoveHorizontal", "MoveVertical");
                //Debug.Log("Changing aim!");
            }
        }
       

    }

    public void UpdateMousePosition(Vector2 mpos)
    {
        _isUsingCursor = true;
        _cursorWorldPos = mpos;
    }



    // I think I changed my mind on having this in the player class, I kinda want it in the Reticle's code to avoid clutter here
    /*
    /// <summary>
    /// Updates the targeting reticle's position based on player input and environment 
    /// 
    ///
    /// </summary>
    private void UpdateReticle()
    {
        Vector2 reticlevector = _inputHandler.RightAnalog;

        if (reticlevector.magnitude == 0)
        {
            reticlevector = _inputHandler.LeftAnalog;
            if (reticlevector.magnitude == 0 )
            {
                //idk what to do here, maybe a private field just for the cases where this happens?
            }
        }
        RaycastHit2D[] impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, this.GetComponent<BoxCollider2D>().size, 0f, new Vector2(velocityX, velocityY), distance: boxCastDistance);

    }
    */
}