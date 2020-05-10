using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class behaves as a spawn point for enemies - specifically that noted by the _prefab field.
/// It performs object pooling, each spawner having its own pool.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private int _maxEnemies;
    [SerializeField] private Transform _prefab;
    [SerializeField] private float _spawnInterval;
    public EnvironmentPhysics _startEnvironment;
    [SerializeField] private GameObject _playerPhysics;

    [SerializeField] private NavigationManager _navManager;
    [SerializeField] private bool canRespawnEnemies; //toggle object pooling
    [SerializeField] private ElementType _shieldElementType = ElementType.NONE;
    private Dictionary<int, GameObject> _enemyPool;
    private float _timer;
    private int enemiesAlive;

	void Start ()
    {
        enemiesAlive = 0;
        _timer = _spawnInterval;
        if (_spawnInterval < 1) _spawnInterval = 1.0f; //floor spawninterval

        if (_enemyPool != null) return;

        Debug.Log("Pool populating...");
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
    public GameObject SpawnEnemy(ElementType shieldType = ElementType.NONE)
    {
        GameObject tempEnemy = GetFromPool();
        EntityPhysics tempPhysics = tempEnemy.GetComponentInChildren<EntityPhysics>();
        tempPhysics.SetObjectElevation(_startEnvironment.GetTopHeight() + 2f);
        // tempPhysics.GetComponent<Rigidbody2D>().MovePosition((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset);
        tempPhysics.transform.parent.position = (Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset - new Vector2(0f, 2f);
        Debug.Log("<color=red>" + _startEnvironment + "</color>");
        Debug.Log((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset);
        tempPhysics.GetComponent<Rigidbody2D>().MovePosition((Vector2)_startEnvironment.transform.position + _startEnvironment.GetComponent<BoxCollider2D>().offset - new Vector2(0f, 2f));
        tempEnemy.GetComponentInChildren<PathfindingAI>().SetPath(_startEnvironment);

        //give shield
        EntityHandler tempHandler = tempEnemy.GetComponentInChildren<EntityHandler>();
        if (tempHandler)
        {
            tempHandler.ActivateShield(shieldType);
        }
        else { Debug.LogError("Spawned enemy without EntityHandler!"); }
        return tempEnemy;
    }

    //========================================| Object Pooling |======================================

    public GameObject GetFromPool()
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

        //if there are no more inactive objects
        GameObject tempEnemy = Instantiate(_prefab, transform).gameObject;
        tempEnemy.GetComponentInChildren<EntityAI>().navManager = _navManager;
        tempEnemy.GetComponentInChildren<EntityPhysics>().navManager = _navManager;
        tempEnemy.GetComponentInChildren<PathfindingAI>().target = _playerPhysics;
        tempEnemy.GetComponentInChildren<EntityPhysics>()._spawner = this;


        //tempEnemy.GetComponentInChildren<BulletHandler>().SourceWeapon = this;
        _enemyPool.Add(tempEnemy.GetInstanceID(), tempEnemy);
        return tempEnemy;
    }

    public void ReturnToPool(int instanceID)
    {
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
