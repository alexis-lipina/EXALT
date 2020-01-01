using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Rewired;

public enum ElementType
{
    ZAP, FIRE, VOID, NONE
}


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
    [SerializeField] private AudioSource _blinkAudioSource;
    [SerializeField] private CursorHandler _cursor;
    [SerializeField] private bool _isUsingCursor; //TEMPORARY
    [SerializeField] private float _projectileStartHeight;
    //[SerializeField] private GameObject _lightRangedZapLine;
    [SerializeField] private ZapFXController _lightRangedZap;
    [SerializeField] private ZapFXController _chargedRangedZap;
    [SerializeField] private Transform _zapNodePrefab;
    [SerializeField] private PlayerProjection _chargedMeleeProjection;
    [SerializeField] private int _maxEnergy;
    [SerializeField] private DeathFlash _deathFlash;
    [SerializeField] private FadeTransition _fadeTransition;
    [SerializeField] private RectTransform _gameplayUI;
    private int _currentEnergy;

    public int MaxEnergy { get { return _maxEnergy; } }
    public int CurrentEnergy { get { return _currentEnergy; } }

    public static string PREVIOUS_SCENE = "";

    private Animator characterAnimator;
    private PlayerInventory inventory;


    //[SerializeField] private UIHealthBar _healthBar;
    [SerializeField] private PlayerHealthBarHandler _healthBar;
    
    enum PlayerState {IDLE, RUN, JUMP, LIGHT_MELEE, HEAVY_MELEE, CHARGE, BURST, LIGHT_RANGED, CHARGED_RANGE, BLINK, CHANGE_STYLE, HEAL, REST, DEAD};

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
    const string STYLE_CHANGE_Anim = "Change_Attunement";
    const string DEATH_WEST_Anim = "PlayerDeath_West";
    const string HEAL_ANIM = "Change_Attunement"; //TODO : have a different animation for this my dude

    


    private const float AttackMovementSpeed = 0.6f;

    private Weapon _equippedWeapon;

    private PlayerState CurrentState;
    private PlayerState PreviousState;

    private ElementType _currentStyle;
    private ElementType _newStyle;

    private FaceDirection currentFaceDirection;
    private FaceDirection previousFaceDirection;
    
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
    private const float LIGHTMELEE_FORCE = 1.0f;

    // heavy melee
    private const float time_heavyMelee = 0.3f;
    private Vector2 heavymelee_hitbox = new Vector2(8, 4);
    private float _lengthOfHeavyMeleeAnimation;
    private const float HEAVYMELEE_VOID_FORCE = 5.0f;
    private const float HEAVYMELEE_ZAP_FORCE = 2.0f;
    private const float HEAVYMELEE_FIRE_FORCE = 2.0f;

    //Charge Stuff
    private const float time_Charge = 1.45f; //total time before player is charged up
    private const float time_ChargeLight = .5f;
    private const float time_ChargeMedium = .5f;
    private const float time_ChargeTransition = 0.25f; //how long is the entire transition animation, anyway?

    //Burst Stuff
    private float _burstDuration = .75f;
    private const float time_burstHit = 0.4f;
    private Vector2 _burstArea = new Vector2(16f, 12f);
    private bool _hasFlashed = false;

    //Light Ranged 
    private const float _lightRangedDuration = 0.25f;
    private const int _lightRangedEnergyCost = 2;

    //Light Ranged Zap Attack
    private const float _lightRangedZapMaxDistance = 30f;
    private const float _chargedRangedZapMaxDistance = 50f;

    // Change Style 
    private const float _changeStyleDuration = 0.95f;
    private const float _changeStyleColorChangeTime = 0.4f;
    private bool _changeStyle_HasChanged = false;

    // Taking damage
    private Vector2 _lastHitDirection = Vector2.right;


    private Vector2 _cursorWorldPos;

    private float PlayerRunSpeed;
    private float xInput; 
    private float yInput;   
    private float StateTimer;
    private bool isFlipped;
    private List<int> hitEnemies;

    private float _lerpedPlayerHeight;

    private Player controller;

    //=====================| BLINK
    private const float BLINK_TIME_PUNISH = 0.4f;
    private const float BLINK_TIME_PRO = 0.2f;
    private float _blinkTimer = 0f; //time since last blink
    private bool _blink_hasButtonMashed = false;
    private bool _hasAlreadyBlinkedInMidAir = false;

    //=====================| HEAL
    private const float heal_duration = 0.95f;
    private const int heal_cost = 4;
    private bool hasHealed = false;

    //======================| REST
    private const float rest_kneel_duration = 0.51f;
    private bool isStanding = false;
    private const float _rest_recover_health_duration = 1.0f;
    private float _rest_recover_health_timer = 0f;
    private const float _rest_recover_energy_duration = 0.33f;
    private float _rest_recover_energy_timer = 0f;


    //=====================| JUMP/FALL FIELDS
    [SerializeField] private float JumpImpulse = 80f;
    [SerializeField] private float JumpHeldGravity;
    [SerializeField] private float JumpFallGravity;
    [SerializeField] private float FallGravity;
    private bool _jump_hasStartedFalling = false;


    public bool IsUsingMouse
    {
        get { return _isUsingCursor; }
        set { _isUsingCursor = value; }
    }

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        if (PREVIOUS_SCENE == "")
        {
            PREVIOUS_SCENE = SceneManager.GetActiveScene().name;
        }
        _currentEnergy = _maxEnergy;
        controller = ReInput.players.GetPlayer(0);
        //this.entityPhysics.GetComponent<Rigidbody2D>().MovePosition(TemporaryPersistentDataScript.getDestinationPosition());
        inventory = gameObject.GetComponent<PlayerInventory>();
        
    }

    void Start()
    {
        EnvironmentPhysics._playerPhysics = entityPhysics;
        EnvironmentPhysics._playerSprite = characterSprite;
        aimDirection = Vector2.right;
        //SwapWeapon("NORTH"); //Debug
        //Debug.Log(_equippedWeapon);


        if (PREVIOUS_SCENE == SceneManager.GetActiveScene().name) { CurrentState = PlayerState.REST; }
        else { CurrentState = PlayerState.IDLE; }
        StateTimer = 0;
        //playerRigidBody = PhysicsObject.GetComponent<Rigidbody2D>();
        
        //TerrainTouched.Add(666, new KeyValuePair<float, float>(0.0f, -20.0f));
        characterAnimator = characterSprite.GetComponent<Animator>();
        hasSwung = false;
        hitEnemies = new List<int>();
        _lengthOfLightMeleeAnimation = LightMeleeSprite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        _lengthOfHeavyMeleeAnimation = HeavyMeleeSprite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;

        //SwapWeapon("NORTH");
        _currentStyle = ElementType.ZAP;
        //characterSprite.GetComponent<SpriteRenderer>().material.SetColor("_MagicColor", new Color(0.3f, 1f, 0.7f, 1f));
        Shader.SetGlobalColor("_MagicColor",  new Color(0.3f, 1f, 0.7f, 1f));

    }


    void Update ()
    {
        if (Time.timeScale == 0) return;

        //increment timers
        _blinkTimer += Time.deltaTime;
        
        xInput = controller.GetAxisRaw("MoveHorizontal");
        yInput = controller.GetAxisRaw("MoveVertical");
        if (entityPhysics.GetCurrentHealth() < 1 && CurrentState != PlayerState.DEAD)
        {
            CurrentState = PlayerState.DEAD;
            entityPhysics.IsDead = true;
            OnDeath();
        }
        //---------------------------| Manage State Machine |
        this.ExecuteState();

        //reset button presses
        PreviousState = CurrentState;
        //FollowingCamera.transform.position = new Vector3(playerCharacterSprite.transform.position.x, playerCharacterSprite.transform.position.y, -100);
        //CheckStyleChange();

        _lerpedPlayerHeight = Mathf.Lerp(_lerpedPlayerHeight, entityPhysics.GetObjectElevation(), 0.1f);

        //_environmentFrontShader.SetGlobalFloat("_PlayerElevation", _lerpedPlayerHeight);
        //_environmentTopShader.SetFloat("_PlayerElevation", _lerpedPlayerHeight);
        Shader.SetGlobalFloat("_PlayerElevation", _lerpedPlayerHeight);
        //Change fighting style

        UpdateAimDirection();
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
            case (PlayerState.LIGHT_RANGED):
                PlayerLightRanged();
                break;
            case (PlayerState.CHARGED_RANGE):
                PlayerChargedRanged();
                break;
            case (PlayerState.BLINK):
                PlayerBlink();
                break;
            case (PlayerState.CHANGE_STYLE):
                PlayerChangeStyle();
                break;
            case (PlayerState.HEAL):
                PlayerHeal();
                break;
            case (PlayerState.REST):
                PlayerRest();
                break;
            case (PlayerState.DEAD):
                break;
            default:
                throw new Exception("Unhandled player state");
        }
    }
    
    /// <summary>
    /// checks to see if the player's fighting style has changed
    /// </summary>
    private void CheckStyleChange()
    {
        if (controller.GetButton("ChangeStyle_Fire") && _currentStyle != ElementType.FIRE)
        {
            //Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.5f, 0f, 1f));
            //ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(1f, 0.5f, 0f));
            CurrentState = PlayerState.CHANGE_STYLE;
            StateTimer = _changeStyleDuration;
            _newStyle = ElementType.FIRE;
        }
        else if (controller.GetButton("ChangeStyle_Void") && _currentStyle != ElementType.VOID)
        {
            //Shader.SetGlobalColor("_MagicColor", new Color(0.5f, 0f, 1f, 1f));
            //ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(0.5f, 0f, 1f));
            CurrentState = PlayerState.CHANGE_STYLE;
            StateTimer = _changeStyleDuration;
            _newStyle = ElementType.VOID;
        }
        else if (controller.GetButton("ChangeStyle_Zap") && _currentStyle != ElementType.ZAP)
        {
            //Shader.SetGlobalColor("_MagicColor", new Color(0f, 1f, 0.5f, 1f));
            //ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(0f, 1f, 0.5f));
            CurrentState = PlayerState.CHANGE_STYLE;
            StateTimer = _changeStyleDuration;
            _newStyle = ElementType.ZAP;
        }

    }

    private void FlipCharacterSprite()
    {
        Vector3 theScale = characterSprite.transform.localScale;
        theScale.x *= -1;
        characterSprite.transform.localScale = theScale;
    }

    //================================================================================| STATE METHODS 

    //----------------------------------------------| Movement
    #region Movement State Methods

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
        if (controller.GetButton("Rest"))
        {
            StateTimer = rest_kneel_duration;
            CurrentState = PlayerState.REST;
        }
        if (Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2) 
        {
            //Debug.Log("IDLE -> RUN");
            CurrentState = PlayerState.RUN;
        }
        if (controller.GetButtonDown("Jump"))
        {
            //Debug.Log("IDLE -> JUMP");
            Vibrate( .5f, 0.05f);
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

        if (controller.GetButton("RangedAttack"))
        {
            PlayerLightRangedTransitionAttempt();
        }
        if (controller.GetButtonDown("Blink"))
        {
            PlayerBlinkTransitionAttempt();
        }
        CheckStyleChange();
        if (controller.GetButtonDown("Heal"))
        {
            PlayerHealTransitionAttempt();
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
            Vibrate( .5f, 0.05f);
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
        if (controller.GetButton("RangedAttack"))
        {
            PlayerLightRangedTransitionAttempt();
        }
        if (controller.GetButtonDown("Blink"))
        {
            PlayerBlinkTransitionAttempt();
        }
        if (controller.GetButtonDown("Heal"))
        {
            PlayerHealTransitionAttempt();
        }
        CheckStyleChange();

        if (CurrentState == PlayerState.RUN)
        {
            entityPhysics.SavePosition();
        }

    }

    private void PlayerJump()
    {
        //Debug.Log("Player Jumping");
        //Facing Determination
        previousFaceDirection = currentFaceDirection;
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
        float threshold = 2f;
        switch (currentFaceDirection)
        {
            case FaceDirection.EAST:
                if (entityPhysics.ZVelocity > threshold)
                {
                    characterAnimator.Play("PlayerJumpEast");
                }
                else if (entityPhysics.ZVelocity <= threshold && !_jump_hasStartedFalling && previousFaceDirection == currentFaceDirection)
                {
                    characterAnimator.Play("PlayerHoverEast");
                    _jump_hasStartedFalling = true;
                }
                else if (entityPhysics.ZVelocity < threshold && previousFaceDirection != currentFaceDirection)
                {
                    characterAnimator.Play("PlayerFallEast");
                }
                break;
            case FaceDirection.WEST:
                if (entityPhysics.ZVelocity > threshold)
                {
                    characterAnimator.Play("PlayerJumpWest");
                }
                else if (entityPhysics.ZVelocity <= threshold && !_jump_hasStartedFalling && previousFaceDirection == currentFaceDirection)
                {
                    characterAnimator.Play("PlayerHoverWest");
                    _jump_hasStartedFalling = true;
                }
                else if (entityPhysics.ZVelocity < threshold && previousFaceDirection != currentFaceDirection)
                {
                    characterAnimator.Play("PlayerFallWest");
                }
                break;
            case FaceDirection.NORTH:
                if (entityPhysics.ZVelocity > threshold)
                {
                    characterAnimator.Play("PlayerJumpNorth");
                }
                else if (entityPhysics.ZVelocity <= threshold && !_jump_hasStartedFalling && previousFaceDirection == currentFaceDirection)
                {
                    characterAnimator.Play("PlayerHoverNorth");
                    _jump_hasStartedFalling = true;
                }
                else if (entityPhysics.ZVelocity < threshold && previousFaceDirection != currentFaceDirection)
                {
                    characterAnimator.Play("PlayerFallNorth");
                }
                break;
            case FaceDirection.SOUTH:
                if (entityPhysics.ZVelocity > threshold)
                {
                    characterAnimator.Play("PlayerJumpSouth");
                }
                else if (entityPhysics.ZVelocity <= threshold && !_jump_hasStartedFalling && previousFaceDirection == currentFaceDirection)
                {
                    characterAnimator.Play("PlayerHoverSouth");
                    _jump_hasStartedFalling = true;
                }
                else if (entityPhysics.ZVelocity < threshold && previousFaceDirection != currentFaceDirection)
                {
                    characterAnimator.Play("PlayerFallSouth");
                }
                break;
        }


        /*
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
        }*/

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

        if (controller.GetButtonDown("Blink") && !_hasAlreadyBlinkedInMidAir)
        {
            PlayerBlinkTransitionAttempt();
            _jump_hasStartedFalling = false;
        }

        if (entityPhysics.GetObjectElevation() <= maxheight)
        {
            float vibrationMagnitude = Mathf.Abs(entityPhysics.ZVelocity) / DynamicPhysics.MAX_Z_VELOCITY_MAGNITUDE;
            Vibrate(vibrationMagnitude, 0.1f * vibrationMagnitude);
            entityPhysics.SetObjectElevation(maxheight);
            _hasAlreadyBlinkedInMidAir = false;
            _jump_hasStartedFalling = false;
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

    private void PlayerBlink()
    {
        TeleportVFX.DeployEffectFromPool(characterSprite.transform.position);
        FollowingCamera.GetComponent<CameraScript>().Jolt(1f, aimDirection);
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.5f, 0.1f);
        //Vibrate( .5f, 0.05f);
        Vibrate(.5f, 0.05f);

        //apply effect to any entities caught within player's path
        RaycastHit2D[] hits = Physics2D.BoxCastAll(entityPhysics.transform.position, entityPhysics.GetComponent<BoxCollider2D>().size, 0.0f, aimDirection, aimDirection.magnitude * 7f);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.tag == "Enemy" && hit.collider.GetComponent<EntityPhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && hit.collider.GetComponent<EntityPhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
            {
                hit.transform.GetComponent<EntityPhysics>().Handler.PrimeEnemy(_currentStyle);
                Vibrate( 1f, 0.1f);
            }
        }

        _blinkAudioSource.Play();
        //Debug.Log(aimDirection);

        entityPhysics.MoveWithCollision(aimDirection.x * 7f, aimDirection.y * 7f); //buggy, occasionally player teleports a much shorter distance than they should
        //entityPhysics.GetComponent<Rigidbody2D>().position = entityPhysics.GetComponent<Rigidbody2D>().position + aimDirection * 7f;


        //setup timer
        _blinkTimer = 0f;
        _blink_hasButtonMashed = false;

        

        //TeleportVFX.DeployEffectFromPool(characterSprite.transform.position);
        CurrentState = PlayerState.JUMP;
    }

    

    #endregion

    //---------------------------------------------| Melee
    #region Melee Attack State Methods
    /// <summary>
    /// This is the player's basic close-range attack. Can be chained for a swipe-swipe-jab combo, and supports aiming in any direction
    /// </summary>
    private void PlayerLightMelee()
    {
        //snappy directional change (if this aint here it sometimes doesnt work)
        
        //Functionality to be done at very beginning
        if (StateTimer == time_lightMelee)
        {
            //Play SFX
            _audioSource.Play();
            Vibrate( 0.5f, 0.1f);

            _hitComboBeforeReady = false;
            StartCoroutine(PlayLightAttack(_readyForThirdHit));

            thrustDirection = aimDirection;

            //Debug.DrawRay(entityPhysics.transform.position, thrustDirection*5.0f, Color.cyan, 0.2f);


            Vector2 hitboxpos = (Vector2)entityPhysics.transform.position + thrustDirection * (lightmelee_hitbox.x / 2.0f);
            Collider2D[] hitobjects = Physics2D.OverlapBoxAll(hitboxpos, lightmelee_hitbox, Vector2.SignedAngle(Vector2.right, thrustDirection));
            Debug.DrawLine(hitboxpos, entityPhysics.transform.position, Color.cyan, 0.2f);
            foreach (Collider2D obj in hitobjects)
            {
                if (obj.GetComponent<EntityPhysics>() && obj.tag == "Enemy")
                {
                    if (obj.GetComponent<EntityPhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && obj.GetComponent<EntityPhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                    {
                        //FollowingCamera.GetComponent<CameraScript>().Jolt(0.2f, aimDirection);
                        FollowingCamera.GetComponent<CameraScript>().Shake(0.3f, 10, 0.01f);
                        Vibrate( 1.0f, 0.15f);

                        //Debug.Log("Owch!");
                        obj.GetComponent<EntityPhysics>().Inflict(1, force: aimDirection.normalized * 1f);
                        ChangeEnergy(1);
                    }
                }
                else if (obj.GetComponent<ProjectilePhysics>())
                {
                    if (obj.GetComponent<ProjectilePhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && obj.GetComponent<ProjectilePhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                    {
                        Vibrate(1.0f, 0.15f);
                        obj.GetComponent<ProjectilePhysics>().PlayerRedirect(aimDirection, "ENEMY", 60f);
                        FollowingCamera.GetComponent<CameraScript>().Jolt(2f, aimDirection * -1f);
                        FollowingCamera.GetComponent<CameraScript>().Shake(0.2f, 10, 0.02f);
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
                //Debug.Log("Woo!");
            }
        }

        //State Switching

        StateTimer -= Time.deltaTime;

        //allow "animation-cancel" teleport after a certain time
        if (StateTimer < 0.1 && controller.GetButtonDown("Blink"))
        {
            PlayerBlinkTransitionAttempt();
        }


        if (StateTimer < 0) //regular transitions at end of animation
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
    /// <summary>
    /// Combo attack
    /// </summary>
    private void PlayerHeavyMelee()
    {
        if (StateTimer == time_heavyMelee)
        {
            _audioSource.Play();
            Vibrate( 0.8f, 0.1f);
            //ChangeEnergy(1);
            StartCoroutine(PlayHeavyAttack(false));

            //Debug.DrawRay(entityPhysics.transform.position, thrustDirection*5.0f, Color.cyan, 0.2f);
            UpdateAimDirection();
            thrustDirection = aimDirection;

            switch (_currentStyle)
            {
                case ElementType.FIRE:
                    PlayerHeavyMelee_Fire();
                    break;
                case ElementType.VOID:
                    PlayerHeavyMelee_Void();
                    break;
                case ElementType.ZAP:
                    PlayerHeavyMelee_Zap();
                    break;
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

        //allow "animation-cancel" teleport after a certain time
        if (StateTimer < 0.1 && controller.GetButtonDown("Blink"))
        {
            PlayerBlinkTransitionAttempt();
        }

        if (StateTimer < 0)
        {
            HeavyMeleeSprite.GetComponent<SpriteRenderer>().enabled = false;
            CurrentState = PlayerState.RUN;
            hitEnemies.Clear();
            _readyForThirdHit = false;
        }
    }

    private void PlayerHeavyMelee_Zap()
    {
        
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
                    Vibrate( 1.0f, 0.3f);

                    //TEST CODE HERE
                    GameObject node = LightningChainNode.GetNode();
                    node.GetComponent<LightningChainNode>()._sourcePosition = entityPhysics.GetComponent<Rigidbody2D>().position + entityPhysics.GetBottomHeight() * Vector2.up;
                    node.GetComponent<Transform>().position = new Vector3(obj.GetComponent<Rigidbody2D>().position.x, obj.GetComponent<Rigidbody2D>().position.y, obj.GetComponent<Rigidbody2D>().position.y);
                    node.GetComponent<LightningChainNode>()._myEnemy = obj.GetComponent<EntityPhysics>();
                    var wavefront = new ChainZapWavefront();
                    wavefront.AlreadyHit = new List<int>();
                    wavefront.AlreadyHit.Add(obj.GetInstanceID());
                    node.GetComponent<LightningChainNode>()._wavefront = wavefront;
                    node.GetComponent<LightningChainNode>().Run();
                    ChangeEnergy(1);
                    Debug.Log("Owch!");
                    obj.GetComponent<EntityPhysics>().Inflict(1, force:aimDirection.normalized*2.0f, type:ElementType.ZAP); 
                }
            }
            else if (obj.GetComponent<ProjectilePhysics>())
            {
                if (obj.GetComponent<ProjectilePhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && obj.GetComponent<ProjectilePhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                {
                    Vibrate(1.0f, 0.15f);
                    obj.GetComponent<ProjectilePhysics>().PlayerRedirect(aimDirection, "ENEMY", 60f);
                    FollowingCamera.GetComponent<CameraScript>().Jolt(2f, aimDirection * -1f);
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.2f, 10, 0.02f);
                }
            }
        }
    }

    private void PlayerHeavyMelee_Fire()
    {
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
                    Vibrate( 1.0f, 0.3f);

                    ChangeEnergy(1);
                    Debug.Log("Owch!");
                    obj.GetComponent<EntityPhysics>().Inflict(1, force:aimDirection.normalized*2.0f, type:ElementType.FIRE);
                    obj.GetComponent<EntityPhysics>().Burn();
                }
            }
            else if (obj.GetComponent<ProjectilePhysics>())
            {
                if (obj.GetComponent<ProjectilePhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && obj.GetComponent<ProjectilePhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                {
                    Vibrate(1.0f, 0.15f);
                    obj.GetComponent<ProjectilePhysics>().PlayerRedirect(aimDirection, "ENEMY", 60f);
                    FollowingCamera.GetComponent<CameraScript>().Jolt(2f, aimDirection * -1f);
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.2f, 10, 0.02f);
                }
            }
        }
    }

    private void PlayerHeavyMelee_Void()
    {
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
                    Vibrate( 1.0f, 0.3f);

                    ChangeEnergy(1);
                    Debug.Log("Owch!");
                    obj.GetComponent<EntityPhysics>().Inflict(1, force:aimDirection.normalized * 3.0f, type:ElementType.VOID);
                }
            }
            else if (obj.GetComponent<ProjectilePhysics>())
            {
                if (obj.GetComponent<ProjectilePhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && obj.GetComponent<ProjectilePhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                {
                    Vibrate(1.0f, 0.15f);
                    obj.GetComponent<ProjectilePhysics>().PlayerRedirect(aimDirection, "ENEMY", 60f);
                    FollowingCamera.GetComponent<CameraScript>().Jolt(2f, aimDirection * -1f);
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.2f, 10, 0.02f);
                }
            }
        }
    }


    private void PlayerCharge()
    {
        StateTimer -= Time.deltaTime;
        //Debug.Log(StateTimer);
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

        //projection
        _chargedMeleeProjection.SetOpacity((time_Charge - StateTimer) / time_Charge);

        UpdateAimDirection();

        Debug.Log("Charging...!!!");


        //State Switching
        if ( !controller.GetButton("Charge") )
        {
            _chargedMeleeProjection.SetOpacity(0);
            CurrentState = PlayerState.IDLE;
        }
        else if (StateTimer < 0.1 && controller.GetButtonDown("Melee"))
        {
            _chargedMeleeProjection.SetOpacity(0);
            StateTimer = _burstDuration;
            CurrentState = PlayerState.BURST;

        }
        else if (StateTimer < 0.1 && controller.GetButtonDown("RangedAttack"))
        {
            StateTimer = _burstDuration;
            _chargedMeleeProjection.SetOpacity(0);
            CurrentState = PlayerState.CHARGED_RANGE;
        }
    }
    
    /// <summary>
    /// Charged melee attack
    /// </summary>
    private void PlayerBurst()
    {
        characterAnimator.Play("New_Unleash");
        if (StateTimer < time_burstHit && !_hasFlashed)
        {
            _hasFlashed = true;
            Vector2 cornerSouthWest = entityPhysics.transform.position;
            ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.5f, 0.1f);
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
                    hitEntities[i].GetComponent<EntityPhysics>().Inflict(1, force: displacementOfEnemy, type:_currentStyle);
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
    #endregion

    //---------------------------------------------| Ranged
    #region Ranged Attack State Methods

    /// <summary>
    /// Charged Ranged Attack
    /// </summary>
    private void PlayerChargedRanged()
    {
        //TODO : Draw Player
        characterAnimator.Play("Anim_Swing_Right_NW");

        if (StateTimer == _burstDuration)
        {
            switch (_currentStyle)
            {
                case ElementType.FIRE:
                    ChargedRanged_Fire();
                    break;
                case ElementType.VOID:
                    ChargedRanged_Void();
                    break;
                case ElementType.ZAP:
                    ChargedRanged_Zap();
                    //Debug.Log("ZAP");
                    break;
                default: break;
            }
        }

        StateTimer -= Time.deltaTime; //tick

        //state switching
        if (StateTimer <= 0)
        {
            CurrentState = PlayerState.IDLE;
        }
    }

    private void PlayerLightRanged()
    {
        //TODO : Draw Player
        if (aimDirection.x <= 0)
        {
            if (aimDirection.y >= 0)
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
            if (aimDirection.y >= 0)
            {
                characterAnimator.Play("Anim_Swing_Right_NE");
            }
            else
            {
                characterAnimator.Play("Anim_Swing_Right_SE");
            }
        }

        if (StateTimer == 0)
        {
            Vibrate(1f, .1f);
            switch (_currentStyle)
            {
                case ElementType.FIRE:
                    LightRanged_Fire();
                    break;
                case ElementType.VOID:
                    LightRanged_Void();
                    break;
                case ElementType.ZAP:
                    LightRanged_Zap();
                    //Debug.Log("ZAP");
                    break;
                default: break;
            }
        }
        /*
        if (controller.GetButton("RangedAttack"))
        {
            if (_equippedWeapon.CanFireBullet())
            {
                FireBullet();
            }
        }
        */

        StateTimer += Time.deltaTime; //tick

        //state switching
        if (StateTimer >= _lightRangedDuration)
        {
            CurrentState = PlayerState.IDLE;
        }
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
            //Debug.Log("Equipping " + temp);
            _equippedWeapon = inventory.GetWeapon(cardinal);
            _equippedWeapon.PopulateBulletPool();
        }
    }


    private void LightRanged_Void()
    {
        //will rework the ranged attack system and remove "weapons" eventually...
        SwapWeapon("NORTH"); //void is north
        FireBullet();
    }

    private void LightRanged_Fire()
    {
        SwapWeapon("WEST");
        FireBullet();
    }

    private void LightRanged_Zap()
    {
        //raycast in direction, to max distance (Might want it to be a circlecast or boxcast, to be more generous with enemy hit detection)
        //if hits an entity or environment object at the height it's cast at, endpoint there
        //else, endpoint at max distance
        //draw linerenderer between players position + offset and endpoint
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(.4f, .1f);
        //RaycastHit2D[] hits = Physics2D.RaycastAll(entityPhysics.GetComponent<Rigidbody2D>().position, aimDirection, _lightRangedZapMaxDistance); //old linear raycast 
        RaycastHit2D[] hits = Physics2D.CircleCastAll(entityPhysics.GetComponent<Rigidbody2D>().position, radius:3f, aimDirection, _lightRangedZapMaxDistance);
        float projectileElevation = entityPhysics.GetBottomHeight() + _projectileStartHeight;
        float shortestDistance = float.MaxValue;
        Vector2 endPoint = Vector2.zero;
        bool hasHitEntity = false;
        EntityPhysics hitEntity = null; 
        for (int i = 0; i < hits.Length; i++)
        {
            //check if is something that can be collided with
            //check to see if projectile z is within z bounds of physicsobject
            //check to see if distance to target is shorter than shortestDistance

            if (hits[i].collider.GetComponent<PhysicsObject>()) //can object be collided with
            {
                PhysicsObject other = hits[i].collider.GetComponent<PhysicsObject>();
                if (other.GetBottomHeight() < projectileElevation && other.GetTopHeight() > projectileElevation) //is projectile height within z bounds of object
                {
                    if (/*other.tag != "Environment" && */ other.tag != "Enemy") continue; //changed to reflect new circlecast
                    if (hits[i].distance < shortestDistance)
                    {
                        //Debug.Log("Hit");
                        //set to current valid collision
                        shortestDistance = hits[i].distance;
                        endPoint = hits[i].point;
                        if (other is EntityPhysics) //checks to see if entity, since entities need to be damaged
                        {
                            //check for collisions with environment along path
                            Vector2 playerPosition = entityPhysics.GetComponent<Rigidbody2D>().position;
                            RaycastHit2D[] environmentHits = Physics2D.RaycastAll( playerPosition, endPoint - playerPosition, (endPoint - playerPosition).magnitude );
                            bool interruptionExists = false;
                            foreach (RaycastHit2D env in environmentHits)
                            {
                                // if environment collision along path
                                if (env.collider.GetComponent<EnvironmentPhysics>() && env.collider.GetComponent<EnvironmentPhysics>().GetBottomHeight() < projectileElevation && env.collider.GetComponent<EnvironmentPhysics>().GetTopHeight() > projectileElevation) 
                                {
                                    interruptionExists = true;
                                }
                            }
                            if (interruptionExists)
                            {
                                continue;
                            }
                            
                            hasHitEntity = true;
                            hitEntity = hits[i].collider.GetComponent<EntityPhysics>();
                        }
                        else
                        {
                            hasHitEntity = false;
                            hitEntity = null;
                        }
                    }
                }
            }   
        }

        //if hasnt hit anything
        if (endPoint == Vector2.zero)
        {
            shortestDistance = _lightRangedZapMaxDistance;
            RaycastHit2D[] environmentHits = Physics2D.RaycastAll(entityPhysics.GetComponent<Rigidbody2D>().position, aimDirection, _lightRangedZapMaxDistance);
            foreach (RaycastHit2D hit in environmentHits)
            {
                if (hit.collider.tag == "Environment" && hit.collider.GetComponent<EnvironmentPhysics>().GetBottomHeight() < projectileElevation && hit.collider.GetComponent<EnvironmentPhysics>().GetTopHeight() > projectileElevation)
                {
                    shortestDistance = hit.distance;
                }
            }

            //Debug.Log("Hit nothing");
            endPoint = entityPhysics.GetComponent<Rigidbody2D>().position + aimDirection.normalized * shortestDistance;
        }
        if (hitEntity)
        {
            hitEntity.Inflict(1, force:aimDirection * 2.0f, type:ElementType.ZAP);
        }
        /*
        _lightRangedZap.GetComponent<LineRenderer>().SetPosition(0, new Vector3(entityPhysics.transform.position.x, entityPhysics.transform.position.y + projectileElevation, entityPhysics.transform.position.y));
        _lightRangedZap.GetComponent<LineRenderer>().SetPosition(1, new Vector3(endPoint.x, endPoint.y + projectileElevation, endPoint.y));
        //StartCoroutine(FlashZap(_lightRangedDuration * 0.5f));
        _lightRangedZap.Play(_lightRangedDuration * 0.5f);
        */
        
        _lightRangedZap.SetupLine(new Vector3(entityPhysics.transform.position.x, entityPhysics.transform.position.y + projectileElevation, entityPhysics.transform.position.y - _projectileStartHeight), new Vector3(endPoint.x, endPoint.y + projectileElevation, endPoint.y - _projectileStartHeight));
        _lightRangedZap.Play(_lightRangedDuration * 0.5f);
        
        //Debug.Log(entityPhysics.transform.position);
        //Debug.Log(endPoint);
    }

    //TODO
    private void ChargedRanged_Void()
    {
        throw new NotImplementedException();
    }
    //TODO
    private void ChargedRanged_Fire()
    {
        throw new NotImplementedException();
    }

    private void ChargedRanged_Zap()
    {
        Debug.Log("Here");
        //raycast in direction, to max distance (Might want it to be a circlecast or boxcast, to be more generous with enemy hit detection)
        //if hits an environment object at the height it's cast at, endpoint there
        //else, endpoint at max distance
        //draw linerenderer between players position + offset and endpoint
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(1f, .1f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(entityPhysics.GetComponent<Rigidbody2D>().position, aimDirection, _chargedRangedZapMaxDistance);
        float projectileElevation = entityPhysics.GetBottomHeight() + _projectileStartHeight;
        float shortestDistance = float.MaxValue;
        Vector2 endPoint = Vector2.zero;
        List<EntityPhysics> enemiesHit = new List<EntityPhysics>();
        for (int i = 0; i < hits.Length; i++)
        {
            //check if is something that can be collided with
            //check to see if projectile z is within z bounds of physicsobject
            //check to see if distance to target is shorter than shortestDistance

            if (hits[i].collider.GetComponent<PhysicsObject>()) //can object be collided with
            {
                PhysicsObject other = hits[i].collider.GetComponent<PhysicsObject>();
                if (other.GetBottomHeight() < projectileElevation && other.GetTopHeight() > projectileElevation) //is projectile height within z bounds of object
                {
                   
                    if (other.tag == "Environment")
                    {
                        if (hits[i].distance < shortestDistance)
                        {
                            //set to current valid collision
                            shortestDistance = hits[i].distance;
                            endPoint = hits[i].point;
                        }
                    }
                    else if (other.tag == "Enemy" && other.GetComponent<EntityPhysics>())
                    {
                        enemiesHit.Add(other.GetComponent<EntityPhysics>());
                    }
                }
            }
        }
        //if hasnt hit anything
        if (endPoint == Vector2.zero)
        {
            //Debug.Log("Hit nothing");
            endPoint = entityPhysics.GetComponent<Rigidbody2D>().position + aimDirection.normalized * _chargedRangedZapMaxDistance;
        }
        
        foreach(EntityPhysics enemy in enemiesHit)
        {
            enemy.Inflict(1, force:aimDirection * 5.0f, type:ElementType.ZAP);
        }

        _chargedRangedZap.SetupLine(new Vector3(entityPhysics.transform.position.x, entityPhysics.transform.position.y + projectileElevation, entityPhysics.transform.position.y - _projectileStartHeight), new Vector3(endPoint.x, endPoint.y + projectileElevation, endPoint.y - _projectileStartHeight));
        _chargedRangedZap.Play(_lightRangedDuration * 0.5f);
    }


    /// <summary>
    /// Fires a bullet
    /// </summary>
    private void FireBullet()
    {
        Vector2 _tempRightAnalogDirection = aimDirection;

        GameObject tempBullet = _equippedWeapon.FireBullet(_tempRightAnalogDirection);
        //tempBullet.GetComponentInChildren<EntityPhysics>().NavManager = entityPhysics.NavManager;
        tempBullet.GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(entityPhysics.GetObjectElevation() + 2f);
        tempBullet.GetComponentInChildren<ProjectilePhysics>().GetComponent<Rigidbody2D>().position = (entityPhysics.GetComponent<Rigidbody2D>().position);
    }

    #endregion

    //---------------------------------------------| Misc
    public void PlayerHeal()
    {
        if (StateTimer == heal_duration )
        {
            characterAnimator.Play(HEAL_ANIM);
            Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.0f, 0.5f, 1f));
        }
        else if (StateTimer < _changeStyleColorChangeTime && !hasHealed )
        {
            switch (_currentStyle)
            {
                case ElementType.FIRE:
                    Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.5f, 0f, 1f));
                    break;
                case ElementType.VOID:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.5f, 0.0f, 1.0f, 1f));
                    break;
                case ElementType.ZAP:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.0f, 1.0f, 0.5f, 1f));
                    break;
                default:
                    Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.2f, 0.1f, 1f));
                    break;
            }
            ChangeEnergy(-4);
            entityPhysics.Heal(1);
            Vibrate(1f, 0.1f);
            hasHealed = true;
        }
        else if (StateTimer < 0 || !controller.GetButton("Heal"))
        {
            CurrentState = PlayerState.IDLE;
            hasHealed = false;
            switch (_currentStyle)
            {
                case ElementType.FIRE:
                    Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.5f, 0f, 1f));
                    break;
                case ElementType.VOID:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.5f, 0.0f, 1.0f, 1f));
                    break;
                case ElementType.ZAP:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.0f, 1.0f, 0.5f, 1f));
                    break;
                default:
                    Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.2f, 0.1f, 1f));
                    break;
            }
        }

        StateTimer -= Time.deltaTime;
    }

    private void PlayerChangeStyle()
    {
        characterAnimator.Play(STYLE_CHANGE_Anim);
        UpdateAimDirection();
        StateTimer -= Time.deltaTime;

        if (StateTimer < _changeStyleColorChangeTime && !_changeStyle_HasChanged)
        {
            _changeStyle_HasChanged = true;
            switch (_newStyle)
            {
                case ElementType.FIRE:
                    Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.5f, 0f, 1f));
                    ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(1f, 0.5f, 0.0f));
                    Vibrate(1f, 0.1f);
                    _currentStyle = ElementType.FIRE;
                    SwapWeapon("WEST");
                    break;
                case ElementType.VOID:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.5f, 0.0f, 1.0f, 1f));
                    ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(0.5f, 0.0f, 1.0f));
                    Vibrate(1f, 0.1f);
                    _currentStyle = ElementType.VOID;
                    SwapWeapon("NORTH");
                    break;
                case ElementType.ZAP:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.0f, 1.0f, 0.5f, 1f));
                    ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(0.0f, 1.0f, 0.5f));
                    Vibrate(1f, 0.1f);
                    _currentStyle = ElementType.ZAP;
                    break;
                default:
                    Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.2f, 0.1f, 1f));
                    ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(1f, 0.2f, 0.1f));
                    Vibrate(1f, 0.1f);
                    Debug.LogError("HOWDY : Somehow, the player changed style to a nonexistent style!");
                    break;
            }
        }


        if ( StateTimer < 0 || ( !controller.GetButton("ChangeStyle_Fire") && !controller.GetButton("ChangeStyle_Void") && !controller.GetButton("ChangeStyle_Zap") ) )
        { 
            CurrentState = PlayerState.RUN;
            _changeStyle_HasChanged = false;
        }

    }

    private void PlayerRest()
    {
        if (!isStanding && StateTimer > 0)
        {
            characterAnimator.Play("PlayerRestTransition");
            _rest_recover_energy_timer = _rest_recover_energy_duration;
            _rest_recover_health_timer = _rest_recover_health_duration;
        }
        else if (isStanding && StateTimer > 0)
        {
            characterAnimator.Play("PlayerRestStanding");
        }
        else if (!isStanding)
        {
            characterAnimator.Play("PlayerRestLoop");
            if (_rest_recover_health_timer < 0)
            {
                entityPhysics.Heal(1);
                _rest_recover_health_timer = _rest_recover_health_duration;
            }
            if (_rest_recover_energy_timer < 0)
            {
                ChangeEnergy(1);
                _rest_recover_energy_timer = _rest_recover_energy_duration;
            }

            _rest_recover_energy_timer -= Time.deltaTime;
            _rest_recover_health_timer -= Time.deltaTime;
            
            //attempt to perform most any action should require the stand up animation play
            if (controller.GetButtonDown("Rest") ||
                Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2 ||
                controller.GetButtonDown("Jump") ||
                controller.GetButtonDown("Melee") ||
                controller.GetButton("Charge") ||
                controller.GetButton("RangedAttack") ||
                controller.GetButtonDown("Blink") ||
                controller.GetButtonDown("Heal")
                )
            {
                StateTimer = rest_kneel_duration;
                isStanding = true;
            }
        }
        else
        {
            currentFaceDirection = FaceDirection.SOUTH;
            CurrentState = PlayerState.IDLE;
            isStanding = false;
        }
        StateTimer -= Time.deltaTime;




        
        #region IDLE state transitions (copied)
        // commented out due to decision to require the "stand up" animation play under most circumstances
        /*
        if (Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2)
        {
            //Debug.Log("IDLE -> RUN");
            isStanding = false;
            CurrentState = PlayerState.RUN;
        }
        if (controller.GetButtonDown("Jump"))
        {
            //Debug.Log("IDLE -> JUMP");
            isStanding = false;
            Vibrate(.5f, 0.05f);
            entityPhysics.ZVelocity = JumpImpulse;
            CurrentState = PlayerState.JUMP;
        }

        if (controller.GetButtonDown("Melee"))
        {
            isStanding = false;
            hasSwung = false;
            //Debug.Log("IDLE -> ATTACK");
            StateTimer = time_lightMelee;
            CurrentState = PlayerState.LIGHT_MELEE;
        }

        if (controller.GetButton("Charge"))
        {
            isStanding = false;
            StateTimer = time_Charge;
            CurrentState = PlayerState.CHARGE;
        }

        if (controller.GetButton("RangedAttack"))
        {
            isStanding = false;
            PlayerLightRangedTransitionAttempt();
        }
        if (controller.GetButtonDown("Blink"))
        {
            isStanding = false;
            PlayerBlinkTransitionAttempt();
        }
        if (controller.GetButtonDown("Heal"))
        {
            isStanding = false;
            PlayerHealTransitionAttempt();
        }
        */
        CheckStyleChange();


        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight) //override other states to trigger fall
        {
            isStanding = false;
            entityPhysics.ZVelocity = 0;
            CurrentState = PlayerState.JUMP;
        }
        else
        {
            entityPhysics.SetObjectElevation(maxheight);
        }
        #endregion

    }
    //================================================================================| TRANSITIONS

    private void PlayerLightRangedTransitionAttempt()
    {
        if (CurrentEnergy < _lightRangedEnergyCost)
        {
            return;
        }

        ChangeEnergy(-2);
        StateTimer = 0f;
        CurrentState = PlayerState.LIGHT_RANGED;
    }

    private void PlayerBlinkTransitionAttempt()
    {
        if (_blink_hasButtonMashed) //use punishment timer
        {
            if (_blinkTimer > BLINK_TIME_PUNISH)
            {
                if (CurrentState == PlayerState.JUMP) _hasAlreadyBlinkedInMidAir = true;
                CurrentState = PlayerState.BLINK;
            }
        }
        else
        {
            if (_blinkTimer > BLINK_TIME_PRO)
            {
                if (CurrentState == PlayerState.JUMP) _hasAlreadyBlinkedInMidAir = true;
                CurrentState = PlayerState.BLINK;
            }
            else
            {
                _blink_hasButtonMashed = true; //fool, you clicked too fast
            }
        }
    }

    private void PlayerHealTransitionAttempt()
    {
        if (entityPhysics.GetCurrentHealth() < entityPhysics.GetMaxHealth() && CurrentEnergy >= heal_cost)
        {
            CurrentState = PlayerState.HEAL;
            StateTimer = heal_duration;
        }
    }

    //==================================================================================| MISC

    public override void JustGotHit(Vector2 hitdirection)
    {
        //Debug.Log("Player: Ow!");
        _lastHitDirection = hitdirection;
        StartCoroutine(VibrateDecay(1f, 0.025f));
        if (entityPhysics.GetCurrentHealth() <= 0) return;
        FollowingCamera.GetComponent<CameraScript>().Shake(1f, 6, 0.01f);
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.7f, 0.1f, Color.red);
        ScreenFlash.InstanceOfScreenFlash.PlayHitPause(0.15f);
        StartCoroutine(VibrateDecay(1f, 0.025f));
        entityPhysics.PlayInvincibilityFrames(0.4f);
        //_healthBar.UpdateBar((int)entityPhysics.GetCurrentHealth());
    }

    public override void OnDeath()
    {
        //Debug.Log("<color=pink>HEY!</color>");
        StartCoroutine(PlayDeathAnimation(_lastHitDirection));
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        //Debug.Log("POW!");
        HeavyMeleeSprite.GetComponent<SpriteRenderer>().enabled = true;
        HeavyMeleeSprite.GetComponent<SpriteRenderer>().flipX = flip;
        HeavyMeleeSprite.GetComponent<Animator>().PlayInFixedTime(0);

        //HeavyMeleeSprite.GetComponent<Animator>().Play("HeavyMeleeSwing!!!!!");
        yield return new WaitForSeconds(_lengthOfHeavyMeleeAnimation);
        HeavyMeleeSprite.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void Vibrate(float magnitude, float duration)
    {
        //Debug.Log("Vibrating : " + magnitude + ", " + duration);
        foreach (Joystick j in controller.controllers.Joysticks)
        {
            if (!j.supportsVibration) continue;
            for (int i = 0; i < j.vibrationMotorCount; i++)
            {
                j.SetVibration(i, magnitude, duration);
            }
        }
    }
    public IEnumerator VibrateDecay(float magnitude, float decayRate)
    {
        /*
        while (magnitude > 0)
        {
            Vibrate(magnitude, 0.01f);
            magnitude -= decayRate;
            yield return new WaitForSeconds(0.01f);
        }*/
        while (magnitude > 0.01)
        {
            Vibrate(magnitude, 0.01f);
            magnitude *= 0.75f;
            yield return new WaitForSecondsRealtime(0.01f);
        }
        

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

    public override void SetXYAnalogInput(float x, float y)
    {
        throw new NotImplementedException();
    }

    public void ChangeEnergy(int delta)
    {
        _currentEnergy += delta;
        if (_currentEnergy > _maxEnergy) _currentEnergy = _maxEnergy;
        if (_currentEnergy < 0) _currentEnergy = 0;
    }

    IEnumerator PlayDeathAnimation(Vector2 killingBlowDirection)
    {
        Debug.Log("Playing death");
        // flash that black + white final slam, freeze time for a moment
        _deathFlash.SetBlammoDirection(killingBlowDirection);
        _deathFlash.transform.position = new Vector3(characterSprite.transform.position.x, characterSprite.transform.position.y, FollowingCamera.transform.position.z + 2.0f);
        SpriteRenderer[] renderers = _deathFlash.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = true;
        }

        _gameplayUI.GetComponent<CanvasGroup>().alpha = 0.0f; // Disable visibility game ui for a moment 

        yield return new WaitForEndOfFrame();
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 0.5f; // -------------| RESUME TIME |--------------

        _gameplayUI.GetComponent<CanvasGroup>().alpha = 1.0f;
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
        characterAnimator.Play(DEATH_WEST_Anim);

        yield return new WaitForSeconds(1.0f);
        Time.timeScale = 1.0f;
        _fadeTransition.FadeToScene(SceneManager.GetActiveScene().name);
        // play death animation, stop for a bit, then fade to black
    }
}