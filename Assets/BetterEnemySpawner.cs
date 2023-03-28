using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterEnemySpawner : MonoBehaviour
{
    [SerializeField] EnvironmentPhysics SpawnPlatform;
    [SerializeField] MovingEnvironment[] ChargeupPillars; // pillars at the corners (?) of the spawner which raise when there are enemies queued up
    public float TimeBetweenSpawns = 2.0f;
    [SerializeField] EnemySpawner Spawner;
    
    bool IsInfinite = false;

    int numberOfEnemiesToSpawn = 0;
    const float heightOfEachCell = 0.75f; // height of each "cell" of the charge-up pillars
    const int maxNumCells = 6;
    List<GameObject> SpawnedEnemies;
    float timer = 0.0f;
    bool EnemiesHaveShields;
    ElementType ShieldType;


    // Start is called before the first frame update
    void Start()
    {
        SpawnedEnemies = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (numberOfEnemiesToSpawn > 0)
        {
            timer += Time.deltaTime;
            if (timer > TimeBetweenSpawns)
            {
                SpawnedEnemies.Add(Spawner.SpawnEnemy());
                timer = 0.0f;
                numberOfEnemiesToSpawn--;
            }

            foreach (MovingEnvironment envt in ChargeupPillars)
            {
                envt.SetToElevation(SpawnPlatform.TopHeight - 0.5f - heightOfEachCell * maxNumCells + heightOfEachCell * (numberOfEnemiesToSpawn - timer / TimeBetweenSpawns));
            }
            Debug.Log(numberOfEnemiesToSpawn);
        }
    }


    public void QueueEnemies(int _numberOfEnemies, float _timeBetweenSpawns, bool _hasShields, ElementType _shieldType)
    {
        numberOfEnemiesToSpawn = _numberOfEnemies;
        TimeBetweenSpawns = _timeBetweenSpawns;
        EnemiesHaveShields = _hasShields;
        ShieldType = _shieldType;
    }
    IEnumerator SpawnEnemiesInfinitely()
    {
        yield return new WaitForEndOfFrame();
    }

    public bool IsWaveComplete()
    {
        foreach (GameObject enemy in SpawnedEnemies)
        {
            if (enemy.active) return false;
        }
        return numberOfEnemiesToSpawn == 0;
    }
}
