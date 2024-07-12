using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// central part of the boss
public class FinalBossCore : EntityHandler
{
    [SerializeField] private BossHealthBarManager _bossHealthBar;
    [SerializeField] private float Acceleration;
    [SerializeField] private float MaxSpeed;
    private Vector2 PreviousVelocity;

    [SerializeField] private List<FinalBossFragment> OrbitingFragments; // order matters, determines order in which fragments descend
    [SerializeField] private FinalBossFragment Fragment_North;
    [SerializeField] private FinalBossFragment Fragment_South;
    [SerializeField] private FinalBossFragment Fragment_East;
    [SerializeField] private FinalBossFragment Fragment_West;
    [SerializeField] private float orbitRate; // degrees per second
    [SerializeField] private float orbitRadius; 
    FinalBossFragment CurrentlyDescendedFragment;

    [Header("Superlaser Stuff")] // ALL vfx that occur when you charge the lightning bolt to destroy this guy
    [SerializeField] private RestPlatform SuperlaserRestPlatform;
    [SerializeField] private List<SpriteRenderer> LaserVFX;
    [SerializeField] Animator AttackFlash;
    private float _superlaserCharge = 0.0f;
    private float _superlaserChargeRate = 0.083333f; // superlasers per second (1 / duration)
    // positions that fragments should be in for superlaser to start
    [SerializeField] AnimationCurve SuperlaserFragmentRepositionCurve; // normalize curve that lets fragments ease in and out of their motion into position.
    [SerializeField] Vector2 SuperlaserChargeStartPositionalOffset; // fragments are pulled in by this amount (x for east/west, y for north/south) from the SuperlaserPosition_Dir while charging superlaser
    [SerializeField] Vector2 SuperlaserChargeEndPositionalOffset; // fragments are pulled in by this amount (x for east/west, y for north/south) from the SuperlaserPosition_Dir while charging superlaser
    bool ShouldRushToSuperlaserPosition = false;
    [SerializeField] SpriteRenderer restPlatformGlowSprite;

    // small laser
    private bool bReadyToAttack = true;
    //[SerializeField] private float SmallLaserWindupDuration = 1.0f;
    //[SerializeField] private AnimationCurve SmallLaser_ShaderRippleElevationCurve;
    //[SerializeField] private AnimationCurve SmallLaser_ShaderRippleWidthCurve;
    //[SerializeField] private AnimationCurve SmallLaser_ShaderRippleWidthCurve;
    [SerializeField] private float SmallLaserCooldown = 4.0f;

    [Space(10)]
    [SerializeField] float flinchDuration = 4.0f;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite dyingSprite;
    //[SerializeField] Sprite flinchSprite;


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

    [SerializeField] private List<Animation> CloudPartAnimations; // play when player is charging lightning bolt
    [SerializeField] private StarController starController;
    [SerializeField] private List<float> BackgroundVerticalOffsets; // offsets to background applied each time a fragment is killed - makes sky take up more and more space over the level
    [SerializeField] private GameObject BackgroundToOffset;
    [SerializeField] private Animation BossDeathLaserVFXAnimator;
    [SerializeField] private StarLightningManager StarLightning;

    [SerializeField] public TriggerVolume BossCameraVolume;

    enum FinalBossState { CHASE, SUPERWEAPON, FLINCH, DYING }
    private FinalBossState CurrentState;
    private PlayerHandler _player;
    private Coroutine CurrentOrbitingFragmentCoroutine;
    private bool _isFlinched = false;

    [Space(10)]
    [Header("AUDIO")]
    [SerializeField] private AudioClip _superlaserAudioClip;
    [SerializeField] private List<AudioClip> _fragmentCombatMusic;
    [SerializeField] private AudioClip _superlaserMusic;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animation>().Play("BossKillAnimation");
        GetComponent<Animation>().Stop();

