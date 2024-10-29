using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultCutsceneManager : MonoBehaviour
{
    [SerializeField] private MovingEnvironment VaultMovingEnvironment;
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

    enum VaultCutsceneOrbitState { Compressed, Expanding, Hovering, Orbiting}
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
    [SerializeField] private List<SpriteRenderer> GlowSprites;
    [SerializeField] private AnimationCurve GlowSpeakCurve;


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

        // idea - little voice blurbs from the orbiting fragments as the main one says its thing, like whispering crowd
        // fix trickle-in text

        StartCoroutine(TrickleInTextWithAudio("welcome home.", "", new[] { 0.5f, 0.5f }, new[] { false, false }));
        yield return new WaitForSeconds(4.0f);
        StartCoroutine(TrickleInTextWithAudio("you seem confused.", "", new[] { 0.3f, 0.3f, 1.5f }, new[] { false, false, false }));
        yield return new WaitForSeconds(4.0f);
        StartCoroutine(TrickleInTextWithAudio("|WHITE|your mind is |FIRE|wracked |WHITE|with", "|VOID|chaos |WHITE|and |ZAP|discord.|WHITE|", new[] { 0.3f, 0.4f, 0.3f, 0.5f, 0.3f, 0.55f, 0.3f, 1.5f }, new[] { false, false, false, true, false, true, false, true }));
        yield return new WaitForSeconds(6.0f);
        StartCoroutine(TrickleInTextWithAudio("you dont even know", "what you are, do you?", new[] { 0.2f, 0.2f, 0.4f, 0.3f, 0.2f, 0.2f, 0.6f, 0.2f, 1.5f }, new[] {false, false, false, false, false, false, false, false, false }));
        
        yield return new WaitForSeconds(6.0f);

        StartCoroutine(TrickleInTextWithAudio("|ICHOR|let me remind you.|WHITE|", "", new[] { 0.3f, 0.3f, 0.4f, 1.2f }, new[] { true, true, true, true }));
        yield return new WaitForSeconds(1.0f);
        _player.TimeSinceCombat = 0.0f;
        _player.ForceShowUI();
        _player.ForceUIVisible = true;
        yield return new WaitForSeconds(2.0f);

        StartCoroutine(TrickleInTextWithAudio("you are my |ICHOR|blood.|WHITE|", "", new[] { 0.333f, 0.333f, 0.333f, 2.0f }, new[] { false, false, false, true }));
        yield return new WaitForSeconds(1.0f);
        _player.CollapsePlayer();
        ScreenFlash.InstanceOfScreenFlash.SetTexture(GlowVFXSprite);
        _player.OvertakeElement(ElementType.ICHOR, 1);
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.0f, 0.7f, Color.white, ElementType.ICHOR);
        yield return new WaitForSeconds(3.0f);


        StartCoroutine(TrickleInTextWithAudio("you are my |ICHOR|flesh.|WHITE|", "", new[] { 0.333f, 0.333f, 0.333f, 2.0f }, new[] { false, false, false, true }));
        yield return new WaitForSeconds(1.0f);
        _player.OvertakeElement(ElementType.ICHOR, 1);
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.0f, 0.7f, Color.white, ElementType.ICHOR);
        yield return new WaitForSeconds(3.0f);

        // zoop, only ichor
        StartCoroutine(TrickleInTextWithAudio("you are |ICHOR|mine.|WHITE|", "", new[] { 0.5f, 0.5f, 2.0f }, new[] { false, false, true }));
        yield return new WaitForSeconds(1.0f);
        _player.OvertakeElement(ElementType.ICHOR, 1);
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.0f, 0.7f, Color.white, ElementType.ICHOR);
        yield return new WaitForSeconds(3.0f);

        _hintTextManager.TrickleInHintTextWords("now kneel, and", "accept my forgiveness", new[] { 0.2f, 0.6f, 0.2f, 0.4f, 0.2f, 0.4f }); // this line sucks
        yield return new WaitForSeconds(2.0f);

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
            BossFragment_N.transform.position = originalPos_N + new Vector3(0, hoverRadius, hoverRadius)      + new Vector3(Mathf.Sin(Time.time * 0.64f) + 0.2f, Mathf.Sin(Time.time * 0.62f) + 0.2f, 0) * breathingNormalized * 2.0f;
            BossFragment_S.transform.position = originalPos_S + new Vector3(0, -hoverRadius, -hoverRadius)    + new Vector3(Mathf.Sin(Time.time * 0.71f) + 0.1f, Mathf.Sin(Time.time * 0.53f) + 0.1f, 0) * breathingNormalized * 2.0f;
            BossFragment_W.transform.position = originalPos_W + new Vector3(-hoverRadius, 0, 0)               + new Vector3(Mathf.Sin(Time.time * 0.65f) + 0.3f, Mathf.Sin(Time.time * 0.60f) + 0.3f, 0) * breathingNormalized * 2.0f;
            BossFragment_E.transform.position = originalPos_E + new Vector3(hoverRadius, 0, 0)                + new Vector3(Mathf.Sin(Time.time * 0.70f) + 0.0f, Mathf.Sin(Time.time * 0.58f) + 0.2f, 0) * breathingNormalized * 2.0f;
            BossCore.transform.position = originalPos_Core + new Vector3(0, -breathingNormalized * 2.0f, 0);
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
        while (timer < duration)
        {
            foreach (SpriteRenderer renderer in GlowSprites)
            {
                renderer.material.SetFloat("_Opacity", Mathf.Lerp(minGlow, maxGlow, GlowSpeakCurve.Evaluate(timer / duration)));
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }
        
}
