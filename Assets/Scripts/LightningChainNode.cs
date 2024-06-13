using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChainNode : MonoBehaviour
{
    public EntityPhysics _myEnemy;

    private const float _chainRadius = 5f;
    private const float _timeToArc = 0.25f;
    private const float _heightOfPlume = 10.0f;
    public Vector3 _sourcePosition; //where this node's source is 
    public ChainZapWavefront _wavefront = null; //the wavefront this node belongs to
    public float randomTimeMultiplier = 1.0f; //randomize the time till next chain occurs

    //object pooling
    private const string PREFAB_PATH = "Prefabs/Bullets/ZapComboNode";
    private static List<GameObject> _pooledNodes = new List<GameObject>();
    private int numUpdatesSinceRun = 0;


    //VFX
    [SerializeField] private ZapFXController _plumeFX;
    [SerializeField] private ZapFXController _arcFX;



    public void Run()
    {
        numUpdatesSinceRun = 0;
        gameObject.SetActive(true);
        if (_pooledNodes == null) _pooledNodes = new List<GameObject>();

        
        if (!_myEnemy) return; // enemy may have died in the interim

        //deal damage to enemy
        _myEnemy.Inflict(1, type:ElementType.ZAP, hitPauseDuration:0f);

        //start to run VFX Plume and Projection
        StartCoroutine(PlayVFX());
        //add myenemy to _alreadyhit

        _wavefront.AlreadyHit.Add(_myEnemy.gameObject.GetInstanceID());
    }

    private void Update()
    {
        if (numUpdatesSinceRun == 2) StartCoroutine(Spread());
        else numUpdatesSinceRun++; // TODO : I'm extremely sus of this but I dont wanna deal with it rn
    }

    IEnumerator PlayVFX()
    {
        
        _arcFX.SetupLine(Vector2.up * _myEnemy.GetBottomHeight(), _sourcePosition - _myEnemy.transform.position);
        _arcFX.Play(_timeToArc * 0.75f * randomTimeMultiplier);
        yield return new WaitForSeconds(_timeToArc * 0.5f * randomTimeMultiplier);
        _plumeFX.SetupLine(Vector2.up * _myEnemy.GetBottomHeight(), Vector2.up * (_myEnemy.GetBottomHeight() + _heightOfPlume));
        _plumeFX.Play(_timeToArc * 0.5f * randomTimeMultiplier);

    }

    IEnumerator Spread()
    {
        yield return new WaitForSeconds(_timeToArc * randomTimeMultiplier);
        yield return new WaitForEndOfFrame();
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
                if (nearbyEnemies[i].gameObject.tag == "Enemy" && !nearbyEnemies[i].GetComponent<EntityPhysics>().IsImmune)
                {
                    enemies.Add(nearbyEnemies[i].GetComponent<EntityPhysics>());
                    Debug.Log(nearbyEnemies[i].gameObject);
                }
            }
        }


        for (int i = 0; i < enemies.Count; i++)
        {
            if (!_wavefront.AlreadyHit.Contains(enemies[i].gameObject.GetInstanceID())) //if hasnt already been hit by the chain
            {
                //new up a boi
                GameObject newNode = GetNode();
                newNode.GetComponent<LightningChainNode>()._wavefront = _wavefront;
                newNode.GetComponent<LightningChainNode>().randomTimeMultiplier = Random.Range(0.75f, 1.25f);
                newNode.GetComponent<LightningChainNode>()._myEnemy = enemies[i];
                newNode.transform.position = new Vector3(enemies[i].GetComponent<Rigidbody2D>().position.x, enemies[i].GetComponent<Rigidbody2D>().position.y, enemies[i].GetComponent<Rigidbody2D>().position.y);
                newNode.GetComponent<LightningChainNode>()._sourcePosition = transform.position;
                //newNode.GetComponent<LightningChainNode>()._sourcePosition = _myEnemy.transform.position + _myEnemy.GetBottomHeight() * Vector3.up;
                newNode.GetComponent<LightningChainNode>().Run();
            }
        }
        

        //return to pool
        yield return new WaitForSeconds(1.0f); // TODO : This just lets sfx play out
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
        if (_pooledNodes == null) _pooledNodes = new List<GameObject>();
        foreach (GameObject obj in _pooledNodes)
        {
            if (!obj.activeSelf) //node is inactive and ready to be deployed
            {
                obj.SetActive(true);
                return obj;
            }
        }

        //no inactive nodes = no available members of pool, make a new one

        _pooledNodes.Add(Instantiate(Resources.Load<GameObject>(PREFAB_PATH)));
        _pooledNodes[_pooledNodes.Count-1].SetActive(true);
        return _pooledNodes[_pooledNodes.Count - 1];
    }

    /// <summary>
    /// Add a node object to the inactive pool
    /// </summary>
    /// <param name="node"></param>
    private void ReturnNode(GameObject node)
    {
        node.SetActive(false);
    }

    private void OnDestroy() //this should only happen on scene unload
    {
        _pooledNodes = new List<GameObject>();
    }
}
