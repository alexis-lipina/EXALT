using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    public int NumberOfRangedPerSpawner = 1;
    public int NumberOfMeleePerSpawner = 1;
    public bool HasShields = false;
    public ElementType ShieldType = ElementType.NONE; // none means random shields if HasShields is false
    public float TimeBetweenRangedSpawns = 2.0f;
    public float TimeBetweenMeleeSpawns = 2.0f;
    public float TimeBeforeWave = 3.0f;
}

public class WaveArenaManager : MonoBehaviour
{
    public List<BetterEnemySpawner> MeleeSpawners;
    public List<BetterEnemySpawner> RangedSpawners;
    public List<EnemyWave> Waves;

    int currentWaveIndex = 0;
    bool HasStarted = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    public void StartWaves()
    {
        if (HasStarted) return;
        HasStarted = true;
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        for (int i = 0; i < Waves.Count; i++)
        {
            yield return new WaitForSeconds(Waves[i].TimeBeforeWave);
            foreach (BetterEnemySpawner spawner in MeleeSpawners)
            {
                spawner.QueueEnemies(Waves[i].NumberOfMeleePerSpawner, Waves[i].TimeBetweenMeleeSpawns, Waves[i].HasShields, Waves[i].ShieldType);
            }
            foreach (BetterEnemySpawner spawner in RangedSpawners)
            {
                spawner.QueueEnemies(Waves[i].NumberOfRangedPerSpawner, Waves[i].TimeBetweenRangedSpawns, Waves[i].HasShields, Waves[i].ShieldType);
            }
            yield return new WaitForSeconds(1.0f);
            while (!CheckIfWaveOver())
            {
                yield return new WaitForSeconds(1.0f);
            }
            Debug.Log("DONE WITH WAVE");

        }
        Debug.Log("DONE WITH WAVES!!!");
    }


    bool CheckIfWaveOver()
    {
        foreach (BetterEnemySpawner spawner in MeleeSpawners)
        {
            if (!spawner.IsWaveComplete())
            {
                return false;
            }
        }
        foreach (BetterEnemySpawner spawner in MeleeSpawners)
        {
            if (!spawner.IsWaveComplete())
            {
                return false;
            }
        }
        return true;
    }
}
