using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// This class handles projectile interaction with the environment.
/// 
/// A projectile, as used in this project, is generally anything that can deal damage to an entity and that moves around the scene without much intelligence.
/// This class is designed to streamline development so changing bullet behaviours is easy.
/// </summary>
public class ProjectilePhysics : DynamicPhysics
{
    
    [SerializeField] private BulletHandler bulletHandler;
    [Space(10)]
    [Header("Projectile Behaviours")]
    [SerializeField] private bool canBounce;
    [SerializeField] private bool isAffectedByGravity;
    [SerializeField] private bool canPenetrate;
    [SerializeField] private bool canBeDamaged;
    [SerializeField] public bool canBeDeflected = true;
    [SerializeField] private bool explodesOnDeath;
    [SerializeField] private bool doesTracking;
    [SerializeField] public bool isSpeedScaledByProximity = false;
    [SerializeField] private bool doesSpriteFaceMoveDirection;
    [SerializeField] private Collider2D trackingArea;
    private ProjectileSeeker trackingAreaSeeker;
    [SerializeField] private float trackingAddedSpeed = 5.0f;
    [SerializeField] public int _damageAmount = 1;
    [SerializeField] private float _impactForce = 0.5f;
    [SerializeField] private ElementType _damageType = ElementType.NONE;
    [SerializeField] private float _zVelocityDamping = 1f;
    // minimum vertical velocity to give projectile on a bounce
    [SerializeField] private float _zMinimumVelocity = 20f;
    [SerializeField] private AudioClip _deflectSFX;
    [SerializeField] private Animator ProjectileAnimator;
    [SerializeField] private string Animator_SpawnStateName;
    [SerializeField] private string Animator_IdleStateName;
    [SerializeField] private string Animator_DespawnStateName;
    [SerializeField] private float Animator_DespawnDuration = 0.0f;
    [SerializeField] private bool _isPooled = true;
    [Space(10)]
    [SerializeField] public SpriteRenderer GlowSprite; // all projectiles should have an emissive glow sprite
    [SerializeField] private AnimationCurve GlowDespawnAlphaAnimationCurve;
    [SerializeField] private AnimationCurve GlowDespawnScaleAnimationCurve;
    [SerializeField] private AudioClip HitSFX;
    private Vector3 DefaultGlowScale;


    private EntityPhysics _trackedTarget;
    private bool IsCurrentlyTracking
    {
        get
        {
            return _trackedTarget;
        }
    }
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public bool CanBounce
    {
        get { return canBounce; }
    }
    public bool IsAffectedByGravity
    {
        get { return isAffectedByGravity; }
    }
    public bool CanPenetrate
    {
        get { return canPenetrate; }
    }
    public bool CanBeDamaged
    {
        get { return canBeDamaged; }
    }

    public ElementType GetElement()
    {
        return _damageType; 
    }

    public string GetWhoToHurt()
    {
        return _whoToHurt;
    }

    /// <summary>
    /// "ALL" - Damages all entity types
    /// "FRIEND" - damages friends
    /// "ENEMY"  - damages enemies
    /// </summary>
    [SerializeField] private string _whoToHurt; //who to damage

    private Rigidbody2D bulletRigidBody;
    private Vector2 _velocity; //purely direction
    [SerializeField] private float _timeToDestroy = 10f;
    private float _timer;
    private bool bIsDespawning = false;

    public Vector2 Velocity
    {
        set { _velocity = value.normalized; }
        get { return _velocity; }
    }
    
    List<PhysicsObject> EntitiesTouched;
    Dictionary<EntityPhysics, bool> _targetsTouched; // (target, hasBeenTouched)

    // Original FIELDS - for any field modified when bullet is deflected that needs to be reset when object is returned to pool
    private string _whoToHurt_original;
    private float _speed_original;
    private LevelManager levelManager;
    private List<EntityPhysics> AlreadyDamagedTargets;
    public bool HasHitEnemy
    {
        get
        {
            return AlreadyDamagedTargets.Count > 0;
        }
    }



