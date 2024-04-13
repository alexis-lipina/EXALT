using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackRestPlatform : MonoBehaviour
{
    
    [SerializeField] ZapFXController TopLightningBolt;
    [SerializeField] ZapFXController BottomLightningBolt;
    [SerializeField] List<ZapFXController> LightningBolts_Near; // these are all background
    [SerializeField] List<ZapFXController> LightningBolts_Mid;
    [SerializeField] List<ZapFXController> LightningBolts_Far;
    
    // Note that these animation curves store 2 "normalized" value ranges. The range x=0...1 defines the value as charge builds, and the range 1...2 defines the value over the duration that the bolt takes to call down.
    [SerializeField] AnimationCurve BoltThicknessModifierOverCharge;
    [SerializeField] AnimationCurve BoltFrequencyOverCharge;
    [SerializeField] AnimationCurve FlashScaleOverCharge;
    [SerializeField] AnimationCurve AmbientGlowOverCharge;
    [SerializeField] AnimationCurve ColumnGlowOverCharge;
    [SerializeField] private AnimationCurve BoltGlowOverTime;
    [SerializeField] private AnimationCurve BoltDistanceOverCharge;
    [SerializeField] private AnimationCurve ZapTargetPointBrightnessOverCharge;
    [SerializeField] private AnimationCurve ZapTargetPointScaleOverCharge;
    [SerializeField] private AnimationCurve CameraSizeOverCharge;
    [SerializeField] private AnimationCurve CameraAttractorPullOverCharge;
    [SerializeField] private List<SpriteRenderer> ColumnGlows;
    [SerializeField] private List<SpriteRenderer> AmbientGlows;
    [SerializeField] private List<GameObject> TargetTrackingObjects; // these follow the target on the x axis
    [SerializeField] private Animation CallLightningAnimation;
    [SerializeField] private CameraAttractor platformAttractor;
    //[SerializeField] private bool bLightningBoltActive = false;
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _fragmentStrikeAudioClip;
    private RestPlatform restPlatform;
    private PlayerHandler _player;
    public bool bShouldRunRampUp = true;

    [Space(10)]
    [Header("Play Lightning Bolt")]
    public FinalBossFragment CurrentTargetFragment;
    public FinalBossCore FinalBoss;
    [SerializeField] private GameObject TopBoltSource;


    float timer = 0.0f;
    float charge = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        restPlatform = GetComponent<RestPlatform>();
        // init lightning
        foreach (var asdf in LightningBolts_Near)
        {
            asdf.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }
        // init lightning
        foreach (var asdf in LightningBolts_Mid)
        {
            asdf.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }
        // init lightning
        foreach (var asdf in LightningBolts_Far)
        {
            asdf.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }
        StartCoroutine(RunVFX());
        StartCoroutine(RampVFX());
        _player = GameObject.FindObjectOfType<PlayerHandler>();
        foreach (SpriteRenderer asdf in ColumnGlows)
        {
            asdf.material.SetFloat("_Opacity", 0.0f);
        }
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        charge = restPlatform.CurrentChargeAmount;
        /*
        if (timer > 1.0f)
        {
            timer = 0.0f;
            TopLightningBolt.SetupLine(Vector3.zero, new Vector3(0, 36, 0));
            TopLightningBolt.Play(0.5f);
            BottomLightningBolt.SetupLine(Vector3.zero, new Vector3(0, 36, 0));
            BottomLightningBolt.Play(0.5f);
            Debug.LogError("Lightning!");
        }*/
    }


    // specifically runs the ramping-up-speed lightning vfx
    IEnumerator RunVFX()
    {
        while (true)
        {
            //float charge = ControllingRestPlatform.CurrentChargeAmount;

            ZapFXController bolt = null;
            float boltOffset = 0f;
            switch (Random.Range(0, 3))
            {
                case 0:
                    bolt = LightningBolts_Near[Random.Range(0, LightningBolts_Near.Count)];
                    boltOffset = 25.0f;
                    break;
                case 1:
                    bolt = LightningBolts_Mid[Random.Range(0, LightningBolts_Mid.Count)];
                    boltOffset = 15.0f;
                    break;
                case 2:
                    bolt = LightningBolts_Far[Random.Range(0, LightningBolts_Far.Count)];
                    boltOffset = 7.5f;
                    break;
            }
                    
            if (bolt.isActiveAndEnabled)
            {
                bolt.SetupLine(Vector3.zero, new Vector3(0, -boltOffset * BoltDistanceOverCharge.Evaluate(charge), 0)); // change distance of bolt over charge
                bolt.SetThickness(BoltThicknessModifierOverCharge.Evaluate(charge), BoltThicknessModifierOverCharge.Evaluate(charge) + 0.125f);
                bolt.Play(0.2f);
                StartCoroutine(PulseLightningGlow(bolt.GetComponentInChildren<SpriteRenderer>(), 0.5f));
            }
            

            //bolt.SetThickness(BoltThicknessModifierOverCharge.Evaluate(charge), BoltSizeOverCharge.Evaluate(currentChargeAmount) * 2.0f);
            //bolt.SetupLine(StartPosition, Vector3.Lerp(StartPosition, EndPosition, BoltDistanceOverCharge.Evaluate(currentChargeAmount)));
            //bolt.Play(BoltDurationOverCharge.Evaluate(currentChargeAmount));
            //yield return new WaitForSeconds(Random.Range(0.8f, 1.2f) * BoltDelayOverCharge.Evaluate(currentChargeAmount));
            yield return new WaitForSeconds(Random.Range(0.0f, 0.6f) / BoltFrequencyOverCharge.Evaluate(charge));
        }
    }

    // calls one bolt of lightning
    IEnumerator PulseLightningGlow(SpriteRenderer sprite, float duration)
    {
        sprite.enabled = true;

        float scaleModifier = FlashScaleOverCharge.Evaluate(charge);

        sprite.transform.localScale *= scaleModifier;

        float timer = 0.0f;
        while (timer < duration)
        {
            sprite.material.SetFloat("_Opacity", BoltGlowOverTime.Evaluate(timer / duration));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        sprite.transform.localScale /= scaleModifier;
        sprite.enabled = false;
    }

    public void LightningBolt()
    {
        StartCoroutine(PlayLightningBolt());
    }

    IEnumerator RampVFX()
    {
        // during the ramp...
        while (true)
        {
            //if (charge > 0.0f && !bLightningBoltActive && _player.IsResting())
            if (bShouldRunRampUp)
            {
                // TODO : ground projection rune materializes
                // player eye flare could charge?
                // ambient lightning gets more intense
                // ambient thunder gets louder in the mix, maybe louder thunderclap audio samples are used or the source has an effect on it changed
                // column of storm energy starts to fade in, like the air is warming up
                foreach (SpriteRenderer asdf in ColumnGlows)
                {
                    asdf.material.SetFloat("_Opacity", ColumnGlowOverCharge.Evaluate(charge));
                }

                foreach (SpriteRenderer asdf in AmbientGlows)
                {
                    asdf.material.SetFloat("_Opacity", AmbientGlowOverCharge.Evaluate(charge));
                }
                // these follow the target's x position
                foreach (GameObject o in TargetTrackingObjects)
                {
                    o.transform.position = new Vector3(CurrentTargetFragment.GetEntityPhysics().ObjectSprite.transform.position.x, o.transform.position.y, o.transform.position.z);
                }

                // electrical zaps start to flicker in at the tips of the target
                CurrentTargetFragment.TopBoltZapPoint.GetComponent<SpriteRenderer>().material.SetFloat("_Opacity", ZapTargetPointBrightnessOverCharge.Evaluate(charge));
                CurrentTargetFragment.TopBoltZapPoint.transform.localScale = new Vector3(ZapTargetPointScaleOverCharge.Evaluate(charge) * 20, ZapTargetPointScaleOverCharge.Evaluate(charge) * 5, 1);
                //CurrentTargetFragment.BottomBoltZapPoint.GetComponent<SpriteRenderer>().material.SetFloat("_Opacity", ZapTargetPointBrightnessOverCharge.Evaluate(charge));
                //CurrentTargetFragment.BottomBoltZapPoint.transform.localScale = new Vector3(ZapTargetPointScaleOverCharge.Evaluate(charge) * 20, ZapTargetPointScaleOverCharge.Evaluate(charge) * 5, 1);
                //CurrentTargetFragment.FullFragmentGlowFX.material.SetFloat("_CurrentElement", 4);
                //CurrentTargetFragment.FullFragmentGlowFX.material.SetFloat("_Opacity", charge);

                FinalBoss.BossCameraVolume.GetComponent<CameraSizeChangeVolume>().IsSizeChangeActive = false;
                Camera.main.GetComponent<CameraScript>().SetCameraSizeImmediate(CameraSizeOverCharge.Evaluate(charge));
                Debug.Log(CameraSizeOverCharge.Evaluate(charge));
                Camera.main.GetComponent<CameraScript>().AddAttractor(platformAttractor);
                platformAttractor.PullMagnitude = CameraAttractorPullOverCharge.Evaluate(charge);

                // particle effects?
            }
            yield return new WaitForEndOfFrame();
        }
    }

    // called when fully charged, damage boss
    IEnumerator PlayLightningBolt()
    {
        //CallLightningAnimation.Play();
        _audioSource.clip = _fragmentStrikeAudioClip;
        _audioSource.Play();

        // ANTICIPATION sees the sky start to glow, a column start to appear 

        //yield return new WaitForSecondsRealtime(1.0f);

        //CallLightningAnimation.Play();
        //bLightningBoltActive = true;
        bShouldRunRampUp = false;
        const float anticipationDuration = 1.0f;
        float anticipationTimer = 0;
        while (anticipationTimer < anticipationDuration)
        {
            foreach (SpriteRenderer sr in ColumnGlows)
            {
                sr.material.SetFloat("_Opacity", ColumnGlowOverCharge.Evaluate(1 + anticipationTimer / anticipationDuration));
            }
            foreach (SpriteRenderer asdf in AmbientGlows)
            {
                asdf.material.SetFloat("_Opacity", AmbientGlowOverCharge.Evaluate(1 + anticipationTimer / anticipationDuration));
            }
            // these follow the target's x position
            foreach (GameObject o in TargetTrackingObjects)
            {
                o.transform.position = new Vector3(CurrentTargetFragment.GetEntityPhysics().ObjectSprite.transform.position.x, o.transform.position.y, o.transform.position.z);
            }
            CurrentTargetFragment.TopBoltZapPoint.GetComponent<SpriteRenderer>().material.SetFloat("_Opacity", ZapTargetPointBrightnessOverCharge.Evaluate(1 + anticipationTimer / anticipationDuration));
            CurrentTargetFragment.TopBoltZapPoint.transform.localScale = new Vector3(ZapTargetPointScaleOverCharge.Evaluate(1 + anticipationTimer / anticipationDuration) * 20, ZapTargetPointScaleOverCharge.Evaluate(1 + anticipationTimer / anticipationDuration) * 5, 1);

            anticipationTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // FLASH
        // bolt of lightning hits the current target
        if (CurrentTargetFragment)
        {
            TopLightningBolt.SetupLine(TopBoltSource.transform.position, CurrentTargetFragment.TopBoltZapPoint.transform.position);
            //BottomLightningBolt.SetupLine(_player.GetEntityPhysics().ObjectSprite.transform.position + new Vector3(0.0f, 2.0f, 0.0f), CurrentTargetFragment.BottomBoltZapPoint.transform.position);
            TopLightningBolt.Play(0.2f);
            //BottomLightningBolt.Play(0.2f);
            CurrentTargetFragment.LightningFlashGlow.gameObject.SetActive(true);
            foreach (SpriteRenderer asdf in ColumnGlows)
            {
                asdf.material.SetFloat("_Opacity", 3);
            }

            foreach (SpriteRenderer asdf in AmbientGlows)
            {
                asdf.material.SetFloat("_Opacity", 1);
            }
            foreach (var sdf in LightningBolts_Far)
            {
                sdf.gameObject.SetActive(false);
            }
            foreach (var sdf in LightningBolts_Mid)
            {
                sdf.gameObject.SetActive(false);
            }
            foreach (var sdf in LightningBolts_Near)
            {
                sdf.gameObject.SetActive(false);
            }

            FinalBoss.OnStruckByLightning();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            Time.timeScale = 0.0f;

            CurrentTargetFragment.TopBoltZapPoint.GetComponent<SpriteRenderer>().material.SetFloat("_Opacity", 2.0f);
            CurrentTargetFragment.TopBoltZapPoint.transform.localScale = new Vector3(80, 10, 1);
            //CurrentTargetFragment.BottomBoltZapPoint.GetComponent<SpriteRenderer>().material.SetFloat("_Opacity", 2.0f);
            //CurrentTargetFragment.BottomBoltZapPoint.transform.localScale = new Vector3(30, 5, 1);
            CurrentTargetFragment.FullFragmentGlowFX.material.SetFloat("_Opacity", 0.0f); // this is now just the fragments emissive ichor glow, which we dont care about here
            CurrentTargetFragment.GetEntityPhysics().ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 1.0f);
            CurrentTargetFragment.GetEntityPhysics().ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", Color.black);
            TopLightningBolt.GetComponentsInChildren<MeshRenderer>()[0].material.SetColor("_Color", Color.white);
            TopLightningBolt.GetComponentsInChildren<MeshRenderer>()[1].material.SetColor("_Color", Color.white);
            foreach (SpriteRenderer asdf in ColumnGlows)
            {
                asdf.material.SetFloat("_Opacity", 1f);
            }
            yield return new WaitForSecondsRealtime(0.06f);

            // flicker between black and white
            CurrentTargetFragment.TopBoltZapPoint.transform.localScale = new Vector3(50, 8, 1);
            CurrentTargetFragment.GetEntityPhysics().ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", Color.white);
            TopLightningBolt.GetComponentsInChildren<MeshRenderer>()[0].material.SetColor("_Color", Color.black);
            TopLightningBolt.GetComponentsInChildren<MeshRenderer>()[1].material.SetColor("_Color", Color.black);
            foreach (SpriteRenderer asdf in ColumnGlows)
            {
                asdf.material.SetFloat("_Opacity", 3);
            }
            yield return new WaitForSecondsRealtime(0.06f);

            CurrentTargetFragment.TopBoltZapPoint.transform.localScale = new Vector3(80, 10, 1);
            CurrentTargetFragment.GetEntityPhysics().ObjectSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", Color.black);
            TopLightningBolt.GetComponentsInChildren<MeshRenderer>()[0].material.SetColor("_Color", Color.white);
            TopLightningBolt.GetComponentsInChildren<MeshRenderer>()[1].material.SetColor("_Color", Color.white);
            foreach (SpriteRenderer asdf in ColumnGlows)
            {
                asdf.material.SetFloat("_Opacity", 1);
            }
            yield return new WaitForSecondsRealtime(0.06f);

        }
        
        //yield return new WaitForSecondsRealtime(0.25f);
        Time.timeScale = 1.0f;
        //Camera.main.GetComponent<CameraScript>().Jolt(20, Vector2.down);
        // restore ambient fx to original state
        CurrentTargetFragment.GetEntityPhysics().ObjectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 0.0f);
        CurrentTargetFragment.TopBoltZapPoint.GetComponent<SpriteRenderer>().material.SetFloat("_Opacity", 0.0f);
        foreach (SpriteRenderer sr in ColumnGlows)
        {
            sr.material.SetFloat("_Opacity", ColumnGlowOverCharge.Evaluate(0));
        }
        foreach (SpriteRenderer asdf in AmbientGlows)
        {
            asdf.material.SetFloat("_Opacity", AmbientGlowOverCharge.Evaluate(0));
        }


        CurrentTargetFragment.LightningFlashGlow.gameObject.SetActive(false);
        foreach (var sdf in LightningBolts_Far)
        {
            sdf.gameObject.SetActive(true);
        }
        foreach (var sdf in LightningBolts_Mid)
        {
            sdf.gameObject.SetActive(true);
        }
        foreach (var sdf in LightningBolts_Near)
        {
            sdf.gameObject.SetActive(true);
        }

        _player.ForceStandFromRest();
        
        foreach (SpriteRenderer sr in ColumnGlows)
        {
            sr.material.SetFloat("_Opacity", 0.0f);
        }
        //bLightningBoltActive = false;
        platformAttractor.PullMagnitude = CameraAttractorPullOverCharge.Evaluate(0);
        //restPlatform.CurrentChargeAmount = 0.99f;
    }
}
