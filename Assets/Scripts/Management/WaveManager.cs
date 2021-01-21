using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class acts as GM for an arena level. It manages waves, deploying enemies at good spawn points and stuff
public class WaveManager : MonoBehaviour
{
    [SerializeField] private float TimeBetweenSpawns = 1.0f; // TODO : Consider putting this in the JSON file, or if it should be fixed make it a constant
    [SerializeField] private List<EnemySpawner> RangedSpawnLocations;
    [SerializeField] private List<EnemySpawner> MeleeSpawnLocations;
    [SerializeField] private EntityPhysics Player;

    [SerializeField] private WaveChangeUIManager WaveChangeUI;

    private List<Wave> WaveList;
    private int CurrentWave = 0;
    private bool IsNextWaveReady = false;

    private float RangedRespawnTimer = 0f;
    private float MeleeRespawnTimer = 0f;

    private List<GameObject> LivingMeleeEnemies;
    private List<GameObject> LivingRangedEnemies;

    private int GetNumberOfMeleeAlive()
    {
        int numberAlive = 0;
        List<int> indicesToDelete = new List<int>();
        for (int i = 0; i < LivingMeleeEnemies.Count; i++)
        {
            if (LivingMeleeEnemies[i] == null || !LivingMeleeEnemies[i].activeSelf)
            {
                indicesToDelete.Add(i);
            }
            else
            {
                ++numberAlive;
            }
        }

        //delete dead ones - iterate backward so indices dont become invalid
        for (int i = indicesToDelete.Count - 1; i >= 0; i--)
        {
            LivingMeleeEnemies.RemoveAt(indicesToDelete[i]);
            Debug.Log("Wave Melee Enemy Dead");
        }

        return numberAlive;
    }

    private int GetNumberOfRangedAlive()
    {
        int numberAlive = 0;
        List<int> indicesToDelete = new List<int>();
        for (int i = 0; i < LivingRangedEnemies.Count; i++)
        {
            if (LivingRangedEnemies[i] == null || !LivingRangedEnemies[i].activeSelf)
            {
                indicesToDelete.Add(i);
                Debug.Log("Wave Ranged Enemy Dead");
            }
            else
            {
                ++numberAlive;
            }
        }

        //delete dead ones - iterate backward so indices dont become invalid
        for (int i = indicesToDelete.Count - 1; i >= 0; i--)
        {
            LivingRangedEnemies.RemoveAt(indicesToDelete[i]);
        }
        return numberAlive;
    }

    void Start()
    {
        WaveList = new List<Wave>();

        LivingMeleeEnemies = new List<GameObject>();
        LivingRangedEnemies = new List<GameObject>();


        for (int i = 1; i < 5; i++)
        {
            Wave wave = new Wave();
            wave.MaxMelee = i;
            wave.MaxRanged = i;
            wave.TotalMelee = i * 2;
            wave.TotalRanged = i * 2;
            WaveList.Add(wave);
        }
        IsNextWaveReady = true;
    }


    void Update()
    {
        if (IsNextWaveReady == true)
        {
            if (CurrentWave >= WaveList.Count)
            {
                // a winner is you!
                Debug.Log("A winner is you!");
            }
            else
            {
                StartCoroutine(RunWave(CurrentWave));
                IsNextWaveReady = false;
                CurrentWave++;
            }
        }

        // handle timers
        MeleeRespawnTimer -= Time.deltaTime;
        RangedRespawnTimer -= Time.deltaTime;

    }

    /// <summary>
    /// Begin execution at the start of the wave
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator RunWave(int index)
    {
        yield return new WaitForSeconds(1.0f);

        WaveChangeUI.PlayWaveChange(index);

        yield return new WaitForSecondsRealtime(5.0f);
        Debug.Log("<color=red>Wave " + index + " start!</color>");
        Wave currentWave = WaveList[index];
        ElementType newEnemyShieldType = ElementType.NONE;

        //continue doing wave things until there are no enemies left on the field and no enemies left to deploy
        while (currentWave.TotalMelee > 0 || currentWave.TotalRanged > 0 || GetNumberOfMeleeAlive() > 0 || GetNumberOfRangedAlive() > 0)
        {
            //spawn ranged enemy if applicable
            if (currentWave.TotalRanged > 0 && GetNumberOfRangedAlive() < currentWave.MaxRanged && RangedRespawnTimer < 0.0f)
            {
                SpawnRanged(GetRandomShieldedStatus(index / 4.0f));
                RangedRespawnTimer = TimeBetweenSpawns;
                --currentWave.TotalRanged;
            }
            //spawn melee enemy if applicable
            if (currentWave.TotalMelee > 0 && GetNumberOfMeleeAlive() < currentWave.MaxMelee && MeleeRespawnTimer < 0.0f)
            {
                SpawnMelee(GetRandomShieldedStatus(index / 4.0f));
                MeleeRespawnTimer = TimeBetweenSpawns;
                --currentWave.TotalMelee;
            }

            yield return new WaitForEndOfFrame();
        }

        Debug.Log("End of wave!");
        IsNextWaveReady = true;
    }

    void SpawnRanged(ElementType shieldType = ElementType.NONE)
    {
        Debug.Log("Spawning Ranged");
        EnemySpawner currentSpawner = RangedSpawnLocations[0];
        float currentDistance = 1000000000f;
        //get spawner furthest from player
        foreach (EnemySpawner es in RangedSpawnLocations)
        {
            if ( (es._startEnvironment.transform.position - Player.transform.position).magnitude > currentDistance )
            {
                currentDistance = (es._startEnvironment.transform.position - Player.transform.position).magnitude;
                currentSpawner = es;
            }
        }

        LivingRangedEnemies.Add(currentSpawner.SpawnEnemy(shieldType));
    }

    void SpawnMelee(ElementType shieldType = ElementType.NONE)
    {
        Debug.Log("Spawning Melee");
        EnemySpawner currentSpawner = MeleeSpawnLocations[0];
        float currentDistance = 1000000000f;
        //get spawner furthest from player
        foreach (EnemySpawner es in MeleeSpawnLocations)
        {
            if ((es._startEnvironment.transform.position - Player.transform.position).magnitude > currentDistance)
            {
                currentDistance = (es._startEnvironment.transform.position - Player.transform.position).magnitude;
                currentSpawner = es;
            }
        }

        LivingMeleeEnemies.Add(currentSpawner.SpawnEnemy(shieldType));
    }

    ElementType GetRandomShieldedStatus(float chanceOfShield)
    {
        if (Random.Range(0.0f, 1.0f) < chanceOfShield)
        {
            return GetRandomElement();
        }
        return ElementType.NONE;
    }

    ElementType GetRandomElement()
    {
        switch (Random.Range(0, 3))
        {
            case 0:
                return ElementType.VOID;
            case 1:
                return ElementType.FIRE;
            case 2:
                return ElementType.ZAP;
            default:
                Debug.LogError("How the hell did this happen?");
                return ElementType.NONE;
        }
    }
}
