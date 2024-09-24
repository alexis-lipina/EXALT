using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnEnemySpawnEvent : UnityEvent<GameObject> { }


/// <summary>
/// This class behaves as a spawn point for enemies - specifically that noted by the _prefab field.
/// It performs object pooling, each spawner having its own pool.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] public bool IsAutomaticallySpawning = false;
    [SerializeField] public bool UseObjectPooling = false; // im tired man. fix this later. maybe.
    [SerializeField] private int _maxEnemies;
    [SerializeField] private Transform _prefab;
    [SerializeField] private float _spawnInterval;
    public EnvironmentPhysics _startEnvironment;
    [SerializeField] private GameObject _playerPhysics;

    [SerializeField] private NavigationManager _navManager;
    [SerializeField] private bool canRespawnEnemies; //toggle object pooling
    [SerializeField] private ElementType _shieldElementType = ElementType.NONE;
    public OnEnemySpawnEvent OnEnemySpawn;
    private Dictionary<int, GameObject> _enemyPool;
    private float _timer;
    private int enemiesAlive;

	void Start ()
    {
        enemiesAlive = 0;
        _timer = _spawnInterval;
        if (_spawnInterval < 1) _spawnInterval = 1.0f; //floor spawninterval

        if (_enemyPool != null) return;

        //Debug.Log("Pool populating...");
        if (!UseObjectPooling) return;

        _enemyPool = new Dictionary<int, GameObject>();
        GameObject tempEnemy;
        for (int i = 0; i < _maxEnemies; i++)
        {
            tempEnemy = Instantiate(_prefab, transform).gameObject;
            //tempBullet.GetComponentInChildren<Rigidbody2D>().position = new Vector2(1000, 1000);
            tempEnemy.GetComponentInChildren<EntityAI>().navManager = _navManager;
            tempEnemy.GetComponentInChildren<EntityPhysics>().navManager = _navManager;
            tempEnemy.GetComponentInChildren<PathfindingAI>().target = _playerPhysics;
            tempEnemy.GetComponentInChildren<EntityPhysics>()._spawner = this;
            tempEnemy.SetActive(false);
            _enemyPool.Add(tempEnemy.GetInstanceID(), tempEnemy);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!IsAutomaticallySpawning) return; // early return if automatic spawning behavior not wanted

        //pool contains "dead" enemies
        //goal of spawner is to have all enemies alive
        //only can spawn one every so often
        //time management (a skill I find myself lacking)
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            _timer = 0f;
        }


        if (enemiesAlive == _maxEnemies)
        {
            //all enemies are alive, do nothin
        }
        else
        {
            //spawn a bad guy if timer is up
            if (_timer == 0)
            {

                SpawnEnemy(_shieldElementType); //THIS SHOULD BE COMMENTED OUT UNDER NORMAL CIRCUMSTANCES
                //move enemy into position

                _timer = _spawnInterval;
            }
            else
            {
                //if timer aint up, wait
            }
        }
        

	}


    /// <summary>
    /// Returns enemy spawned
    /// </summary>
    /// <returns></returns>
    public GameObject SpawnEnemy(ElementType shieldType = ElementType.NONE, bool isHostile = true, float overrideDetectionRange = -1.0f)
    {
        GameObject tempEnemy = GetFromPool();
        EntityPhysics tempPhysics = tempEnemy.GetComponentInChildren<EntityPhysics>();
        tempPhysics.SetObjectElevation(_startEnvironment.GetTopHeight() + 0.5f);
        // tempPhysics.GetComponent<Rigidbody2D>().MovePosition((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset);
        tempPhysics.transform.parent.position = (Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset; //- new Vector2(0f, 2f);
        Debug.Log("<color=red>" + _startEnvironment + "</color>");
        Debug.Log((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset);
        tempPhysics.GetComponent<Rigidbody2D>().MovePosition((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset + new Vector2(Random.Range(-2.0f, 2.0f), Random.Range(-1.5f, 1.5f))); //- new Vector2(0f, 2f); 
        tempEnemy.GetComponentInChildren<PathfindingAI>().SetPath(_startEnvironment);
        if (overrideDetectionRange > 0)
        {
            tempEnemy.GetComponentInChildren<PathfindingAI>().SetDetectionRange(overrideDetectionRange);
        }
        if (!isHostile)
        {
            tempEnemy.GetComponentInChildren<PathfindingAI>().SetDetectionRange(0.0f);
        }
        tempPhysics.Heal(tempPhysics.GetMaxHealth() - tempPhysics.GetCurrentHealth());

        //give shield
        EntityHandler tempHandler = tempEnemy.GetComponentInChildren<EntityHandler>();
        if (tempHandler)
        {
            tempHandler.ActivateShield(shieldType);
        }
        else { Debug.LogError("Spawned enemy without EntityHandler!"); }
        OnEnemySpawn.Invoke(tempEnemy);
        return tempEnemy;
    }

    //========================================| Object Pooling |======================================

    public GameObject GetFromPool()
    {
        if (UseObjectPooling)
        {
            foreach (KeyValuePair<int, GameObject> entry in _enemyPool)
            {
                if (entry.Value == null) continue;
                if (!entry.Value.activeSelf)
                {
                    enemiesAlive++;

                    entry.Value.SetActive(true);
                    Debug.Log("Deploying");
                    return entry.Value;
                }
            }
        }

        //if there are no more inactive objects
        GameObject tempEnemy = Instantiate(_prefab, transform).gameObject;
        tempEnemy.GetComponentInChildren<EntityAI>().navManager = _navManager;
        tempEnemy.GetComponentInChildren<EntityPhysics>().navManager = _navManager;
        tempEnemy.GetComponentInChildren<PathfindingAI>().target = _playerPhysics;
        tempEnemy.GetComponentInChildren<EntityPhysics>()._spawner = this;
        //tempEnemy.GetComponentInChildren<BulletHandler>().SourceWeapon = this;
        tempEnemy.SetActive(true);
        enemiesAlive++;

        if (!UseObjectPooling) return tempEnemy;

        _enemyPool.Add(tempEnemy.GetInstanceID(), tempEnemy);
        return tempEnemy;
    }

    public void ReturnToPool(int instanceID)
    {
        if (!UseObjectPooling)
        {
            enemiesAlive--;
            return;
        }
        if (!canRespawnEnemies)
        {
            _enemyPool[instanceID].gameObject.SetActive(false);
            _enemyPool.Remove(instanceID);
            return;
        }

        //Debug.Log("Returning to Pool");
        if (_enemyPool.ContainsKey(instanceID))
        {
            if (_enemyPool[instanceID].activeSelf)
            {
                enemiesAlive--;
                Debug.Log("Retracting");
                //_enemyPool[instanceID].GetComponentInChildren<Rigidbody2D>().MovePosition((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset - new Vector2(0f, 5f));
                //_enemyPool[instanceID].GetComponentInChildren<Rigidbody2D>().position = (Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset - new Vector2(0f, 5f);
                //_bulletPool[instanceID].GetComponentInChildren<DynamicPhysics>().MoveCharacterPosition();
                _enemyPool[instanceID].SetActive(false);
            }
            else
            {
                Debug.Log("Attempting to return object to pool that is already in pool.");
            }
        }
        else
        {
            Debug.Log("Invalid InstanceID - Object not in pool.");
        }
    }

    public int GetEnemiesAlive()
    {
        return enemiesAlive;
    }



}
