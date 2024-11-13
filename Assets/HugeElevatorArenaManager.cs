using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugeElevatorArenaManager : MonoBehaviour
{
    [SerializeField] private EnvironmentPhysics TrackingEnvironmentObject;

    [SerializeField] EnemySpawner[] WaveOneSpawners;
    [SerializeField] float WaveOneStartElevation = 50.0f;
    [SerializeField] int WaveOneSpawnLoopCount = 1;
    [SerializeField] float WaveOneSpawnDelay = 1.0f;
    [SerializeField] EnemySpawner[] WaveTwoSpawners;
    [SerializeField] float WaveTwoStartElevation = 150.0f;
    [SerializeField] int WaveTwoSpawnLoopCount = 1;
    [SerializeField] float WaveTwoSpawnDelay = 1.0f;

    private PlayerHandler _player;
    private List<GameObject> _spawnedEnemies;
    private const int MAX_ENEMIES = 1;
    private FiringChamberManager _firingChamberManager;

    private float ElevatorRideDuration; // pulled from tracking object
    private float elevatorTimer = 0.0f;
    private ElementType CurrentShieldType = ElementType.ICHOR;


    [SerializeField] AnimationCurve SpawnRate;
    [SerializeField] AnimationCurve EnemyCap;
    [SerializeField] AnimationCurve GlowScalar;

    [SerializeField] List<RestPlatform> StartRestPlatforms;

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindObjectOfType<PlayerHandler>();
        _spawnedEnemies = new List<GameObject>();
        _firingChamberManager = GameObject.FindObjectOfType<FiringChamberManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartElevatorManager()
    {
        foreach (var platform in StartRestPlatforms)
        {
            if (platform.CurrentChargeAmount < 1.0f)
            {
                return;
            }
        }
        StartCoroutine(RunElevator());
    }

    IEnumerator RunElevator()
    {
        ElevatorRideDuration = TrackingEnvironmentObject.GetComponent<MovingEnvironment>().CycleDuration;
        TrackingEnvironmentObject.GetComponent<MovingEnvironment>().ZVelocityForElevator = 400.0f / TrackingEnvironmentObject.GetComponent<MovingEnvironment>().CycleDuration;
        TrackingEnvironmentObject.GetComponent<MovingEnvironment>().PlayAnim();
        StartCoroutine(RunEnemies());
        StartCoroutine(ChangeShields(ElevatorRideDuration));
        while (elevatorTimer < ElevatorRideDuration)
        {
            EntityPhysics.KILL_PLANE_ELEVATION = TrackingEnvironmentObject.BottomHeight - 20.0f;
            _firingChamberManager.GlowScalar = GlowScalar.Evaluate(elevatorTimer / ElevatorRideDuration);
            yield return new WaitForEndOfFrame();
            elevatorTimer += Time.deltaTime;
        }
    }

    IEnumerator ChangeShields(float totalduration)
    {
        CurrentShieldType = ElementType.ICHOR;
        yield return new WaitForSeconds(totalduration / 4.0f);
        CurrentShieldType = ElementType.FIRE;
        yield return new WaitForSeconds(totalduration / 4.0f);
        CurrentShieldType = ElementType.VOID;
        yield return new WaitForSeconds (totalduration / 4.0f);
        CurrentShieldType = ElementType.ZAP;
    }


    IEnumerator RunEnemies()
    {
        while (TrackingEnvironmentObject.TopHeight < WaveOneStartElevation)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(SpawnFurthest(WaveOneSpawners, WaveOneSpawnDelay));
        /*
        while (TrackingEnvironmentObject.TopHeight < WaveTwoStartElevation)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(SpawnWinding(WaveTwoSpawners, WaveTwoSpawnDelay, WaveTwoSpawnLoopCount));*/
    }

    //spawns a 
    IEnumerator SpawnWinding(EnemySpawner[] spawners, float timeBetweenSpawns, int NumberOfLoops)
    {
        for (int i = 0; i < NumberOfLoops; i++)
        {
            foreach (EnemySpawner spawner in spawners)
            {
                spawner.SpawnEnemy(ElementType.NONE, true, 10000.0f);
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }
    }
 
    IEnumerator SpawnFurthest(EnemySpawner[] spawners, float timeBetweenSpawns) 
    {
        List<int> goodIndices = new List<int>(spawners.Length);

        while (true)// for now this is just gonna go forever
        {
            RefreshDeadEnemies();
            if (_spawnedEnemies.Count < EnemyCap.Evaluate(elevatorTimer / ElevatorRideDuration))
            {
                goodIndices.Clear();
                // refresh player distances
                for (int i = 0; i < spawners.Length; i++)
                {
                    if (Vector3.Scale(spawners[i].GetComponent<BoxCollider2D>().bounds.center - _player.GetEntityPhysics().transform.position, new Vector3(1, 1, 0)).sqrMagnitude > 1000)
                    {
                        goodIndices.Add(i);
                    }
                }

                _spawnedEnemies.Add(spawners[goodIndices[Random.Range(0, goodIndices.Count)]].SpawnEnemy(CurrentShieldType, true, 10000.0f));
            }
            
            yield return new WaitForSeconds(SpawnRate.Evaluate(elevatorTimer / ElevatorRideDuration));
        }
    }

    void RefreshDeadEnemies()
    {
        for (int i = _spawnedEnemies.Count-1; i>0; i--)
        {
            if (_spawnedEnemies[i] == null)
            {
                _spawnedEnemies.RemoveAt(i);
            }
        }
    }
}
