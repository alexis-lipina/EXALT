using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChainNode : MonoBehaviour
{
    private static List<int> _alreadyHit = new List<int>();
    public EntityPhysics _myEnemy;

    private const float _chainRadius = 5f;
    private const float _timeToArc = 0.25f;
    private const float _heightOfPlume = 10.0f;
    public Vector3 _sourcePosition; //where this node's source is 

    //object pooling
    private const string PREFAB_PATH = "Prefabs/Bullets/ZapComboNode";
    private static List<GameObject> _pooledNodes = new List<GameObject>();
    private int numUpdatesSinceRun = 0;
    private static int totalNumberOfObjects = 0; //count of pool size

    //VFX
    [SerializeField] private ZapFXController _plumeFX;
    [SerializeField] private ZapFXController _arcFX;



    public void Run()
    {
        numUpdatesSinceRun = 0;
        gameObject.SetActive(true);
        if (_alreadyHit == null) _alreadyHit = new List<int>();
        if (_pooledNodes == null) _pooledNodes = new List<GameObject>();

        //deal damage to enemy
        _myEnemy.Inflict(1f);

        //start to run VFX Plume and Projection
        StartCoroutine(PlayVFX());
        //add myenemy to _alreadyhit

        _alreadyHit.Add(_myEnemy.gameObject.GetInstanceID());
        
    }

    private void Update()
    {
        if (numUpdatesSinceRun == 2) StartCoroutine(Spread());
        else numUpdatesSinceRun++;
    }

    IEnumerator PlayVFX()
    {
        
        _arcFX.SetupLine(Vector2.up * _myEnemy.GetBottomHeight(), _sourcePosition - _myEnemy.transform.position);
        _arcFX.Play(_timeToArc * 0.75f);
        yield return new WaitForSeconds(_timeToArc * 0.5f);
        _plumeFX.SetupLine(Vector2.up * _myEnemy.GetBottomHeight(), Vector2.up * (_myEnemy.GetBottomHeight() + _heightOfPlume));
        _plumeFX.Play(_timeToArc * 0.5f);

    }

    IEnumerator Spread()
    {
        yield return new WaitForSeconds(_timeToArc);
        //Later, check all enemies in radius 
        Collider2D[] nearbyEnemies = Physics2D.OverlapAreaAll((Vector2)transform.position - new Vector2(_chainRadius, _chainRadius), (Vector2)transform.position + new Vector2(_chainRadius, _chainRadius));
        Debug.DrawLine((Vector2)transform.position - new Vector2(_chainRadius, _chainRadius), (Vector2)transform.position + new Vector2(_chainRadius, _chainRadius), Color.cyan, 1f, false);



        int numEnemiesNear = 0;

        //filter out non-enemies
        List<EntityPhysics> enemies = new List<EntityPhysics>();
        for (int i = 0; i < nearbyEnemies.Length; i++)
        {
            if (nearbyEnemies[i].GetComponent<EntityPhysics>())
            {
                numEnemiesNear++;
                if (nearbyEnemies[i].gameObject.tag == "Enemy")
                {
                    enemies.Add(nearbyEnemies[i].GetComponent<EntityPhysics>());
                    Debug.Log(nearbyEnemies[i].gameObject);
                }
            }
        }


        for (int i = 0; i < enemies.Count; i++)
        {
            if (!_alreadyHit.Contains(enemies[i].gameObject.GetInstanceID())) //if hasnt already been hit by the chain
            {
                //new up a boi
                GameObject newNode = GetNode();
                newNode.GetComponent<LightningChainNode>()._myEnemy = enemies[i];
                newNode.transform.position = enemies[i].GetComponent<Rigidbody2D>().position;
                newNode.GetComponent<LightningChainNode>()._sourcePosition = _myEnemy.transform.position + _myEnemy.GetBottomHeight() * Vector3.up;
                newNode.GetComponent<LightningChainNode>().Run();
            }
        }
        

        //return to pool

        if (_pooledNodes.Count == totalNumberOfObjects - 1)
        {
            _alreadyHit.Clear();
        }
        numUpdatesSinceRun = 0;
        ReturnNode(gameObject);
    }

    //=========================================| OBJECT POOLING METHODS

    /// <summary>
    /// Get an object from the pool, returned object is active.
    /// </summary>
    /// <returns></returns>
    public static GameObject GetNode()
    {
        GameObject returnThis;
        if (_pooledNodes.Count == 0)
        {
            returnThis = Instantiate(Resources.Load<GameObject>(PREFAB_PATH));
            totalNumberOfObjects++;
            returnThis.SetActive(true);
            return returnThis;
        }
        else
        {
            returnThis = _pooledNodes[_pooledNodes.Count - 1];
            _pooledNodes.RemoveAt(_pooledNodes.Count - 1);
            returnThis.SetActive(true);

            return returnThis;
        }

    }

    /// <summary>
    /// Add a node object to the inactive pool
    /// </summary>
    /// <param name="node"></param>
    private void ReturnNode(GameObject node)
    {
        node.SetActive(false);
        if (!_pooledNodes.Contains(node))
        {
            _pooledNodes.Add(node);
        }
    }
}