        _player = GameObject.FindObjectOfType<PlayerHandler>();
        CurrentState = FinalBossState.SUPERWEAPON;
        SuperlaserRestPlatform.GetComponent<BossAttackRestPlatform>().CurrentTargetFragment = OrbitingFragments[0];
        if (CurrentOrbitingFragmentCoroutine != null) StopCoroutine(CurrentOrbitingFragmentCoroutine);
        CurrentOrbitingFragmentCoroutine = StartCoroutine(PositionFragmentsForSuperlaser());
        MusicManager.GetMusicManager().CrossfadeToSong(2.0f, _superlaserMusic);
        foreach (var rend in LaserVFX)
        {
            rend.enabled = false;
        }
        restPlatformGlowSprite.gameObject.active = false;
        //StartCoroutine(Orbit());
        _bossHealthBar.SetupForBoss(entityPhysics, "ASPECT OF ICHOR");
        _bossHealthBar.DramaticAppearance(3.0f);
        
        foreach (var asdf in BackgroundToOffset.GetComponentsInChildren<BackgroundParallax>())
        {
            asdf.OffsetOriginalPosition(new Vector2(0, 5));
        }


        // skip to final phase for boss
        if (false)
        {
            for (int i = 0; i < 4; i++)
            {
                CloudPartAnimations[0].gameObject.SetActive(false); // we want to hide clouds from previous iteration since they dont always move offscreen, I think? may want to not do this, not sure
                starController.AdvancePhase();
                CloudPartAnimations.RemoveAt(0);
                foreach (var asdf in BackgroundToOffset.GetComponentsInChildren<BackgroundParallax>())
                {
                    asdf.OffsetOriginalPosition(new Vector2(0, BackgroundVerticalOffsets[0]));
                }
                BackgroundVerticalOffsets.RemoveAt(0);
            }

            foreach (var asdf in OrbitingFragments)
            {
                GameObject.Destroy(asdf.gameObject);
            }

            OrbitingFragments.Clear();
            SuperlaserRestPlatform.GetComponent<BossAttackRestPlatform>().CurrentTargetFragment = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ExecuteState();
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_TopElevation", entityPhysics.GetTopHeight());
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_BottomElevation", entityPhysics.GetBottomHeight());
        BossCameraVolume.transform.position = entityPhysics.transform.position;

        ShouldRushToSuperlaserPosition = ((Vector2)SuperlaserRestPlatform.transform.position - (Vector2)_player.GetEntityPhysics().transform.position).magnitude < 5.0f; // if player is close to middle we want to hurry up to catch up
    }

    protected override void ExecuteState()
    {
        switch (CurrentState)
        {
            case FinalBossState.CHASE:
                if (!CurrentlyDescendedFragment)
                {
                    Debug.Log("Changing boss state from chase to superweapon!");
                    CurrentState = FinalBossState.SUPERWEAPON;
                    SuperlaserRestPlatform.GetComponent<BossAttackRestPlatform>().CurrentTargetFragment = OrbitingFragments[0];
                    if (CurrentOrbitingFragmentCoroutine != null) StopCoroutine(CurrentOrbitingFragmentCoroutine);
                    CurrentOrbitingFragmentCoroutine = StartCoroutine(PositionFragmentsForSuperlaser());
                    MusicManager.GetMusicManager().CrossfadeToSong(0.0f, _superlaserMusic);
                    CloudPartAnimations[0].gameObject.SetActive(false); // we want to hide clouds from previous iteration since they dont always move offscreen, I think? may want to not do this, not sure
                    starController.AdvancePhase();
                    CloudPartAnimations.RemoveAt(0);
                    foreach (var asdf in BackgroundToOffset.GetComponentsInChildren<BackgroundParallax>())
                    {
                        asdf.OffsetOriginalPosition(new Vector2(0, BackgroundVerticalOffsets[0]));
                    }
                    BackgroundVerticalOffsets.RemoveAt(0);
                }
                else
                {
                    ProximityLaserAOE();
                    ChasePlayer();
                }
                break;
            case FinalBossState.SUPERWEAPON:
                State_Superlaser();
                break;
            case FinalBossState.FLINCH:
                State_Flinch();
                break;
            case FinalBossState.DYING:
                State_Dying();
                break;
        }
    }

