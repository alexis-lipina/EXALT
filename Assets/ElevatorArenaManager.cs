using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// elevator arena that does some tricks to make combat on the elevator bearable, since combat on a moving object
// sounds prone to issues. Works like this:
// 1. platform begins to move when triggered.
// 2. geometry beneath the platform becomes INACTIVE at a certain distance (20?) from the start.
// 3. geometry above the platform becomes visible at a certain distance (20?) from the end. (this could just always be there)
public class ElevatorArenaManager : MonoBehaviour
{
    [SerializeField] private EntityPhysics PlayerPhysics;
    [SerializeField] private EnvironmentPhysics[] ElevatorPlatform; // platform that the elevator is made out of
    [SerializeField] private EnvironmentPhysics[] LowerFloor; // Floor elements from below that should be disabled when the elevator reaches a certain height
    [SerializeField] private EnvironmentPhysics[] IllusoryPillars; // Pillar objects that move during Loop phase to give the illusion of motion
    [SerializeField] private TriggerVolume StartingVolume;

    enum ElevatorArenaState { WaitingAtLowerFloor, RunningTowardLoop, Looping, RunningAwayFromLoop, StoppedAtUpperFloor };
    private ElevatorArenaState CurrentElevatorState = ElevatorArenaState.WaitingAtLowerFloor;

    // elevation uses the TOP of the surface
    [SerializeField] float StartingElevation = -1.0f;
    [SerializeField] float EndingElevation = 127.0f;
    [SerializeField] float ElevatorRiseDuration = 60.0f; // seconds
    [SerializeField] float ElevatorRiseTime = 0.0f;
    float CurrentElevation;
    float ElevatorSpeed;

    [SerializeField] float CombatLoop_StartElevation; // elevation at which the elevator will STOP, lower floor will DISAPPEAR, and pillars will move, giving the illusion that the elevator is in motion
    [SerializeField] float CombatLoop_DeltaToLoop; // Height difference necessary for pillars to leap back this same amount 
    float CombatLoop_CurrentDelta; // elevation at which the elevator will STOP, lower floor will DISAPPEAR, 
    float PillarOriginalBottomHeight;
    float PillarOriginalTopHeight;
    Vector3 PillarOriginalSprite0LocalTransform;
    Vector3 PillarOriginalSprite1LocalTransform;

    [SerializeField] EnemySpawner[] MeleeSpawners;
    [SerializeField] EnemySpawner[] RangedSpawners;


    [SerializeField] int WaveOne_NumSwords; //combat music should fade out and fade in between combat phases, to indicate when player can rest
    [SerializeField] int WaveOne_NumRanged;
    [SerializeField] ElementType WaveOne_ShieldType;


    [SerializeField] int WaveTwo_NumSwords;
    [SerializeField] int WaveTwo_NumRanged;
    [SerializeField] ElementType WaveTwo_ShieldType;

    [SerializeField] int WaveThree_NumSwords;
    [SerializeField] int WaveThree_NumRanged;
    [SerializeField] ElementType WaveThree_ShieldType;


    [SerializeField] int WaveFour_NumSwords;
    [SerializeField] int WaveFour_NumRanged;
    [SerializeField] ElementType WaveFour_ShieldType;
    bool CombatOver = false;

    bool IsSpawningMelee = false;
    bool IsSpawningRanged = false;
    List<EntityPhysics> LivingEnemies;


    // Start is called before the first frame update
    void Start()
    {
        CurrentElevation = StartingElevation;
        ElevatorSpeed = (EndingElevation - StartingElevation) / ElevatorRiseDuration; // units / second
        LivingEnemies = new List<EntityPhysics>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (CurrentElevatorState)
        {
            case ElevatorArenaState.WaitingAtLowerFloor:
                if (StartingVolume.IsTriggered)
                {
                    CurrentElevatorState = ElevatorArenaState.RunningTowardLoop;
                    foreach (EnvironmentPhysics physics in LowerFloor) // prevent player from being able to "teleport back" to the floor.
                    {
                        physics.IsSavePoint = false;
                    }
                }
                break;
            case ElevatorArenaState.RunningTowardLoop:
                RunElevator(true);
                break;
            case ElevatorArenaState.Looping:
                Loop();
                break;
            case ElevatorArenaState.RunningAwayFromLoop:
                RunElevator(false);
                break;
            case ElevatorArenaState.StoppedAtUpperFloor:
                break;
        }
    }

