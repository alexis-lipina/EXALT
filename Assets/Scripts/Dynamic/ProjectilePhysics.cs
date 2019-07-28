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
    [SerializeField] private bool explodesOnDeath;
    [SerializeField] private bool doesTracking;
    [SerializeField] private Collider2D trackingArea;
    [SerializeField] private int _damageAmount = 1;
    [SerializeField] private float _impactForce = 0.5f;
    [SerializeField] private ElementType _damageType = ElementType.NONE;
    [SerializeField] private float _zVelocityDamping = 1f;
    [SerializeField] private float _zMinimumVelocity = 20f;
    [SerializeField] private AudioSource _deflectSFX;
    



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



    override protected void Awake()
    {
        base.Awake();
        EntitiesTouched = new List<PhysicsObject>();
        _targetsTouched = new Dictionary<EntityPhysics, bool>();
        _timer = 0f;

        // Set original Fields
        _speed_original = speed;
        _whoToHurt_original = _whoToHurt;
    }

	

	void Update ()
    {
        _timer += Time.deltaTime;

        //physics
        _velocity = Bounce(_velocity);

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
        else _velocity *= 0.9f;
        //Debug.Log("Elevation : " + bottomHeight);

        MoveCharacterPosition();
        if (bottomHeight < -18 || _timer > _timeToDestroy)
        {
            Debug.Log(bulletHandler);
            Debug.Log(bulletHandler.SourceWeapon);
            //bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID()); //"deletes" if out of bounds
            transform.parent.gameObject.SetActive(false);
            Reset();
        }
    }


    //------------------------------------------| COLLISION DETECTION

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
        {
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
        if (other.gameObject.tag == "Enemy" && (_whoToHurt == "ENEMY" || _whoToHurt == "ALL"))
        {

            if (other.gameObject.GetComponent<EntityPhysics>().GetBottomHeight() < topHeight && other.gameObject.GetComponent<EntityPhysics>().GetTopHeight() > bottomHeight)//enemy hit
            {
                _targetsTouched.Add(other.gameObject.GetComponent<EntityPhysics>(), true);
                other.gameObject.GetComponent<EntityPhysics>().Inflict(_damageAmount, force:_impactForce * Velocity.normalized, type:_damageType);
                if (_damageType == ElementType.FIRE) other.gameObject.GetComponent<EntityPhysics>().Burn();
                if (!canPenetrate)
                {
                    Reset();
                    if (trackingArea) trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
                    transform.position = new Vector3(-999, -999, transform.position.z);
                    ObjectSprite.transform.position = new Vector3(-999, -999, ObjectSprite.transform.position.z);
                    //transform.parent.position = new Vector3(-999, -999, transform.parent.position.z);
                    bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID());
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
                _targetsTouched.Add(other.gameObject.GetComponent<EntityPhysics>(), true);
                other.gameObject.GetComponent<EntityPhysics>().Inflict(_damageAmount, force: _impactForce * Velocity.normalized, type: _damageType);
                if (_damageType == ElementType.FIRE) other.gameObject.GetComponent<EntityPhysics>().Burn();
                if (!canPenetrate)
                {
                    Reset();
                    if (trackingArea) trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
                    transform.position = new Vector3(-999, -999, transform.position.z);
                    ObjectSprite.transform.position = new Vector3(-999, -999, ObjectSprite.transform.position.z);
                    //transform.parent.position = new Vector3(-999, -999, transform.parent.position.z);
                    bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID());
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
                other.gameObject.GetComponent<EntityPhysics>().Inflict(_damageAmount, force:Velocity.normalized * _impactForce, type:_damageType);
                if (_damageType == ElementType.FIRE) other.gameObject.GetComponent<EntityPhysics>().Burn();
                _targetsTouched[other.gameObject.GetComponent<EntityPhysics>()] = true;
                if (!canPenetrate)
                {
                    Reset();
                    if (trackingArea) trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
                    transform.position = new Vector3(-999, -999, transform.position.z);
                    ObjectSprite.transform.position = new Vector3(-999, -999, ObjectSprite.transform.position.z);

                    bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID());
                }
            }
        }
        else if (other.gameObject.tag == "Friend" && (_whoToHurt == "FRIEND" || _whoToHurt == "ALL"))
        {
            if (other.gameObject.GetComponent<EntityPhysics>().GetBottomHeight() < topHeight && other.gameObject.GetComponent<EntityPhysics>().GetTopHeight() > bottomHeight)//enemy hit
            {
                _targetsTouched.Add(other.gameObject.GetComponent<EntityPhysics>(), true);
                other.gameObject.GetComponent<EntityPhysics>().Inflict(_damageAmount, force: _impactForce * Velocity.normalized, type: _damageType);
                if (_damageType == ElementType.FIRE) other.gameObject.GetComponent<EntityPhysics>().Burn();
                if (!canPenetrate)
                {
                    Reset();
                    if (trackingArea) trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
                    transform.position = new Vector3(-999, -999, transform.position.z);
                    ObjectSprite.transform.position = new Vector3(-999, -999, ObjectSprite.transform.position.z);
                    //transform.parent.position = new Vector3(-999, -999, transform.parent.position.z);
                    bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID());
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
                    if (trackingArea) trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
                    transform.position = new Vector3(-999, -999, transform.position.z);
                    ObjectSprite.transform.position = new Vector3(-999, -999, ObjectSprite.transform.position.z);
                    //transform.parent.position = new Vector3(-999, -999, transform.parent.position.z);
                    bulletHandler.SourceWeapon.ReturnToPool(GetComponent<Transform>().parent.gameObject.GetInstanceID());
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
        //searches through objects within range
        foreach (EntityPhysics other in trackingArea.GetComponent<ProjectileSeeker>().trackedTargets)
        {
            if (other.tag == "Enemy")
            {
                Debug.Log("ENEMY");
                desiredVelocity = other.GetComponent<Rigidbody2D>().position - GetComponent<Rigidbody2D>().position;
                //make the force applied inversely proportional to distance
                desiredVelocity = desiredVelocity.normalized * (1 / (desiredVelocity.magnitude * 2f) );
                //Z tracking
                float desiredZ = (other.GetBottomHeight() + other.GetTopHeight()) / 2.0f;
                float currentZ = (GetBottomHeight() + GetTopHeight()) / 2.0f;
                ZVelocity = ( desiredZ - currentZ );
            }
        }


        currentVelocity += desiredVelocity;
        
        return currentVelocity.normalized;
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
        
        _deflectSFX.Play();
        SlashDeflectVFX.DeployFromPool(ObjectSprite.transform.position, redirection_vector);
        DeflectFlareVFX.DeployFromPool(ObjectSprite.transform.position);

    }

    //==========================================| OBJECT POOLING

    /// <summary>
    /// Should only be called when object is "removed" and returned to pool.
    /// </summary>
    public override void Reset()
    {
        _velocity = Vector2.zero;
        TerrainTouching = new Dictionary<GameObject, KeyValuePair<float, float>>();
        TerrainTouched.Clear();
        EntitiesTouched = new List<PhysicsObject>();
        _targetsTouched = new Dictionary<EntityPhysics, bool>();
        _timer = 0f;
        TerrainTouched = new Dictionary<int, EnvironmentPhysics>();
        ZVelocity = 0f;
        if (trackingArea) trackingArea.GetComponent<ProjectileSeeker>().trackedTargets = new List<EntityPhysics>();
        _whoToHurt = _whoToHurt_original;
        speed = _speed_original;

        //trackingArea.transform.position = new Vector3(-999, -999, trackingArea.transform.position.z);
        //transform.position = new Vector3(-999, -999, transform.position.z);

    }
    
    

}
