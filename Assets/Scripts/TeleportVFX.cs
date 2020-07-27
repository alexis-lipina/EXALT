using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportVFX : MonoBehaviour
{
    private static List<GameObject> _objectPool;


    public static void InitializePool()
    {
        _objectPool = new List<GameObject>();
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Player/Effects/TeleportEffect", typeof(GameObject)) as GameObject));
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Player/Effects/TeleportEffect", typeof(GameObject)) as GameObject));
        
        foreach (GameObject o in _objectPool)
        {
            o.SetActive(false);
        }
    }


    public static void DeployEffectFromPool(Vector3 position)
    {
        if (_objectPool == null) InitializePool();
        for (int i = 0; i < _objectPool.Count; i++)
        {
            if (!_objectPool[i].activeSelf)
            {
                _objectPool[i].SetActive(true);
                _objectPool[i].transform.position = position;
                return;
            }
        }
        //if no inactive objects exist, make a new one and deploy it
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Player/Effects/TeleportEffect", typeof(GameObject)) as GameObject));
        DeployEffectFromPool(position);
        return;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable()
    {
        StartCoroutine("PlayAnimation");
    }

    IEnumerator PlayAnimation()
    {
        for (float f = 1f; f >= 0; f -= 0.1f)
        {
            GetComponent<SpriteRenderer>().material.SetFloat("_Transparency", f);
            yield return new WaitForSeconds(0.01f);
        }
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _objectPool = null;
    }
}