    void RunElevator(bool StopAtLoopPoint)
    {
        ElevatorRiseTime += Time.deltaTime;
        if (ElevatorRiseTime > ElevatorRiseDuration)
        {
            ElevatorRiseTime = ElevatorRiseDuration;
            CurrentElevatorState = ElevatorArenaState.StoppedAtUpperFloor;
        }

        CurrentElevation = Mathf.Lerp(StartingElevation, EndingElevation, ElevatorRiseTime / ElevatorRiseDuration);

        foreach (EnvironmentPhysics envt in ElevatorPlatform)
        {
            envt.BottomHeight = CurrentElevation;
            envt.TopHeight = CurrentElevation + 1; // cuz theyre all thickness 1

            // top
            envt.GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition += Vector3.up * ElevatorSpeed * Time.deltaTime;
            // front
            envt.GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition += Vector3.up * ElevatorSpeed * Time.deltaTime;
        }

        if (CurrentElevation > CombatLoop_StartElevation && StopAtLoopPoint)
        {
            CurrentElevatorState = ElevatorArenaState.Looping;
            CombatLoop_CurrentDelta = 0.0f;
            PillarOriginalBottomHeight = IllusoryPillars[0].BottomHeight;
            PillarOriginalTopHeight = IllusoryPillars[0].TopHeight;
            PillarOriginalSprite0LocalTransform = IllusoryPillars[0].GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition;
            PillarOriginalSprite1LocalTransform = IllusoryPillars[0].GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition;

            foreach (EnvironmentPhysics platform in LowerFloor)
            {
                platform.gameObject.SetActive(false);
            }

            StartCoroutine(RunEnemyWaves());
        }
        if (CurrentElevation > EndingElevation)
        {
            foreach (EnvironmentPhysics envt in ElevatorPlatform)
            {
                envt.BottomHeight = EndingElevation;
                envt.TopHeight = EndingElevation + 1; // cuz theyre all thickness 1
            }
        }
    }

    void Loop()
    {
        CombatLoop_CurrentDelta += Time.deltaTime * ElevatorSpeed;
        foreach (EnvironmentPhysics envt in IllusoryPillars)
        {
            envt.BottomHeight = PillarOriginalBottomHeight - CombatLoop_CurrentDelta;
            envt.TopHeight = PillarOriginalTopHeight - CombatLoop_CurrentDelta;

            // top
            envt.GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition = PillarOriginalSprite0LocalTransform - Vector3.up * CombatLoop_CurrentDelta;
            // front
            envt.GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition = PillarOriginalSprite1LocalTransform - Vector3.up * CombatLoop_CurrentDelta;
        }

        if (CombatLoop_CurrentDelta > CombatLoop_DeltaToLoop)
        {
            CombatLoop_CurrentDelta -= CombatLoop_DeltaToLoop;

            if (CombatOver)
            {
                CurrentElevatorState = ElevatorArenaState.RunningAwayFromLoop;
                foreach (EnvironmentPhysics envt in IllusoryPillars)
                {
                    envt.BottomHeight = PillarOriginalBottomHeight;
                    envt.TopHeight = PillarOriginalTopHeight;
                    envt.GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition = PillarOriginalSprite0LocalTransform;
                    envt.GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition = PillarOriginalSprite1LocalTransform;
                }
            }
        }
    }

