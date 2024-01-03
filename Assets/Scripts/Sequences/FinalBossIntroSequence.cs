using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossIntroSequence : MonoBehaviour
{
    [SerializeField] PlayerHandler player;
    [SerializeField] TwoStateElevator PlayerCarryingElevator;
    [SerializeField] List<SpriteRenderer> ParalysisBeamVFX;
    [SerializeField] AnimationCurve ParalysisBeamPulseSustainOpacityCurve;
    [SerializeField] AnimationCurve ParalysisBeamPulseOffOpacityCurve;
    private HintTextManager hintTextManager;

    /*
    private static readonly string[] BOSS_LINE_1 = { "Ominous welcome message", "" };
    private static readonly float[] BOSS_LINE_3_DURATIONS = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }; // TODO : use these to animate the text
    private static readonly string[] BOSS_LINE_2 = { "HAVE YOU COME TO ATONE", "FOR YOUR TRANSGRESSIONS?" };
    private static readonly string[] BOSS_LINE_3 = { "YOU WILL BE CLEANSED OF", "YOUR FALSE IDOLS" };
    private static readonly float[] BOSS_LINE_3_DURATIONS = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }; // TODO : use these to animate the text
    private static readonly string[] BOSS_LINE_4 = { "Tells you to kneel", "and repent" };
    */
    private static readonly string[] BOSS_LINE_1 = { "WELCOME HOME, |ichor|CHILD", "" };
    private static readonly float[] BOSS_LINE_1_DURATIONS = { 0.5f, 1.0f, 1.0f}; 
    private static readonly string[] BOSS_LINE_2 = { "|ichor|BLOOD|white| OF MY |ichor|BLOOD", "" };
    private static readonly float[] BOSS_LINE_2_DURATIONS = { 0.5f, 0.25f, 0.25f, 0.5f };
    private static readonly string[] BOSS_LINE_3 = { "|white|YOU HAVE |ichor|SCORNED", "|white|MY GIFTS" };
    private static readonly float[] BOSS_LINE_3_DURATIONS = { 0.25f, 0.25f, 0.4f, 0.25f, 1 };
    private static readonly string[] BOSS_LINE_4 = { "|white|AND SUCCUMBED TO |zap|TEMPTATIONS", "|white|OF |fire|FALSE |void|GODS" };
    private static readonly float[] BOSS_LINE_4_DURATIONS = { 0.25f, 0.4f, 0.2f, 0.4f, 0.2f, 0.4f, 0.4f };
    private static readonly string[] BOSS_LINE_5 = { "|white|YOU RETURN TO ME AS", "AN |zap|ABOMINATION" };
    private static readonly float[] BOSS_LINE_5_DURATIONS = { 0.25f, 0.25f, 0.25f, 0.25f, 0.4f, 0.25f, 1.0f }; 
    private static readonly string[] BOSS_LINE_6 = { "|white|BE |ichor|CLEANSED|white| BY OUR BLOOD", "AND MADE WHOLE AGAIN" };
    private static readonly float[] BOSS_LINE_6_DURATIONS = { 0.25f, 0.5f, 0.2f, 0.2f, 0.5f, 0.2f, 0.2f, 0.2f, 0.2f }; 
    private static readonly string[] BOSS_LINE_7 = { "NOW KNEEL IN ATONEMENT", "" };
    private static readonly float[] BOSS_LINE_7_DURATIONS = { 0.25f, 0.5f, 0.2f, 0.2f }; // TODO : use these to animate the text

    // Start is called before the first frame update
    void Start()
    {
        foreach (SpriteRenderer sprite in ParalysisBeamVFX)
        {
            sprite.gameObject.active = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunSequence()
    {
        StartCoroutine(RunSequenceCoroutine());
    }

    IEnumerator RunSequenceCoroutine()
    {
        // hit player with paralysis, wait for a sec for them to realize whats going on
        player.GetEntityPhysics().transform.position = PlayerCarryingElevator.GetComponentInChildren<EnvironmentPhysics>().transform.position;
        player.BlockInput = true;
        //StartCoroutine(PulseSustainParalysisBeam(2.0f));
        yield return new WaitForSeconds(2.0f);

        // lift player slowly, wait until at the top
        PlayerCarryingElevator.Toggle();
        while (PlayerCarryingElevator.GetState() != TwoStateElevator.TwoStateElevatorState.AtTopFloor)
        {
            yield return new WaitForSeconds(0.3f);
        }

        // start talking to the player
        player.BlockInput = false;
        hintTextManager = HintTextManager.GetInstanceOf();
        hintTextManager.ShowHintText(BOSS_LINE_1[0], BOSS_LINE_1[1]); // TODO : have some visual & sound effect when it says something
        yield return new WaitForSeconds(4.0f);
        //hintTextManager.HideHintText();
        yield return new WaitForSeconds(1.0f);
        //hintTextManager.ShowHintText(BOSS_LINE_2[0], BOSS_LINE_2[1]);
        hintTextManager.TrickleInHintTextWords(BOSS_LINE_2[0], BOSS_LINE_2[1], BOSS_LINE_2_DURATIONS);
        yield return new WaitForSeconds(4.0f);
        //hintTextManager.HideHintText();
        yield return new WaitForSeconds(1.0f);

        hintTextManager.ShowHintText(BOSS_LINE_3[0], BOSS_LINE_3[1]); 
        hintTextManager.TrickleInHintTextWords(BOSS_LINE_3[0], BOSS_LINE_3[1], BOSS_LINE_3_DURATIONS);
        yield return new WaitForSeconds(4.0f);

        hintTextManager.ShowHintText(BOSS_LINE_4[0], BOSS_LINE_4[1]);
        hintTextManager.TrickleInHintTextWords(BOSS_LINE_4[0], BOSS_LINE_4[1], BOSS_LINE_4_DURATIONS);
        yield return new WaitForSeconds(4.0f);

        hintTextManager.ShowHintText(BOSS_LINE_5[0], BOSS_LINE_5[1]);
        hintTextManager.TrickleInHintTextWords(BOSS_LINE_5[0], BOSS_LINE_5[1], BOSS_LINE_5_DURATIONS);
        yield return new WaitForSeconds(4.0f);

        hintTextManager.ShowHintText(BOSS_LINE_6[0], BOSS_LINE_6[1]);
        hintTextManager.TrickleInHintTextWords(BOSS_LINE_6[0], BOSS_LINE_6[1], BOSS_LINE_6_DURATIONS);
        yield return new WaitForSeconds(4.0f);
        player.ForceUIVisible = true;
        yield return new WaitForSeconds(2.0f);
        StartCoroutine(PulseOffParalysisBeam(2.0f));
        player.OvertakeElement(ElementType.ICHOR);
        player.CollapsePlayer();
        yield return new WaitForSeconds(4.0f);
        hintTextManager.HideHintText();
        yield return new WaitForSeconds(1.0f);
        hintTextManager.TrickleInHintTextWords(BOSS_LINE_7[0], BOSS_LINE_7[1], BOSS_LINE_7_DURATIONS);

        player.StandFromCollapsePlayer();

    }

    IEnumerator PulseSustainParalysisBeam(float duration)
    {
        float timer = 0.0f;
        foreach (SpriteRenderer sprite in ParalysisBeamVFX)
        {
            sprite.gameObject.active = true;
        }
        while (timer < duration)
        {
            foreach (SpriteRenderer sprite in ParalysisBeamVFX)
            {
                sprite.material.SetFloat("_Opacity", ParalysisBeamPulseSustainOpacityCurve.Evaluate(timer / duration));
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator PulseOffParalysisBeam(float duration)
    {
        float timer = 0.0f;
        foreach (SpriteRenderer sprite in ParalysisBeamVFX)
        {
            sprite.gameObject.active = true;
        }
        while (timer < duration)
        {
            foreach (SpriteRenderer sprite in ParalysisBeamVFX)
            {
                sprite.material.SetFloat("_Opacity", ParalysisBeamPulseOffOpacityCurve.Evaluate(timer / duration));
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        foreach (SpriteRenderer sprite in ParalysisBeamVFX)
        {
            sprite.gameObject.active = false;
        }
    }

}
