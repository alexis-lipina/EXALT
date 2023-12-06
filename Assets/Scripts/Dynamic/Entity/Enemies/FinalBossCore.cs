using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// central part of the boss
public class FinalBossCore : EntityHandler
{
    [SerializeField] private float Acceleration;
    [SerializeField] private float MaxSpeed;
    private Vector2 PreviousVelocity;

    [SerializeField] private List<FinalBossFragment> OrbitingFragments; // order matters, determines order in which fragments descend
    [SerializeField] private float orbitRate; // degrees per second
    [SerializeField] private float orbitRadius; 
    FinalBossFragment CurrentlyDescendedFragment;

    [SerializeField] private RestPlatform SuperlaserRestPlatform;
    [SerializeField] private List<SpriteRenderer> LaserVFX;
    [SerializeField] Animator AttackFlash;
    private float _superlaserCharge = 0.0f;
    private float _superlaserChargeRate = 0.125f; // superlasers per second

    // small laser
    private bool bReadyToAttack = true;
    [SerializeField] private float SmallLaserCooldown = 4.0f;

    [Space(10)]
    [SerializeField] float flinchDuration = 4.0f;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite flinchSprite;


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

    enum FinalBossState { CHASE, SUPERWEAPON, FLINCH }
    private FinalBossState CurrentState;
    private PlayerHandler _player;


    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindObjectOfType<PlayerHandler>();
        CurrentState = FinalBossState.SUPERWEAPON;
        foreach (var rend in LaserVFX)
        {
            rend.enabled = false;
        }
        StartCoroutine(Orbit());
    }

    // Update is called once per frame
    void Update()
    {
        ExecuteState();
    }

    protected override void ExecuteState()
    {
        switch (CurrentState)
        {
            case FinalBossState.CHASE:
                ProximityLaserAOE();
                ChasePlayer();
                if (!CurrentlyDescendedFragment)
                {
                    CurrentState = FinalBossState.SUPERWEAPON;
                }
                break;
            case FinalBossState.SUPERWEAPON:
                State_Superlaser();
                break;
            case FinalBossState.FLINCH:
                State_Flinch();
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

    

    private void State_Superlaser()
    {
        Vector2 offset = SuperlaserRestPlatform.GetComponent<EnvironmentPhysics>().TopSprite.transform.position - entityPhysics.transform.position;
        if (offset.magnitude > 1.0f)
        {
            offset.Normalize();
            Vector2 currentspeed = offset * MaxSpeed * Time.deltaTime;
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

        if (SuperlaserRestPlatform.CurrentChargeAmount == 1.0f) // you win!
        {
            if (OrbitingFragments.Count > 0)
            {
                DropFragment();
                CurrentState = FinalBossState.FLINCH;
                _superlaserCharge = 0.0f;
            }
            else 
            {
                // self destruct
                StopAllCoroutines();
                //CurrentState = FragmentState.DEATH;
                StartCoroutine(Death());
            }
        }
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
        StartCoroutine(DoFlinch());
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

    IEnumerator FireSuperlaser()
    {
        foreach (var rend in LaserVFX)
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


    IEnumerator LightningBuildupVFX()
    {
        bool hasPlayerStartedCharging = false;
        //BottomLightningBolt.ShowBolt();
        //TopLightningBolt.ShowBolt();
        float currentChargeAmount;

        while (CurrentState == FinalBossState.SUPERWEAPON)
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

                //StormProjection.material.SetFloat("_Opacity", currentChargeAmount);
            }
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
        TopLightningBolt.SetThickness(1.0f, 1.125f);
        TopLightningBolt.SetupLine(TopBoltZapPoint.transform.position + new Vector3(0, 26, 0), TopBoltZapPoint.transform.position);
        BottomLightningBolt.SetThickness(1.0f, 1.125f);
        BottomLightningBolt.SetupLine(_player.GetEntityPhysics().ObjectSprite.transform.position, BottomBoltZapPoint.transform.position);
        TopLightningBolt.ShowBolt();
        BottomLightningBolt.ShowBolt();

        //StopCoroutine(_superlaserCoroutine);
        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
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
        foreach (var rend in LaserVFX)
        {
            rend.enabled = false;
        }
        GetComponent<Animation>().Stop();
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().sprite = flinchSprite;
        yield return new WaitForSeconds(flinchDuration);
        entityPhysics.ObjectSprite.GetComponent<SpriteRenderer>().sprite = normalSprite;
        CurrentState = FinalBossState.CHASE;
    }
}
