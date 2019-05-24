using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZapDetonationHandler : ProjectionHandler
{
    //global/static stuff
    private static List<GameObject> _objectPool;

    public static void InitializePool()
    {
        _objectPool = new List<GameObject>();
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/ZapDetonation", typeof(GameObject)) as GameObject));

        foreach (GameObject o in _objectPool)
        {
            o.SetActive(false);
        }
    }


    public static void DeployFromPool(EntityPhysics enemyPhysics)
    {
        if (_objectPool == null) InitializePool();
        for (int i = 0; i < _objectPool.Count; i++)
        {
            if (!_objectPool[i].activeSelf)
            {
                _objectPool[i].GetComponent<ZapDetonationHandler>().DesiredPosition = enemyPhysics.transform.position;
                _objectPool[i].GetComponent<ZapDetonationHandler>()._sourceEnemy = enemyPhysics;
                _objectPool[i].SetActive(true);
                return;
            }
        }
        //if no inactive objects exist, make a new one and deploy it
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/ZapDetonation", typeof(GameObject)) as GameObject));
        _objectPool[_objectPool.Count - 1].SetActive(false);
        DeployFromPool(enemyPhysics);
        return;
    }


    //instance stuff

    [SerializeField] private PlayerProjection _projection;
    [SerializeField] private DynamicPhysics _physics;
    [SerializeField] private BoxCollider2D _damageVolume;
    public Vector2 DesiredPosition { get; set; }
    public EntityPhysics _sourceEnemy;

    public void MoveTo(Vector2 pos)
    {
        _physics.transform.position = pos;
    }

    private void OnEnable()
    {
        Debug.Log("Deployed!");
        MoveTo(DesiredPosition);
        Collider2D[] collidersHit = Physics2D.OverlapBoxAll(_damageVolume.bounds.center, _damageVolume.bounds.size, 0.0f);
        
        foreach (Collider2D collider in collidersHit)
        {
            if (collider.gameObject.tag == "Enemy")
            {
                if (collider.GetComponent<EntityPhysics>().GetInstanceID() == _sourceEnemy.GetInstanceID())
                {
                    collider.GetComponent<EntityPhysics>().Inflict(1f, ElementType.ZAP);
                }
                else collider.GetComponent<EntityPhysics>().Inflict(1f, ElementType.ZAP);
            }
        }
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        _projection.SetOpacity(0f);
        yield return null;
        _projection.SetOpacity(1f);
        yield return new WaitForSeconds(0.02f);
        _projection.SetOpacity(0f);
        yield return new WaitForSeconds(0.04f);

        float opacity = 1f;
        while (opacity > 0)
        {
            _projection.SetOpacity(opacity);
            opacity -= 0.1f;
            yield return new WaitForSeconds(0.01f);
        }

        gameObject.SetActive(false);
    }
}