    private IEnumerator RunEnemyWaves()
    {
        // --- wave 1
        yield return new WaitForSeconds(2.0f);
        LivingEnemies.Clear();
        StartCoroutine(SpawnNumberOfEnemies(WaveOne_NumRanged, true, WaveOne_ShieldType));
        StartCoroutine(SpawnNumberOfEnemies(WaveOne_NumSwords, false, WaveOne_ShieldType));
        yield return new WaitForSeconds(3.0f);

        while (IsSpawningMelee || IsSpawningRanged)
        {
            yield return new WaitForSeconds(1.0f);
        }
        while (AreAnyEnemiesAlive())
        {
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(5.0f);

        // --- wave 2
        LivingEnemies.Clear();
        StartCoroutine(SpawnNumberOfEnemies(WaveTwo_NumRanged, true, WaveTwo_ShieldType));
        StartCoroutine(SpawnNumberOfEnemies(WaveTwo_NumSwords, false, WaveTwo_ShieldType));
        yield return new WaitForSeconds(1.0f);

        while (IsSpawningMelee || IsSpawningRanged)
        {
            yield return new WaitForSeconds(1.0f);
        }
        while (AreAnyEnemiesAlive())
        {
            yield return new WaitForSeconds(1.0f);
        }
        yield return new WaitForSeconds(5.0f);

        // --- wave 3
        LivingEnemies.Clear();
        StartCoroutine(SpawnNumberOfEnemies(WaveThree_NumRanged, true, WaveThree_ShieldType));
        StartCoroutine(SpawnNumberOfEnemies(WaveThree_NumSwords, false, WaveThree_ShieldType));
        yield return new WaitForSeconds(1.0f);

        while (IsSpawningMelee || IsSpawningRanged)
        {
            yield return new WaitForSeconds(1.0f);
        }
        while (AreAnyEnemiesAlive())
        {
            yield return new WaitForSeconds(1.0f);
        }
        yield return new WaitForSeconds(5.0f);

        // --- wave 4
        LivingEnemies.Clear();
        StartCoroutine(SpawnNumberOfEnemies(WaveFour_NumRanged, true, WaveFour_ShieldType));
        StartCoroutine(SpawnNumberOfEnemies(WaveFour_NumSwords, false, WaveFour_ShieldType));
        yield return new WaitForSeconds(1.0f);

        while (IsSpawningMelee || IsSpawningRanged)
        {
            yield return new WaitForSeconds(1.0f);
        }
        while (AreAnyEnemiesAlive())
        {
            yield return new WaitForSeconds(1.0f);
        }
        CombatOver = true;
    }

    private IEnumerator SpawnNumberOfEnemies(int count, bool ranged, ElementType shieldtype)
    {
        if (ranged) IsSpawningRanged = true;
        else IsSpawningMelee = true;

        bool RandomShields = shieldtype == ElementType.NONE;
        
        for (int i = 0; i < count; i++)
        {
            LivingEnemies.Add(GetFurthestEnemySpawner(ranged).SpawnEnemy(shieldtype).GetComponentInChildren<EntityPhysics>());
            yield return new WaitForSeconds(Random.Range(0.7f, 1.6f));
        }

        if (ranged) IsSpawningRanged = false;
        else IsSpawningMelee = false;
    }
    
    private EnemySpawner GetFurthestEnemySpawner(bool ranged)
    {
        float FurthestDistance = 0.0f;
        EnemySpawner FurthestSpawner = null;
        EnemySpawner[] spawners = ranged ? RangedSpawners : MeleeSpawners;
        foreach (EnemySpawner currentspawner in spawners)
        {
            float currentdistance = (PlayerPhysics.transform.position - currentspawner._startEnvironment.transform.position).sqrMagnitude;
            if (FurthestDistance < currentdistance)
            {
                FurthestDistance = currentdistance;
                FurthestSpawner = currentspawner;
            }
        }
        return FurthestSpawner;
    }

    private bool AreAnyEnemiesAlive()
    {
        foreach (EntityPhysics physics in LivingEnemies)
        {
            if (physics.GetCurrentHealth() > 0)
            {
                return true;
            }
        }
        return false;
        /*
        foreach (EnemySpawner spawner in MeleeSpawners)
        {
            if (spawner.GetEnemiesAlive() > 0)
            {
                return true;
            }
        }
        foreach (EnemySpawner spawner in RangedSpawners)
        {
            if (spawner.GetEnemiesAlive() > 0)
            {
                return true;
            }
        }
        return false;
        */
    }
}