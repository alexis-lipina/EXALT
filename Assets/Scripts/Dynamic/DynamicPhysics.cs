using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines any physical object which can move throughout the game world, specifically
/// including entities (such as the player and enemies) and projectiles (such as bullets and grenades)
/// </summary>
public class DynamicPhysics : PhysicsObject
{
    [SerializeField] protected float _objectHeight; //height of object from "head to toe"
    [SerializeField] protected float _startElevation; //elevation at which objects base will be set at start of scene
    [SerializeField] protected float _spriteZOffset;
    [SerializeField] protected GameObject _objectSprite;
    [SerializeField] protected GameObject _environmentHandler;
    [SerializeField] protected float gravity;
    [SerializeField] protected float speed;


    public const float MAX_Z_VELOCITY_MAGNITUDE = 80f;
    protected Rigidbody2D PlayerRigidBody;


    public Dictionary<GameObject, KeyValuePair<float, float>> TerrainTouching; //each element of terrain touching the ***collider***
    //                                     bottom-^    ^-top
    protected Dictionary<int, EnvironmentPhysics> TerrainTouched;//each element touching ***EnvironmentHandler***
    //                   ^ instanceID
    //protected Dictionary<int, KeyValuePair<float, GameObject>> Shadows;
    //                    ^ instanceID       ^ height    ^ shadowobject 
    public float ZVelocity;

    public GameObject ObjectSprite
    {
        get { return _objectSprite; }
    }

    public float Gravity
    {
        get { return gravity; }
        set { gravity = value; }
    }

