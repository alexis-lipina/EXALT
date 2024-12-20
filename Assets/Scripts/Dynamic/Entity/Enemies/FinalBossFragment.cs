﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FinalBossFragment : EntityHandler
{
    [SerializeField] BossHealthBarManager _bossHealthManager;
    [SerializeField] string _bossName;
    [SerializeField] MovingEnvironment _mainEnvironmentObject;
    [SerializeField] List<MovingEnvironment> EnvironmentObjects;
    [SerializeField] TriggerVolume PatrolArea; // idle unless player is within this volume
    [SerializeField] RestPlatform SuperlaserRestPlatform;
    [SerializeField] Animator AttackFlash;
    [SerializeField] List<BetterEnemySpawner> EnemySpawners;
    [SerializeField] List<EnemySpawner> SimpleEnemySpawners;
    [SerializeField] List<SpriteRenderer> TEMP_SmallLaserVFX; // for now we're just gonna flash these really quickly
    [SerializeField] int FragmentNumber = 0;
    [SerializeField] float CrystalHailDuration = 8.0f;
    [SerializeField] float SmallLaserCooldown = 3.0f;
    [SerializeField] FinalBossCore bossCore;
    [SerializeField] AudioClip DeathScreenAmbience;
    [SerializeField] AudioClip FragmentCombatMusic;
    private bool bIsBossDoingSomething = false; // prevents the boss from doing multiple things at a time like a local AOE and also a volley, which probs would suck
    private Vector2 CenterPosition;

    [Space(10)]
    [Header("Superlaser")]
    //[SerializeField] AnimationCurve ;

    [Space(10)]
    [Header("Storm VFX")] // ALL vfx that occur when you charge the lightning bolt to destroy this guy
    [SerializeField] ZapFXController TopLightningBolt;
    [SerializeField] public GameObject TopBoltZapPoint; // point on the crystal that should be hit with the zap
    [SerializeField] ZapFXController BottomLightningBolt;
    [SerializeField] public GameObject BottomBoltZapPoint; // point on the crystal that should be hit with the zap
    [SerializeField] List<ZapFXController> RandomAmbientLightningBolts;
    [SerializeField] private AnimationCurve BoltDelayOverCharge;
    [SerializeField] private AnimationCurve BoltSizeOverCharge;
    [SerializeField] private AnimationCurve BoltDistanceOverCharge;
    [SerializeField] private AnimationCurve BoltDurationOverCharge;
    [SerializeField] public SpriteRenderer LightningFlashGlow;
    public SpriteRenderer FullFragmentGlowFX;

    [SerializeField] private PlayerProjection _projection;
    [SerializeField] private DynamicPhysics _physics;

    [SerializeField] float RaisedElevation = 18.0f;
    [SerializeField] float LoweredElevation = 1.0f;
    bool IsRaised = true;
    bool IsLowered = false; // if both are false, it is transitioning

    [Space(10)]
    [Header("Spear Phase")]
    private Weapon SpearProjectileWeapon; // why the hell did I design this this way
    [SerializeField] int SpearsPerVolley; // Number of spears in a volley
    [SerializeField] float VolleySpawnDelay; // delay between each projectile spawning in a volley
    [SerializeField] float VolleySpawnToFireDelay; // delay between the spawn and fire stage of a volley
    [SerializeField] float VolleyFireDelay; // delay between each projectile firing in a volley
    [SerializeField] float VolleyCooldown; // delay between volleys
    [SerializeField] float SpearSpeed; 
    [SerializeField] float AOEDelay_SpearPhase; 
    bool bVolleyReady = true;

    // TODO : particle system?

    enum FragmentPhase { ORBITING, LOWER_SPEARS, SPAWN_ENEMIES, CRYSTAL_HAIL, SUPERLASER }; //phases are sequenced & describe a small encounter with the boss.
    enum FragmentState { INACTIVE, IDLE, CHASE, FIRING, SUPERLASER, DEATH };
    private FragmentPhase CurrentPhase;
    private FragmentState CurrentState;
    private PlayerHandler _player;
    private BoxCollider2D _mainEnvironmentObjectCollider;
    private float ChaseMaxSpeed = 7.0f;
    private float Acceleration = 10.0f;
    private Vector2 PreviousVelocity;

    private float _superlaserCharge = 0.0f;
    private float _superlaserChargeRate = 0.125f; // superlasers per second

    private float _phaseTimer = 0.0f;
    private bool bReadyToAttack = true;

    private Coroutine _volleyCoroutine;

    private Vector2 WanderPosition;
    [SerializeField] private Vector2 WanderAreaSize;
    [SerializeField] private Vector2 WanderAreaCenter;
    [SerializeField] private float WanderAcceleration = 7.0f;
    [SerializeField] private float WanderMaxSpeed = 10.0f;
    [SerializeField] private float WanderTargetSwitchDelay = 4.0f;
    private float WanderTimer = 0.0f;

    [Space(10)]
    [Header("Hail")]
    [SerializeField] Transform HailPrefab;
    //[SerializeField] float HailDuration = 8.0f;
    [SerializeField] AnimationCurve HailDelayOverHealth;
    [SerializeField] Vector2 HailArea;
    [SerializeField] Vector2 HailCenter;
    [SerializeField] float TargetedHailRadius;
    [SerializeField] AnimationCurve TargetedHailDelayOverHealth;
    bool bIsHailing = false;


    [SerializeField] private float DescentDuration = 4.0f;
    [SerializeField] private AnimationCurve DescentCurve;

    [SerializeField] private SpriteRenderer WeakeningSprite;

    [SerializeField] private List<FragmentPhase> OrderOfPhases;


    [Space(10)]
    [Header("Death")]
    [SerializeField] FragmentDeathShatterController deathShatterController;
    [SerializeField] FragmentDeathShatterController healthBarShatterController;
    [SerializeField] Texture2D PP_ShatterTex_Mask;
    [SerializeField] Texture2D PP_ShatterTex_Cracks;
    [SerializeField] Texture2D PP_ShatterTex_Offsets;
    [SerializeField] Texture2D PP_White;
    [SerializeField] Texture2D PP_Black;
    [SerializeField] GameObject ShatterHealthSystem;
    [SerializeField] GameObject ShatterHealthBarBlock;
    [SerializeField] Sprite ShatterHealthBarNewTexture;
    [SerializeField] EnvironmentPhysics CorpsePositionEnvtObj; // used to position player relative to the fragment
    [SerializeField] SpriteRenderer CorpseSprite;
    [SerializeField] AudioClip DeathSound;
    [SerializeField] AudioClip FadeInSound;
    [SerializeField] List<EndingGlitchUIText> GlitchTextPopups;
    [SerializeField] AnimationCurve GlitchIntensityCurve;
    [SerializeField] AnimationCurve GlitchMaskCurve;
    [SerializeField] FadeTransition fadeTransition;
    [SerializeField] private AudioMixerGroup DuckedSFXMixer;
    [SerializeField] private AudioMixerGroup UnduckedSFXMixer;
    [SerializeField] private List<EnvironmentPhysics> holeUnderneath; // for holes
    [SerializeField] private List<CollapsingPlatform> cascadingFloorStarts;
    [SerializeField] private EnvironmentPhysics newPlayerFallSavePoint;
    [SerializeField] private Vector2 PlayerAwakenLocation;

    bool bIsDead = false;
    private CameraScript _camera;


    [Space(10)]
    [SerializeField] AudioClip ProjectileSummonSFX;
    [SerializeField] AudioClip ProjectileLaunchSFX;


    // Start is called before the first frame update
    void Start()
    {
        //OrderOfPhases = new List<FragmentPhase>();
        //OrderOfPhases.Add(FragmentPhase.ORBITING);
        //OrderOfPhases.Add(FragmentPhase.LOWER_SPEARS);
        //OrderOfPhases.Add(FragmentPhase.SPAWN_ENEMIES);
        //OrderOfPhases.Add(FragmentPhase.CRYSTAL_HAIL);
        //OrderOfPhases.Add(FragmentPhase.SUPERLASER);
        _player = GameObject.FindObjectOfType<PlayerHandler>();
        //_mainEnvironmentObjectCollider = _mainEnvironmentObject.GetComponent<BoxCollider2D>();
        CurrentState = FragmentState.IDLE;
        foreach (var rend in TEMP_SmallLaserVFX)
        {
            rend.enabled = false;
        }
        AttackFlash.GetComponent<SpriteRenderer>().enabled = false;
        currentPrimes = new List<ElementType>();
        SpearProjectileWeapon = (Weapon)ScriptableObject.CreateInstance("BossSpearLauncher");
        SpearProjectileWeapon.PopulateBulletPool();
        CurrentPhase = OrderOfPhases[0];
        CenterPosition = SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position;
        SetElevation(RaisedElevation);
        _camera = FindObjectOfType<CameraScript>();
        if (healthBarShatterController)
        {
            //healthBarShatterController.gameObject.SetActive(false);
        }
        CorpseSprite.enabled = false;
        LightningFlashGlow.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        //ExecuteState();
        _phaseTimer += Time.deltaTime;
        ExecutePhase();

        WeakeningSprite.material.SetFloat("_FadeInMask", (1 - ( entityPhysics.GetCurrentHealth() / (float)entityPhysics.GetMaxHealth())) * 0.5f);

        if (entityPhysics.GetCurrentHealth() <= 0 && !bIsDead)
        {
            StartCoroutine(Death());
        }
    }
    void ExecutePhase()
    {
        if (IsLowered && entityPhysics.GetCurrentHealth() > 0) ExecuteFragmentSpecificState();
        return;
        switch (CurrentPhase)
        {
            case FragmentPhase.LOWER_SPEARS:
                //lower boss to ground, chase player on the ground, fire projectiles intermittently
                if (IsRaised) StartCoroutine(RaiseOrLower(false, 1.0f));
                if (bVolleyReady && !bIsBossDoingSomething) _volleyCoroutine = StartCoroutine(FireVolley());
                ProximityLaserAOE();
                //ChasePlayer();
                Wander();
                break;
            case FragmentPhase.CRYSTAL_HAIL:
                if (!bIsHailing) StartCoroutine(HailPlayer());
                break;
            case FragmentPhase.SUPERLASER:
                State_Superlaser();
                break;
            default:
                break;
        }
    }

    void ExecuteFragmentSpecificState()
    {
        switch (FragmentNumber)
        {
            case 1:
                Wander();
                ProximityLaserAOE();
                break;
            case 2:
                Wander();
                ProximityLaserAOE();
                if (bVolleyReady && !bIsBossDoingSomething) _volleyCoroutine = StartCoroutine(FireVolley());
                foreach (EnemySpawner spawner in SimpleEnemySpawners)
                {
                    spawner.IsAutomaticallySpawning = true;
                }
                break;
            case 3:
                if (entityPhysics.GetCurrentHealth() > entityPhysics.GetMaxHealth() / 2)
                {
                    if (!bIsHailing) StartCoroutine(HailPlayer());
                }
                else
                {
                    Wander();
                    ProximityLaserAOE();
                    if (bVolleyReady && !bIsBossDoingSomething) _volleyCoroutine = StartCoroutine(FireVolley());
                    foreach (EnemySpawner spawner in SimpleEnemySpawners)
                    {
                        spawner.IsAutomaticallySpawning = true;
                    }
                }
                break;
            case 4:
                Wander();
                if (!bIsHailing)
                {
                    StartCoroutine(HailRandom());
                    StartCoroutine(HailPlayer());
                }
                break;
        }
    }

    void State_Inactive() // before boss fight starts, where this object should be controlled by other stuff going on probably
    {
        
    }
    void State_Idle() // idle until player enters their area, at which point the player should be sealed off and isolated to their area
    {
        if (PatrolArea.IsTriggered)
        {
            CurrentPhase = OrderOfPhases[0];
            BeginPhase();

            CurrentState = FragmentState.CHASE;
        }
    }
    void ChasePlayer() // follow the player, try to kill them
    {
        Vector2 offset = _player.GetEntityPhysics().transform.position - entityPhysics.transform.position;
        if (Mathf.Abs(offset.x) < 8.0f && Mathf.Abs(offset.y) < 6.0f)
        {

            {
                if (bReadyToAttack && !bIsBossDoingSomething)
                {
                    StartCoroutine(FireSmallLaser(CurrentPhase == FragmentPhase.LOWER_SPEARS ? AOEDelay_SpearPhase : SmallLaserCooldown));
                }
                offset = Vector2.zero;
            }
            // fire!
        }

        offset.Normalize();
        PreviousVelocity += offset * Acceleration * Time.deltaTime;
        if (PreviousVelocity.magnitude > ChaseMaxSpeed)
        {
            PreviousVelocity.Normalize();
            PreviousVelocity *= ChaseMaxSpeed;
        }
        MoveLateral(PreviousVelocity * Time.deltaTime);
    }

    void ProximityLaserAOE()
    {
        Vector2 offset = _player.GetEntityPhysics().transform.position - entityPhysics.transform.position;
        if (Mathf.Abs(offset.x) < 8.0f && Mathf.Abs(offset.y) < 6.0f)
        {
            if (bReadyToAttack && !bIsBossDoingSomething)
            {
                StartCoroutine(FireSmallLaser(CurrentPhase == FragmentPhase.LOWER_SPEARS ? AOEDelay_SpearPhase : SmallLaserCooldown));
            }
        }
    }

    void Wander()
    {
        Vector2 offset = WanderPosition - (Vector2)entityPhysics.transform.position;
        if (offset.sqrMagnitude < 1 || WanderPosition.sqrMagnitude == 0 || WanderTimer > WanderTargetSwitchDelay) // get new random point
        {
            WanderTimer = 0.0f;
            WanderPosition = CenterPosition + WanderAreaCenter + new Vector2(Random.Range(-1.0f, 1.0f) * WanderAreaSize.x, Random.Range(-1.0f, 1.0f) * WanderAreaSize.y);
        }
        offset.Normalize();
        PreviousVelocity += offset * WanderAcceleration * Time.deltaTime;
        if (PreviousVelocity.magnitude > WanderMaxSpeed)
        {
            PreviousVelocity.Normalize();
            PreviousVelocity *= WanderMaxSpeed;
        }
        MoveLateral(PreviousVelocity * Time.deltaTime);
        WanderTimer += Time.deltaTime;
    }

    void State_Superlaser()
    {
        Vector2 offset = SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position - entityPhysics.transform.position;
        if (offset.magnitude > 0.1f)
        {
            offset.Normalize();
            offset *= ChaseMaxSpeed / 2.0f;
            MoveLateral(offset * Time.deltaTime);
        }
        else if (offset.magnitude != 0.0f)
        {
            MoveLateral(offset);
        }

        if (!IsRaised && IsLowered)
        {
            StartCoroutine(RaiseOrLower(true, 1.0f));
        }

        if (!IsRaised) return; // --- dont do shit after this line until we're fully raised

        // we are over it. it's superlaser time.
        if (_superlaserCharge == 0.0f)
        {
            //_superlaserCoroutine = StartCoroutine(FireSuperlaser());
            //StartCoroutine(LightningBuildupVFX());
        }
        _superlaserCharge += _superlaserChargeRate * Time.deltaTime;

        if (SuperlaserRestPlatform.CurrentChargeAmount == 1.0f) // you win!
        {
            // self destruct
            StopAllCoroutines();
            CurrentState = FragmentState.DEATH;
            StartCoroutine(Death());
        }
        if (_superlaserCharge > 1.0f) // you lose!
        {
            //_player.GetEntityPhysics().Inflict(1000, hitPauseDuration:0.25f);
        }
    }

    void MoveLateral(Vector2 vector)
    {
        //_mainEnvironmentObject.transform.position += new Vector3(vector.x, vector.y, vector.y); // there should be a generic move function for objects already
        foreach (MovingEnvironment entry in EnvironmentObjects)
        {
            //entry.transform.position += new Vector3(vector.x, vector.y, vector.y); 
        }
        //BeneathRestPlatform.
        _projection.transform.position += new Vector3(vector.x, vector.y, vector.y);
        //vector = entityPhysics.MoveAvoidEntities(vector); // dont actually want to avoid entities - player should get pushed by the boss
        entityPhysics.MoveWithCollision(vector.x, vector.y);
    }

    void MoveVertical(float offset)
    {
        //_mainEnvironmentObject.AddElevationOffset(offset);
        foreach (MovingEnvironment entry in EnvironmentObjects)
        {
            //entry.AddElevationOffset(offset);
        }
        entityPhysics.SetElevation(entityPhysics.GetBottomHeight() + offset);
    }

    void SetElevation(float newElevation)
    {
        float oldElevation = entityPhysics.GetBottomHeight();

        //_mainEnvironmentObject.SetToElevation(newElevation);
        foreach (MovingEnvironment entry in EnvironmentObjects)
        {
            // other environment objects that are offset vertically
            //entry.SetToElevation(entry.environmentPhysics.BottomHeight - oldElevation + newElevation ); 
        }
        entityPhysics.SetElevation(newElevation);
    }

    void SpawnEnemies()
    {
        foreach (BetterEnemySpawner spawner in EnemySpawners)
        {
            spawner.QueueEnemies(2, 4.0f, true, ElementType.ZAP);
        }
    }
    
    void BeginPhase()
    {
        switch (CurrentPhase)
        {
            case FragmentPhase.LOWER_SPEARS:
                if (IsRaised) StartCoroutine(RaiseOrLower(false, 1.0f));
                CurrentState = FragmentState.CHASE;
                break;
            case FragmentPhase.SPAWN_ENEMIES:
                SpawnEnemies();
                break;
            case FragmentPhase.SUPERLASER:
                CurrentState = FragmentState.CHASE;
                break;
            default:
                break;
        }
    }

    void CheckPhaseComplete()
    {
        bool bIsComplete = true;
        switch (CurrentPhase)
        {
            case FragmentPhase.ORBITING:
                if (!IsLowered && IsRaised)
                {
                    bIsComplete = false;
                }
                break;
            case FragmentPhase.LOWER_SPEARS:
                if (_phaseTimer < CrystalHailDuration && false)
                {
                    bIsComplete = false;
                }
                break;
            case FragmentPhase.SPAWN_ENEMIES:
                foreach (BetterEnemySpawner spawner in EnemySpawners)
                {
                    if (!spawner.IsWaveComplete())
                    {
                        bIsComplete = false;
                    }
                }
                break;
            case FragmentPhase.CRYSTAL_HAIL:
                if (_phaseTimer < CrystalHailDuration)
                {
                    bIsComplete = false;
                }
                break;
            case FragmentPhase.SUPERLASER:
                return;
                break;
        }

        if (!bIsComplete) return;

        OrderOfPhases.RemoveAt(0);
        if (OrderOfPhases.Count > 0)
        {
            CurrentPhase = OrderOfPhases[0];
            BeginPhase();
            Debug.Log("Switching to phase : " + CurrentPhase);
        }
    }

    IEnumerator FireSmallLaser(float Cooldown)
    {
        Debug.Log("SMALL LASERING!");
        GetComponent<Animation>().Play("fragmentLaserBurst");
        foreach (var rend in TEMP_SmallLaserVFX)
        {
            rend.enabled = true;
        }

        //flash lens flare, other vfx
        AttackFlash.GetComponent<SpriteRenderer>().enabled = true;
        bReadyToAttack = false;
        bIsBossDoingSomething = true;
        AttackFlash.Play("BigFlare", 0, 0);
        yield return new WaitForSeconds(1.0f);
        /*
        foreach (var rend in TEMP_SmallLaserVFX)
        {
            rend.enabled = true;
        }*/

        // flash damage area, deal damage
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(entityPhysics.transform.position, new Vector2(24, 18), 0);
        foreach (Collider2D hit in hitobjects)
        {
            EntityPhysics hitEntity = hit.gameObject.GetComponent<EntityPhysics>();
            if (hit.tag == "Friend")
            {
                Debug.Log("Hit player!");
                hitEntity.Inflict(1);
            }
        }
        AttackFlash.GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(1.0f);
        foreach (var rend in TEMP_SmallLaserVFX)
        {
            rend.enabled = false;
        }

        bIsBossDoingSomething = false;

        // play followthru
        yield return new WaitForSeconds(Cooldown);
        bReadyToAttack = true;
    }

    IEnumerator FireSuperlaser()
    {
        foreach (var rend in TEMP_SmallLaserVFX)
        {
            rend.enabled = true;
        }
        GetComponent<Animation>().Play("testsuperlaser");
        AttackFlash.GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(7.5f);
        // play lens flare flash thing
        AttackFlash.GetComponent<SpriteRenderer>().enabled = true;
        AttackFlash.Play("BigFlare", 0, 0);
        yield return new WaitForSeconds(0.5f);
        AttackFlash.GetComponent<SpriteRenderer>().enabled = false;
    }

    // everything that should happen when the boss is killed by the lightning strike, starting the moment the killing blow strikes it
    IEnumerator Death()
    {
        /*
        TopLightningBolt.SetThickness(1.0f, 1.125f);
        TopLightningBolt.SetupLine(TopBoltZapPoint.transform.position + new Vector3(0, 26, 0), TopBoltZapPoint.transform.position);
        BottomLightningBolt.SetThickness(1.0f, 1.125f);
        BottomLightningBolt.SetupLine(_player.GetEntityPhysics().ObjectSprite.transform.position, BottomBoltZapPoint.transform.position);
        TopLightningBolt.ShowBolt();
        BottomLightningBolt.ShowBolt();
        */
        //StopCoroutine(_superlaserCoroutine);
        bIsDead = true;
        bossCore.GetEntityPhysics().Inflict(1);
        if (!deathShatterController) Destroy(gameObject);
        _bossHealthManager.gameObject.SetActive(false);
        MusicManager.GetMusicManager().CrossfadeToSong(0.0f, DeathScreenAmbience);
        foreach (EnemySpawner spawner in SimpleEnemySpawners)
        {
            spawner.IsAutomaticallySpawning = false;
        }

        foreach (SwordEnemyHandler handler in GameObject.FindObjectsOfType<SwordEnemyHandler>())
        {
            handler.GetEntityPhysics().Inflict(100);
        }
        foreach (HailShard hail in GameObject.FindObjectsOfType<HailShard>())
        {
            GameObject.Destroy(hail.gameObject);
        }


        deathShatterController.gameObject.SetActive(true);
        deathShatterController.transform.position = new Vector3(deathShatterController.transform.position.x, deathShatterController.transform.position.y, _camera.transform.position.z + 2);
        deathShatterController.Shatter(DeathVector);

        ShatterHealthSystem.gameObject.SetActive(true);
        ShatterHealthSystem.transform.position = new Vector3(healthBarShatterController.transform.parent.position.x, healthBarShatterController.transform.parent.position.y, _camera.transform.position.z + 1);
        ShatterHealthBarBlock.GetComponent<SpriteRenderer>().sprite = ShatterHealthBarNewTexture;
        ShatterHealthBarBlock.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // new texture is twice as big so
        
        healthBarShatterController.gameObject.SetActive(true);
        healthBarShatterController.Shatter(DeathVector);

        _camera.SetPostProcessParam("_ShatterMaskTex", PP_White);
        _camera.SetPostProcessParam("_CrackTex", PP_Black);
        _camera.SetPostProcessParam("_OffsetTex", PP_Black);


        entityPhysics.GetComponent<AudioSource>().outputAudioMixerGroup = UnduckedSFXMixer;

        _player.GetEntityPhysics().GetComponent<AudioSource>().clip = DeathSound;
        _player.GetEntityPhysics().GetComponent<AudioSource>().outputAudioMixerGroup = UnduckedSFXMixer;
        _player.GetEntityPhysics().GetComponent<AudioSource>().Play();

        _player.ForceHideUI();
        Time.timeScale = 0.0f;

        DuckedSFXMixer.audioMixer.SetFloat("FreezeFrameVolume", -80.0f);

        yield return new WaitForSecondsRealtime(2.5f);

        if (GlitchTextPopups.Count == 0)
        {
            yield return new WaitForSecondsRealtime(3.0f);
        }
        // show glitch text
        for (int i = 0; i < GlitchTextPopups.Count; i++)
        {
            GlitchTextPopups[i].gameObject.SetActive(true);
            entityPhysics.GetComponent<AudioSource>().clip = GlitchTextPopups[i].Appear();
            entityPhysics.GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
            entityPhysics.GetComponent<AudioSource>().Play();
            yield return new WaitForSecondsRealtime(GlitchTextPopups[i].delayAfterAppear);
        }

        // everything's been shown. Transition back to realtime with some cool glitchy effects
        _player.GetEntityPhysics().GetComponent<AudioSource>().clip = FadeInSound;
        _player.GetEntityPhysics().GetComponent<AudioSource>().Play();


        float phaseOutDuration = 1.0f;
        float timer = 0.0f;
        float updateInterval = 0.025f;
        while (timer < phaseOutDuration)
        {
            _camera.SetPostProcessParam("_GlitchOffsetStrength", GlitchIntensityCurve.Evaluate(timer / phaseOutDuration));
            _camera.SetPostProcessParam("_GlitchOffsetMask", GlitchMaskCurve.Evaluate(timer / phaseOutDuration));
            _camera.SetPostProcessParam("_GlitchTime", timer);
            yield return new WaitForSecondsRealtime(updateInterval);
            timer += updateInterval;
        }


        // back to normal!

        _player.GetEntityPhysics().GetComponent<AudioSource>().outputAudioMixerGroup = DuckedSFXMixer;
        DuckedSFXMixer.audioMixer.SetFloat("FreezeFrameVolume", 0.0f);

        Time.timeScale = 1.0f;
        _camera.SetPostProcessParam("_GlitchOffsetStrength", 0.0f);
        _camera.SetPostProcessParam("_GlitchOffsetMask", 0.0f);

        _player.ForceShowUI();
        fadeTransition.JustPlayFadeIn(Color.white, 2.0f); // TODO : kinda abusing this for a flash. not great for accessibility flashing settings
        for (int i = 0; i < GlitchTextPopups.Count; i++)
        {
            GameObject.Destroy(GlitchTextPopups[i].gameObject); // we done with it
        }

        _player.ShatterHealth();

        _camera.SetPostProcessParam("_ShatterMaskTex", PP_ShatterTex_Mask);
        _camera.SetPostProcessParam("_CrackTex", PP_ShatterTex_Cracks);
        _camera.SetPostProcessParam("_OffsetTex", PP_ShatterTex_Offsets);

        healthBarShatterController.gameObject.SetActive(false);
        ShatterHealthSystem.gameObject.SetActive(false);

        // position player and camera in the correct location relative to the shattered fragment
        Vector2 fragmentCorpseRoot = CorpsePositionEnvtObj.ObjectCollider.bounds.center;
        Vector2 fragmentPositon = entityPhysics.transform.position;

        // cool idea, but not that valuable and error prone
        /*
        Vector2 playerPosition = _player.GetEntityPhysics().transform.position;
        _player.GetEntityPhysics().transform.position = (playerPosition - fragmentPositon) + fragmentCorpseRoot;
        _camera.transform.position = ((Vector2)_camera.transform.position - fragmentPositon) + fragmentCorpseRoot; */

        _player.GetEntityPhysics().transform.position = PlayerAwakenLocation;

        CorpseSprite.enabled = true;
        _player.StandFromCollapsePlayer();
        _player.GetEntityPhysics().GetComponent<AudioSource>().outputAudioMixerGroup = DuckedSFXMixer;
        foreach (var envtPhys in holeUnderneath)
        {
            GameObject.Destroy(envtPhys.gameObject);
        }
        foreach (var collapsable in cascadingFloorStarts)
        {
            collapsable.PropagateInvalidReposition();
            collapsable.StartCollapse();
        }
        _player.GetEntityPhysics().ForceSavePosition(newPlayerFallSavePoint);

        bossCore.GetEntityPhysics().transform.position = fragmentCorpseRoot;

        foreach (HailShard hail in GameObject.FindObjectsOfType<HailShard>())
        {
            GameObject.Destroy(hail.gameObject);
        }

        _player.BossFightDeathResurrection_Position = PlayerAwakenLocation;
        _player.BossFightDeathResurrection_CrackTex = PP_ShatterTex_Cracks;
        _player.BossFightDeathResurrection_OffsetTex = PP_ShatterTex_Offsets;
        _player.BossFightDeathResurrection_ShatterMaskTex = PP_ShatterTex_Mask;
        _player.BossFightDeathResurrection_CollapsingPlatforms = cascadingFloorStarts;
        bossCore.BossCameraVolume.GetComponent<CameraSizeChangeVolume>().IsSizeChangeActive = false;
        Camera.main.GetComponent<CameraScript>().SetCameraSizeImmediate(4.0f);

        Destroy(gameObject);
    }

    IEnumerator LightningBuildupVFX()
    {
        bool hasPlayerStartedCharging = false;
        //BottomLightningBolt.ShowBolt();
        //TopLightningBolt.ShowBolt();
        float currentChargeAmount;
        
        while (true)
        {
            currentChargeAmount = SuperlaserRestPlatform.CurrentChargeAmount;
            if (currentChargeAmount > 0.0f)
            {
                if (!hasPlayerStartedCharging) // bunch of stuff that runs the moment player begins charging
                {
                    hasPlayerStartedCharging = true;
                    StartCoroutine(RandomFlashBolt(TopLightningBolt, TopBoltZapPoint.transform.position + new Vector3(0, 26, 0), TopBoltZapPoint.transform.position));
                    StartCoroutine(RandomFlashBolt(BottomLightningBolt, _player.GetEntityPhysics().ObjectSprite.transform.position, BottomBoltZapPoint.transform.position));
                    for (int i = 0; i < RandomAmbientLightningBolts.Count; i++)
                    {
                        StartCoroutine(RandomFlashRandomBolt(RandomAmbientLightningBolts[i], TopBoltZapPoint.transform.position + new Vector3(i * 26 - 39, 26, 0)));
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
    // use for bolts who grow from StarPosition to EndPosition over time and have their flash rate increase with charge amt.
    IEnumerator RandomFlashBolt(ZapFXController Bolt, Vector3 StartPosition, Vector3 EndPosition) 
    {
        while (true)
        {
            float currentChargeAmount = SuperlaserRestPlatform.CurrentChargeAmount;
            Bolt.SetThickness(BoltSizeOverCharge.Evaluate(currentChargeAmount), BoltSizeOverCharge.Evaluate(currentChargeAmount) * 2.0f);
            Bolt.SetupLine(StartPosition, Vector3.Lerp(StartPosition, EndPosition, BoltDistanceOverCharge.Evaluate(currentChargeAmount)));
            Bolt.Play(BoltDurationOverCharge.Evaluate(currentChargeAmount));
            yield return new WaitForSeconds(Random.Range(0.8f, 1.2f) * BoltDelayOverCharge.Evaluate(currentChargeAmount));
        }
    }

    // use for bolts who grow from StarPosition to EndPosition over time and have their flash rate increase with charge amt.
    IEnumerator RandomFlashRandomBolt(ZapFXController Bolt, Vector3 StartPosition)
    {
        while (true)
        {
            float currentChargeAmount = SuperlaserRestPlatform.CurrentChargeAmount;
            Bolt.SetThickness(BoltSizeOverCharge.Evaluate(currentChargeAmount) * 0.5f, BoltSizeOverCharge.Evaluate(currentChargeAmount));
            Bolt.SetupLine(StartPosition, Vector3.Lerp(
                StartPosition + new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-2, -5), 0), 
                StartPosition + new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-30, -50), 0), 
                BoltDistanceOverCharge.Evaluate(currentChargeAmount)));

            Bolt.Play(BoltDurationOverCharge.Evaluate(currentChargeAmount));
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f) * BoltDelayOverCharge.Evaluate(currentChargeAmount));
        }
    }

    IEnumerator RaiseOrLower(bool ShouldRaise, float duration)
    {
        float newElevation = ShouldRaise ? RaisedElevation : LoweredElevation;
        IsRaised = false;
        IsLowered = false;
        float progress = 0.0f;
        float oldElevation = entityPhysics.GetObjectElevation();
        while (progress < 1.0f)
        {
            SetElevation(Mathf.Lerp(oldElevation, newElevation, progress));
            progress += Time.deltaTime / duration;
            yield return new WaitForEndOfFrame();
        }
        SetElevation(newElevation);
        IsRaised = ShouldRaise;
        IsLowered = !ShouldRaise;
    }

    // Spawn a set of spears which look at the player's position when they spawn, then yeet toward that position after a delay
    IEnumerator FireVolley()
    {
        bVolleyReady = false;
        bIsBossDoingSomething = true;
        List<ProjectilePhysics> VolleySpears = new List<ProjectilePhysics>();

        Vector2 dirBossToPlayer = _player.GetEntityPhysics().transform.position - entityPhysics.transform.position;
        dirBossToPlayer.Normalize();

        for (int i = 0; i < SpearsPerVolley; i++)
        {
            Vector3 bulletPosition = entityPhysics.transform.position - new Vector3(dirBossToPlayer.x, dirBossToPlayer.y, 0) * 6 + Quaternion.Euler(0f, 0f, 90) * dirBossToPlayer * Random.Range(-8, 8);
            Vector2 dirBulletToPlayer = _player.GetEntityPhysics().transform.position - bulletPosition;
            dirBulletToPlayer.Normalize();
            VolleySpears.Add(SpearProjectileWeapon.FireBullet(dirBulletToPlayer).GetComponentInChildren<ProjectilePhysics>());
            VolleySpears[i].transform.position = bulletPosition;
            VolleySpears[i].SetObjectElevation(entityPhysics.GetObjectElevation());
            VolleySpears[i].GetComponent<AudioSource>().clip = ProjectileSummonSFX;
            VolleySpears[i].GetComponent<AudioSource>().Play();

            yield return new WaitForSeconds(VolleySpawnDelay);
        }
        yield return new WaitForSeconds(VolleySpawnToFireDelay);
        for (int i = 0; i < SpearsPerVolley; i++)
        {
            Vector2 dir = _player.GetEntityPhysics().transform.position - entityPhysics.transform.position;
            VolleySpears[i].GetComponent<AudioSource>().clip = ProjectileLaunchSFX;
            VolleySpears[i].GetComponent<AudioSource>().Play();
            VolleySpears[i].Speed = SpearSpeed;
            yield return new WaitForSeconds(VolleyFireDelay);
        }
        bIsBossDoingSomething = false;
        yield return new WaitForSeconds(VolleyCooldown);
        bVolleyReady = true;
    }

    IEnumerator HailRandom()
    {
        bIsHailing = true;
        float timer = 0.0f;
        //while (timer < HailDuration)
        while (true)
        {
            float currentDelay = HailDelayOverHealth.Evaluate(entityPhysics.GetCurrentHealth() / (float)entityPhysics.GetMaxHealth());
            Vector2 target = HailCenter + new Vector2(Random.Range(-1.0f, 1.0f) * HailArea.x, Random.Range(-1.0f, 1.0f) * HailArea.y);
            bool shouldDrop = false;
            foreach (var asdf in Physics2D.OverlapPointAll(target))
            {
                if (asdf.GetComponent<EnvironmentPhysics>())
                {
                    shouldDrop = true;
                }
            }
            if (shouldDrop)
            {
                Transform newHail = Instantiate(HailPrefab);
                newHail.position = target;
                newHail.GetComponent<HailShard>().playerPhys = _player.GetEntityPhysics();
                
            }
            timer += currentDelay;
            yield return new WaitForSeconds(currentDelay);

        }
        bIsHailing = false;
    }

    IEnumerator HailPlayer()
    {
        bIsHailing = true;
        float timer = 0.0f;
        //while (timer < HailDuration)
        while (true)
        {
            Vector3 hailPosition = (Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f)) * Vector3.right * Random.Range(0.0f, TargetedHailRadius));
            hailPosition.y *= 0.75f;
            hailPosition += _player.GetEntityPhysics().transform.position;
            Collider2D[] hitobjects = Physics2D.OverlapBoxAll(hailPosition, new Vector2(0.2f,0.2f), 0);
            bool hitEnvironment = false;
            foreach (Collider2D hit in hitobjects)
            {
                if (hit.transform.gameObject.tag == "Environment")
                {
                    hitEnvironment = true;
                    break;
                }
            }
            float currentDelay = TargetedHailDelayOverHealth.Evaluate(entityPhysics.GetCurrentHealth() / (float)entityPhysics.GetMaxHealth());
            timer += currentDelay;
            if (hitEnvironment)
            {
                Transform newHail = Instantiate(HailPrefab);
                newHail.position = _player.GetEntityPhysics().transform.position + (Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f)) * Vector3.right * Random.Range(0.0f, TargetedHailRadius));
                newHail.GetComponent<HailShard>().playerPhys = _player.GetEntityPhysics();
            }            
            yield return new WaitForSeconds(currentDelay);
        }
        bIsHailing = false;
    }

    IEnumerator ChangePhaseAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

    public void Ascend_Instantaneous()
    {
        SetElevation(RaisedElevation);
        _bossHealthManager.gameObject.SetActive(false);
        IsRaised = true;
        IsLowered = false;
    }

    public void Descend()
    {
        StartCoroutine(DescendToFightPlayer(DescentDuration));
    }

    IEnumerator DescendToFightPlayer( float duration)
    {
        float newElevation = LoweredElevation;
        IsRaised = false;
        IsLowered = false;
        float progress = 0.0f;
        float oldElevation = entityPhysics.GetObjectElevation();
        while (progress < 1.0f)
        {
            SetElevation(Mathf.Lerp(oldElevation, newElevation, DescentCurve.Evaluate(progress)));
            progress += Time.deltaTime / duration;
            yield return new WaitForEndOfFrame();
        }
        SetElevation(newElevation);
        IsRaised = false;
        IsLowered = true;
        MusicManager.GetMusicManager().CrossfadeToSong(0.0f, FragmentCombatMusic);
        CheckPhaseComplete();
        _bossHealthManager.gameObject.SetActive(true);
        _bossHealthManager.SetupForBoss(entityPhysics, _bossName);
        _bossHealthManager.DramaticAppearance(0.5f);
    }

    public void OnPlayerResurrected()
    {
        // cleanup any shit thats left over
        if (_volleyCoroutine != null) StopCoroutine(_volleyCoroutine);
    }

    protected override void ExecuteState()
    {
        switch (CurrentState)
        {
            case FragmentState.INACTIVE:
                State_Inactive();
                return;
                break;
            case FragmentState.IDLE:
                State_Idle();
                return;
                break;
            case FragmentState.CHASE:
                //State_Chase();
                break;
            case FragmentState.FIRING:
                State_Inactive();
                break;
            case FragmentState.SUPERLASER:
                State_Superlaser();
                break;
            case FragmentState.DEATH:
                break;
        }
    }

    

    public override void SetXYAnalogInput(float x, float y)
    {
        // fuck it we not using this
        throw new System.NotImplementedException();
    }

    public override void JustGotHit(Vector2 hitDirection)
    {
        if (hitDirection.sqrMagnitude > 0)
        {
            DeathVector = hitDirection;
        }
    }
}
