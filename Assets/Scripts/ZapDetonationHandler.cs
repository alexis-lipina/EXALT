using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZapDetonationHandler : ProjectionHandler
{
    private const float DETONATION_DURATION = 0.3f;

    //global/static stuff
    private static List<GameObject> _objectPool;
    protected override ElementType Element
    {
        get { return ElementType.ZAP; }
    }

    public static void InitializePool()
    {
        _objectPool = new List<GameObject>();
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/ZapDetonation", typeof(GameObject)) as GameObject));

        foreach (GameObject o in _objectPool)
        {
            o.SetActive(false);
        }
    }


    public static GameObject DeployFromPool(EntityPhysics enemyPhysics)
    {
        if (_objectPool == null) InitializePool();
        for (int i = 0; i < _objectPool.Count; i++)
        {
            if (_objectPool[i] == null)
            {
                _objectPool[i] = Instantiate(Resources.Load("Prefabs/Detonations/ZapDetonation", typeof(GameObject)) as GameObject);
                _objectPool[i].SetActive(false);
            }
            if (!_objectPool[i].activeSelf)
            {
                _objectPool[i].GetComponent<ZapDetonationHandler>().DesiredPosition = enemyPhysics.transform.position;
                _objectPool[i].GetComponent<ZapDetonationHandler>()._sourceEnemy = enemyPhysics;
                _objectPool[i].GetComponent<ZapDetonationHandler>()._projection.SetOpacity(1.0f);
                _objectPool[i].SetActive(true);
                return _objectPool[i];
            }
        }
        //if no inactive objects exist, make a new one and deploy it
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/ZapDetonation", typeof(GameObject)) as GameObject));
        _objectPool[_objectPool.Count - 1].SetActive(false);
        var deployedObject = DeployFromPool(enemyPhysics);
        return deployedObject;
    }


    //instance stuff

    [SerializeField] private PlayerProjection _projection;
    [SerializeField] private DynamicPhysics _physics;
    [SerializeField] private BoxCollider2D _damageVolume;
    public Vector2 DesiredPosition { get; set; }
    public EntityPhysics _sourceEnemy;
    private bool hasDetonated = false;

    public void MoveTo(Vector2 pos)
    {
        _physics.transform.position = pos;
        GetComponentInChildren<SpriteRenderer>().transform.position = new Vector3(pos.x, pos.y, pos.y);
    }

    private void OnEnable()
    {
        hasDetonated = false;
        Debug.Log("Deployed!");
        MoveTo(DesiredPosition);
        _projection.SetOpacity(1.0f);
    }

    protected void Update()
    {
        if (!hasDetonated)
        {
            _projection.SetOpacity(1f);
            MoveTo(_sourceEnemy.transform.position);
            _physics.SetObjectElevation(_sourceEnemy.GetObjectElevation());
        }

    }

    /// <summary>
    /// Activate
    /// </summary>
    public void Detonate()
    {
        GetComponent<AudioSource>().Play();
        hasDetonated = true;
        Collider2D[] collidersHit = Physics2D.OverlapBoxAll(_damageVolume.bounds.center, _damageVolume.bounds.size * 0.5f, 0.0f);
        foreach (Collider2D collider in collidersHit)
        {
            if (collider.gameObject.tag == "Enemy")
            {
                if (collider.GetComponent<EntityPhysics>().GetInstanceID() == _sourceEnemy.GetInstanceID())
                {
                    collider.GetComponent<EntityPhysics>().Inflict(5, type:Element);
                }
                else collider.GetComponent<EntityPhysics>().Inflict(5, type:Element);
            }
        }
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.6f, 0.15f);
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        GetComponentInChildren<SpriteRenderer>().enabled = true;
        GetComponentInChildren<Animator>().Play("ZapDetonation_V2");
        _projection.SetColor(Color.black);
        yield return new WaitForSeconds(0.02f);
        //GetComponentInChildren<SpriteRenderer>().enabled = false;
        _projection.SetColor(Color.white);
        yield return new WaitForSeconds(DETONATION_DURATION - 0.02f);
        GetComponentInChildren<SpriteRenderer>().enabled = false;

        /*
        float opacity = 1f;
        while (opacity > 0)
        {
            _projection.SetOpacity(opacity);
            opacity -= 0.2f;
            yield return new WaitForSeconds(0.01f);
        }*/
        GetComponentInChildren<Animator>().Play("ZapDetonationIdle");

        gameObject.SetActive(false);
    }
}