    void DropFragment()
    {
        if (!CurrentlyDescendedFragment)
        {
            if (OrbitingFragments.Count > 0)
            {
                CurrentlyDescendedFragment = OrbitingFragments[0];
                OrbitingFragments.RemoveAt(0);
                CurrentlyDescendedFragment.Descend();
                CurrentlyDescendedFragment.GetEntityPhysics().IsImmune = false;
            }
            else
            {
                // we shouldve died actually!
            }
            
        }
    }

    public override void JustGotHit(Vector2 hitDirection)
    {
        throw new System.NotImplementedException();
    }

    public override void SetXYAnalogInput(float x, float y)
    {
        throw new System.NotImplementedException();
    }

    
    public void OnStruckByLightning()
    {
        if (SuperlaserRestPlatform.CurrentChargeAmount == 1.0f) // you win!
        {
            if (OrbitingFragments.Count > 0)
            {
                DropFragment();
                SuperlaserRestPlatform.IsUseable = false;
                restPlatformGlowSprite.gameObject.active = false;
                if (CurrentOrbitingFragmentCoroutine != null) StopCoroutine(CurrentOrbitingFragmentCoroutine);
                CurrentOrbitingFragmentCoroutine = StartCoroutine(Orbit());

                CurrentState = FinalBossState.FLINCH;
                _superlaserCharge = 0.0f;
                _primeAudioSource.Stop();
            }
            else
            {
                // self destruct
                StopAllCoroutines();
                //CurrentState = FragmentState.DEATH;
                StartCoroutine(Death());
            }
        }
    }

    public void BeginDeath()
    {
        CurrentState = FinalBossState.DYING;
        StartCoroutine(Death());
    }

    private void State_Superlaser()
    {
        Vector2 offset = SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position - entityPhysics.transform.position;
        if (offset.magnitude > 1.0f)
        {
            offset.Normalize();
            Vector2 currentspeed = offset * MaxSpeed * Time.deltaTime * (ShouldRushToSuperlaserPosition ? 2.0f : 1.0f);
            entityPhysics.MoveWithCollision(currentspeed.x, currentspeed.y);
        }
        else if (offset.magnitude != 0.0f)
        {
            entityPhysics.MoveWithCollision(offset.x, offset.y);
        }

        if (offset.sqrMagnitude != 0) return;

        // we are over it. it's superlaser time.
        if (_superlaserCharge == 0.0f)
        {
            StartCoroutine(FireSuperlaser());
            StartCoroutine(LightningBuildupVFX());
        }
        _superlaserCharge += _superlaserChargeRate * Time.deltaTime;
        //SuperlaserProjection.material.SetFloat("_Opacity", _superlaserCharge);

        /*
        if (SuperlaserRestPlatform.CurrentChargeAmount == 1.0f) // you win!
        {
            if (OrbitingFragments.Count > 0)
            {
                DropFragment();
                if (CurrentOrbitingFragmentCoroutine != null) StopCoroutine(CurrentOrbitingFragmentCoroutine);
                CurrentOrbitingFragmentCoroutine = StartCoroutine(Orbit());

                CurrentState = FinalBossState.FLINCH;
                _superlaserCharge = 0.0f;
                _primeAudioSource.Stop();
            }
            else 
            {
                // self destruct
                StopAllCoroutines();
                //CurrentState = FragmentState.DEATH;
                StartCoroutine(Death());
            }
        }*/
        if (_superlaserCharge > 1.0f) // you lose!
        {
            _player.GetEntityPhysics().Inflict(1000, hitPauseDuration:0.25f);
        }
    }

