using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Rewired;
using UnityEngine.Audio;

public enum ElementType
{
    ZAP, FIRE, VOID, ICHOR, NONE
}


/// <summary>
/// This class controls player state and contains methods for each state. It also receives input from the InputHandler and acts in accordance with said input.
/// In addition, it handles sprites, shadows, and player height
/// </summary>
public class PlayerHandler : EntityHandler
{
    //[SerializeField] private InputHandler _inputHandler;
    [SerializeField] private GameObject characterSprite;
    [SerializeField] private SpriteRenderer weaponSprite;
    [SerializeField] private SpriteRenderer weaponGlowSprite;
    [SerializeField] private GameObject FollowingCamera;
    [SerializeField] private GameObject LightMeleeSprite;
    [SerializeField] private GameObject HeavyMeleeSprite;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _blinkAudioSource;
    /*[SerializeField]*/ private AudioSource _weaponAudioSource; // just pulling this from weapon gameobect
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
    [SerializeField] private SpriteRenderer _haloSprite;
    [SerializeField] private ProjectilePhysics SolFlailProjectile; // god. this fucking game
    [SerializeField] private LineRenderer SolFlailChain;
    [SerializeField] private SpriteRenderer EyeGlowSprite;

    private int _currentEnergy;

    public int MaxEnergy { get { return _maxEnergy; } }
    public int CurrentEnergy { get { return _currentEnergy; } }

    public static string PREVIOUS_SCENE = "";
    public static string PREVIOUS_SCENE_DOOR = "";

    private Animator characterAnimator;
    private PlayerInventory inventory;

    public ElementType OvertakenElement = ElementType.NONE; // if the player is suffused with a single element, this is the element they're hit with


    //[SerializeField] private UIHealthBar _healthBar;
    [SerializeField] private PlayerHealthBarHandler _healthBar;
    
    enum PlayerState {IDLE, RUN, JUMP, LIGHT_MELEE, HEAVY_MELEE, CHARGE, BURST, LIGHT_RANGED, CHARGED_RANGE, BLINK, CHANGE_STYLE, HEAL, REST, DEAD, COLLAPSE, STAND_FROM_COLLAPSE};

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
    //const string HEAL_ANIM = "ChangeAttunement_Anim"; //TODO : have a different animation for this my dude // dont fucking misgender me

    private const float AttackMovementSpeed = 0.6f;

    private Weapon _equippedWeapon;

    private PlayerState CurrentState;
    private PlayerState PreviousState;

    private ElementType _currentStyle;
    private ElementType _newStyle;

    private FaceDirection currentFaceDirection;
    private FaceDirection previousFaceDirection;
    
    private bool hasSwung;
    private bool bHasHeavyMeleeInflicted = false; // used to ensure heavy melees only flash hitbox once

    //=================| NEW COMBAT STUFF
    //state times
    private const float time_lightMelee = 0.25f; //duration of state
    private bool _hasHitAttackAgain = false; //used for combo chaining 
    private bool _readyForThirdHit = false; //true during second attack, if player hits x again changes to heavy attack
    private float _lengthOfLightMeleeAnimation;

    private Vector2 lightmelee_hitbox = new Vector2(5, 4);
    private Vector2 thrustDirection;
    private Vector2 aimDirection; // direction AND magnitude of "right stick", used for attack direction, camera, never a 0 vector
    private float _timeToComboReady = 0.17f; //higher means more generous
    private const float LIGHTMELEE_FORCE = 0.5f;

    // heavy melee
    private const float time_heavyMelee = 0.3f;
    private float _lengthOfHeavyMeleeAnimation;
    private Coroutine CurrentWeaponGlowCoroutine; 

    private Vector2 heavymelee_hitbox = new Vector2(8, 4); // old combo system that was ok but not as cool


    private const int HEAVYMELEE_ICHOR_DAMAGE = 3;
    private Vector2 HEAVYMELEE_ICHOR_HITBOX = new Vector2(14, 10);
    private Vector2 HEAVYMELEE_ICHOR_HITBOXOFFSET = new Vector2(3,0);
    private const float HEAVYMELEE_ICHOR_FORCE = 2.0f;
    private const float HEAVYMELEE_ICHOR_INFLICTTIME = 0.1f;    // time after attack cast when the hitbox flashes
    private const float HEAVYMELEE_ICHOR_HITSTOP = 0.2f;
    [SerializeField] private AnimationCurve HEAVYMELEE_ICHOR_ATTACKBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_ICHOR_SUMMONBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_ICHOR_READYBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_ICHOR_VANISHBRIGHTNESSCURVE;
    [SerializeField] private Sprite HEAVYMELEE_ICHOR_GLOWSPRITE;


    // sol does solid damage over time, but should also feel really weighty. Hard to land a shot, but when you do, oomph.
    private const int HEAVYMELEE_SOL_DAMAGE = 2;
    private const float HEAVYMELEE_SOL_MAXTRAVELTIME = 0.2f;
    private const float HEAVYMELEE_SOL_RETURNTIME = 1.0f;
    private const float HEAVYMELEE_SOL_FORCE = 2.0f;
    private const float HEAVYMELEE_SOL_MAXVELOCITY = 150.0f;
    private const float HEAVYMELEE_SOL_RETURNACCELERATION = 15.0f;
    private const float HEAVYMELEE_SOL_ORBITRADIUS = 2.5f;
    private const float HEAVYMELEE_SOL_ORBITPERIOD = 0.5f;
    private const float HEAVYMELEE_SOL_APPEARDURATION = 0.2f;
    private const float HEAVYMELEE_SOL_VANISHDURATION = 0.2f;
    private const float HEAVYMELEE_SOL_AIMASSIST_RADIUS = 6.0f;
    private const float HEAVYMELEE_SOL_AIMASSIST_DISTANCE = 16.0f;
    private Vector2 HEAVYMELEE_SOL_HITBOX_DETONATION = new Vector2(8, 6);
    [SerializeField] private AnimationCurve HEAVYMELEE_SOL_VELOCITY_OVER_TIME_NORMALIZED;
    [SerializeField] private AnimationCurve HEAVYMELEE_SOL_IMPACTBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_SOL_RECALLBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_SOL_SUMMONBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_SOL_READYBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_SOL_VANISHBRIGHTNESSCURVE;
    [SerializeField] private Sprite HEAVYMELEE_SOL_GLOWSPRITE;
    private Coroutine CurrentSolFlailCoroutine; // either the orbit, or the melee
    private Coroutine CurrentSolFlailSpawnDespawnCoroutine; // If the spawn or despawn animations are playing, this stores them
    private bool bIsSolFlailAttackCoroutineRunning;

    // rift is all about shoving ppl around, clearing the area around you. knockback is the big boon of this one
    private const int HEAVYMELEE_RIFT_DAMAGE = 2;
    private const float HEAVYMELEE_RIFT_FORCE = 5.0f;
    private const float HEAVYMELEE_RIFT_INFLICTTIME = 0.1f;
    private const float HEAVYMELEE_RIFT_RADIUS = 7.0f;
    [SerializeField] private AnimationCurve HEAVYMELEE_RIFT_ATTACKBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_RIFT_SUMMONBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_RIFT_READYBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_RIFT_VANISHBRIGHTNESSCURVE;
    [SerializeField] private Sprite HEAVYMELEE_RIFT_GLOWSPRITE;

    // storm lets you hit deep with melees and do chain lightning, which is really good damage
    private const int HEAVYMELEE_STORM_DAMAGE = 2;
    private Vector2 HEAVYMELEE_STORM_HITBOXSIZE = new Vector2(16, 4);
    private Vector2 HEAVYMELEE_STORM_HITBOXOFFSET = new Vector2(7, 0);
    private const float HEAVYMELEE_STORM_INFLICTTIME = 0.1f;
    private const float HEAVYMELEE_STORM_FORCE = 2.0f;
    private const float HEAVYMELEE_STORM_AIMASSIST_RADIUS = 4.0f;
    private const float HEAVYMELEE_STORM_AIMASSIST_DISTANCE = 16.0f;
    [SerializeField] private AnimationCurve HEAVYMELEE_STORM_ATTACKBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_STORM_SUMMONBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_STORM_READYBRIGHTNESSCURVE;
    [SerializeField] private AnimationCurve HEAVYMELEE_STORM_VANISHBRIGHTNESSCURVE;
    [SerializeField] private Sprite HEAVYMELEE_STORM_GLOWSPRITE;

    private const float DEFLECT_AIMASSIST_RADIUS = 4.0f;
    private const float DEFLECT_AIMASSIST_DISTANCE = 30.0f;

    //Charge Stuff
    //private const float time_Charge = 1.45f; //total time before player is charged up
    //private const float time_ChargeLight = .5f;
    //private const float time_ChargeMedium = .5f;
    //private const float time_ChargeTransition = 0.25f; //how long is the entire transition animation, anyway?

    //Burst Stuff
    private float _burstDuration = .75f;
    private const float time_burstHit = 0.4f;
    private Vector2 _burstArea = new Vector2(16f, 12f);
    private bool _hasFlashed = false;