    override protected void Awake()
    {
        base.Awake();
        EntitiesTouched = new List<PhysicsObject>();
        _targetsTouched = new Dictionary<EntityPhysics, bool>();
        AlreadyDamagedTargets = new List<EntityPhysics>();
        _timer = 0f;
        DefaultGlowScale = GlowSprite.transform.localScale;

        // Set original Fields
        _speed_original = speed;
        _whoToHurt_original = _whoToHurt;
        _trackedTarget = null;
        if (trackingArea)
        {
            trackingAreaSeeker = trackingArea.GetComponent<ProjectileSeeker>();
        }
        switch (_damageType)
        {
            case ElementType.ICHOR:
                GlowSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 1);
                break;
            case ElementType.FIRE:
                GlowSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 2);
                break;
            case ElementType.VOID:
                GlowSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 3);
                break;
            case ElementType.ZAP:
                GlowSprite.GetComponent<SpriteRenderer>().material.SetFloat("_CurrentElement", 4);
                break;
        }
    }



    void Update()
    {
        transform.position.Scale(new Vector3(1, 1, 0)); // should NEVER not be zero depth.
        _timer += Time.deltaTime;
        topHeight = bottomHeight + _objectHeight;

        //physics
        _velocity = Bounce(_velocity);
        if (doesSpriteFaceMoveDirection)
        {
            _objectSprite.transform.right = _velocity.normalized;
        }

        if (doesTracking)
        {
            //Debug.Log("MOVIN");
            trackingArea.transform.position = this.transform.position;
            _velocity = Seek(_velocity);
        }

        //MoveWithCollision(_velocity.x, _velocity.y);
        MoveCharacterPositionPhysics(_velocity.x, _velocity.y);
        //Debug.Log("Velocity : " + _velocity);
        if (isAffectedByGravity) FreeFall();
        //else _velocity *= 0.9f; // commenting this out on 8/4/2023 cuz it seems wrong... but I'm not sure if this'll break something, so....
        //Debug.Log("Elevation : " + bottomHeight);

        MoveCharacterPosition();
        if (bottomHeight < EntityPhysics.KILL_PLANE_ELEVATION || _timer > _timeToDestroy) 
        {
            //Debug.Log(bulletHandler);
            //Debug.Log(bulletHandler.SourceWeapon);
            //bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID()); //"deletes" if out of bounds
            //transform.parent.gameObject.SetActive(false);
            Despawn();
        }
    }


    //------------------------------------------| COLLISION DETECTION

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
        {
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
        if (other.gameObject.tag == "Enemy" && (_whoToHurt == "ENEMY" || _whoToHurt == "ALL"))
        {

            if (other.gameObject.GetComponent<EntityPhysics>().GetBottomHeight() < topHeight && other.gameObject.GetComponent<EntityPhysics>().GetTopHeight() > bottomHeight)//enemy hit
            {
                ApplyImpactEffect(other.gameObject.GetComponent<EntityPhysics>());
                if (!canPenetrate)
                {
                    Despawn();
                }
            }
            else
            {
                _targetsTouched.Add(other.gameObject.GetComponent<EntityPhysics>(), false);
            }
            
        }
        else if (other.gameObject.tag == "Friend" && (_whoToHurt == "FRIEND" || _whoToHurt == "ALL"))
        {
            if (other.gameObject.GetComponent<EntityPhysics>().GetBottomHeight() < topHeight && other.gameObject.GetComponent<EntityPhysics>().GetTopHeight() > bottomHeight)//enemy hit
            {
                ApplyImpactEffect(other.gameObject.GetComponent<EntityPhysics>());
                if (!canPenetrate)
                {
                    Despawn();
                }
            }
            else
            {
                _targetsTouched.Add(other.gameObject.GetComponent<EntityPhysics>(), false);
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
        {
            Debug.Log("This should never happen. ");
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
        if (other.gameObject.tag == "Enemy" && (_whoToHurt == "ENEMY" || _whoToHurt == "ALL"))
        {
            if (!_targetsTouched[other.gameObject.GetComponent<EntityPhysics>()] && other.gameObject.GetComponent<EntityPhysics>().GetBottomHeight() < topHeight && other.gameObject.GetComponent<EntityPhysics>().GetTopHeight() > bottomHeight) //if has not been hit and is overlapping
            {
                ApplyImpactEffect(other.gameObject.GetComponent<EntityPhysics>());
                if (!canPenetrate)
                {
                    Despawn();
                }
            }
        }
        else if (other.gameObject.tag == "Friend" && (_whoToHurt == "FRIEND" || _whoToHurt == "ALL"))
        {
            if (other.gameObject.GetComponent<EntityPhysics>().GetBottomHeight() < topHeight && other.gameObject.GetComponent<EntityPhysics>().GetTopHeight() > bottomHeight)//enemy hit
            {
                ApplyImpactEffect(other.gameObject.GetComponent<EntityPhysics>());
                if (!canPenetrate)
                {
                    Despawn();
                }
            }
            else
            {
                _targetsTouched.Add(other.gameObject.GetComponent<EntityPhysics>(), false);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Environment")
        {
            TerrainTouching.Remove(other.gameObject);
        }
        else if (other.gameObject.tag == "Enemy")
        {
            _targetsTouched.Remove(other.GetComponent<EntityPhysics>());
        }
    }

    void ApplyImpactEffect(EntityPhysics other)
    {
        if (AlreadyDamagedTargets.Contains(other))
        {
            return;
        }
        if (_damageAmount > 0)
        {
            other.Inflict(_damageAmount, force: Velocity.normalized * _impactForce, type: _damageType);
            if (_damageType == ElementType.FIRE) other.gameObject.GetComponent<EntityPhysics>().Burn();
            else if (_damageType == ElementType.ICHOR) other.gameObject.GetComponent<EntityPhysics>().IchorCorrupt(1);
            else if (_damageType == ElementType.VOID) other.Stagger(); // might want all to stagger? idk
            if (HitSFX)
            {
                GetComponent<AudioSource>().clip = HitSFX;
                GetComponent<AudioSource>().Play();
            }
        }
        else 
        {
            other.Heal(1);
        }
        AlreadyDamagedTargets.Add(other);
    }


    //==========================================|  SPECIAL PHYSICS

    /// <summary>
    /// Bounces the projectile off of walls
    /// </summary>
    /// <param name="currentvelocity">The velocity of the projectile as it stands right now.</param>
    /// <returns >The final velocity of the projectile after bouncing has been performed if necessary.</returns>
    public Vector2 Bounce(Vector2 currentvelocity)
    {
        //if (!IsCollidingWithEnvironment()) return currentvelocity;


        bool hasXHit = false;
        bool hasYHit = false;
        bool hasZHit = false;

        foreach (KeyValuePair<GameObject, KeyValuePair<float, float>> entry in TerrainTouching)
        {
            if (topHeight > entry.Value.Key && bottomHeight < entry.Value.Value) //if this top is above their bottom and if this bottom is below their top
            {
                if (!canBounce) //if unable to bounce, d e l e t
                {
                    Reset();
                    if (_isPooled)
                    {
                        if (trackingArea) trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
                        transform.position = new Vector3(-999, -999, transform.position.z);
                        ObjectSprite.transform.position = new Vector3(-999, -999, ObjectSprite.transform.position.z);
                        //transform.parent.position = new Vector3(-999, -999, transform.parent.position.z);
                        bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID());
                    }
                }


                float playerEnvtHandlerYPos = _environmentHandler.GetComponent<Transform>().position.y;
                float playerEnvtHandlerYSize = _environmentHandler.GetComponent<BoxCollider2D>().size.y;
                float obstacleYPos = entry.Key.GetComponent<Transform>().position.y;
                float obstacleYSize = entry.Key.GetComponent<BoxCollider2D>().size.y;
                float playerEnvtHandlerXPos = _environmentHandler.GetComponent<Transform>().position.x;
                float playerEnvtHandlerXSize = _environmentHandler.GetComponent<BoxCollider2D>().size.x;
                float obstacleXPos = entry.Key.GetComponent<Transform>().position.x;
                float obstacleXSize = entry.Key.GetComponent<BoxCollider2D>().size.x;
                float obstacleYOffset = entry.Key.GetComponent<BoxCollider2D>().offset.y;
                //Debug.Log("PASS"); 
                if (currentvelocity.y > 0 & !hasYHit) //player moving North (velocityY > 0)
                {
                    //Debug.Log("here");
                    if (obstacleXPos + obstacleXSize / 2.0 > playerEnvtHandlerXPos - playerEnvtHandlerXSize / 2.0 && //if player left bound to left of terrain right bound
                       obstacleXPos - obstacleXSize / 2.0 < playerEnvtHandlerXPos + playerEnvtHandlerXSize / 2.0)  //if player right bound is to right of terrain left bound
                    {
                        currentvelocity.Set(currentvelocity.x, -currentvelocity.y);
                        hasYHit = true;
                        //Debug.Log("NorthCollision");
                    }
                }
                else if (currentvelocity.y < 0 & !hasYHit) //player moving South (velocityY < 0) / player lower bound above box upper bound
                {
                    //Debug.Log("here");
                    if (obstacleXPos + obstacleXSize / 2.0 > playerEnvtHandlerXPos - playerEnvtHandlerXSize / 2.0 && //if player left bound to left of terrain right bound
                       obstacleXPos - obstacleXSize / 2.0 < playerEnvtHandlerXPos + playerEnvtHandlerXSize / 2.0)  //if player right bound is to right of terrain left bound
                    {
                        currentvelocity.Set(currentvelocity.x, -currentvelocity.y);
                        hasYHit = true;
                        //Debug.Log("SouthCollision");
                    }
                }
                if (currentvelocity.x > 0 & !hasXHit) //player moving East (velocityX > 0) / player to left
                {
                    if (obstacleYPos + obstacleYOffset + obstacleYSize / 2.0 > playerEnvtHandlerYPos - playerEnvtHandlerYSize / 2.0 && //if player south bound to south of terrain north bound
                       obstacleYPos + obstacleYOffset - obstacleYSize / 2.0 < playerEnvtHandlerYPos + playerEnvtHandlerYSize / 2.0)  //if player north bound is to north of terrain south bound
                    {
                        currentvelocity.Set(-currentvelocity.x, currentvelocity.y);
                        hasXHit = true;
                        //Debug.Log("EastCollision");
                    }
                }
                else if (currentvelocity.x < 0 && !hasXHit) //player moving West (velocityX < 0)
                {
                    if (obstacleYPos + obstacleYOffset + obstacleYSize / 2.0 > playerEnvtHandlerYPos - playerEnvtHandlerYSize / 2.0 && //if player south bound to south of terrain north bound
                       obstacleYPos + obstacleYOffset - obstacleYSize / 2.0 < playerEnvtHandlerYPos + playerEnvtHandlerYSize / 2.0)  //if player north bound is to north of terrain south bound
                    {
                        currentvelocity.Set(-currentvelocity.x, currentvelocity.y);
                        hasXHit = true;
                        //Debug.Log("WestCollision");
                    }
                }
            }
            else //Z-Testing is outsite if-block cuz otherwise we'd be detecting z-collisions too late.
            {
                if (ZVelocity > 0 && !hasZHit) //top hit
                {
                    if (entry.Key.GetComponent<EnvironmentPhysics>().GetTopHeight() > this.GetBottomHeight() + ZVelocity * Time.deltaTime && entry.Key.GetComponent<EnvironmentPhysics>().GetBottomHeight() < this.GetTopHeight() + +ZVelocity * Time.deltaTime)
                    {
                        //Debug.Log("CeilingCollision");
                        ZVelocity = -ZVelocity;
                        hasZHit = true;
                    }
                }
                else if (ZVelocity < 0 && !hasZHit) //bottom hit
                {
                    if (entry.Key.GetComponent<EnvironmentPhysics>().GetTopHeight() > this.GetBottomHeight() + ZVelocity * Time.deltaTime * 2f && entry.Key.GetComponent<EnvironmentPhysics>().GetBottomHeight() < this.GetTopHeight() + +ZVelocity * Time.deltaTime * 2f)
                    {
                        //Debug.Log("FloorCollision");
                        ZVelocity = -ZVelocity * _zVelocityDamping;
                        currentvelocity.Set(currentvelocity.x * _zVelocityDamping, currentvelocity.y * _zVelocityDamping);
                        if (ZVelocity < _zMinimumVelocity)
                        {
                            ZVelocity = _zMinimumVelocity;
                            //isAffectedByGravity = false;
                        }
                        //Debug.Log("CURRENTZVEL : " + ZVelocity);
                        hasZHit = true;
                    }
                }
            }
            
        }

        return currentvelocity;
    }
    
    /// <summary>
    /// Pushes projectile in direction of a target entity that passes within its seek radius.
    /// </summary>
    private Vector2 Seek(Vector2 currentVelocity)
    {
        Vector2 desiredVelocity = Vector2.zero;
        //float NearestTargetDistance = 1000.0f;
        float proximityScalar = 0.1f; // 0...1, 1 = on top of enemy, 0 = far from / no enemy

        //searches through objects within range
        foreach (EntityPhysics other in trackingAreaSeeker.trackedTargets)
        {
            if (other.tag == "Enemy" && _whoToHurt == "ENEMY")
            {

                if (_trackedTarget == null)
                {
                    _trackedTarget = other;
                }
                if ((_trackedTarget.transform.position - transform.position).sqrMagnitude > (other.transform.position - transform.position).sqrMagnitude)
                {
                    _trackedTarget = other;
                }
                /*
                Vector2 positiondifferential = other.GetComponent<Rigidbody2D>().position - GetComponent<Rigidbody2D>().position;
                //make the force applied inversely proportional to distance
                desiredVelocity = positiondifferential.normalized * (1 / (positiondifferential.magnitude * 2f) );
                //Z tracking
                float desiredZ = (other.GetBottomHeight() + other.GetTopHeight()) / 2.0f;
                float currentZ = (GetBottomHeight() + GetTopHeight()) / 2.0f;
                if (isSpeedScaledByProximity)
                {
                    proximityScalar = Mathf.Max(proximityScalar, 1 - positiondifferential.magnitude / trackingAreaSeeker.GetComponent<CircleCollider2D>().radius);
                }
                if (speed > 0) ZVelocity = ( desiredZ - currentZ );*/
            }
            else if (other.tag == "Friend" && _whoToHurt == "FRIEND")
            {
                if (_trackedTarget == null)
                {
                    _trackedTarget = other;
                }
                if ((other.transform.position - transform.position).sqrMagnitude > (other.transform.position - transform.position).sqrMagnitude)
                {
                    _trackedTarget = other;
                }
                desiredVelocity = other.GetComponent<Rigidbody2D>().position - GetComponent<Rigidbody2D>().position;
                //make the force applied inversely proportional to distance
                desiredVelocity = desiredVelocity.normalized * (1 / (desiredVelocity.magnitude * 2f));
                //Z tracking
                float desiredZ = (other.GetBottomHeight() + other.GetTopHeight()) / 2.0f;
                float currentZ = (GetBottomHeight() + GetTopHeight()) / 2.0f;
                if (speed > 0) ZVelocity = (desiredZ - currentZ); 
            }
        }
        if (_trackedTarget)
        {
            Vector2 positiondifferential = _trackedTarget.GetComponent<Rigidbody2D>().position - GetComponent<Rigidbody2D>().position;
            //make the force applied inversely proportional to distance
            desiredVelocity = positiondifferential.normalized * (1 / (positiondifferential.magnitude * 2f / trackingArea.bounds.size.magnitude));
            //Z tracking
            float desiredZ = (_trackedTarget.GetBottomHeight() + _trackedTarget.GetTopHeight()) / 2.0f;
            float currentZ = (GetBottomHeight() + GetTopHeight()) / 2.0f;
            if (isSpeedScaledByProximity)
            {
                proximityScalar = Mathf.Max(proximityScalar, 1 - positiondifferential.magnitude / trackingAreaSeeker.GetComponent<CircleCollider2D>().radius);
            }
            else
            {
                proximityScalar = 1.0f;
            }
            if (speed > 0) ZVelocity = (desiredZ - currentZ);
        }
        

        //currentVelocity += desiredVelocity;
        currentVelocity = Vector2.Lerp(currentVelocity, desiredVelocity, Mathf.Sqrt(proximityScalar)); // modulates direction 

        float trackingScalar = 1 + trackingAddedSpeed * proximityScalar; // modulates speed

        return currentVelocity.normalized * trackingScalar;
    }
    //==========================================| ENTITY COLLISION

    /// <summary>
    /// If player smacks this bullet
    /// </summary>
    public void PlayerRedirect(Vector2 redirection_vector, string newWhoToHurt, float newSpeed)
    {
        _velocity = redirection_vector;
        speed = newSpeed;
        _whoToHurt = newWhoToHurt;
        _timer = 0f;
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.5f, 0.1f);
        bulletHandler.OnPlayerDeflect();

        if (_deflectSFX != null)
        {
            GetComponent<AudioSource>().clip = _deflectSFX;
            GetComponent<AudioSource>().Play();
        }
        else
        {
            Debug.Log("Deflect SFX is null for object " + this);
        }
        SlashDeflectVFX.DeployFromPool(ObjectSprite.transform.position, redirection_vector);
        DeflectFlareVFX.DeployFromPool(ObjectSprite.transform.position);

    }

    public void Despawn()
    {
        if (bIsDespawning) return;
        bIsDespawning = true;
        if (ProjectileAnimator)
        {
            ProjectileAnimator.Play(Animator_DespawnStateName);
        }
        StartCoroutine(PlayDespawnAndReset(Animator_DespawnDuration));
    }

    public void Spawn()
    {
        if (ProjectileAnimator)
        {
            ProjectileAnimator.Play(Animator_SpawnStateName);
        }
        foreach (var trail in GlowSprite.transform.GetComponentsInChildren<TrailRenderer>())
        {
            trail.Clear();
        }
        //StartCoroutine(ClearTrails());
    }

    // need to do this cuz theres a one frame delay in the repositioning of the sprite    
    private IEnumerator ClearTrails()
    {
        yield return new WaitForEndOfFrame();
        foreach (var trail in GlowSprite.transform.GetComponentsInChildren<TrailRenderer>())
        {
            trail.Clear();
        }
    }

    private IEnumerator PlayDespawnAndReset(float duration)
    {
        speed = 0;
        //ZVelocity = 0;
        if (_isPooled)
        {
            float timer = 0;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                GlowSprite.material.SetFloat("_Opacity", GlowDespawnAlphaAnimationCurve.Evaluate(timer / duration));
                GlowSprite.transform.localScale = Vector3.one * GlowDespawnScaleAnimationCurve.Evaluate(timer / duration);
                speed = 0;
                //ZVelocity = 0;
                yield return new WaitForEndOfFrame();
            }
            if (trackingArea) trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
            transform.position = new Vector3(-999, -999, transform.position.z);
            ObjectSprite.transform.position = new Vector3(-999, -999, ObjectSprite.transform.position.z);
            //transform.parent.position = new Vector3(-999, -999, transform.parent.position.z);
            if (bulletHandler.SourceWeapon) bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID());
            Reset();
        }
    }

    //==========================================| OBJECT POOLING

        /// <summary>
        /// Should only be called when object is "removed" and returned to pool.
        /// </summary>
    public override void Reset()
    {
        if (!_isPooled) return;
        _velocity = Vector2.zero;
        TerrainTouching = new Dictionary<GameObject, KeyValuePair<float, float>>();
        TerrainTouched.Clear();
        EntitiesTouched = new List<PhysicsObject>();
        _targetsTouched = new Dictionary<EntityPhysics, bool>();
        _timer = 0f;
        TerrainTouched = new Dictionary<int, EnvironmentPhysics>();
        //ZVelocity = 0f;
        if (trackingArea && trackingAreaSeeker) trackingAreaSeeker.trackedTargets = new List<EntityPhysics>();
        _whoToHurt = _whoToHurt_original;
        speed = _speed_original;
        AlreadyDamagedTargets.Clear();
        GlowSprite.material.SetFloat("_Opacity", 1.0f);
        GlowSprite.transform.localScale = DefaultGlowScale;
        bIsDespawning = false;
        _trackedTarget = null;

    //trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
    //transform.position = new Vector3(-999, -999, transform.position.z);

}



}