    void ChasePlayer() // follow the player, try to kill them
    {
        Vector2 offset = _player.GetEntityPhysics().transform.position - entityPhysics.transform.position;

        offset.Normalize();
        PreviousVelocity += offset * Acceleration * Time.deltaTime;
        if (PreviousVelocity.magnitude > MaxSpeed)
        {
            PreviousVelocity.Normalize();
            PreviousVelocity *= MaxSpeed;
        }
        entityPhysics.MoveWithCollision(PreviousVelocity.x * Time.deltaTime, PreviousVelocity.y * Time.deltaTime);
    }

    void State_Flinch()
    {
        if (!_isFlinched) StartCoroutine(DoFlinch());
    }

    void State_Dying()
    {
        // dont think we do much
    }

    void ProximityLaserAOE()
    {
        Vector2 offset = _player.GetEntityPhysics().transform.position - entityPhysics.transform.position;
        if (Mathf.Abs(offset.x) < 8.0f && Mathf.Abs(offset.y) < 6.0f)
        {
            if (bReadyToAttack)
            {
                StartCoroutine(FireSmallLaser(SmallLaserCooldown));
            }
        }
    }


    IEnumerator FireSmallLaser(float Cooldown)
    {
        Debug.Log("SMALL LASERING!");

        AttackFlash.GetComponent<SpriteRenderer>().enabled = true;
        bReadyToAttack = false;
        AttackFlash.Play("BigFlare", 0, 0);
        //flash lens flare, other vfx
        foreach (var rend in LaserVFX)
        {
            rend.enabled = true;
        }
        GetComponent<Animation>().Play("smalllaser");

        yield return new WaitForSeconds(1.0f);
        
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
        foreach (var rend in LaserVFX)
        {
            rend.enabled = false;
        }

        // play followthru
        yield return new WaitForSeconds(Cooldown);
        bReadyToAttack = true;
    }

    // Gets core and all boss fragments into their rightful positions to charge laser.
    IEnumerator PositionFragmentsForSuperlaser()
    {
        Vector2 offset = SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position - entityPhysics.transform.position;
        float EstimatedTimeForBossToArrive = offset.magnitude / MaxSpeed - 0.5f;
        float timer = 0.0f;
        Vector2 StartPosition_North = Fragment_North == null ? Vector2.zero : (Vector2)Fragment_North.GetEntityPhysics().transform.position; 
        Vector2 StartPosition_South = Fragment_South == null ? Vector2.zero : (Vector2)Fragment_South.GetEntityPhysics().transform.position; 
        Vector2 StartPosition_East = Fragment_East == null ? Vector2.zero : (Vector2)Fragment_East.GetEntityPhysics().transform.position; 
        Vector2 StartPosition_West = Fragment_West == null ? Vector2.zero : (Vector2)Fragment_West.GetEntityPhysics().transform.position;

        // new, edits a field that was previously manually assingned
        Vector2 EndPosition_North = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeStartPositionalOffset * new Vector2(0, 1);
        Vector2 EndPosition_South = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeStartPositionalOffset * new Vector2(0, -1);
        Vector2 EndPosition_East = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeStartPositionalOffset * new Vector2(1, 0) + new Vector2(-2, 0);
        Vector2 EndPosition_West = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeStartPositionalOffset * new Vector2(-1, 0);


        while (timer < EstimatedTimeForBossToArrive)
        {
            float normalizedProgress = SuperlaserFragmentRepositionCurve.Evaluate(timer / EstimatedTimeForBossToArrive);

            if (Fragment_North) Fragment_North.GetEntityPhysics().transform.position = Vector2.Lerp(StartPosition_North, EndPosition_North, normalizedProgress);
            if (Fragment_South) Fragment_South.GetEntityPhysics().transform.position = Vector2.Lerp(StartPosition_South, EndPosition_South, normalizedProgress);
            if (Fragment_East) Fragment_East.GetEntityPhysics().transform.position = Vector2.Lerp(StartPosition_East, EndPosition_East, normalizedProgress);
            if (Fragment_West) Fragment_West.GetEntityPhysics().transform.position = Vector2.Lerp(StartPosition_West, EndPosition_West, normalizedProgress);

            timer += Time.deltaTime * (ShouldRushToSuperlaserPosition ? 2.0f : 1.0f);
            yield return null; // should just make this run every frame?
        }

    }

