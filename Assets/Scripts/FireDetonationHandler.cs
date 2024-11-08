﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDetonationHandler : ProjectionHandler
{
    [SerializeField] private SpriteRenderer AnimationSprite;
    [SerializeField] private SpriteRenderer RadialGlowSprite;
    [SerializeField] private SpriteRenderer StarGlowSprite;
    [SerializeField] private AnimationCurve StarGlowCurve;
    [SerializeField] private AnimationCurve RadialGlowCurve;


    private const float DETONATION_DURATION = 2.0f;

    //global/static stuff
    private static List<GameObject> _objectPool;
    protected override ElementType Element
    {
        get { return ElementType.FIRE; }
    }



    public static void InitializePool()
    {
        _objectPool = new List<GameObject>();
        

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
                _objectPool[i] = Instantiate(Resources.Load("Prefabs/Detonations/FireDetonation", typeof(GameObject)) as GameObject);
                _objectPool[i].SetActive(false);
            }
            if (!_objectPool[i].activeSelf)
            {
                //_objectPool[i].GetComponentInChildren<FireDetonationHandler>().MoveTo(position);
                _objectPool[i].GetComponent<FireDetonationHandler>().DesiredPosition = enemyPhysics.transform.position;
                _objectPool[i].GetComponent<FireDetonationHandler>()._sourceEnemy = enemyPhysics;
                _objectPool[i].GetComponent<FireDetonationHandler>()._projection.SetOpacity(1.0f);
                _objectPool[i].SetActive(true);
                //_objectPool[i].transform.position = position;
                return _objectPool[i];
            }
        }
        //if no inactive objects exist, make a new one and deploy it
        _objectPool.Add(Instantiate(Resources.Load("Prefabs/Detonations/FireDetonation", typeof(GameObject)) as GameObject));
        _objectPool[_objectPool.Count - 1].SetActive(false);
        var deployedObj = DeployFromPool(enemyPhysics);
        return deployedObj;
    }


    //instance stuff

    [SerializeField] private PlayerProjection _projection;
    [SerializeField] private DynamicPhysics _physics;
    [SerializeField] private BoxCollider2D _damageVolume;
    private bool hasDetonated = false;
    public Vector2 DesiredPosition { get; set; }

    private List<GameObject> _sparks;

    public EntityPhysics _sourceEnemy;


    public void MoveTo(Vector2 pos)
    {
        _physics.transform.position = pos;
        AnimationSprite.transform.position = new Vector3(pos.x, pos.y + 2 + _sourceEnemy.GetObjectElevation(), pos.y - 10);
        RadialGlowSprite.transform.position = new Vector3(pos.x, pos.y + 2 + _sourceEnemy.GetObjectElevation(), pos.y - 10);
        StarGlowSprite.transform.position = new Vector3(pos.x, pos.y + 2 + _sourceEnemy.GetObjectElevation(), pos.y - 10);
    }

    private void OnEnable()
    {
        hasDetonated = false;
        Debug.Log("Deployed!");
        _projection.SetOpacity(1.0f);
        
        // I'm canceling sparks, sparks are canceled
        /*
        if (_sparks == null)
        {
            _sparks = new List<GameObject>();
            GameObject tempBullet;
            for (int i = 0; i < 6; i++)
            {
                tempBullet = Instantiate(Resources.Load("Prefabs/Bullets/Spark")) as GameObject;
                //tempBullet.GetComponentInChildren<Rigidbody2D>().position = new Vector2(1000, 1000);
                tempBullet.SetActive(false);
                _sparks.Add(tempBullet);
            }
            
        }*/
    }
    

    protected void Update()
    {
        if (!_sourceEnemy)
        {
            gameObject.SetActive(false);
        }
        else if (!hasDetonated)
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
        Collider2D[] collidersHit = Physics2D.OverlapBoxAll(_damageVolume.bounds.center, new Vector2(16f, 12f), 0.0f);
        Debug.DrawLine(_damageVolume.bounds.center + new Vector3(4f, 3f, 0f), _damageVolume.bounds.center + new Vector3(-4f, -3f, 0f), Color.cyan, 1f, false);
        foreach (Collider2D collider in collidersHit)
        {
            if (collider.gameObject.tag == "Enemy")
            {
                if (!collider.GetComponent<EntityPhysics>().IsImmune)
                {
                    if (collider.GetComponent<EntityPhysics>().GetInstanceID() == _sourceEnemy.GetInstanceID())
                    {
                        collider.GetComponent<EntityPhysics>().Inflict(2, type: Element);
                        collider.GetComponent<EntityPhysics>().Stagger();
                        collider.GetComponent<EntityPhysics>().Burn();
                    }
                    else
                    {
                        collider.GetComponent<EntityPhysics>().Inflict(1, type: Element);
                        collider.GetComponent<EntityPhysics>().Stagger();
                        collider.GetComponent<EntityPhysics>().Burn();
                    }
                }
                
            }
        }
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.6f, 0.15f);
        StartCoroutine(PlayAnimation());

        //spawn projectiles

        // not doin sparks
        /*
        for (int i = 0; i < _sparks.Count; i++)
        {
            _sparks[i].SetActive(true);
            _sparks[i].GetComponentInChildren<BulletHandler>().MoveDirection = Random.insideUnitCircle;
            _sparks[i].GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(_physics.GetObjectElevation() + 2.5f);
            //_sparks[i].GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(10f);
            _sparks[i].GetComponentInChildren<ProjectilePhysics>().GetComponent<Rigidbody2D>().position = (_physics.GetComponent<Rigidbody2D>().position);
            Debug.Log("SPARK HEIGHT : " + _sparks[i].GetComponentInChildren<ProjectilePhysics>().GetObjectElevation());
            //_sparks[i].GetComponentInChildren<ProjectilePhysics>().ZVelocity = 100f;
        }*/
    }

    IEnumerator PlayAnimation()
    {
        AnimationSprite.enabled = true;
        RadialGlowSprite.enabled = true;
        StarGlowSprite.enabled = true;
        GetComponentInChildren<Animator>().Play("FireDetonation", 0, 0);
        RadialGlowSprite.GetComponent<Animator>().Play("Sol_Ball", 0, 0); // controls size
        StarGlowSprite.GetComponent<Animator>().Play("Sol_Star", 0, 0);
        //_projection.SetColor(Color.black);
        //yield return new WaitForSeconds(0.02f);
        //_projection.SetColor(Color.white);
        _projection.SetOpacity(0.0f);
        float timer = DETONATION_DURATION;
        while (timer > 0)
        {
            float radialglow = RadialGlowCurve.Evaluate(1 - (timer / DETONATION_DURATION));
            float starglow = StarGlowCurve.Evaluate(1 - (timer / DETONATION_DURATION));
            RadialGlowSprite.material.SetFloat("_Opacity", RadialGlowCurve.Evaluate(1 - (timer / DETONATION_DURATION)));
            StarGlowSprite.material.SetFloat("_Opacity", StarGlowCurve.Evaluate(1 - (timer / DETONATION_DURATION)));
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //yield return new WaitForSeconds(DETONATION_DURATION);
        AnimationSprite.enabled = false;
        RadialGlowSprite.enabled = false;
        StarGlowSprite.enabled = false;

        GetComponentInChildren<Animator>().Play("FireDetonation_Idle");

        gameObject.SetActive(false);
    }
}
