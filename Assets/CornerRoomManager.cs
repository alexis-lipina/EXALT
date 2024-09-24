using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// These corner rooms form the narrative structure for the Monolith ascent. They will contain
//      1) a brief wave-based combat section, beginning shortly after the player enters where exits are locked
//      2) a puzzle section where the player will complete a simple "puzzle" that reveals parts of the story through it on the mural
//      3) a "seal tablet" that gets charged up over the course of the puzzle and, when interacted with, shows the player a seal being removed from the boss' vault
public class CornerRoomManager : MonoBehaviour
{
    [Header("Initial Room Sealing")]
    [SerializeField] List<MovingEnvironment> RoomSealingObjects; // these will play forward to lock the room when the player triggers the "trap", and will play backwards (or just be open) when it is unlocked
    [SerializeField] List<SpriteRenderer> CombatGlows; // these will flash when combat begins
    [SerializeField] AnimationCurve CombatGlowFlashCurve; // curve
    [SerializeField] AnimationCurve DoorOpenCurve; // lol. moving environment should just be reversible but I'm not doing that shit rn
    [SerializeField] TriggerVolume RoomSealTrigger;
    [SerializeField] List<GameObject> DestroyOnCombatStart;
    [SerializeField] List<TriggerVolume> TriggersToEnableOnCombatStart;


    [Space(10)]
    [Header("Combat Phase")]
    [SerializeField] List<EnemySpawner> EnemySpawners; // these all trigger when combat begins and exhaust themselves to complete combat
    [SerializeField] int LiveEnemiesPerSpawner;
    [SerializeField] int TotalEnemiesPerSpawner;
    [SerializeField] float SpawnDelay;
    [SerializeField] ElementType ShieldElement = ElementType.NONE;

    [Space(10)]
    [Header("Puzzle Phase")]
    [SerializeField] List<RestPlatform> PuzzleRestPlatforms;
    [SerializeField] RestPlatform SealRestPlatform;
    private bool ShouldRunRoom = true;
    private List<bool> CoroutinesComplete;


    // Start is called before the first frame update
    void Start()
    {
        foreach (TriggerVolume trigger in TriggersToEnableOnCombatStart)
        {
            trigger.gameObject.active = false;
        }
        foreach (var restPlatform in PuzzleRestPlatforms)
        {
            restPlatform.IsUseable = false;
        }

        foreach (SpriteRenderer renderer in CombatGlows)
        {
            renderer.material.SetFloat("_Opacity", 0);
        }

        SealRestPlatform.IsUseable = false;
        CoroutinesComplete = new List<bool>();

        StartCoroutine(RunRoom());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RunRoom()
    {
        while (!RoomSealTrigger.IsTriggered)
        {
            yield return new WaitForSeconds(0.1f);
        }

        //combat start
        StartCoroutine(FlashCombatStartGlows());
        foreach (GameObject thing in DestroyOnCombatStart)
        {
            GameObject.Destroy(thing);
        }
        foreach (TriggerVolume trigger in TriggersToEnableOnCombatStart)
        {
            trigger.gameObject.active = true;
        }
        foreach (var movingobject in RoomSealingObjects)
        {
            movingobject.PlayAnim();
        }
        foreach (var spawner in EnemySpawners)
        {
            //spawner.QueueEnemies(1, 1.0f, false, ElementType.NONE);
            CoroutinesComplete.Add(false);
            StartCoroutine(RunEnemySpawner(spawner, LiveEnemiesPerSpawner, TotalEnemiesPerSpawner, SpawnDelay, ShieldElement, CoroutinesComplete.Count-1));
        }

        bool isDoneCombat = false;
        while (!isDoneCombat)
        {
            yield return new WaitForSeconds(1.0f);
            isDoneCombat = true;
            foreach (bool bIsComplete in CoroutinesComplete)
            {
                if (!bIsComplete)
                {
                    isDoneCombat = false;
                }
            }
        }

        // puzzle phase
        foreach (SpriteRenderer renderer in CombatGlows)
        {
            renderer.material.SetFloat("_Opacity", 0.0f);
        }

        foreach (var restPlatform in PuzzleRestPlatforms)
        {
            restPlatform.IsUseable = true;
            while (restPlatform.CurrentChargeAmount < 1.0f)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        SealRestPlatform.IsUseable = true;

        while (SealRestPlatform.CurrentChargeAmount < 1)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(OpenDoor());
    }

    IEnumerator FlashCombatStartGlows()
    {
        float duration = 1.0f;
        float timer = 0;
        while (timer < duration)
        {
            foreach (SpriteRenderer renderer in CombatGlows)
            {
                renderer.material.SetFloat("_Opacity", CombatGlowFlashCurve.Evaluate(timer / duration));
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        foreach (SpriteRenderer renderer in CombatGlows)
        {
            renderer.material.SetFloat("_Opacity", 1.0f);
        }
    }

    IEnumerator OpenDoor()
    {
        float duration = 1.0f;
        float timer = 0;
        while (timer < duration)
        {
            foreach (MovingEnvironment door in RoomSealingObjects)
            {
                door.SetToElevation(DoorOpenCurve.Evaluate(timer / duration));
            }
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
    }

    public void OnLoadFromVault()
    {
        // setup stuff
        ShouldRunRoom = false;
        StopAllCoroutines();
        foreach (GameObject thing in DestroyOnCombatStart)
        {
            GameObject.Destroy(thing);
        }
        foreach (TriggerVolume trigger in TriggersToEnableOnCombatStart)
        {
            trigger.gameObject.active = false;
        }

        RoomSealTrigger.enabled = false;
    }

    IEnumerator RunEnemySpawner(EnemySpawner spawner, int MaxLiveEnemies, int TotalEnemies, float SpawnDelay, ElementType ShieldType = ElementType.NONE, int CoroutinesCompleteIndex = 0)
    {
        List<GameObject> LivingEnemies = new List<GameObject>();
        while (TotalEnemies > 0)
        {
            for (int i = LivingEnemies.Count-1; i >= 0; i--)
            {
                if (!LivingEnemies[i])
                {
                    LivingEnemies.RemoveAt(i);
                }
            }

            if (LivingEnemies.Count < MaxLiveEnemies)
            {
                yield return new WaitForSeconds(SpawnDelay);
                LivingEnemies.Add(spawner.SpawnEnemy(ShieldType, true, 100000));
                TotalEnemies--;
            }
            yield return new WaitForSeconds(0.5f);
        }

        while (LivingEnemies.Count > 0)
        {
            for (int i = LivingEnemies.Count - 1; i >= 0; i--)
            {
                if (!LivingEnemies[i])
                {
                    LivingEnemies.RemoveAt(i);
                }
            }
            yield return new WaitForSeconds(1.0f); // prevents coroutine from evaporating so we can track completion
        }
        CoroutinesComplete[CoroutinesCompleteIndex] = true;
    }
}
