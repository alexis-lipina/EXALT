using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultCutsceneManager : MonoBehaviour
{
    [SerializeField] private MovingEnvironment VaultMovingEnvironment;
    [SerializeField] private MovingEnvironment InvisiblePlatform;
    [SerializeField] private AnimationCurve NormalizedDarknessFade;
    [SerializeField] private float DarknessFadeDuration = 1.0f;
    [SerializeField] private ExitVolume ExitToFinalBoss;
    [SerializeField] private List<GameObject> KillTheseNow;
    [SerializeField] private AudioClip NormalAmbience;
    [SerializeField] private AudioClip CutsceneAmbience;
    [SerializeField] private Sprite GlowVFXSprite;
    [SerializeField] private CameraAttractor CutsceneCameraAttractor;
    [SerializeField] private List<GameObject> EnableAtEnd;
    [SerializeField] private List<GameObject> InvisibleWalls;
    [SerializeField] private List<GameObject> DestroyAtEnd;

    enum VaultCutsceneOrbitState { Compressed, Expanding, Hovering, Orbiting }
    private VaultCutsceneOrbitState cutsceneOrbitState = VaultCutsceneOrbitState.Compressed;
    [SerializeField] private GameObject BossCore;
    [SerializeField] private GameObject BossFragment_N;
    [SerializeField] private GameObject BossFragment_S;
    [SerializeField] private GameObject BossFragment_W;
    [SerializeField] private GameObject BossFragment_E;
    private PlayerHandler _player;
    private LevelManager _levelManager;
    private HintTextManager _hintTextManager;
    [SerializeField] private AnimationCurve BreathingCurve;
    [SerializeField] private AudioClip StrongVoice;
    [SerializeField] private AudioClip WeakVoice;
    [SerializeField] private AudioClip AmbienceLoop;
    [SerializeField] private List<SpriteRenderer> GlowSprites;
    [SerializeField] private AnimationCurve GlowSpeakCurve;
    [SerializeField] private AnimationCurve SpeakPitchCurve; // not actual pitch, just for the glowy band to shift
    [SerializeField] private SpriteRenderer GlowyElementOrbSprite;
    [SerializeField] private SpriteRenderer GlowyElementOrbGlowSprite;
    [SerializeField] private SpriteRenderer SuspensionBeam;
    private float SuspensionBeamGlowAmount = 0.0f;
    [SerializeField] private AnimationCurve PlayerLiftAnimCurve;
    [SerializeField] private AnimationCurve PlayerDropAnimCurve;
    private const float PlayerLiftedElevation = 5.0f;
    private const float PlayerDroppedElevation = 1.6f;


    private void Awake()
    {
        _levelManager = GameObject.FindObjectOfType<LevelManager>();
        _player = GameObject.FindObjectOfType<PlayerHandler>();
        foreach (var asdf in EnableAtEnd)
        {
            asdf.active = false;
        }
        foreach (var asdf in InvisibleWalls)
        {
            asdf.active = false;
        }
        foreach (SpriteRenderer renderer in GlowSprites)
        {
            renderer.material.SetFloat("_Opacity", 0);
        }
        BossCore.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", 0.5f);
        BossFragment_N.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", 0.5f);
        BossFragment_S.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", 0.5f);
        BossFragment_E.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", 0.5f);
        BossFragment_W.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", 0.5f);
        GlowyElementOrbSprite.gameObject.SetActive(false);
        SuspensionBeam.material.SetFloat("_Opacity", 0.0f);

        StartCoroutine(RunSuspensionBeam());
    }

    private void Start()
    {
        _hintTextManager = HintTextManager.GetInstanceOf();
        MusicManager.GetMusicManager().CrossfadeToSong(1.0f, NormalAmbience);
    }

    public void StartVaultCutscene()
    {
        StartCoroutine(VaultCutscene());
        StartCoroutine(HoverFragments());
    }

    IEnumerator VaultCutscene()
    {
        foreach (var asdf in InvisibleWalls)
        {
            asdf.active = true;
        }
        StartCoroutine(Breathe(10.0f));

        VaultMovingEnvironment.PlayAnim();
        MusicManager.GetMusicManager().CrossfadeToSong(3.0f, CutsceneAmbience);
        yield return new WaitForSeconds(7.0f);
        Camera.main.gameObject.GetComponent<CameraScript>().AddAttractor(CutsceneCameraAttractor);
        foreach (GameObject kill in KillTheseNow)
        {
            GameObject.Destroy(kill);
        }
        float DarknessFadeTimer = 0;
        float LevelManagerStartOffset = _levelManager.elevationOffset;
        _player.GetEntityPhysics().Gravity = 0.0f;
        //_player.GetEntityPhysics().SetElevation(5.8f);
        while (DarknessFadeTimer < DarknessFadeDuration)
        {
            DarknessFadeTimer += Time.deltaTime;
            Shader.SetGlobalFloat("_MaxElevationOffset", Mathf.Lerp(0, LevelManagerStartOffset, NormalizedDarknessFade.Evaluate(DarknessFadeTimer / DarknessFadeDuration)));
            yield return new WaitForEndOfFrame();
            // also cut the audio in here, have that fade out to low trancey drone
        }
        yield return new WaitForSeconds(2.0f);


        // control player, force to walk forward
        _player.BlockInput = true;
        //_player.ForceHideUI();
        _player.ForceStandFromRest();

        MusicManager.GetMusicManager().CrossfadeToSong(1.0f, AmbienceLoop);

        // idea - little voice blurbs from the orbiting fragments as the main one says its thing, like whispering crowd
        // fix trickle-in text

        StartCoroutine(TrickleInTextWithAudio("welcome home.", "", new[] { 0.5f, 0.0f, 0.5f }, new[] { false, false, false }));
        yield return new WaitForSeconds(4.0f);
        StartCoroutine(TrickleInTextWithAudio("you seem confused.", "", new[] { 0.3f, 0.3f, 0.0f, 1.5f }, new[] { false, false, false, false }));
        yield return new WaitForSeconds(4.0f);
        StartCoroutine(TrickleInTextWithAudio("|WHITE|your mind is |FIRE|wracked |WHITE|with", "|VOID|chaos |WHITE|and |ZAP|discord.|WHITE|", new[] { 0.3f, 0.4f, 0.4f, 0.5f, 0.4f, 0.55f, 0.4f, 1.5f }, new[] { false, false, false, true, false, true, false, true, false }));
        yield return new WaitForSeconds(6.0f);
        StartCoroutine(TrickleInTextWithAudio("you dont even know", "what you are, do you?", new[] { 0.2f, 0.2f, 0.4f, 0.3f, 0.2f, 0.2f, 0.6f, 0.2f, 1.5f }, new[] { false, false, false, false, false, false, false, false, false, false }));

        yield return new WaitForSeconds(6.0f);

        StartCoroutine(TrickleInTextWithAudio("|ICHOR|let me remind you.|WHITE|", "", new[] { 0.3f, 0.3f, 0.4f, 0.0f, 1.2f}, new[] { true, true, true, true, true }));
        yield return new WaitForSeconds(0.5f);
        
        StartCoroutine(RampSuspensionBeam(0.5f, 0.8f));
        _player.TimeSinceCombat = 0.0f;
        _player.ForceShowUI();
        _player.ForceUIVisible = true;
        _player.Lift();
        _player.GetEntityPhysics().transform.position = new Vector3(0, -12, 0);
        StartCoroutine(LiftPlayer(1.0f));
        yield return new WaitForSeconds(1.0f);

        Camera.main.gameObject.GetComponent<CameraScript>().SmoothToSize(1.5f, 3.0f);
        StartCoroutine(TranslateCamera(3.0f, -35.0f));
        yield return new WaitForSeconds(3.0f);

        GlowyElementOrbSprite.gameObject.SetActive(true);
        GlowyElementOrbSprite.material.SetColor("_MagicColor", new Color(1.0f, 0.5f, 0.0f));
        GlowyElementOrbGlowSprite.material.SetFloat("_CurrentElement", 2);
        GlowyElementOrbSprite.GetComponent<Animator>().Play("ElementOrb_Absorb");

        StartCoroutine(TrickleInTextWithAudio("you are my |ICHOR|blood.|WHITE|", "", new[] { 0.66f, 0.66f, 0.66f, 0.0f, 2.0f }, new[] { false, false, false, true, true }));
        yield return new WaitForSeconds(2.0f);
        GlowyElementOrbSprite.GetComponent<Animator>().Play("ElementOrb_Burst");
        ScreenFlash.InstanceOfScreenFlash.SetTexture(GlowVFXSprite);
        _player.OvertakeElement(ElementType.ICHOR, 1);
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.0f, 0.7f, Color.white, ElementType.ICHOR);
        yield return new WaitForSeconds(4.0f);

        GlowyElementOrbSprite.material.SetColor("_MagicColor", new Color(0.5f, 0.0f, 1.0f));
        GlowyElementOrbGlowSprite.material.SetFloat("_CurrentElement", 3);
        GlowyElementOrbSprite.GetComponent<Animator>().Play("ElementOrb_Absorb");

        StartCoroutine(TrickleInTextWithAudio("you are my |ICHOR|flesh.|WHITE|", "", new[] { 0.66f, 0.66f, 0.66f, 0.0f, 2.0f }, new[] { false, false, false, true, true }));
        yield return new WaitForSeconds(2.0f);
        GlowyElementOrbSprite.GetComponent<Animator>().Play("ElementOrb_Burst");
        _player.OvertakeElement(ElementType.ICHOR, 1);
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.0f, 0.7f, Color.white, ElementType.ICHOR);
        yield return new WaitForSeconds(4.0f);

        GlowyElementOrbSprite.material.SetColor("_MagicColor", new Color(0f, 1.0f, 0.5f));
        GlowyElementOrbGlowSprite.material.SetFloat("_CurrentElement", 4);
        GlowyElementOrbSprite.GetComponent<Animator>().Play("ElementOrb_Absorb");

        // zoop, only ichor
        StartCoroutine(TrickleInTextWithAudio("you are |ICHOR|mine.|WHITE|", "", new[] { 1.0f, 1.0f, 0.0f, 2.0f }, new[] { false, false, true, true }));
        yield return new WaitForSeconds(2.0f);
        GlowyElementOrbSprite.GetComponent<Animator>().Play("ElementOrb_Burst");
        _player.OvertakeElement(ElementType.ICHOR, 1);
        _player.CollapsePlayer();
        StartCoroutine(DropPlayer(0.5f));
        StartCoroutine(RampSuspensionBeam(1.0f, 0.0f));
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.0f, 0.7f, Color.white, ElementType.ICHOR);
        yield return new WaitForSeconds(3.0f);
        StartCoroutine(TrickleInTextWithAudio("now kneel, and", "accept my forgiveness", new[] { 0.3f, 0.6f, 0.2f, 0.4f, 0.2f, 0.4f }, new[] { false, true, false, false, false, false })); // this line sucks
        yield return new WaitForSeconds(2.0f);

        Camera.main.gameObject.GetComponent<CameraScript>().SmoothToSize(3.0f, 3.0f);
        StartCoroutine(TranslateCamera(3.0f, 20.0f));

        // call lightning
        _player.StandFromCollapsePlayer();
        _player.BlockInput = false;
        foreach (var asdf in EnableAtEnd)
        {
            asdf.active = true;
        }
        foreach (var asdf in DestroyAtEnd)
        {
            GameObject.Destroy(asdf);
        }
        StartCoroutine(Breathe(100.0f));



        //_hintTextManager.ShowHintText("dearest child", "");
        //_hintTextManager.ShowHintText("something inside you", "lashes out at us");
        //_hintTextManager.ShowHintText("fear not.", "we will eradicate it.");
        //_hintTextManager.ShowHintText("and then", "we will make you whole");

        yield return new WaitForSeconds(2.0f);
        FadeTransition.FadeColor = Color.white;
        //ExitToFinalBoss.ChangeLevel();
    }

    private IEnumerator HoverFragments()
    {
        float hoverRadiusMin = 1f;
        float hoverRadiusMax = 12f;
        float hoverRadius = 0.0f;
        float startTime = Time.time;
        float breathingDuration = 10.0f;

        Vector3 originalPos_Core = BossCore.transform.position;
        Vector3 originalPos_N = BossFragment_N.transform.position;
        Vector3 originalPos_S = BossFragment_S.transform.position;
        Vector3 originalPos_W = BossFragment_W.transform.position;
        Vector3 originalPos_E = BossFragment_E.transform.position;
        while (true)
        { //                                                                                                            Random floaty
            float breathingNormalized = BreathingCurve.Evaluate(((Time.time - startTime) % breathingDuration) / breathingDuration);
            hoverRadius = Mathf.Lerp(hoverRadiusMin, hoverRadiusMax, breathingNormalized);
            BossFragment_N.transform.position = originalPos_N + new Vector3(0, hoverRadius, hoverRadius) + new Vector3(Mathf.Sin(Time.time * 0.64f) + 0.2f, Mathf.Sin(Time.time * 0.62f) + 0.2f, 0) * breathingNormalized * 2.0f;
            BossFragment_S.transform.position = originalPos_S + new Vector3(0, -hoverRadius, -hoverRadius) + new Vector3(Mathf.Sin(Time.time * 0.71f) + 0.1f, Mathf.Sin(Time.time * 0.53f) + 0.1f, 0) * breathingNormalized * 2.0f;
            BossFragment_W.transform.position = originalPos_W + new Vector3(-hoverRadius, 0, 0) + new Vector3(Mathf.Sin(Time.time * 0.65f) + 0.3f, Mathf.Sin(Time.time * 0.60f) + 0.3f, 0) * breathingNormalized * 2.0f;
            BossFragment_E.transform.position = originalPos_E + new Vector3(hoverRadius, 0, 0) + new Vector3(Mathf.Sin(Time.time * 0.70f) + 0.0f, Mathf.Sin(Time.time * 0.58f) + 0.2f, 0) * breathingNormalized * 2.0f;
            BossCore.transform.position = originalPos_Core + new Vector3(0, -breathingNormalized * 2.0f, 0);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator TranslateCamera(float duration, float offset)
    {
        float timer = 0f;
        Vector3 startPos = CutsceneCameraAttractor.transform.position;
        Vector3 endPos = CutsceneCameraAttractor.transform.position + new Vector3(0.0f, offset, 0.0f);

        while (timer < duration)
        {
            CutsceneCameraAttractor.transform.position = Vector3.Lerp(startPos, endPos, SpeakPitchCurve.Evaluate(timer / duration));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator TrickleInTextWithAudio(string lineOne, string lineTwo, float[] delays, bool[] impactStrengths)
    {
        _hintTextManager.TrickleInHintTextWords(lineOne, lineTwo, delays);
        for (int i = 0; i < delays.Length; i++)
        {
            GetComponent<AudioSource>().clip = impactStrengths[i] ? StrongVoice : WeakVoice;
            GetComponent<AudioSource>().pitch = Random.Range(0.94f, 1.05f);
            GetComponent<AudioSource>().Play();
            StartCoroutine(FlashGlow(delays[i], impactStrengths[i]));
            yield return new WaitForSeconds(delays[i]);
        }
        _hintTextManager.HideHintText();
    }

    private IEnumerator FlashGlow(float duration, bool bIsIntense)
    {
        float timer = 0;
        float minGlow = bIsIntense ? 0.8f : 0.4f;
        float maxGlow = bIsIntense ? 1.4f : 0.8f;

        //float _RippleWidth;
        //float _RippleIntensity;
        //float _RipplePosition;

        float bossPitchOld = BossCore.GetComponent<SpriteRenderer>().material.GetFloat("_RipplePosition");
        float northPitchOld = BossFragment_N.GetComponent<SpriteRenderer>().material.GetFloat("_RipplePosition");
        float southPitchOld = BossFragment_S.GetComponent<SpriteRenderer>().material.GetFloat("_RipplePosition");
        float eastPitchOld = BossFragment_E.GetComponent<SpriteRenderer>().material.GetFloat("_RipplePosition");
        float westPitchOld = BossFragment_W.GetComponent<SpriteRenderer>().material.GetFloat("_RipplePosition");

        float bossPitchNew = Random.Range(0.2f, 0.8f);
        float northPitchNew = Random.Range(0.2f, 0.8f);
        float southPitchNew = Random.Range(0.2f, 0.8f);
        float eastPitchNew = Random.Range(0.2f, 0.8f);
        float westPitchNew = Random.Range(0.2f, 0.8f);


        while (timer < duration)
        {
            foreach (SpriteRenderer renderer in GlowSprites)
            {
                renderer.material.SetFloat("_Opacity", Mathf.Lerp(minGlow, maxGlow, GlowSpeakCurve.Evaluate(timer / duration)));
            }

            BossCore.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", Mathf.Lerp(bossPitchOld, bossPitchNew, SpeakPitchCurve.Evaluate(timer / Mathf.Min(0.5f, duration))));
            BossFragment_N.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", Mathf.Lerp(northPitchOld, northPitchNew, SpeakPitchCurve.Evaluate(timer / Mathf.Min(0.5f, duration))));
            BossFragment_S.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", Mathf.Lerp(southPitchOld, southPitchNew, SpeakPitchCurve.Evaluate(timer / Mathf.Min(0.5f, duration))));
            BossFragment_E.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", Mathf.Lerp(eastPitchOld, eastPitchNew, SpeakPitchCurve.Evaluate(timer / Mathf.Min(0.5f, duration))));
            BossFragment_W.GetComponent<SpriteRenderer>().material.SetFloat("_RipplePosition", Mathf.Lerp(westPitchOld, westPitchNew, SpeakPitchCurve.Evaluate(timer / Mathf.Min(0.5f, duration))));

            BossCore.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", Mathf.Lerp(minGlow, maxGlow, GlowSpeakCurve.Evaluate(timer / duration)) * 2.0f);
            BossFragment_N.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", Mathf.Lerp(minGlow, maxGlow, GlowSpeakCurve.Evaluate(timer / duration)) * 2.0f);
            BossFragment_S.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", Mathf.Lerp(minGlow, maxGlow, GlowSpeakCurve.Evaluate(timer / duration)) * 2.0f);
            BossFragment_E.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", Mathf.Lerp(minGlow, maxGlow, GlowSpeakCurve.Evaluate(timer / duration)) * 2.0f);
            BossFragment_W.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", Mathf.Lerp(minGlow, maxGlow, GlowSpeakCurve.Evaluate(timer / duration)) * 2.0f);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Breathe(float duration)
    {
        float timer = 0;
        float minGlow = 0.2f;
        float maxGlow = 1.0f;
        float breathsPerSecond = 2.0f;

        while (timer < duration)
        {
            float breathStrength = Mathf.Lerp(minGlow, maxGlow, (Mathf.Sin(timer * breathsPerSecond) / 2.0f) + 0.5f);
            foreach (SpriteRenderer renderer in GlowSprites)
            {
                renderer.material.SetFloat("_Opacity", breathStrength);
            }

            BossCore.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", breathStrength);
            BossFragment_N.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", breathStrength);
            BossFragment_S.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", breathStrength);
            BossFragment_E.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", breathStrength);
            BossFragment_W.GetComponent<SpriteRenderer>().material.SetFloat("_RippleIntensity", breathStrength);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator RampSuspensionBeam(float duration, float target)
    {
        float timer = 0.0f;
        float original = SuspensionBeamGlowAmount;
        while (timer < duration)
        {
            SuspensionBeamGlowAmount = Mathf.Lerp(original, target, timer / duration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator RunSuspensionBeam()
    {
        //SuspensionBeamGlowAmount;
        float frequency = 15.0f;
        float originalXScale = SuspensionBeam.transform.localScale.x;

        while (true)
        {
            float Sine = Mathf.Sin(Time.time * frequency) * 0.1f + 1f;
            SuspensionBeam.material.SetFloat("_Opacity", SuspensionBeamGlowAmount);
            SuspensionBeam.transform.localScale = new Vector3(originalXScale * Sine, SuspensionBeam.transform.localScale.y, SuspensionBeam.transform.localScale.z);
            yield return null;
        }

    }
    private IEnumerator LiftPlayer(float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            InvisiblePlatform.SetToElevation(Mathf.Lerp(PlayerDroppedElevation, PlayerLiftedElevation, PlayerLiftAnimCurve.Evaluate(timer / duration)));
            timer += Time.deltaTime;
            Debug.Log("lift");
            yield return null;
        }
        InvisiblePlatform.SetToElevation(PlayerLiftedElevation);
    }
    private IEnumerator DropPlayer(float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            InvisiblePlatform.SetToElevation(Mathf.Lerp(PlayerLiftedElevation, PlayerDroppedElevation, PlayerDropAnimCurve.Evaluate(timer / duration)));
            timer += Time.deltaTime;
            yield return null;
        }
        InvisiblePlatform.SetToElevation(PlayerLiftedElevation);
    }
}