    IEnumerator FireSuperlaser()
    {
        foreach (var rend in LaserVFX)
        {
            rend.enabled = true;
        }
        SuperlaserRestPlatform.IsUseable = true;
        restPlatformGlowSprite.gameObject.active = true;
        SuperlaserRestPlatform.GetComponent<BossAttackRestPlatform>().bShouldRunRampUp = true;
        _primeAudioSource.clip = _superlaserAudioClip;
        _primeAudioSource.Play();
        GetComponent<Animation>().Play("testsuperlaser");
        AttackFlash.GetComponent<SpriteRenderer>().enabled = false;
        if (CurrentOrbitingFragmentCoroutine != null) StopCoroutine(CurrentOrbitingFragmentCoroutine);
        CurrentOrbitingFragmentCoroutine = StartCoroutine(AnimateFragmentsDuringSuperlaser());

        yield return new WaitForSeconds(11.5f);
        // play lens flare flash thing
        AttackFlash.GetComponent<SpriteRenderer>().enabled = true;
        AttackFlash.Play("BigFlare", 0, 0);
        yield return new WaitForSeconds(0.5f);
        AttackFlash.GetComponent<SpriteRenderer>().enabled = false;
    }

    IEnumerator AnimateFragmentsDuringSuperlaser()
    {
        Vector2 StartPosition_North = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeStartPositionalOffset * new Vector2(0, 1);
        Vector2 StartPosition_South = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeStartPositionalOffset * new Vector2(0, -1);
        Vector2 StartPosition_East = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeStartPositionalOffset * new Vector2(1, 0) + new Vector2(-2, 0);
        Vector2 StartPosition_West = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeStartPositionalOffset * new Vector2(-1, 0);

        Vector2 EndPosition_North = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeEndPositionalOffset * new Vector2(0, 1);
        Vector2 EndPosition_South = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeEndPositionalOffset * new Vector2(0, -1);
        Vector2 EndPosition_East = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeEndPositionalOffset * new Vector2(1, 0) + new Vector2(-2, 0);
        Vector2 EndPosition_West = (Vector2)SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position + SuperlaserChargeEndPositionalOffset * new Vector2(-1, 0);

        float timer = 0.0f;
        float duration = 1.0f / _superlaserChargeRate;
        while (timer < duration)
        {
            float normalizedProgress = SuperlaserFragmentRepositionCurve.Evaluate(timer / duration);

            if (Fragment_North) Fragment_North.GetEntityPhysics().transform.position = Vector2.Lerp(StartPosition_North, EndPosition_North, normalizedProgress);
            if (Fragment_South) Fragment_South.GetEntityPhysics().transform.position = Vector2.Lerp(StartPosition_South, EndPosition_South, normalizedProgress);
            if (Fragment_East) Fragment_East.GetEntityPhysics().transform.position = Vector2.Lerp(StartPosition_East, EndPosition_East, normalizedProgress);
            if (Fragment_West) Fragment_West.GetEntityPhysics().transform.position = Vector2.Lerp(StartPosition_West, EndPosition_West, normalizedProgress);

            timer += Time.deltaTime;
            yield return null; // should just make this run every frame?
        }
        
    }


    IEnumerator LightningBuildupVFX()
    {
        bool hasPlayerStartedCharging = false;
        //BottomLightningBolt.ShowBolt();
        //TopLightningBolt.ShowBolt();
        float currentChargeAmount;

        while (CurrentState == FinalBossState.SUPERWEAPON)
        {
            currentChargeAmount = SuperlaserRestPlatform.CurrentChargeAmount;
            if (!CloudPartAnimations[0].isPlaying && currentChargeAmount > 0.1f)
            {
                CloudPartAnimations[0].Play();
                CloudPartAnimations[0].GetComponent<AudioSource>().Play();
            }
            /*
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

                //StormProjection.material.SetFloat("_Opacity", currentChargeAmount);
            }*/
            yield return new WaitForEndOfFrame();
        }
    }

