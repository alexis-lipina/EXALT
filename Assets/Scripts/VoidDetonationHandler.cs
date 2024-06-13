using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidDetonationHandler : ProjectionHandler
{
    //private const float DETONATION_DURATION = 0.1875f;
    private const float DETONATION_DURATION = 0.25f;

    [SerializeField] private SpriteRenderer AnimationSprite;
    [SerializeField] private SpriteRenderer SuckGlowSprite;
    [SerializeField] private AnimationCurve SuckGlowCurve;

    //global/static stuff
    private static List<GameObject> _objectPool;
    protected override ElementType Element
    {
        get { return ElementType.VOID; }
    }

    public static void InitializePool()
    {
        _objectPool = new List<GameObject>();
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/VoidDetonation", typeof(GameObject)) as GameObject));

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
            if (_objectPool[i] == null) //should take care of reloading scenes - its hacky, should probably do on scene load or something
            {
                _objectPool[i] = Instantiate(Resources.Load("Prefabs/Detonations/VoidDetonation", typeof(GameObject)) as GameObject);
                _objectPool[i].SetActive(false);
            }
            if (!_objectPool[i].activeSelf)
            {
                _objectPool[i].GetComponent<VoidDetonationHandler>().DesiredPosition = enemyPhysics.transform.position;
                _objectPool[i].GetComponent<VoidDetonationHandler>()._sourceEnemy = enemyPhysics;
                _objectPool[i].GetComponent<VoidDetonationHandler>()._projection.SetOpacity(1.0f);
                _objectPool[i].SetActive(true);
                return _objectPool[i];
            }
        }
        //if no inactive objects exist, make a new one and deploy it
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/VoidDetonation", typeof(GameObject)) as GameObject));
        _objectPool[_objectPool.Count - 1].SetActive(false);
        var deployedObj = DeployFromPool(enemyPhysics);
        return deployedObj;
    }


    //instance stuff

    [SerializeField] private PlayerProjection _projection;
    [SerializeField] private DynamicPhysics _physics;
    [SerializeField] private BoxCollider2D _damageVolume;
    [SerializeField] private Transform _pullVFXPrefab;

    private bool hasDetonated = false;
    public Vector2 DesiredPosition { get; set; }


    public EntityPhysics _sourceEnemy;


    public void MoveTo(Vector2 pos)
    {
        _physics.transform.position = pos;
        AnimationSprite.transform.position = new Vector3(pos.x, pos.y + 2, pos.y - 10);
        SuckGlowSprite.transform.position = new Vector3(pos.x, pos.y + 2, pos.y - 10);
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
        Collider2D[] collidersHit = Physics2D.OverlapBoxAll(_damageVolume.bounds.center, new Vector2(32f, 24f), 0.0f);
        foreach (Collider2D collider in collidersHit)
        {
            if (collider.gameObject.tag == "Enemy")
            {
                if (!collider.GetComponent<EntityPhysics>().IsImmune)
                {
                    if (collider.GetComponent<EntityPhysics>().GetInstanceID() == _sourceEnemy.GetInstanceID())
                    {
                        collider.GetComponent<EntityPhysics>().Inflict(1, type: Element);
                    }
                    else
                    {
                        Vector2 enemyToCenter = _physics.transform.position - collider.transform.position;
                        collider.GetComponent<EntityPhysics>().Inflict(1, force: enemyToCenter.normalized * 2f, type: ElementType.VOID);

                        Transform pullVFX = Instantiate(_pullVFXPrefab);
                        pullVFX.position = collider.GetComponent<EntityPhysics>().ObjectSprite.transform.position;
                        pullVFX.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, enemyToCenter)));
                    }
                }
            }
        }
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.6f, 0.15f);
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        AnimationSprite.enabled = true;
        SuckGlowSprite.enabled = true;
        AnimationSprite.GetComponent<Animator>().Play("VoidDetonation", 0, 0);
        SuckGlowSprite.GetComponent<Animator>().Play("Void_Suck", 0, 0);

        GetComponentInChildren<Animator>().transform.position += new Vector3(0, 0, 9);
        //_projection.SetColor(Color.black);
        //yield return new WaitForSeconds(0.02f);
        //_projection.SetColor(Color.white);
        //yield return new WaitForSeconds(DETONATION_DURATION - 0.02f);
        float timer = DETONATION_DURATION;
        while (timer > 0)
        {
            SuckGlowSprite.material.SetFloat("_Opacity", SuckGlowCurve.Evaluate(1 - (timer / DETONATION_DURATION)));
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }


        AnimationSprite.enabled = false;
        SuckGlowSprite.enabled = false;
        GetComponentInChildren<Animator>().Play("VoidDetonationIdle");

        gameObject.SetActive(false);
    }
}
