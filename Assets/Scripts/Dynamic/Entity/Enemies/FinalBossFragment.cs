using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossFragment : EntityHandler
{
    [SerializeField] MovingEnvironment _mainEnvironmentObject;
    [SerializeField] List<MovingEnvironment> EnvironmentObjects;
    [SerializeField] TriggerVolume PatrolArea; // idle unless player is within this volume
    [SerializeField] RestPlatform SuperlaserRestPlatform;
    [SerializeField] Animator AttackFlash;
    [SerializeField] List<BetterEnemySpawner> EnemySpawners;
    [SerializeField] List<SpriteRenderer> TEMP_SmallLaserVFX; // for now we're just gonna flash these really quickly
    [SerializeField] float CrystalHailDuration = 8.0f;
    [SerializeField] float SmallLaserCooldown = 3.0f;
    private bool bIsBossDoingSomething = false; // prevents the boss from doing multiple things at a time like a local AOE and also a volley, which probs would suck
    private Vector2 CenterPosition;

    [Space(10)]
    [Header("Superlaser")]
    [SerializeField] SpriteRenderer SuperlaserProjection;
    //[SerializeField] AnimationCurve ;

    [Space(10)]
    [Header("Storm VFX")] // ALL vfx that occur when you charge the lightning bolt to destroy this guy
    [SerializeField] SpriteRenderer StormProjection;
    [SerializeField] ZapFXController TopLightningBolt;
    [SerializeField] GameObject TopBoltZapPoint; // point on the crystal that should be hit with the zap
    [SerializeField] ZapFXController BottomLightningBolt;
    [SerializeField] GameObject BottomBoltZapPoint; // point on the crystal that should be hit with the zap
    [SerializeField] List<ZapFXController> RandomAmbientLightningBolts;
    [SerializeField] private AnimationCurve BoltDelayOverCharge;
    [SerializeField] private AnimationCurve BoltSizeOverCharge;
    [SerializeField] private AnimationCurve BoltDistanceOverCharge;
    [SerializeField] private AnimationCurve BoltDurationOverCharge;

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
    private List<FragmentPhase> OrderOfPhases;

    private float _phaseTimer = 0.0f;
    private bool bReadyToAttack = true;

    private Coroutine _superlaserCoroutine;

    private Vector2 WanderPosition;
    [SerializeField] private Vector2 WanderAreaSize;
    [SerializeField] private float WanderAcceleration = 7.0f;
    [SerializeField] private float WanderMaxSpeed = 10.0f;
    [SerializeField] private float WanderTargetSwitchDelay = 4.0f;
    private float WanderTimer = 0.0f;

    [Space(10)]
    [Header("Hail")]
    [SerializeField] Transform HailPrefab;
    [SerializeField] float HailDuration = 8.0f;
    [SerializeField] AnimationCurve HailDelayOverDuration;
    [SerializeField] Vector2 HailArea;
    [SerializeField] float TargetedHailRadius;
    [SerializeField] AnimationCurve TargetedHailDelayOverDuration;
    bool bIsHailing = false;


    [SerializeField] private float DescentDuration = 4.0f;
    [SerializeField] private AnimationCurve DescentCurve;

    // Start is called before the first frame update
    void Start()
    {
        OrderOfPhases = new List<FragmentPhase>();
        OrderOfPhases.Add(FragmentPhase.ORBITING);
        OrderOfPhases.Add(FragmentPhase.LOWER_SPEARS);
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
        SuperlaserProjection.material.SetFloat("_Opacity", 0);
        StormProjection.material.SetFloat("_Opacity", 0);
        AttackFlash.GetComponent<SpriteRenderer>().enabled = false;
        currentPrimes = new List<ElementType>();
        SpearProjectileWeapon = (Weapon)ScriptableObject.CreateInstance("BossSpearLauncher");
        SpearProjectileWeapon.PopulateBulletPool();
        CurrentPhase = OrderOfPhases[0];
        CenterPosition = SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position;
        SetElevation(RaisedElevation);
    }

    // Update is called once per frame
    void Update()
    {
        //ExecuteState();
        _phaseTimer += Time.deltaTime;
        ExecutePhase();
        if (entityPhysics.GetCurrentHealth() <= 0)
        {
            StartCoroutine(Death());
        }
    }
    void ExecutePhase()
    {
        switch (CurrentPhase)
        {
            case FragmentPhase.LOWER_SPEARS:
                //lower boss to ground, chase player on the ground, fire projectiles intermittently
                if (IsRaised) StartCoroutine(RaiseOrLower(false, 1.0f));
                if (bVolleyReady && !bIsBossDoingSomething) StartCoroutine(FireVolley());
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
            WanderPosition = CenterPosition + new Vector2(Random.Range(-1.0f, 1.0f) * WanderAreaSize.x, Random.Range(-1.0f, 1.0f) * WanderAreaSize.y);
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
            _superlaserCoroutine = StartCoroutine(FireSuperlaser());
            StartCoroutine(LightningBuildupVFX());
        }
        _superlaserCharge += _superlaserChargeRate * Time.deltaTime;
        SuperlaserProjection.material.SetFloat("_Opacity", _superlaserCharge);

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

        //flash lens flare, other vfx
        AttackFlash.GetComponent<SpriteRenderer>().enabled = true;
        bReadyToAttack = false;
        bIsBossDoingSomething = true;
        AttackFlash.Play("BigFlare", 0, 0);
        yield return new WaitForSeconds(0.7f);

        foreach (var rend in TEMP_SmallLaserVFX)
        {
            rend.enabled = true;
        }

        // flash damage area, deal damage
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(entityPhysics.transform.position, new Vector2(16, 12), 0);
        foreach (Collider2D hit in hitobjects)
        {
            EntityPhysics hitEntity = hit.gameObject.GetComponent<EntityPhysics>();
            if (hit.tag == "Friend")
            {
                hit.gameObject.GetComponent<EntityPhysics>().Inflict(1);
                Debug.Log("Hit player!");
            }
        }
        AttackFlash.GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(0.1f);
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
        TopLightningBolt.SetThickness(1.0f, 1.125f);
        TopLightningBolt.SetupLine(TopBoltZapPoint.transform.position + new Vector3(0, 26, 0), TopBoltZapPoint.transform.position);
        BottomLightningBolt.SetThickness(1.0f, 1.125f);
        BottomLightningBolt.SetupLine(_player.GetEntityPhysics().ObjectSprite.transform.position, BottomBoltZapPoint.transform.position);
        TopLightningBolt.ShowBolt();
        BottomLightningBolt.ShowBolt();

        //StopCoroutine(_superlaserCoroutine);
        yield return new WaitForSeconds(0.2f);

        _player.ShatterHealth();

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
                
                StormProjection.material.SetFloat("_Opacity", currentChargeAmount);
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

            yield return new WaitForSeconds(VolleySpawnDelay);
        }
        yield return new WaitForSeconds(VolleySpawnToFireDelay);
        for (int i = 0; i < SpearsPerVolley; i++)
        {
            Vector2 dir = _player.GetEntityPhysics().transform.position - entityPhysics.transform.position;
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
        while (timer < HailDuration)
        {
            float currentDelay = HailDelayOverDuration.Evaluate(timer / HailDuration);
            Transform newHail = Instantiate(HailPrefab);
            newHail.position = CenterPosition + new Vector2(Random.Range(-1.0f, 1.0f) * HailArea.x, Random.Range(-1.0f, 1.0f) * HailArea.y);
            timer += currentDelay;
            yield return new WaitForSeconds(currentDelay);
        }
        bIsHailing = false;
    }

    IEnumerator HailPlayer()
    {
        bIsHailing = true;
        float timer = 0.0f;
        while (timer < HailDuration)
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
            float currentDelay = TargetedHailDelayOverDuration.Evaluate(timer / HailDuration);
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
        CheckPhaseComplete();
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
        // idk
    }
}