    // use for bolts who grow from StarPosition to EndPosition over time and have their flash rate increase with charge amt.
    IEnumerator RandomFlashBolt(ZapFXController Bolt, Vector3 StartPosition, Vector3 EndPosition)
    {
        while (CurrentState == FinalBossState.SUPERWEAPON)
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
        while (CurrentState == FinalBossState.SUPERWEAPON)
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

    // everything that should happen when the boss is killed by the lightning strike, starting the moment the killing blow strikes it
    IEnumerator Death()
    {
        // idk what this does honestly
        /*
        TopLightningBolt.SetThickness(1.0f, 1.125f);
        TopLightningBolt.SetupLine(TopBoltZapPoint.transform.position + new Vector3(0, 26, 0), TopBoltZapPoint.transform.position);
        BottomLightningBolt.SetThickness(1.0f, 1.125f);
        BottomLightningBolt.SetupLine(_player.GetEntityPhysics().ObjectSprite.transform.position, BottomBoltZapPoint.transform.position);
        TopLightningBolt.ShowBolt();
        BottomLightningBolt.ShowBolt();

        //StopCoroutine(_superlaserCoroutine);
        yield return new WaitForSeconds(0.2f);
        */

        //StopCoroutine(_superlaserCoroutine);
        _primeAudioSource.Stop();
        MusicManager.GetMusicManager().CrossfadeToSong(1.0f, null);
        GetComponent<Animation>().Play("BossKillAnimation");
        yield return new WaitForSeconds(2.0f);

        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().sprite = dyingSprite; //first gets struck by the lightning beam, starts cracking
        _player.GetEntityPhysics().ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 1.0f);
        _player.GetEntityPhysics().ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", Color.white);
        _player.GetEntityPhysics().ObjectSprite.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        _player.GetEntityPhysics().transform.position = new Vector3(0, 62, 0);
        StarLightning.StartStarLightning();

        yield return new WaitForSeconds(8.0f);

        BossDeathLaserVFXAnimator.gameObject.active = true;
        BossDeathLaserVFXAnimator.Play();
        BossDeathLaserVFXAnimator.gameObject.GetComponent<BossDeathBeamScene>().PlayDeathScene();
        Destroy(gameObject);
        _player.GetEntityPhysics().Gravity = 0.0f;
        //_player.transform.position += new Vector3(0, 0, 1000);
        Destroy(_player);
        GameObject.Find("CAMERA").GetComponent<CameraScript>().TrackPlayer = false;
        _bossHealthBar.gameObject.active = false;

        foreach (var asdf in GameObject.FindObjectsOfType<EnvironmentPhysics>())
        {
            Destroy(asdf.gameObject);
        }
    }

    IEnumerator Orbit()
    {
        float timeOffset = 0.0f;
        while (OrbitingFragments.Count > 0)
        {
            float wedgeSize = 360.0f / OrbitingFragments.Count;
            for (int i = 0; i < OrbitingFragments.Count; i++)
            {
                float targetAngle = wedgeSize * i + timeOffset;
                Vector3 newOffset = Quaternion.AngleAxis(targetAngle, Vector3.back) * Vector3.left * orbitRadius;
                newOffset.y *= 0.75f;
                OrbitingFragments[i].GetEntityPhysics().transform.position = entityPhysics.transform.position + newOffset;
            }
            timeOffset += orbitRate * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DoFlinch()
    {
        _isFlinched = true;
        foreach (var rend in LaserVFX)
        {
            rend.enabled = false;
        }
        GetComponent<Animation>().Stop();
        //entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().sprite = flinchSprite;
        yield return new WaitForSeconds(flinchDuration);
        //entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().sprite = normalSprite;
        Debug.Log("Changing boss state to CHASE in DoFlinch");
        CurrentState = FinalBossState.CHASE;
        _isFlinched = false;
    }
}
