using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossIntroSequence : MonoBehaviour
{
    [SerializeField] PlayerHandler player;
    [SerializeField] TwoStateElevator PlayerCarryingElevator;
    private HintTextManager hintTextManager;

    private static readonly string[] BOSS_LINE_1 = { "Ominous welcome message", "" };
    private static readonly string[] BOSS_LINE_2 = { "chastises you for rebelling", "" };
    private static readonly string[] BOSS_LINE_3 = { "something about cleansing", "your false elements" };
    private static readonly string[] BOSS_LINE_4 = { "Tells you to kneel", "and repent" };

    // Start is called before the first frame update
    void Start()
    {
        
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
        player.GetEntityPhysics().transform.position = PlayerCarryingElevator.GetComponentInChildren<EnvironmentPhysics>().transform.position;
        hintTextManager = HintTextManager.GetInstanceOf();
        while (PlayerCarryingElevator.GetState() != TwoStateElevator.TwoStateElevatorState.AtTopFloor)
        {
            yield return new WaitForSeconds(0.3f);
        }

        hintTextManager.ShowHintText(BOSS_LINE_1[0], BOSS_LINE_1[1]); // TODO : have some visual & sound effect when it says something
        yield return new WaitForSeconds(4.0f);
        hintTextManager.ShowHintText(BOSS_LINE_2[0], BOSS_LINE_2[1]);
        yield return new WaitForSeconds(4.0f); 
        hintTextManager.ShowHintText(BOSS_LINE_3[0], BOSS_LINE_3[1]); // actually take away the elements here
        yield return new WaitForSeconds(4.0f); 
        hintTextManager.ShowHintText(BOSS_LINE_4[0], BOSS_LINE_4[1]);

        player.StandFromCollapsePlayer();

    }

}
