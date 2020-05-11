using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDetonationHandler : ProjectionHandler
{
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
        GetComponentInChildren<SpriteRenderer>().transform.position = new Vector3(pos.x, pos.y, pos.y);
    }

    private void OnEnable()
    {
        hasDetonated = false;
        Debug.Log("Deployed!");
        MoveTo(DesiredPosition);
        //_physics.SetObjectElevation(_sourceEnemy.GetObjectElevation());
        _projection.SetOpacity(1.0f);

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
            
        }
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
        Collider2D[] collidersHit = Physics2D.OverlapBoxAll(_damageVolume.bounds.center, new Vector2(4f, 3f), 0.0f);
        Debug.DrawLine(_damageVolume.bounds.center + new Vector3(4f, 3f, 0f), _damageVolume.bounds.center + new Vector3(-4f, -3f, 0f), Color.cyan, 1f, false);
        foreach (Collider2D collider in collidersHit)
        {
            if (collider.gameObject.tag == "Enemy")
            {
                if (collider.GetComponent<EntityPhysics>().GetInstanceID() == _sourceEnemy.GetInstanceID())
                {
                    collider.GetComponent<EntityPhysics>().Inflict(1, type:Element);
                }
                else collider.GetComponent<EntityPhysics>().Inflict(1, type:Element);
            }
        }
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.6f, 0.15f);
        StartCoroutine(PlayAnimation());

        //spawn projectiles

        for (int i = 0; i < _sparks.Count; i++)
        {
            _sparks[i].SetActive(true);
            _sparks[i].GetComponentInChildren<BulletHandler>().MoveDirection = Random.insideUnitCircle;
            _sparks[i].GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(_physics.GetObjectElevation() + 2.5f);
            //_sparks[i].GetComponentInChildren<ProjectilePhysics>().SetObjectElevation(10f);
            _sparks[i].GetComponentInChildren<ProjectilePhysics>().GetComponent<Rigidbody2D>().position = (_physics.GetComponent<Rigidbody2D>().position);
            Debug.Log("SPARK HEIGHT : " + _sparks[i].GetComponentInChildren<ProjectilePhysics>().GetObjectElevation());
            //_sparks[i].GetComponentInChildren<ProjectilePhysics>().ZVelocity = 100f;
        }
    }

    IEnumerator PlayAnimation()
    {
        GetComponentInChildren<SpriteRenderer>().enabled = true;
        _projection.SetColor(Color.black);
        yield return new WaitForSeconds(0.02f);
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        _projection.SetColor(Color.white);
        yield return new WaitForSeconds(0.02f);
        

        float opacity = 1f;
        while (opacity > 0)
        {
            _projection.SetOpacity(opacity);
            opacity -= 0.2f;
            yield return new WaitForSeconds(0.01f);
        }
        
        gameObject.SetActive(false);
    }
}