    protected virtual void Awake()
    {
        bottomHeight = _startElevation;
        topHeight = _startElevation + _objectHeight;
        PlayerRigidBody = gameObject.GetComponent<Rigidbody2D>();
        TerrainTouching = new Dictionary<GameObject, KeyValuePair<float, float>>();
        TerrainTouched = new Dictionary<int, EnvironmentPhysics>();
        _objectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 0.0f);
    }

    //TODO : Delete this once the shadowcasting issue has been resolved
    /*
    private void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(GetComponent<Rigidbody2D>().position, GetComponent<BoxCollider2D>().size, 0f);
        foreach (Collider2D other in colliders)
        {
            if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
            {
                //Debug.Log("Adding " + other.gameObject);
                //Debug.Log("N U T: " + other.gameObject.GetInstanceID());
                TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
            }
        }
    }
    */


    //===========================================================================| MOVEMENT

    /// <summary>
    /// Calls MoveWithCollision but with Time.deltaTime and "speed" field accounted for.
    /// </summary>
    /// <param name="xInput"></param>
    /// <param name="yInput"></param>
    public void MoveCharacterPositionPhysics(float xInput, float yInput)
    {
        //Vector2 temp = MoveAvoidEntities(new Vector2(xInput, yInput));
        //xInput = temp.x;
        //yInput = temp.y;
        this.MoveWithCollision(xInput * speed * Time.deltaTime , yInput * speed * Time.deltaTime);
        //playerRigidBody.MovePosition(new Vector2(playerRigidBody.position.x + xInput * 0.3f, playerRigidBody.position.y + yInput * 0.3f));
    }



    /// <summary>
    /// Moves entity strictly vertically along a (somewhat) ballistic trajectory, and checks for headbutt collisions
    /// </summary>
    public void FreeFall()
    {
        //CheckHitHeadOnCeiling();
        // deltaV = deltaT * a
        ZVelocity += Time.deltaTime * gravity;
        Mathf.Clamp(ZVelocity, MAX_Z_VELOCITY_MAGNITUDE * -1, MAX_Z_VELOCITY_MAGNITUDE);
        bottomHeight += ZVelocity * Time.deltaTime; //CHANGE WITH FRAMERATE??
        topHeight = bottomHeight + _objectHeight;
        // ZVelocity -= gravity;
        CheckHitHeadOnCeiling();
    }

    /// <summary>
    /// Allows objects which have a small or zero ZVelocity, which are atop an environment object that is moving down from beneath them to "stay glued" to 
    /// it rather than constantly toggling between falling and standing on it.
    /// </summary>
    public void SnapToFloor()
    {
        float delta = 0f;
        foreach(KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            delta = bottomHeight - entry.Value.GetTopHeight();
            if (delta > 0 && delta < 1f) 
            {
                bottomHeight -= delta;
                topHeight = bottomHeight + _objectHeight;
                ZVelocity = 0;
                return;
            }
        }
    }


    /// <summary>
    /// Returns true if, during the next frame, the player will fall into an object
    /// </summary>
    /// <returns></returns>
    public bool TestFeetCollision()
    {
        if (bottomHeight + ZVelocity < GetMaxTerrainHeightBelow())
        {
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Changes position of character image as player moves. 
    /// </summary>
    public void MoveCharacterPosition()
    {
        //                           X: Horizontal position                    Y: Vertical position - accounts for height and depth               Z: Depth - order of object draw calls
        Vector3 coords = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + _spriteZOffset + bottomHeight, gameObject.transform.position.y + _environmentHandler.GetComponent<BoxCollider2D>().offset.y - _environmentHandler.GetComponent<BoxCollider2D>().size.y / 2); //change here
        _objectSprite.transform.position = coords;
    }

    /// <summary>
    /// If the entity is about to "hit their head" on the underside of a collider, set the ZVelocity to 0
    /// </summary>
    public void CheckHitHeadOnCeiling()
    {
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            if (entry.Value.GetBottomHeight() < _objectHeight + bottomHeight && bottomHeight < entry.Value.GetBottomHeight() && ZVelocity > 0) ZVelocity = 0; // hit head on ceiling
            //Debug.Log("Testing if hit head");
        }
    }

    protected bool EntityWillCollide(float terrainBottom, float terrainTop, float playerBottom, float playerTop)
    {
        if (playerTop > terrainBottom && playerBottom + 0.8f < terrainTop)// +0.6 is a tolerance so entity moves up anyway
        {
            //Debug.Log("Collision");
            return true;
        }
        return false;
    }

    /// <summary>
    /// CALL MOVEPOSITIONPHYSICS FOR DELTATIME ACCOUNTABILITY
    /// 
    /// Moves the entity in a direction, stopping at any potential collisions and moving along walls as needed
    /// 
    /// Entity can only collide with EnvironmentObjects.
    /// </summary>
    /// <param name="velocityX">The desired change in x position</param>
    /// <param name="velocityY">The desired change in y position</param>
    public void MoveWithCollision(float velocityX, float velocityY)
    {
        //boxcast along movement path, as normal
        //if no collisions with high environment, move there
        //if yes collisions with high environment...
        //    Find boxcast-wall collision with shortest distance
        //    Save the point of collision, but 0.1 (or some other tolerance/buffer space) before the wall
        //    Subtract the traveled distance from  the original distance, and based on the remaining x and y components, fire off two more boxcasts in the cardinal directions
        //    Deal with both - if one of them goes further than 0.1 without collision, move along that axis until limit or collision
        float boxCastDistance = Mathf.Sqrt(velocityX * velocityX + velocityY * velocityY);
        List<RaycastHit2D> badCollisions = new List<RaycastHit2D>();
        //RaycastHit2D[] impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, new Vector2(2.0f, 1.2f), 0f, new Vector2(velocityX, velocityY), distance: boxCastDistance);
        RaycastHit2D[] impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, this.GetComponent<BoxCollider2D>().size, 0f, new Vector2(velocityX, velocityY), distance: boxCastDistance);
        bool NorthCollision = false;
        bool SouthCollision = false;
        bool EastCollision = false;
        bool WestCollision = false;
        float tempFract = 1.0f;
        //Debug.Log("BoxCastDistance:" + boxCastDistance);
        //Debug.Log("------------------------------------------");
        //Debug.Log("X:" + velocityX + "Y:" + velocityY);
        float environmentbottomHeight;
        float topHeight;
        float playerElevation;
        float playerHeight;


        foreach (RaycastHit2D hit in impendingCollisions) //BoxCast in direction of motion
        {
            if (hit.transform.gameObject.tag == "Environment")
            {
                //if (hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight() > bottomHeight) // if the height of the terrain object is greater than the altitude of the player
                if (EntityWillCollide(hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetBottomHeight(), hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight(), GetBottomHeight(), GetTopHeight()))
                {
                    //Debug.Log("Player is about to move illegally!");
                    badCollisions.Add(hit);
                    //return;
                }
            }
        }
        if (badCollisions.Count > 0) //any problematic collisions?
        {
            foreach (KeyValuePair<GameObject, KeyValuePair<float, float>> entry in TerrainTouching) //is entity currently colliding with anything
            {
                //Debug.Log("VALUE:" + entry.Value);

                environmentbottomHeight = entry.Key.GetComponent<EnvironmentPhysics>().GetBottomHeight();
                topHeight = entry.Key.GetComponent<EnvironmentPhysics>().GetTopHeight();
                playerElevation = bottomHeight;
                playerHeight = _objectHeight;



                if (EntityWillCollide(environmentbottomHeight, topHeight, playerElevation, playerElevation + playerHeight)) //if given element is a wall in the way
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

                    //entry.Key is the colliding terrainObject
                    //determine which direction the player is CURRENTLY having a collision in - North, South, East or West
                    //Debug.Log("Player is currently touching a wall");
                    //If player speed y component is positive, and left and right bounds are between right and left bounds, then there exists a Northern collision
                    if (playerEnvtHandlerYPos + playerEnvtHandlerYSize / 2.0 < obstacleYPos + obstacleYOffset - obstacleYSize / 2.0) //player moving North (velocityY > 0)
                    {
                        //Debug.Log("here");
                        if (obstacleXPos + obstacleXSize / 2.0 > playerEnvtHandlerXPos - playerEnvtHandlerXSize / 2.0 && //if player left bound to left of terrain right bound
                           obstacleXPos - obstacleXSize / 2.0 < playerEnvtHandlerXPos + playerEnvtHandlerXSize / 2.0)  //if player right bound is to right of terrain left bound
                        {
                            NorthCollision = true;
                            //Debug.Log("NorthCollision");
                        }
                    }
                    else if (playerEnvtHandlerYPos - playerEnvtHandlerYSize / 2.0 > obstacleYPos + obstacleYOffset + obstacleYSize / 2.0) //player moving South (velocityY < 0) / player lower bound above box upper bound
                    {
                        //Debug.Log("here");
                        if (obstacleXPos + obstacleXSize / 2.0 > playerEnvtHandlerXPos - playerEnvtHandlerXSize / 2.0 && //if player left bound to left of terrain right bound
                           obstacleXPos - obstacleXSize / 2.0 < playerEnvtHandlerXPos + playerEnvtHandlerXSize / 2.0)  //if player right bound is to right of terrain left bound
                        {
                            SouthCollision = true;
                            //Debug.Log("SouthCollision");
                        }
                    }
                    if (playerEnvtHandlerXPos + playerEnvtHandlerXSize / 2.0 < obstacleXPos - obstacleXSize / 2.0) //player moving East (velocityX > 0) / player to left
                    {
                        if (obstacleYPos + obstacleYOffset + obstacleYSize / 2.0 > playerEnvtHandlerYPos - playerEnvtHandlerYSize / 2.0 && //if player south bound to south of terrain north bound
                           obstacleYPos + obstacleYOffset - obstacleYSize / 2.0 < playerEnvtHandlerYPos + playerEnvtHandlerYSize / 2.0)  //if player north bound is to north of terrain south bound
                        {
                            EastCollision = true;
                            //Debug.Log("EastCollision");
                        }
                    }
                    else if (playerEnvtHandlerXPos - playerEnvtHandlerXSize / 2.0 > obstacleXPos + obstacleXSize / 2.0) //player moving West (velocityX < 0)
                    {
                        if (obstacleYPos + obstacleYOffset + obstacleYSize / 2.0 > playerEnvtHandlerYPos - playerEnvtHandlerYSize / 2.0 && //if player south bound to south of terrain north bound
                           obstacleYPos + obstacleYOffset - obstacleYSize / 2.0 < playerEnvtHandlerYPos + playerEnvtHandlerYSize / 2.0)  //if player north bound is to north of terrain south bound
                        {
                            WestCollision = true;
                            //Debug.Log("WestCollision");
                        }
                    }
                }
            }
            
            if ((NorthCollision && velocityY > 0 || SouthCollision && velocityY < 0) && (EastCollision && velocityX > 0 || WestCollision && velocityX < 0)) //Wedged into a corner, disallow motion
            {
                //Debug.Log("Stuck in a corner!");
                return;
            }
            else if (NorthCollision && velocityY > 0 || SouthCollision && velocityY < 0)
            {
                //Debug.Log("North/South Collision");
                //try to move along x axis
                velocityY = 0;
                //first, boxcast along axis                                                           USED TO BE new Vector2(2.0, 1.2)
                impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, GetComponent<BoxCollider2D>().size, 0f, new Vector2(1, 0), distance: velocityX);
                foreach (RaycastHit2D hit in impendingCollisions)
                {
                    //check to see if the hit is an east or west wall (aka a problem) 
                    //=====| basically figure out if the entity hit by a box is a potential problem. Maybe if it's not currently being touched, since if it were we'd be in a corner and that'd be handled?
                    if ((hit.transform.gameObject.tag == "Environment") && 
                        ((hit.transform.position.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().offset.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().size.y / 2.0 > PlayerRigidBody.GetComponent<Transform>().position.y - (PlayerRigidBody.GetComponent<BoxCollider2D>().size.y * 0.6) / 2.0 &&
                           (hit.transform.position.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().offset.y - hit.transform.gameObject.GetComponent<BoxCollider2D>().size.y / 2.0 < PlayerRigidBody.GetComponent<Transform>().position.y + (PlayerRigidBody.GetComponent<BoxCollider2D>().size.y * 0.6) / 2))))
                    {
                        if (EntityWillCollide(hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetBottomHeight(), hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight(), bottomHeight, bottomHeight + _objectHeight)) //TODO : Added tolerance for stepping up staircases
                        {
                            // Debug.Log("YEET");
                            //Debug.Log("HitDistance:" + hit.distance);
                            //found a problematic collision, go up to the shortest-distanced one
                            if (hit.distance < Mathf.Abs(velocityX))
                            {
                                // Debug.Log("YEETERRRR");
                                if (velocityX >= 0)
                                {
                                    velocityX = hit.distance;
                                }
                                else
                                {
                                    velocityX = hit.distance * -1.0f;
                                }
                            }
                        }
                    }
                }
                //Debug.Log("VelocityX:" + velocityX);
                velocityX = velocityX * 0.8f;

            }
            else if (EastCollision && velocityX > 0 || WestCollision && velocityX < 0)
            {
                //Debug.Log("East/WestCollision");
                //try to move along y axis
                velocityX = 0;
                //first, boxcast along axis                                                   USED TO BE new Vector2(2.0, 1.2)
                impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, GetComponent<BoxCollider2D>().size, 0f, new Vector2(0, 1), distance: velocityY);
                foreach (RaycastHit2D hit in impendingCollisions)
                {
                    //check to see if the hit is a North or South wall (aka a problem) 
                    //maybe check to see if the hit is a non-east/west wall?
                    //if it were a east/west wall, player north and player south bounds would be within env south and north bounds, respectively

                    if (hit.transform.gameObject.tag == "Environment" &&
                        (hit.transform.position.x + hit.transform.gameObject.GetComponent<BoxCollider2D>().size.x / 2.0 > PlayerRigidBody.GetComponent<Transform>().position.x - PlayerRigidBody.GetComponent<BoxCollider2D>().size.x / 2.0 &&
                           (hit.transform.position.x - hit.transform.gameObject.GetComponent<BoxCollider2D>().size.x / 2.0 < PlayerRigidBody.GetComponent<Transform>().position.x + PlayerRigidBody.GetComponent<BoxCollider2D>().size.x / 2)))
                    {
                        if (EntityWillCollide(hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetBottomHeight(), hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight(), bottomHeight, bottomHeight + _objectHeight))
                        {//found a problematic collision, go up to the shortest-distanced one
                            if (hit.distance < Mathf.Abs(velocityY))
                            {
                                if (velocityY >= 0)
                                {
                                    velocityY = hit.distance;

                                }
                                else
                                {
                                    velocityY = hit.distance * -1.0f;

                                }
                            }
                        }
                    }
                }
                velocityY = velocityY * 0.8f;
            }
            else
            {
                //Debug.Log("No Problematic Collision");
                //no current collision, go to shortest distance

                foreach (RaycastHit2D hit in impendingCollisions)
                {
                    if (hit.transform.gameObject.tag == "Environment" && hit.fraction < tempFract && hit.distance > 0)
                    {
                        tempFract = hit.fraction;
                        //Debug.Log("Stopping short by " + tempFract);
                    }
                }
                //Debug.Log(tempFract);
                velocityX = velocityX * tempFract;
                velocityY = velocityY * tempFract;
                //Debug.Log("X:" + velocityX + " Y:" + velocityY);

            }
        }
        else //move player character normally
        {
            //PlayerRigidBody.MovePosition(new Vector2(PlayerRigidBody.position.x + velocityX, PlayerRigidBody.position.y + velocityY));
        }
        
        //PlayerRigidBody.MovePosition(new Vector2(PlayerRigidBody.position.x + velocityX, PlayerRigidBody.position.y + velocityY));
        PlayerRigidBody.position = PlayerRigidBody.position + new Vector2(velocityX, velocityY);

    }



    //===========================================================================| TERRAIN COLLISION METHODS
    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Entering!");
        if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
        {
            //Debug.Log("Adding " + other.gameObject);
            //Debug.Log("N U T: " + other.gameObject.GetInstanceID());
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {

        if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
        {
            //Debug.Log("???? " + other.gameObject);
            Debug.Log("This should never happen. ");
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Exiting!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Removing " + other.gameObject);
            //Debug.Log("Hurk");
            TerrainTouching.Remove(other.gameObject);
        }
    }


    //===========================================================================| TERRAIN MANAGEMENT

    public void AddTerrainTouched(int terrainInstanceID, EnvironmentPhysics environment)
    {
        if (TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched already contains ID " + terrainInstanceID);
        }
        else
        {
            //Debug.Log("Shadow Added");
            TerrainTouched.Add(terrainInstanceID, environment);
            /*
            Shadows.Add(terrainInstanceID, new KeyValuePair<float, GameObject>(environment.GetTopHeight(), Instantiate(FirstShadow, this.transform.parent)));
            Shadows[terrainInstanceID].Value.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + environment.GetTopHeight(), environment.GetTopHeight());
            Shadows[terrainInstanceID].Value.SetActive(true);
            */
        }
        //PrintTerrain();
    }

    public void RemoveTerrainTouched(int terrainInstanceID)
    {
        if (!TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched does not contain ID " + terrainInstanceID);
        }
        TerrainTouched.Remove(terrainInstanceID);
        //Destroy(Shadows[terrainInstanceID].Value);
        //Shadows.Remove(terrainInstanceID);
        //Debug.Log("Shadow Removed");
        //PrintTerrain();
    }

    public float GetMaxTerrainHeightBelow()
    {
        float max = -20;
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            if (entry.Value.GetTopHeight() > max && _objectHeight + bottomHeight > entry.Value.GetTopHeight()) max = entry.Value.GetTopHeight();
        }

        return max;
    }

    protected void PrintTerrain()
    {
        Debug.Log("Terrain touching:");
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            Debug.Log("ID: " + entry.Key + "  heights:" + entry.Value.GetBottomHeight() + " " + entry.Value);
        }
    }

    /// <summary>
    /// Returns true if the entity is touching an environment object
    /// </summary>
    /// <returns></returns>
    public bool IsCollidingWithEnvironment()
    {
        foreach (KeyValuePair<GameObject, KeyValuePair<float, float>> entry in TerrainTouching)
        {
            // if entry.bottom < this.top and entry.top > this.bottom  (if inside)
            if (entry.Value.Key < topHeight && entry.Value.Value > bottomHeight) return true;
        }
        return false;
    }


    //===========================================================================| GETTERS & SETTERS

    public float GetObjectHeight()
    {
        return _objectHeight;
    }
    public float GetObjectElevation()
    {
        return bottomHeight;
    }
    public void SetObjectElevation(float e)
    {
        _startElevation = e;
        bottomHeight = e;
    }

    /// <summary>
    /// For object pooling - returns to its base state
    /// </summary>
    public virtual void Reset()
    {
        TerrainTouched.Clear();
        TerrainTouching.Clear();
        ZVelocity = 0;
    }

}
