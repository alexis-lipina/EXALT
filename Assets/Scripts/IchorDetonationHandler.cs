using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IchorDetonationHandler : ProjectionHandler
{
    private const float DETONATION_DURATION = 0.6f;

    [SerializeField] private SpriteRenderer AnimationSprite;
    [SerializeField] private SpriteRenderer RadialGlowSprite;
    [SerializeField] private SpriteRenderer StarGlowSprite;
    [SerializeField] private AnimationCurve StarGlowCurve;
    [SerializeField] private AnimationCurve RadialGlowCurve;

    //global/static stuff
    private static List<GameObject> _objectPool;
    protected override ElementType Element
    {
        get { return ElementType.ICHOR; }
    }

    public static void InitializePool()
    {
        _objectPool = new List<GameObject>();
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/IchorDetonation", typeof(GameObject)) as GameObject));

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
                _objectPool[i] = Instantiate(Resources.Load("Prefabs/Detonations/IchorDetonation", typeof(GameObject)) as GameObject);
                _objectPool[i].SetActive(false);
            }
            if (!_objectPool[i].activeSelf)
            {
                _objectPool[i].GetComponent<IchorDetonationHandler>().DesiredPosition = enemyPhysics.transform.position;
                _objectPool[i].GetComponent<IchorDetonationHandler>()._sourceEnemy = enemyPhysics;
                _objectPool[i].GetComponent<IchorDetonationHandler>()._projection.SetOpacity(1.0f);
                _objectPool[i].SetActive(true);
                return _objectPool[i];
            }
        }
        //if no inactive objects exist, make a new one and deploy it
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/IchorDetonation", typeof(GameObject)) as GameObject));
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
        AnimationSprite.transform.position = new Vector3(pos.x, pos.y + _physics.GetSpriteZOffset(), pos.y);
        RadialGlowSprite.transform.position = new Vector3(pos.x, pos.y + _physics.GetSpriteZOffset(), pos.y);
        StarGlowSprite.transform.position = new Vector3(pos.x, pos.y + _physics.GetSpriteZOffset(), pos.y);
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
                    collider.GetComponent<EntityPhysics>().IchorCorrupt(3);
                }
                else collider.GetComponent<EntityPhysics>().IchorCorrupt(3);
            }
        }
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.6f, 0.15f);
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        GetComponentInChildren<SpriteRenderer>().enabled = true;
        AnimationSprite.enabled = true;
        RadialGlowSprite.enabled = true;
        StarGlowSprite.enabled = true;
        AnimationSprite.GetComponent<Animator>().Play("IchorDetonation", 0, 0);
        RadialGlowSprite.GetComponent<Animator>().Play("Ichor_Radial", 0, 0);
        StarGlowSprite.GetComponent<Animator>().Play("Ichor_Star", 0, 0);
        //_projection.SetColor(Color.black);
        //yield return new WaitForSeconds(0.02f);
        //GetComponentInChildren<SpriteRenderer>().enabled = false;
        //_projection.SetColor(Color.white);
        //yield return new WaitForSeconds(DETONATION_DURATION - 0.02f);

        float timer = DETONATION_DURATION;
        while (timer > 0)
        {
            RadialGlowSprite.material.SetFloat("_Opacity", RadialGlowCurve.Evaluate(1 - (timer / DETONATION_DURATION)));
            StarGlowSprite.material.SetFloat("_Opacity", StarGlowCurve.Evaluate(1 - (timer / DETONATION_DURATION)));
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        AnimationSprite.enabled = false;
        RadialGlowSprite.enabled = false;
        StarGlowSprite.enabled = false;
        /*
        float opacity = 1f;
        while (opacity > 0)
        {
            _projection.SetOpacity(opacity);
            opacity -= 0.2f;
            yield return new WaitForSeconds(0.01f);
        }*/
        GetComponentInChildren<Animator>().Play("IchorDetonationIdle");

        gameObject.SetActive(false);
    }
}
