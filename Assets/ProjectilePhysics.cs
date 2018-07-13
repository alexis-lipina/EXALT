﻿using System.Collections;
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
   

    [SerializeField] private bool canBounce;
    [SerializeField] private bool isAffectedByGravity;
    [SerializeField] private bool canPenetrate;
    [SerializeField] private bool canBeDamaged;

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
    [SerializeField] private string _whoToHurt; //who to avoid damaging

    private Rigidbody2D bulletRigidBody;
    private Vector2 _velocity;
    public Vector2 Velocity
    {
        set { _velocity = value; }
        get { return _velocity; }
    }
    
    List<PhysicsObject> EntitiesTouched;


    override protected void Awake()
    {
        base.Awake();
        EntitiesTouched = new List<PhysicsObject>();
        
    }

	

	void Update ()
    {
        MoveCharacterPosition();
        if (bottomHeight < -18)
        {
            //TODO : Object Pooling  -  destroy
        }
    }




    //------------------------------------------| COLLISION DETECTION

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
        {
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
        if (other.gameObject.tag == "Enemy" && (_whoToHurt == "ENEMY" || _whoToHurt == "ALL"))
        {
            Debug.Log("Damaging Enemy");
            other.gameObject.GetComponent<EntityPhysics>().Inflict(1);
        }
        if (other.gameObject.tag == "Enemy") Debug.Log("Enemy!");
    }

    void OnTriggerStay2D(Collider2D other)
    {

        if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
        {
            Debug.Log("This should never happen. ");
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Environment")
        {
            TerrainTouching.Remove(other.gameObject);
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
                    if (entry.Key.GetComponent<EnvironmentPhysics>().GetTopHeight() > this.GetBottomHeight() + 2.0 * ZVelocity && entry.Key.GetComponent<EnvironmentPhysics>().GetBottomHeight() < this.GetTopHeight() + 2.0 * ZVelocity)
                    {
                        //Debug.Log("CeilingCollision");
                        ZVelocity = -ZVelocity;
                        hasZHit = true;
                    }
                }
                else if (ZVelocity < 0 && !hasZHit) //bottom hit
                {
                    if (entry.Key.GetComponent<EnvironmentPhysics>().GetTopHeight() > this.GetBottomHeight() + 2.0 * ZVelocity && entry.Key.GetComponent<EnvironmentPhysics>().GetBottomHeight() < this.GetTopHeight() + 2.0 * ZVelocity)
                    {
                        //Debug.Log("FloorCollision");
                        ZVelocity = -ZVelocity;
                        hasZHit = true;
                    }
                }
            }
            
        }

        return currentvelocity;
    }
    




}