    //Light Ranged 
    private const float _lightRangedDuration = 0.25f;
    private int _lightRangedEnergyCost = 2;

    private const float RANGED_ICHOR_AIMASSIST_RADIUS = 6.0f;
    private const float RANGED_ICHOR_AIMASSIST_DISTANCE = 40.0f;


    //Light Ranged Zap Attack
    private const float _lightRangedZapMaxDistance = 30f;
    private const float _chargedRangedZapMaxDistance = 50f;

    // Change Style 
    private const float _changeStyleDuration = 0.875f;//0.7125f;//0.95f;
    private const float _changeStyleColorChangeTime = 0.67f;//0.4f;
    private bool _changeStyle_HasChanged = false;

    // Collapse
    private const float COLLAPSE_STAND_DURATION = 1.5f;

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
    private const float BLINK_COOLDOWN = 0.2f;
    private float _blinkTimer = 0f; //time since last blink
    private bool _hasAlreadyBlinkedInMidAir = false;

    //=====================| HEAL
    private const float heal_duration = 0.95f;
    private const int heal_cost = 4;
    private bool hasHealed = false;

    //======================| REST
    private const float rest_kneel_duration = 0.51f;
    private bool isStanding = false;
    private const float _rest_recover_health_duration = 2.0f;
    private float _rest_recover_health_timer = 0f;
    private const float _rest_recover_energy_duration = 0.5f;
    private float _rest_recover_energy_timer = 0f;

    //======================| HALO SPRITE RELATIVE POSITION OFFSETS
    private Vector3 HaloOffset_RunWest;
    private Vector3 HaloOffset_RunEast;
    private Vector3 HaloOffset_RunNorth;
    private Vector3 HaloOffset_RunSouth;
    private Vector3 HaloOffset_Default;


    //=====================| JUMP/FALL FIELDS
    [SerializeField] private float JumpImpulse = 80f;
    [SerializeField] private float JumpHeldGravity;
    [SerializeField] private float JumpFallGravity;
    [SerializeField] private float FallGravity;

    [SerializeField] private Sprite Halo_Void;
    [SerializeField] private Sprite Halo_Zap;
    [SerializeField] private Sprite Halo_Fire;
    [SerializeField] private Sprite Halo_Ichor;
    private bool _jump_hasStartedFalling = false;

    private static int NumberOfShatteredHealthBlocks = 0;

    public float TimeSinceCombat = 0.0f;
    public bool ForceUIVisible = false;

    public RestPlatform CurrentRestPlatform; // if player is currently interacting with a rest platform, this is it

    public bool BlockInput = false;

    [Space(10)]
    [SerializeField] List<AudioClip> SFX_LightSlashes;
    [SerializeField] AudioClip SFX_IchorBlade_Slash;
    [SerializeField] AudioClip SFX_IchorBlade_Summon;
    [SerializeField] AudioClip SFX_IchorBlade_Vanish;

    [SerializeField] AudioClip SFX_StormSpear_Slash;
    [SerializeField] AudioClip SFX_StormSpear_Summon;
    [SerializeField] AudioClip SFX_StormSpear_Vanish;

    [SerializeField] AudioClip SFX_RiftScythe_Slash;
    [SerializeField] AudioClip SFX_RiftScythe_Summon;
    [SerializeField] AudioClip SFX_RiftScythe_Vanish;

    [SerializeField] AudioClip SFX_SolFlail_Throw;
    [SerializeField] AudioClip SFX_SolFlail_Hit;
    [SerializeField] AudioClip SFX_SolFlail_Summon;
    [SerializeField] AudioClip SFX_SolFlail_Vanish;
    [SerializeField] AudioClip SFX_SolFlail_Catch;

    [SerializeField] AudioClip SFX_Death;

    [SerializeField] AudioMixerGroup AudioMixer_Ducked; // mutes sound effects when player dies or other stuff happens
    [SerializeField] AudioMixerGroup AudioMixer_Unducked; // plays directly to SFX mixer/bus
    [SerializeField] AnimationCurve AudioFadeInCurve; // mutes sound effects when player dies or other stuff happens

    [SerializeField] Texture2D PP_Black;
    [SerializeField] Texture2D PP_White;

    public ElementType GetStyle()
    {
        return _currentStyle;
    }

    public bool IsUsingMouse
    {
        get { return _isUsingCursor; }
        set { _isUsingCursor = value; }
    }

    public bool IsResting()
    {
        return CurrentState == PlayerState.REST;
    }

    void Awake()
    {
        HaloOffset_RunEast = new Vector3(0.25f, 2.0f, 0.0f);
        HaloOffset_RunWest = new Vector3(-0.25f, 2.0f, 0.0f);
        HaloOffset_RunNorth = new Vector3(0.0f, 2.5f, 0.0f);
        HaloOffset_RunSouth = new Vector3(0.0f, 2.5f, 0.0f);
        HaloOffset_Default = new Vector3(0.0f, 2.25f, 0.0f);


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
        CustomizationMenu.ApplyPlayerColorPalette();
    }

