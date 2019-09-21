using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashDeflectVFX : MonoBehaviour
{
    private static List<GameObject> _objectPool;

    public static void InitializePool()
    {
        _objectPool = new List<GameObject>();
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/VFX/SlashDeflect", typeof(GameObject)) as GameObject));

        foreach (GameObject o in _objectPool)
        {
            o.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos">position of bullet?</param>
    /// <param name="dir">direction vector along bullet path</param>
    /// <returns></returns>
    public static GameObject DeployFromPool(Vector3 pos, Vector2 dir)
    {
        if (_objectPool == null) InitializePool();
        for (int i = 0; i < _objectPool.Count; i++)
        {
            if (_objectPool[i] == null) //should take care of reloading scenes - its hacky, should probably do on scene load or something
            {
                _objectPool[i] = Instantiate(Resources.Load("Prefabs/VFX/SlashDeflect", typeof(GameObject)) as GameObject);
                _objectPool[i].SetActive(false);
            }
            if (!_objectPool[i].activeSelf)
            {
                _objectPool[i].SetActive(true);
                _objectPool[i].transform.position = pos;
                _objectPool[i].transform.up = dir;
                return _objectPool[i];
            }
        }
        //if no inactive objects exist, make a new one and deploy it
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/VFX/SlashDeflect", typeof(GameObject)) as GameObject));
        _objectPool[_objectPool.Count - 1].SetActive(false);
        var deployedObj = DeployFromPool(pos, dir);
        deployedObj.transform.position = pos;
        deployedObj.transform.up = dir;
        return deployedObj;
    }

    private void OnEnable()
    {
        GetComponent<Animator>().Play("SlashDeflect");
        StartCoroutine(PlayAnim());
    }

    IEnumerator PlayAnim()
    {
        yield return new WaitForSeconds(.2f);
        gameObject.SetActive(false);
    }
}
