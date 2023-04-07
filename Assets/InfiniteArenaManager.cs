using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InfiniteArenaManager : MonoBehaviour
{
    public List<BetterEnemySpawner> MeleeSpawners;
    public List<BetterEnemySpawner> RangedSpawners;
    public EnemyWave EnemyComposition;

    int currentWaveIndex = 0;
    bool IsRunning = false;

    public void StartCombat()
    {
        if (IsRunning) return;
        IsRunning = true;
        foreach (BetterEnemySpawner spawner in MeleeSpawners)
        {
            spawner.QueueEnemies(EnemyComposition.NumberOfMeleePerSpawner, EnemyComposition.TimeBetweenMeleeSpawns, EnemyComposition.HasShields, EnemyComposition.ShieldType);
        }
        foreach (BetterEnemySpawner spawner in RangedSpawners)
        {
            spawner.QueueEnemies(EnemyComposition.NumberOfRangedPerSpawner, EnemyComposition.TimeBetweenRangedSpawns, EnemyComposition.HasShields, EnemyComposition.ShieldType);
        }
    }

    public void StopCombat()
    {
        if (!IsRunning) return;
        IsRunning = false;
        
        foreach (BetterEnemySpawner spawner in MeleeSpawners)
        {
            spawner.StopSpawning();
        }
        foreach (BetterEnemySpawner spawner in RangedSpawners)
        {
            spawner.StopSpawning();
        }
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