    void Start()
    {
        _gameplayUI.parent.gameObject.SetActive(true); // if I put this at the bottom it just... doesnt execute???
        _gameplayUI.gameObject.SetActive(true);
        weaponSprite.enabled = false;
        _weaponAudioSource = weaponSprite.GetComponent<AudioSource>();
        weaponGlowSprite.enabled = false;
        SolFlailProjectile.transform.parent.gameObject.SetActive(false);
        //weaponSprite.transform.right = new Vector3(0, 1, 0);

        //entityPhysics.SetMaxHealth(5 - NumberOfShatteredHealthBlocks);
        //_healthBar.ShatterHealthBarSegment(5-NumberOfShatteredHealthBlocks);

        

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

        // TODO : Element should persist across scenes, default ichor if anything
        _currentStyle = ElementType.ZAP;
        Shader.SetGlobalColor("_MagicColor",  new Color(0.0f, 1f, 0.5f, 1f));
        if (_currentStyle == ElementType.ZAP) _lightRangedEnergyCost = 1;
        weaponGlowSprite.material.SetFloat("_CurrentElement", 4);
        weaponGlowSprite.sprite = HEAVYMELEE_STORM_GLOWSPRITE;
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 4);
        EyeGlowSprite.material.SetFloat("_CurrentElement", 4);
        StartCoroutine(FadeInAudio());
        FollowingCamera.GetComponent<CameraScript>().SetPostProcessParam("_ShatterMaskTex", PP_White);
        FollowingCamera.GetComponent<CameraScript>().SetPostProcessParam("_CrackTex", PP_Black);
        FollowingCamera.GetComponent<CameraScript>().SetPostProcessParam("_OffsetTex", PP_Black);
        Debug.Log("Number of joysticks : " + controller.controllers.joystickCount);
        //Debug.Log("Joystick name : " + controller.controllers.Joysticks[0].name);
    }


    void Update ()
    {
        if (Time.timeScale == 0) return;

        if (_currentEnergy == MaxEnergy && entityPhysics.GetCurrentHealth() == 5)
        {
            TimeSinceCombat += Time.deltaTime;
        }
        else
        {
            TimeSinceCombat = 0.0f;
        }

        //Debug.Log(TimeSinceCombat);
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

        //weaponSprite.gameObject.transform.position = new Vector3(weaponSprite.gameObject.transform.position.x, weaponSprite.gameObject.transform.position.y, weaponSprite.gameObject.transform.position.y + entityPhysics.GetObjectElevation());
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
            case (PlayerState.BURST):
                PlayerBurst();
                break;
            case (PlayerState.LIGHT_RANGED):
                PlayerLightRanged();
                break;
            case (PlayerState.BLINK):
                PlayerBlink();
                break;
            case (PlayerState.CHANGE_STYLE):
                PlayerChangeStyle();
                break;
            /*case (PlayerState.HEAL):
                PlayerHeal();*/
                break;
            case (PlayerState.REST):
                PlayerRest();
                break;
            case (PlayerState.DEAD):
                break;
            case PlayerState.COLLAPSE:
                Player_Collapsed();
                break;
            case PlayerState.STAND_FROM_COLLAPSE:
                Player_StandingFromCollapsed();
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
        if (OvertakenElement != ElementType.NONE) return; // don't allow style changes if it's overtaken. Maybe have some visual indicator that you can't?

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
        else if (controller.GetButton("ChangeStyle_Ichor") && _currentStyle != ElementType.ICHOR)
        {
            //Shader.SetGlobalColor("_MagicColor", new Color(0f, 1f, 0.5f, 1f));
            //ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(0f, 1f, 0.5f));
            CurrentState = PlayerState.CHANGE_STYLE;
            StateTimer = _changeStyleDuration;
            _newStyle = ElementType.ICHOR;
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
        entityPhysics.ZVelocity = 0.0f;
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
        if (!BlockInput)
        {
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
                Vibrate(.5f, 0.05f);
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

            if (controller.GetButton("RangedAttack"))
            {
                PlayerLightRangedTransitionAttempt();
            }
            if (controller.GetButtonDown("Blink"))
            {
                PlayerBlinkTransitionAttempt();
            }
            CheckStyleChange();
        }
        
        //gravity
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
        entityPhysics.ZVelocity = 0.0f;
        //Face Direction Determination
        Vector2 direction = controller.GetAxis2DRaw("MoveHorizontal", "MoveVertical");
        if (direction.sqrMagnitude > 1) direction.Normalize(); //prevents going too fast on KB
        if (BlockInput) direction = Vector2.zero;
        if (direction.sqrMagnitude > 0.01f)
        {
            if (Vector2.Angle(new Vector2(1, 0), direction) < 60)
            {
                currentFaceDirection = FaceDirection.EAST;
                _haloSprite.transform.localPosition = HaloOffset_RunEast;
            }
            else if (Vector2.Angle(new Vector2(0, 1), direction) < 30)
            {
                currentFaceDirection = FaceDirection.NORTH;
                _haloSprite.transform.localPosition = HaloOffset_RunNorth;
            }
            else if (Vector2.Angle(new Vector2(0, -1), direction) < 30)
            {
                currentFaceDirection = FaceDirection.SOUTH;
                _haloSprite.transform.localPosition = HaloOffset_RunSouth;
            }
            else if (Vector2.Angle(new Vector2(-1, 0), direction) < 60)
            {
                currentFaceDirection = FaceDirection.WEST;
                _haloSprite.transform.localPosition = HaloOffset_RunWest;
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
        
        LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * lightmelee_hitbox.x / 2.0f, 
                characterSprite.transform.position.y + aimDirection.normalized.y * lightmelee_hitbox.x / 2.0f, 
                characterSprite.transform.position.z + aimDirection.normalized.y), 
            Quaternion.identity);
        LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
        /*
        LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.x, characterSprite.transform.position.y + aimDirection.y, characterSprite.transform.position.z + aimDirection.y), Quaternion.identity);
        LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
        */
        HeavyMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * heavymelee_hitbox.x / 2.0f, 
                characterSprite.transform.position.y + aimDirection.normalized.y * heavymelee_hitbox.x / 2.0f, 
                characterSprite.transform.position.z + aimDirection.normalized.y), 
            Quaternion.identity);
        HeavyMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));

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
        
        if (controller.GetButton("RangedAttack"))
        {
            PlayerLightRangedTransitionAttempt();
        }
        if (controller.GetButtonDown("Blink"))
        {
            PlayerBlinkTransitionAttempt();
        }
        CheckStyleChange();

        if (CurrentState == PlayerState.RUN)
        {
            entityPhysics.SavePosition();
        }
        else
        {
            _haloSprite.transform.localPosition = HaloOffset_Default;
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
        Vector2 blinkDirection = AccessibilityOptionsSingleton.GetInstance().IsBlinkInDirectionOfMotion ? aimDirection : new Vector2(xInput, yInput);
        TeleportVFX.DeployEffectFromPool(characterSprite.transform.position);
        FollowingCamera.GetComponent<CameraScript>().Jolt(1f, blinkDirection);
        //Vibrate( .5f, 0.05f);
        Vibrate(.5f, 0.05f);

        //apply effect to any entities caught within player's path..                                                                   V-- Scaling collider for more generous detection
        RaycastHit2D[] hits = Physics2D.BoxCastAll(entityPhysics.transform.position, entityPhysics.GetComponent<BoxCollider2D>().size * 2.0f, 0.0f, blinkDirection, blinkDirection.magnitude * 7f);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.tag == "Enemy" && hit.collider.GetComponent<EntityPhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && hit.collider.GetComponent<EntityPhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
            {
                hit.transform.GetComponent<EntityPhysics>().Handler.PrimeEnemy(_currentStyle);
                Vibrate( 1f, 0.1f);
                ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.3f, 0.05f, Color.white, _currentStyle);
            }
        }

        _blinkAudioSource.Play();
        //Debug.Log(aimDirection);

        entityPhysics.MoveWithCollision(blinkDirection.x * 7f, blinkDirection.y * 7f); //buggy, occasionally player teleports a much shorter distance than they should
        //entityPhysics.GetComponent<Rigidbody2D>().position = entityPhysics.GetComponent<Rigidbody2D>().position + aimDirection * 7f;


        //setup timer
        _blinkTimer = 0f;

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
        TimeSinceCombat = 0.0f;

        //Functionality to be done at very beginning
        if (StateTimer == time_lightMelee)
        {
            //Play SFX
            _audioSource.clip = SFX_LightSlashes[UnityEngine.Random.Range(0, SFX_LightSlashes.Count)];
            _audioSource.Play();
            Vibrate( 0.5f, 0.1f);

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
                        obj.GetComponent<EntityPhysics>().Inflict(1, force: aimDirection.normalized * LIGHTMELEE_FORCE);
                        ChangeEnergy(1);
                    }
                }
                else if (obj.GetComponent<ProjectilePhysics>())
                {
                    if (obj.GetComponent<ProjectilePhysics>().canBeDeflected && 
                        obj.GetComponent<ProjectilePhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && 
                        obj.GetComponent<ProjectilePhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                    {
                        Vibrate(1.0f, 0.15f);
                        obj.GetComponent<ProjectilePhysics>().PlayerRedirect(AimAssist(DEFLECT_AIMASSIST_DISTANCE, DEFLECT_AIMASSIST_DISTANCE), "ENEMY", 60f);
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
            _hasHitAttackAgain = true;
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

            if (_hasHitAttackAgain && _readyForThirdHit)
            {

                CurrentState = PlayerState.HEAVY_MELEE;
                StateTimer = time_heavyMelee;
                hitEnemies.Clear();
                _readyForThirdHit = false;
                _hasHitAttackAgain = false;

                //allow adjusting direction of motion/attack
                UpdateAimDirection();

                LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * lightmelee_hitbox.x / 2.0f, 
                    characterSprite.transform.position.y + aimDirection.normalized.y * lightmelee_hitbox.x / 2.0f, 
                    characterSprite.transform.position.z + aimDirection.normalized.y), 
                    Quaternion.identity);
                LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
                HeavyMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * heavymelee_hitbox.x / 2.0f, 
                    characterSprite.transform.position.y + aimDirection.normalized.y * heavymelee_hitbox.x / 2.0f, 
                    characterSprite.transform.position.z + aimDirection.normalized.y), 
                    Quaternion.identity);
                HeavyMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
            }
            else if (_hasHitAttackAgain)
            {
                _readyForThirdHit = true;
                CurrentState = PlayerState.LIGHT_MELEE;
                StateTimer = time_lightMelee;
                _hasHitAttackAgain = false;
                //allow adjusting direction of motion/attack
                UpdateAimDirection();

                LightMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * lightmelee_hitbox.x / 2.0f,
                    characterSprite.transform.position.y + aimDirection.normalized.y * lightmelee_hitbox.x / 2.0f, 
                    characterSprite.transform.position.z + aimDirection.normalized.y), 
                    Quaternion.identity);
                LightMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
                HeavyMeleeSprite.transform.SetPositionAndRotation(new Vector3(characterSprite.transform.position.x + aimDirection.normalized.x * heavymelee_hitbox.x / 2.0f, 
                    characterSprite.transform.position.y + aimDirection.normalized.y * heavymelee_hitbox.x / 2.0f, 
                    characterSprite.transform.position.z + aimDirection.normalized.y), 
                    Quaternion.identity);
                HeavyMeleeSprite.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, aimDirection)));
                
                weaponSprite.enabled = true;
                weaponGlowSprite.enabled = true;
                weaponSprite.gameObject.transform.parent = characterSprite.gameObject.transform;
                switch (_currentStyle)
                {
                    case ElementType.ZAP:
                        weaponSprite.transform.right = aimDirection;
                        weaponSprite.transform.localPosition = new Vector3(0, 0, -3);
                        weaponSprite.GetComponent<Animator>().Play("StormSpear_Manifest", 0, 0.0f);
                        _weaponAudioSource.clip = SFX_StormSpear_Summon;
                        _weaponAudioSource.Play();
                        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_STORM_READYBRIGHTNESSCURVE));
                        break;
                    case ElementType.VOID:
                        weaponSprite.transform.right = -aimDirection;
                        weaponSprite.transform.localPosition = weaponSprite.transform.right * 3;
                        weaponSprite.GetComponent<Animator>().Play("RiftScythe_Manifest", 0, 0.0f);
                        _weaponAudioSource.clip = SFX_RiftScythe_Summon;
                        _weaponAudioSource.Play();
                        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_RIFT_READYBRIGHTNESSCURVE));
                        break;
                    case ElementType.FIRE:
                        if (bIsSolFlailAttackCoroutineRunning) StopCoroutine(CurrentSolFlailCoroutine);
                        CurrentSolFlailCoroutine = StartCoroutine(SolFlailOrbit(bIsSolFlailAttackCoroutineRunning));
                        _weaponAudioSource.clip = SFX_SolFlail_Summon;
                        _weaponAudioSource.Play();
                        bIsSolFlailAttackCoroutineRunning = false;
                        break;
                    case ElementType.ICHOR:
                        weaponSprite.transform.right = -aimDirection;
                        weaponSprite.transform.localPosition = /*weaponSprite.transform.right * 1 +*/ weaponSprite.transform.up * 2;
                        weaponSprite.GetComponent<Animator>().Play("IchorBlade_Manifest", 0, 0.0f);
                        _weaponAudioSource.clip = SFX_IchorBlade_Summon;
                        _weaponAudioSource.Play();
                        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_ICHOR_READYBRIGHTNESSCURVE));
                        break;
                }
                
            }
            else if (_readyForThirdHit)
            {
                LightMeleeSprite.GetComponent<SpriteRenderer>().enabled = false;
                switch (_currentStyle)
                {
                    case ElementType.ZAP:
                        weaponSprite.GetComponent<Animator>().Play("StormSpear_Vanish");
                        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_STORM_VANISHBRIGHTNESSCURVE));
                        _weaponAudioSource.clip = SFX_StormSpear_Vanish;
                        _weaponAudioSource.Play();

                        break;
                    case ElementType.VOID:
                        weaponSprite.GetComponent<Animator>().Play("RiftScythe_Vanish");
                        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_RIFT_VANISHBRIGHTNESSCURVE));
                        _weaponAudioSource.clip = SFX_RiftScythe_Vanish;
                        _weaponAudioSource.Play();
                        break;
                    case ElementType.FIRE:
                        //weaponSprite.GetComponent<Animator>().Play("SolFlail_Vanish");
                        if (CurrentSolFlailSpawnDespawnCoroutine != null) StopCoroutine(CurrentSolFlailSpawnDespawnCoroutine);
                        CurrentSolFlailSpawnDespawnCoroutine = StartCoroutine(SolFlailVanish());
                        _weaponAudioSource.clip = SFX_SolFlail_Vanish;
                        _weaponAudioSource.Play();
                        break;
                    case ElementType.ICHOR:
                        weaponSprite.GetComponent<Animator>().Play("IchorBlade_Vanish");
                        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_ICHOR_VANISHBRIGHTNESSCURVE));
                        _weaponAudioSource.clip = SFX_IchorBlade_Vanish;
                        _weaponAudioSource.Play();
                        break;
                }
                _hasHitAttackAgain = false;
                CurrentState = PlayerState.RUN;
                hitEnemies.Clear();
                _readyForThirdHit = false;
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

    // Should be called by the 2nd light melee in a combo. Makes the sol flail orbit the player. Should work even if sol flail hasnt fully returned to player position yet
    private IEnumerator SolFlailOrbit(bool bInterruptingAttackAnim)
    {
        float AngleFromStart = 0.0f;
        float SolChainCurrentWidth = 0.0f;
        if (!bInterruptingAttackAnim)
        {
            StartCoroutine(SolFlailAppear());
        }

        //weaponSprite.transform.right = aimDirection;
        //weaponSprite.transform.localPosition = new Vector3(0, 0, 0);
        //weaponSprite.GetComponent<Animator>().Play("SolFlail_Manifest", 0, 0.0f);


        SolFlailProjectile.Speed = 0;
        SolFlailProjectile.canBeDeflected = false;

        // start to right of player
        Vector2 targetposition = entityPhysics.transform.position + LightMeleeSprite.transform.right * HEAVYMELEE_SOL_ORBITRADIUS * (bInterruptingAttackAnim ? 2f : 1f);
        while (true)
        {
            if (bInterruptingAttackAnim)
            {
                SolFlailProjectile.transform.position = Vector2.Lerp(SolFlailProjectile.transform.position, targetposition, 0.2f);
            }
            else
            {
                SolFlailProjectile.transform.position = targetposition;
            }
            SolFlailChain.SetPosition(0, entityPhysics.ObjectSprite.transform.position);
            SolFlailChain.SetPosition(1, SolFlailProjectile.ObjectSprite.transform.position);

            AngleFromStart += Time.deltaTime * 360 / HEAVYMELEE_SOL_ORBITPERIOD;
            targetposition = entityPhysics.transform.position + Quaternion.AngleAxis(AngleFromStart, Vector3.back) * LightMeleeSprite.transform.right * HEAVYMELEE_SOL_ORBITRADIUS * (bInterruptingAttackAnim ? 2f : 1f); // wider orbit radius for returning flail looks better
            yield return new WaitForEndOfFrame();
        }
    }

    // Vanish coroutine, should be played on top of the orbit coroutine
    private IEnumerator SolFlailAppear()
    {
        SolFlailProjectile.transform.parent.gameObject.SetActive(true);
        SolFlailProjectile.ObjectSprite.transform.position = entityPhysics.ObjectSprite.transform.position; // this line resolves an issue with a one-frame appearance of the flail head elsewhere
        SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", 0.0f);
        SolFlailProjectile.SetObjectElevation(entityPhysics.GetObjectElevation() + 1.0f);
        SolFlailProjectile.transform.position = entityPhysics.transform.position;
        SolFlailProjectile.ObjectSprite.GetComponent<Animator>().Play("SolFlailBall_Manifest");
        SolFlailChain.enabled = true;
        float timer = 0;
        while (timer < HEAVYMELEE_SOL_APPEARDURATION)
        {
            SolFlailChain.endWidth = timer / HEAVYMELEE_SOL_APPEARDURATION;
            SolFlailChain.startWidth = timer / HEAVYMELEE_SOL_APPEARDURATION;
            SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", timer / HEAVYMELEE_SOL_APPEARDURATION);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SolFlailChain.endWidth = 1;
        SolFlailChain.startWidth = 1;
        SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", 1.0f);
    }

    // Vanish coroutine, should be played on top of the orbit coroutine
    private IEnumerator SolFlailVanish()
    {
        SolFlailProjectile.ObjectSprite.GetComponent<Animator>().Play("SolFlailBall_Vanish");
        float timer = 0;
        while (timer < HEAVYMELEE_SOL_VANISHDURATION)
        {
            SolFlailChain.endWidth = 1 - timer / HEAVYMELEE_SOL_VANISHDURATION;
            SolFlailChain.startWidth = 1 - timer / HEAVYMELEE_SOL_VANISHDURATION;
            SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", 1 - timer / HEAVYMELEE_SOL_VANISHDURATION);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SolFlailChain.endWidth = 0;
        SolFlailChain.startWidth = 0;
        SolFlailProjectile.transform.parent.gameObject.SetActive(false);
        SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", 0.0f);
        StopCoroutine(CurrentSolFlailCoroutine); // stops the loop coroutine 
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
            StartCoroutine(PlayHeavyAttack(false));

            //Debug.DrawRay(entityPhysics.transform.position, thrustDirection*5.0f, Color.cyan, 0.2f);
            UpdateAimDirection();
            thrustDirection = aimDirection;

            switch (_currentStyle)
            {
                case ElementType.FIRE:
                    if (CurrentSolFlailCoroutine != null) { StopCoroutine(CurrentSolFlailCoroutine); }
                    if (CurrentSolFlailSpawnDespawnCoroutine != null) StopCoroutine(CurrentSolFlailSpawnDespawnCoroutine);
                    CurrentSolFlailCoroutine = StartCoroutine(PlayerHeavyMelee_Fire());
                    _weaponAudioSource.clip = SFX_SolFlail_Throw;
                    _weaponAudioSource.Play();
                    break;
                case ElementType.VOID:
                    StartCoroutine(PlayerHeavyMelee_Rift());
                    _weaponAudioSource.clip = SFX_RiftScythe_Slash;
                    _weaponAudioSource.Play();
                    break;
                case ElementType.ZAP:
                    PlayerHeavyMelee_Zap();
                    _weaponAudioSource.clip = SFX_StormSpear_Slash;
                    _weaponAudioSource.Play();
                    break;
                case ElementType.ICHOR:
                    StartCoroutine(PlayerHeavyMelee_Ichor());
                    _weaponAudioSource.clip = SFX_IchorBlade_Slash;
                    _weaponAudioSource.Play();
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
        thrustDirection = AimAssist(HEAVYMELEE_STORM_AIMASSIST_RADIUS, HEAVYMELEE_STORM_AIMASSIST_DISTANCE);

        Vector2 hitboxpos = (Vector2)entityPhysics.transform.position + thrustDirection * HEAVYMELEE_STORM_HITBOXOFFSET.x;
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(hitboxpos, HEAVYMELEE_STORM_HITBOXSIZE, Vector2.SignedAngle(Vector2.right, thrustDirection));
        Debug.DrawLine(hitboxpos, entityPhysics.transform.position, Color.cyan, 0.2f);
        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_STORM_ATTACKBRIGHTNESSCURVE));
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
                    obj.GetComponent<EntityPhysics>().Inflict(HEAVYMELEE_STORM_DAMAGE, force:aimDirection.normalized* HEAVYMELEE_STORM_FORCE, type:ElementType.ZAP);
                    obj.GetComponent<EntityPhysics>().Stagger();
                }
            }
            else if (obj.GetComponent<ProjectilePhysics>())
            {
                if (obj.GetComponent<ProjectilePhysics>().canBeDeflected &&
                    obj.GetComponent<ProjectilePhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && 
                    obj.GetComponent<ProjectilePhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                {
                    Vibrate(1.0f, 0.15f);
                    obj.GetComponent<ProjectilePhysics>().PlayerRedirect(aimDirection, "ENEMY", 60f);
                    FollowingCamera.GetComponent<CameraScript>().Jolt(2f, aimDirection * -1f);
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.2f, 10, 0.02f);
                }
            }
        }
    }

    IEnumerator PlayerHeavyMelee_Fire()
    {
        bIsSolFlailAttackCoroutineRunning = true;
        /*
         * PROPOSED BEHAVIOR:
         * Start with flail launching forward in direction of attack, dragging small hitbox
         * If the hitbox hits an enemy OR hits the maximum distance, ball fucking explodes
         * After short duration, ball retracts (to player? to floating hilt?)
         * 
         */
        float timer = 0.0f;
        bool HasHitEnemy = false;
        SolFlailProjectile.SetObjectElevation(entityPhysics.GetObjectElevation() + 1.0f);
        //SolFlailProjectile.transform.position = entityPhysics.transform.position;
        SolFlailProjectile.Velocity = Quaternion.AngleAxis(9.0f, Vector3.back) * AimAssist(HEAVYMELEE_SOL_AIMASSIST_RADIUS, HEAVYMELEE_SOL_AIMASSIST_DISTANCE);
        //SolFlailChain.positionCount = 2;
        //SolFlailChain.SetWidth(1.0f, 1.0f);
        //SolFlailChain.enabled = true;
        SolFlailProjectile.ObjectSprite.GetComponent<Animator>().Play("SolFlailBall_Embiggen");

        while (!HasHitEnemy && timer < HEAVYMELEE_SOL_MAXTRAVELTIME)
        {
            // velocity should prob ramp up to some value real fast. maybe make a float curve for this?
            SolFlailProjectile.Speed = HEAVYMELEE_SOL_MAXVELOCITY * HEAVYMELEE_SOL_VELOCITY_OVER_TIME_NORMALIZED.Evaluate(timer / HEAVYMELEE_SOL_MAXTRAVELTIME);
            SolFlailChain.SetPosition(0, entityPhysics.ObjectSprite.transform.position);
            SolFlailChain.SetPosition(1, SolFlailProjectile.ObjectSprite.transform.position);
            SolFlailChain.endWidth = 1;
            SolFlailChain.startWidth = 1;
            SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", timer / HEAVYMELEE_SOL_MAXTRAVELTIME * 5.0f + 1);
            // hitbox check
            Collider2D[] flailTouchedEntities = Physics2D.OverlapBoxAll(SolFlailProjectile.transform.position, new Vector2(3,3), 0.0f);
            foreach (Collider2D obj in flailTouchedEntities)
            {
                if (obj.GetComponent<EntityPhysics>() && obj.tag == "Enemy")
                {
                    HasHitEnemy = true;
                    break;
                }   
            }
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        // detonate
        SolFlailProjectile.Velocity = Vector2.zero;
        _weaponAudioSource.clip = SFX_SolFlail_Hit;
        _weaponAudioSource.Play();
        SolFlailProjectile.ObjectSprite.GetComponent<Animator>().Play("SolFlailBall_Explode");
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(SolFlailProjectile.transform.position, HEAVYMELEE_SOL_HITBOX_DETONATION, 0.0f);
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
                    obj.GetComponent<EntityPhysics>().Inflict(HEAVYMELEE_SOL_DAMAGE, force:aimDirection.normalized*HEAVYMELEE_SOL_FORCE, type:ElementType.FIRE);
                    obj.GetComponent<EntityPhysics>().Burn();
                    obj.GetComponent<EntityPhysics>().Stagger();
                }
            }
        }
        timer = 0.0f;
        Vector3 originalFlailScale = SolFlailProjectile.GlowSprite.transform.localScale;
        while (timer < 0.1f)
        {
            SolFlailChain.SetPosition(0, entityPhysics.ObjectSprite.transform.position);
            SolFlailChain.SetPosition(1, SolFlailProjectile.ObjectSprite.transform.position);
            timer += Time.deltaTime;
            SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", HEAVYMELEE_SOL_IMPACTBRIGHTNESSCURVE.Evaluate(timer / 0.1f));
            SolFlailProjectile.GlowSprite.transform.localScale = originalFlailScale + Vector3.one * HEAVYMELEE_SOL_IMPACTBRIGHTNESSCURVE.Evaluate(timer / 0.1f);
            yield return new WaitForEndOfFrame();
        }

        // recall
        timer = 0.0f;
        while (timer < HEAVYMELEE_SOL_RETURNTIME)
        {
            Vector2 vecToPlayer = entityPhysics.transform.position - SolFlailProjectile.transform.position;
            if (vecToPlayer.magnitude < 2.0f)
            {
                break;
            }
            SolFlailProjectile.Velocity = vecToPlayer;
            SolFlailProjectile.Speed = Mathf.Pow(timer*6.0f, 2) * HEAVYMELEE_SOL_RETURNACCELERATION;
            SolFlailChain.SetPosition(0, entityPhysics.ObjectSprite.transform.position);
            SolFlailChain.SetPosition(1, SolFlailProjectile.ObjectSprite.transform.position);
            SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", HEAVYMELEE_SOL_RECALLBRIGHTNESSCURVE.Evaluate(timer / HEAVYMELEE_SOL_RETURNTIME));
            SolFlailProjectile.GlowSprite.transform.localScale = originalFlailScale + Vector3.one * HEAVYMELEE_SOL_RECALLBRIGHTNESSCURVE.Evaluate(timer / HEAVYMELEE_SOL_RETURNTIME);

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        SolFlailProjectile.GlowSprite.transform.localScale = originalFlailScale;
        SolFlailProjectile.GlowSprite.material.SetFloat("_Opacity", 1.0f);
        SolFlailProjectile.Speed = 1.0f;
        SolFlailProjectile.ObjectSprite.GetComponent<Animator>().Play("SolFlailBall_Vanish");
        _weaponAudioSource.clip = SFX_SolFlail_Catch;
        _weaponAudioSource.Play();

        timer = 0;
        while (timer < 0.1f)
        {
            SolFlailChain.SetWidth(1 - timer / 0.1f, 1 - timer / 0.1f);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        SolFlailProjectile.transform.parent.gameObject.SetActive(false);
        SolFlailChain.enabled = false;
        bIsSolFlailAttackCoroutineRunning = false;
    }

    private IEnumerator PlayerHeavyMelee_Rift()
    {
        yield return new WaitForSeconds(HEAVYMELEE_RIFT_INFLICTTIME);

        Vector2 hitboxpos = (Vector2)entityPhysics.transform.position;
        Collider2D[] hitobjects = Physics2D.OverlapCircleAll(hitboxpos, HEAVYMELEE_RIFT_RADIUS);
        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_RIFT_ATTACKBRIGHTNESSCURVE));
        //Debug.DrawLine(hitboxpos, entityPhysics.transform.position, Color.cyan, 0.2f);
        foreach (Collider2D obj in hitobjects)
        {
            if (obj.GetComponent<EntityPhysics>() && obj.tag == "Enemy")
            {
                EntityPhysics enemyPhys = obj.GetComponent<EntityPhysics>();
                if (enemyPhys.GetTopHeight() > entityPhysics.GetBottomHeight() && enemyPhys.GetBottomHeight() < entityPhysics.GetTopHeight())
                {
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.5f, 10, 0.01f);
                    Vibrate( 1.0f, 0.3f);

                    ChangeEnergy(1);
                    Debug.Log("Owch!");
                    enemyPhys.Inflict(HEAVYMELEE_RIFT_DAMAGE, force: Quaternion.AngleAxis(60.0f, Vector3.forward) * (enemyPhys.transform.position - entityPhysics.transform.position).normalized * HEAVYMELEE_RIFT_FORCE, type:ElementType.VOID);
                    obj.GetComponent<EntityPhysics>().Stagger();
                }
            }
            else if (obj.GetComponent<ProjectilePhysics>())
            {
                if (obj.GetComponent<ProjectilePhysics>().canBeDeflected && 
                    obj.GetComponent<ProjectilePhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && 
                    obj.GetComponent<ProjectilePhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                {
                    Vibrate(1.0f, 0.15f);
                    obj.GetComponent<ProjectilePhysics>().PlayerRedirect(aimDirection, "ENEMY", 60f);
                    FollowingCamera.GetComponent<CameraScript>().Jolt(2f, aimDirection * -1f);
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.2f, 10, 0.02f);
                }
            }
        }
    }

    private IEnumerator PlayerHeavyMelee_Ichor()
    {
        if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
        CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_ICHOR_ATTACKBRIGHTNESSCURVE));

        yield return new WaitForSeconds(HEAVYMELEE_ICHOR_INFLICTTIME);

        Vector2 hitboxpos = (Vector2)entityPhysics.transform.position + thrustDirection * HEAVYMELEE_ICHOR_HITBOXOFFSET;
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(hitboxpos, HEAVYMELEE_ICHOR_HITBOX, Vector2.SignedAngle(Vector2.right, thrustDirection));
        Debug.DrawLine(hitboxpos, entityPhysics.transform.position, Color.cyan, 0.2f);
        Debug.DrawLine(hitboxpos + thrustDirection * HEAVYMELEE_ICHOR_HITBOX.x * 0.5f, hitboxpos - thrustDirection * HEAVYMELEE_ICHOR_HITBOX.x * 0.5f, Color.cyan, 3.0f);
        Debug.DrawLine(hitboxpos + Vector2.right * HEAVYMELEE_ICHOR_HITBOX.y * 0.5f, hitboxpos + Vector2.left * HEAVYMELEE_ICHOR_HITBOX.y * 0.5f, Color.red, 3.0f);

        foreach (Collider2D obj in hitobjects)
        {
            EntityPhysics enemyPhysics = obj.GetComponent<EntityPhysics>();
            if (enemyPhysics && obj.tag == "Enemy")
            {
                if (enemyPhysics.GetTopHeight() > entityPhysics.GetBottomHeight() && enemyPhysics.GetBottomHeight() < entityPhysics.GetTopHeight())
                {
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.5f, 10, 0.01f);
                    Vibrate(1.0f, 0.3f);

                    ChangeEnergy(1);
                    Debug.Log("Owch!");
                    enemyPhysics.Inflict(HEAVYMELEE_ICHOR_DAMAGE, force: aimDirection.normalized * HEAVYMELEE_ICHOR_FORCE, type: ElementType.ICHOR);
                    obj.GetComponent<EntityPhysics>().Stagger();
                    enemyPhysics.IchorCorrupt(2);                    
                }
            }
            else if (obj.GetComponent<ProjectilePhysics>())
            {
                if (obj.GetComponent<ProjectilePhysics>().canBeDeflected && 
                    obj.GetComponent<ProjectilePhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() && 
                    obj.GetComponent<ProjectilePhysics>().GetBottomHeight() < entityPhysics.GetTopHeight())
                {
                    Vibrate(1.0f, 0.15f);
                    obj.GetComponent<ProjectilePhysics>().PlayerRedirect(aimDirection, "ENEMY", 60f);
                    FollowingCamera.GetComponent<CameraScript>().Jolt(2f, aimDirection * -1f);
                    FollowingCamera.GetComponent<CameraScript>().Shake(0.2f, 10, 0.02f);
                }
            }
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
                    break;
                case ElementType.ICHOR:
                    LightRanged_Ichor();
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
            hitEntity.Inflict(1, force:aimDirection/* * 2.0f*/, type:ElementType.ZAP);
        }
        
        _lightRangedZap.SetupLine(new Vector3(entityPhysics.transform.position.x, entityPhysics.transform.position.y + projectileElevation, entityPhysics.transform.position.y - _projectileStartHeight), new Vector3(endPoint.x, endPoint.y + projectileElevation, endPoint.y - _projectileStartHeight));
        _lightRangedZap.Play(_lightRangedDuration * 0.5f);
        
        //Debug.Log(entityPhysics.transform.position);
        //Debug.Log(endPoint);
    }
    private void LightRanged_Ichor()
    {
        //will rework the ranged attack system and remove "weapons" eventually...
        SwapWeapon("SOUTH"); //Ichor is south
        aimDirection = AimAssist(RANGED_ICHOR_AIMASSIST_RADIUS, RANGED_ICHOR_AIMASSIST_DISTANCE);
        FireBullet();
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
        tempBullet.GetComponentInChildren<ProjectilePhysics>().ObjectSprite.transform.position = entityPhysics.ObjectSprite.transform.position;
        tempBullet.GetComponentInChildren<ProjectilePhysics>().Spawn();
    }

    #endregion

    //---------------------------------------------| Misc
    /*
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
    }*/

    private void PlayerChangeStyle()
    {
        characterAnimator.Play(STYLE_CHANGE_Anim);
        UpdateAimDirection();
        StateTimer -= Time.deltaTime;
        TimeSinceCombat = 0.0f;
        if (StateTimer < _changeStyleColorChangeTime && !_changeStyle_HasChanged)
        {
            _changeStyle_HasChanged = true;
            weaponSprite.enabled = true;
            weaponGlowSprite.enabled = true;
            weaponSprite.transform.position = entityPhysics.ObjectSprite.transform.position + new Vector3(0.375f, -0.375f, 1.0f);
            ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.0f, 0.2f, Color.white, _newStyle);
            switch (_newStyle)
            {
                case ElementType.ICHOR:
                    Shader.SetGlobalColor("_MagicColor", new Color(1.0f, 0.0f, 0.5f, 1f));
                    entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 1);
                    EyeGlowSprite.material.SetFloat("_CurrentElement", 1);
                    Vibrate(1f, 0.1f);
                    _currentStyle = ElementType.ICHOR;
                    _haloSprite.sprite = Halo_Ichor;
                    _lightRangedEnergyCost = 1;
                    weaponSprite.GetComponent<Animator>().Play("IchorBlade_Summon", 0, 0.0f);
                    weaponSprite.transform.right = Vector3.right;
                    if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                    CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_ICHOR_SUMMONBRIGHTNESSCURVE));
                    weaponGlowSprite.material.SetFloat("_CurrentElement", 1);
                    weaponGlowSprite.sprite = HEAVYMELEE_ICHOR_GLOWSPRITE;
                    break;
                case ElementType.FIRE:
                    Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.5f, 0f, 1f));
                    entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 2);
                    EyeGlowSprite.material.SetFloat("_CurrentElement", 2);
                    Vibrate(1f, 0.1f);
                    _currentStyle = ElementType.FIRE;
                    SwapWeapon("WEST");
                    _haloSprite.sprite = Halo_Fire;
                    _lightRangedEnergyCost = 2;
                    weaponSprite.GetComponent<Animator>().Play("SolFlail_Summon", 0, 0.0f);
                    weaponSprite.transform.right = Vector3.right;
                    if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                    CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_SOL_SUMMONBRIGHTNESSCURVE));
                    weaponGlowSprite.material.SetFloat("_CurrentElement", 2);
                    weaponGlowSprite.sprite = HEAVYMELEE_SOL_GLOWSPRITE;
                    break;
                case ElementType.VOID:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.5f, 0.0f, 1.0f, 1f));
                    entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 3);
                    EyeGlowSprite.material.SetFloat("_CurrentElement", 3);
                    Vibrate(1f, 0.1f);
                    _currentStyle = ElementType.VOID;
                    SwapWeapon("NORTH");
                    _haloSprite.sprite = Halo_Void;
                    _lightRangedEnergyCost = 2;
                    weaponSprite.GetComponent<Animator>().Play("RiftScythe_Summon", 0, 0.0f);
                    weaponSprite.transform.right = Vector3.right;
                    if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                    CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_RIFT_SUMMONBRIGHTNESSCURVE));
                    weaponGlowSprite.material.SetFloat("_CurrentElement", 3);
                    weaponGlowSprite.sprite = HEAVYMELEE_RIFT_GLOWSPRITE;
                    break;
                case ElementType.ZAP:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.0f, 1.0f, 0.5f, 1f));
                    entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 4);
                    EyeGlowSprite.material.SetFloat("_CurrentElement", 4);
                    Vibrate(1f, 0.1f);
                    _currentStyle = ElementType.ZAP;
                    _haloSprite.sprite = Halo_Zap;
                    _lightRangedEnergyCost = 1;
                    weaponSprite.GetComponent<Animator>().Play("StormSpear_Summon", 0, 0.0f);
                    weaponSprite.transform.right = Vector3.right;
                    if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                    CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_STORM_SUMMONBRIGHTNESSCURVE));
                    weaponGlowSprite.material.SetFloat("_CurrentElement", 4);
                    weaponGlowSprite.sprite = HEAVYMELEE_STORM_GLOWSPRITE;
                    break;
                default:
                    Shader.SetGlobalColor("_MagicColor", new Color(0.5f, 1.0f, 0.0f, 1f));
                    ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(0.5f, 1.0f, 0.0f));
                    Vibrate(1f, 0.1f);
                    Debug.LogError("HOWDY : Somehow, the player changed style to a nonexistent style!");
                    break;
            }
        }

        // early cancellation
        if (StateTimer < 0.5f)
        {
            if (Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2)
            {
                //Debug.Log("IDLE -> RUN");
                CurrentState = PlayerState.RUN;
                _changeStyle_HasChanged = false;
            }
            if (controller.GetButtonDown("Jump"))
            {
                //Debug.Log("IDLE -> JUMP");
                Vibrate(.5f, 0.05f);
                entityPhysics.ZVelocity = JumpImpulse;
                CurrentState = PlayerState.JUMP;
                _changeStyle_HasChanged = false;
            }

            if (controller.GetButtonDown("Melee"))
            {
                hasSwung = false;
                //Debug.Log("IDLE -> ATTACK");
                StateTimer = time_lightMelee;
                CurrentState = PlayerState.LIGHT_MELEE;
                _changeStyle_HasChanged = false;
            }

            if (controller.GetButton("RangedAttack"))
            {
                PlayerLightRangedTransitionAttempt();
                _changeStyle_HasChanged = false;
            }
        }

        if ( StateTimer < 0 /*|| ( !controller.GetButton("ChangeStyle_Fire") && !controller.GetButton("ChangeStyle_Void") && !controller.GetButton("ChangeStyle_Zap") )*/ )
        { 
            CurrentState = PlayerState.RUN;
            _changeStyle_HasChanged = false;
        }
        if (controller.GetButtonDown("Blink"))
        {
            PlayerBlinkTransitionAttempt();
            _changeStyle_HasChanged = false;
        }
    }

    private void PlayerRest()
    {
        // TODO : can we do this for rest platform?
        CurrentRestPlatform = entityPhysics.currentNavEnvironmentObject.GetComponent<RestPlatform>();
        entityPhysics.SnapToFloor();

        if (!isStanding && StateTimer > 0) // transition INTO rest state
        {
            characterAnimator.Play("PlayerRestTransition");
            _rest_recover_energy_timer = _rest_recover_energy_duration;
            _rest_recover_health_timer = _rest_recover_health_duration;
        }
        else if (isStanding && StateTimer > 0) // transition OUT OF rest state
        {
            characterAnimator.Play("PlayerRestStanding");
            if (CurrentRestPlatform && CurrentRestPlatform.IsActivated)
            {
                CurrentRestPlatform.OnDeactivated.Invoke();
                CurrentRestPlatform.IsActivated = false;
                CurrentRestPlatform.IsActionPressed = false;
            }
        }
        else if (!isStanding)
        {
            characterAnimator.Play("PlayerRestLoop");
            if (_rest_recover_health_timer < 0 && entityPhysics.GetCurrentHealth() < entityPhysics.GetMaxHealth())
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
                //Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2 ||
                controller.GetButtonDown("Jump") ||
                //controller.GetButtonDown("Melee") ||
                controller.GetButton("RangedAttack") ||
                controller.GetButtonDown("Blink") ||
                controller.GetButtonDown("Heal")
                )
            {
                StateTimer = rest_kneel_duration;
                isStanding = true;
                CurrentRestPlatform.OnActionReleased.Invoke();
                CurrentRestPlatform.InputDirection = new Vector2(0, 0);
            }

            if (CurrentRestPlatform) // control rest platform
            {
                CurrentRestPlatform.IsPlayerRestingOn = true;
                if (!CurrentRestPlatform.IsActivated)
                {
                    CurrentRestPlatform.OnActivated.Invoke();
                    CurrentRestPlatform.IsActivated = true;
                }
                if (CurrentRestPlatform.DoesPlatformUseActionPress)
                {
                    if (controller.GetButtonDown("Melee"))
                    {
                        CurrentRestPlatform.OnActionPressed.Invoke();
                        CurrentRestPlatform.SetTargetGlowAmount(1.0f);
                        CurrentRestPlatform.IsActionPressed = true;
                        Debug.Log("Rest platform PRESSED!");
                    }
                    if (controller.GetButtonUp("Melee"))
                    {
                        CurrentRestPlatform.OnActionReleased.Invoke();
                        CurrentRestPlatform.SetTargetGlowAmount(0.5f);
                        CurrentRestPlatform.IsActionPressed = false;
                    }
                }
                if (CurrentRestPlatform.DoesPlatformUseInputDirection)
                {
                    if (Mathf.Abs(xInput) > 0.2 || Mathf.Abs(yInput) > 0.2)
                    {
                        CurrentRestPlatform.InputDirection = new Vector2(xInput, yInput);
                        CurrentRestPlatform.OnDirectionInputReceived.Invoke();
                    }
                    else
                    {
                        CurrentRestPlatform.InputDirection = new Vector2(0, 0);
                    }
                }
            }
        }
        else
        {
            if (CurrentRestPlatform) CurrentRestPlatform.IsPlayerRestingOn = false;
            currentFaceDirection = FaceDirection.SOUTH;
            CurrentState = PlayerState.IDLE;
            isStanding = false;
        }
        StateTimer -= Time.deltaTime;

        #region IDLE state transitions (copied)

        CheckStyleChange();

        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        if (entityPhysics.GetObjectElevation() > maxheight + 0.1f) //override other states to trigger fall
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

    private void Player_Collapsed()
    {
        entityPhysics.SnapToFloor();
        entityPhysics.MoveAvoidEntities(Vector2.zero);
        // player is forced into collapse animation, stays there until allowed back
        if (StateTimer == 0.0f)
        {
            characterAnimator.Play("PlayerCollapse");
        }
        StateTimer += Time.deltaTime;

        float maxheight = entityPhysics.GetMaxTerrainHeightBelow();
        entityPhysics.SetObjectElevation(maxheight);
    }

    private void Player_StandingFromCollapsed()
    {
        entityPhysics.SnapToFloor();
        entityPhysics.MoveAvoidEntities(Vector2.zero);
        if (StateTimer == 0.0f)
        {
            characterAnimator.Play("PlayerStandFromCollapse");
        }
        if (StateTimer > COLLAPSE_STAND_DURATION)
        {
            CurrentState = PlayerState.IDLE;
            currentFaceDirection = FaceDirection.SOUTH;
        }
        StateTimer += Time.deltaTime;
    }

    //================================================================================| TRANSITIONS

    private void PlayerLightRangedTransitionAttempt()
    {
        if (CurrentEnergy < _lightRangedEnergyCost)
        {
            return;
        }

        ChangeEnergy(_lightRangedEnergyCost * -1);
        StateTimer = 0f;
        CurrentState = PlayerState.LIGHT_RANGED;
    }

    private void PlayerBlinkTransitionAttempt()
    {
        if (_blinkTimer > BLINK_COOLDOWN)
        {
            if (CurrentState == PlayerState.JUMP) _hasAlreadyBlinkedInMidAir = true;
            CurrentState = PlayerState.BLINK;
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
        switch (_currentStyle)
        {
            case ElementType.ZAP:
                weaponSprite.gameObject.transform.parent = null;
                weaponSprite.transform.right = AimAssist(HEAVYMELEE_STORM_AIMASSIST_RADIUS, HEAVYMELEE_STORM_AIMASSIST_DISTANCE);
                weaponSprite.GetComponent<Animator>().Play("StormSpear_Stab", 0, 0.0f);
                break;
            case ElementType.VOID:
                weaponSprite.transform.right = -aimDirection;
                weaponSprite.transform.localPosition = weaponSprite.transform.right * 3;
                weaponSprite.gameObject.transform.parent = null;
                weaponSprite.GetComponent<Animator>().Play("RiftScythe_Slash", 0, 0.0f);
                break;
            case ElementType.FIRE:
                weaponSprite.transform.right = aimDirection;
                weaponSprite.gameObject.transform.parent = null;
                //weaponSprite.GetComponent<Animator>().Play("SolFlail_Throw", 0, 0.0f);
                // todo : chain stuff
                break;
            case ElementType.ICHOR:
                // TODO
                weaponSprite.transform.right = -aimDirection;
                weaponSprite.transform.localPosition = /*weaponSprite.transform.right * 1 +*/ weaponSprite.transform.up * 2;
                weaponSprite.gameObject.transform.parent = null;
                weaponSprite.GetComponent<Animator>().Play("IchorBlade_Slash", 0, 0.0f);
                break;
        }

        //Debug.Log("POW!");
        //HeavyMeleeSprite.GetComponent<SpriteRenderer>().enabled = true;
        //HeavyMeleeSprite.GetComponent<SpriteRenderer>().flipX = flip;
        //HeavyMeleeSprite.GetComponent<Animator>().PlayInFixedTime(0);

        //HeavyMeleeSprite.GetComponent<Animator>().Play("HeavyMeleeSwing!!!!!");
        yield return new WaitForSeconds(_lengthOfHeavyMeleeAnimation);
        //HeavyMeleeSprite.GetComponent<SpriteRenderer>().enabled = false;
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
        if (_currentEnergy > _maxEnergy)
        {
            _currentEnergy = _maxEnergy;
        }
        else if (_currentEnergy < 0)
        {
            TimeSinceCombat = 0.0f;
            _currentEnergy = 0;
        }
        else
        {
            TimeSinceCombat = 0.0f;
        }
    }

    IEnumerator PlayDeathAnimation(Vector2 killingBlowDirection)
    {
        Debug.Log("Playing death");
        // flash that black + white final slam, freeze time for a moment
        _audioSource.clip = SFX_Death;
        _audioSource.Play();
        _audioSource.outputAudioMixerGroup = AudioMixer_Unducked;
        AudioMixer_Ducked.audioMixer.SetFloat("FreezeFrameVolume", -80.0f);

        _deathFlash.SetBlammoDirection(killingBlowDirection);
        _deathFlash.transform.position = new Vector3(characterSprite.transform.position.x, characterSprite.transform.position.y, FollowingCamera.transform.position.z + 2.0f);
        SpriteRenderer[] renderers = _deathFlash.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            /*if (renderers[i].name != "Background")*/ renderers[i].enabled = true;
        }

        _gameplayUI.GetComponent<CanvasGroup>().alpha = 0.0f; // Disable visibility game ui for a moment 

        yield return new WaitForEndOfFrame();
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.0f);

        Time.timeScale = 0.5f; // -------------| RESUME TIME |--------------

        _gameplayUI.GetComponent<CanvasGroup>().alpha = 1.0f;
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
        characterAnimator.Play(DEATH_WEST_Anim);

        yield return new WaitForSeconds(1.0f);
        Time.timeScale = 1.0f;
        _fadeTransition.FadeToScene(SceneManager.GetActiveScene().name, "");
        // play death animation, stop for a bit, then fade to black
    }

    public ElementType GetElementalAttunement()
    {
        return _currentStyle;
    }

    /// <summary>
    /// "Permanently" reduce the amount of HP available to the player. Occurs in the campaign when the player destroys power cores in the Monolith
    /// </summary>
    public void ShatterHealth()
    {
        int newMaxHP = entityPhysics.GetMaxHealth() - 1;
        entityPhysics.SetMaxHealth(newMaxHP);
        _healthBar.ShatterHealthBarSegment(newMaxHP);
        NumberOfShatteredHealthBlocks++;
    }

    public Vector2 GetLookDirection()
    {
        return aimDirection;
    }
    public Vector2 AimAssist(float radius, float distance)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(entityPhysics.GetComponent<Rigidbody2D>().position + aimDirection * radius, radius: radius, aimDirection, distance);
        float projectileElevation = entityPhysics.GetBottomHeight() + _projectileStartHeight;
        float shortestDistance = float.MaxValue;
        Vector2 endPoint = Vector2.zero;
        for (int i = 0; i < hits.Length; i++)
        {
            //check if is target entity
            //check to see if projectile z is within z bounds of physicsobject
            //check to see if distance to target is shorter than shortestDistance

            if (!hits[i].collider.GetComponent<EntityPhysics>()) //can object be collided with
            {
                continue;
            }
            PhysicsObject other = hits[i].collider.GetComponent<PhysicsObject>();
            if (other.GetBottomHeight() < projectileElevation && other.GetTopHeight() > projectileElevation) //is projectile height within z bounds of object
            {
                if (other.tag != "Enemy") continue; //changed to reflect new circlecast
                if (hits[i].distance < shortestDistance)
                {
                    return (other.transform.position - entityPhysics.transform.position).normalized;
                }
            }
        }

        return aimDirection;
    }

    public void CollapsePlayer()
    {
        StateTimer = 0.0f;
        CurrentState = PlayerState.COLLAPSE;
    }

    public void StandFromCollapsePlayer()
    {
        StateTimer = 0.0f;
        CurrentState = PlayerState.STAND_FROM_COLLAPSE;
    }

    public void ForceStandFromRest()
    {
        if (CurrentState == PlayerState.REST)
        {
            isStanding = true;
            StateTimer = rest_kneel_duration;
        }
    }

    public void OvertakeElement(ElementType newElement) // Forces player into one element
    {
        OvertakenElement = newElement;
        switch (newElement)
        {
            case ElementType.ICHOR:
                Shader.SetGlobalColor("_MagicColor", new Color(1.0f, 0.0f, 0.5f, 1f));
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 1);
                EyeGlowSprite.material.SetFloat("_CurrentElement", 1);
                Vibrate(1f, 0.1f);
                _currentStyle = ElementType.ICHOR;
                _haloSprite.sprite = Halo_Ichor;
                _lightRangedEnergyCost = 1;
                weaponSprite.GetComponent<Animator>().Play("IchorBlade_Summon", 0, 0.0f);
                weaponSprite.transform.right = Vector3.right;
                if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_ICHOR_SUMMONBRIGHTNESSCURVE));
                weaponGlowSprite.material.SetFloat("_CurrentElement", 1);
                weaponGlowSprite.sprite = HEAVYMELEE_ICHOR_GLOWSPRITE;
                break;
            case ElementType.ZAP:
                Shader.SetGlobalColor("_MagicColor", new Color(0.0f, 1.0f, 0.5f, 1f));
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 4);
                EyeGlowSprite.material.SetFloat("_CurrentElement", 4);
                Vibrate(1f, 0.1f);
                _currentStyle = ElementType.ZAP;
                _haloSprite.sprite = Halo_Zap;
                _lightRangedEnergyCost = 1;
                weaponSprite.GetComponent<Animator>().Play("StormSpear_Summon", 0, 0.0f);
                weaponSprite.transform.right = Vector3.right;
                if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_STORM_SUMMONBRIGHTNESSCURVE));
                weaponGlowSprite.material.SetFloat("_CurrentElement", 4);
                weaponGlowSprite.sprite = HEAVYMELEE_STORM_GLOWSPRITE;
                break; 
                /*
            case ElementType.FIRE:
                Shader.SetGlobalColor("_MagicColor", new Color(1f, 0.5f, 0f, 1f));
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 2);
                EyeGlowSprite.material.SetFloat("_CurrentElement", 2);
                Vibrate(1f, 0.1f);
                _currentStyle = ElementType.FIRE;
                SwapWeapon("WEST");
                _haloSprite.sprite = Halo_Fire;
                _lightRangedEnergyCost = 2;
                weaponSprite.GetComponent<Animator>().Play("SolFlail_Summon", 0, 0.0f);
                weaponSprite.transform.right = Vector3.right;
                if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_SOL_SUMMONBRIGHTNESSCURVE));
                weaponGlowSprite.material.SetFloat("_CurrentElement", 2);
                weaponGlowSprite.sprite = HEAVYMELEE_SOL_GLOWSPRITE;
                break;
            case ElementType.VOID:
                Shader.SetGlobalColor("_MagicColor", new Color(0.5f, 0.0f, 1.0f, 1f));
                entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 3);
                EyeGlowSprite.material.SetFloat("_CurrentElement", 3);
                Vibrate(1f, 0.1f);
                _currentStyle = ElementType.VOID;
                SwapWeapon("NORTH");
                _haloSprite.sprite = Halo_Void;
                _lightRangedEnergyCost = 2;
                weaponSprite.GetComponent<Animator>().Play("RiftScythe_Summon", 0, 0.0f);
                weaponSprite.transform.right = Vector3.right;
                if (CurrentWeaponGlowCoroutine != null) StopCoroutine(CurrentWeaponGlowCoroutine);
                CurrentWeaponGlowCoroutine = StartCoroutine(PlayWeaponSpriteGlow(HEAVYMELEE_RIFT_SUMMONBRIGHTNESSCURVE));
                weaponGlowSprite.material.SetFloat("_CurrentElement", 3);
                weaponGlowSprite.sprite = HEAVYMELEE_RIFT_GLOWSPRITE;
                break;*/
            default:
                Shader.SetGlobalColor("_MagicColor", new Color(0.5f, 1.0f, 0.0f, 1f));
                ScreenFlash.InstanceOfScreenFlash.PlayFlash(.5f, .1f, new Color(0.5f, 1.0f, 0.0f));
                Vibrate(1f, 0.1f);
                Debug.LogError("HOWDY : Somehow, the player changed style to a nonexistent style!");
                break;
        }

    }

    private IEnumerator PlayWeaponSpriteGlow(AnimationCurve glowcurve)
    {
        float duration = glowcurve.keys[glowcurve.length - 1].time;
        float timer = duration;
        while (timer > 0.0f)
        {
            weaponGlowSprite.material.SetFloat("_Opacity", glowcurve.Evaluate(1 - timer / duration));
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        weaponGlowSprite.material.SetFloat("_Opacity", glowcurve.Evaluate(glowcurve.keys[glowcurve.length - 1].time));
    }

    private IEnumerator FadeInAudio()
    {
        float duration = 1.0f;
        float startVolume = -80.0f; // this is as low as the dB scale goes
        float endVolume = 0.0f; // unaltered volume
        float timer = 0.0f;
        while (timer < duration)
        {
            AudioMixer_Ducked.audioMixer.SetFloat("FreezeFrameVolume", Mathf.Lerp(startVolume, endVolume, AudioFadeInCurve.Evaluate(timer / duration)));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        AudioMixer_Ducked.audioMixer.SetFloat("FreezeFrameVolume", endVolume);
    }

    public void ForceHideUI()
    {
        _healthBar.transform.parent.gameObject.SetActive(false);
    }
    public void ForceShowUI()
    {
        _healthBar.transform.parent.gameObject.SetActive(true);
    }
}