using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class handles player interaction with environment - namely, terrain traversal. An area of terrain has a 2D Box Collider associated with it,
/// which defines the area, and the terrain object stores the height of the terrain surface. Also, within the PlayerHandler is stored a HashMap which
/// stores each terrain object it is currently above. When a player's collider enters a new terrain collider, a new entry is added to this HashMap, 
/// with the terrain object's InstanceID as the key, and the terrain object's height as the value. When the player collider exits a terrain collider,
/// the entry in the HashMap with the terrain object's InstanceID as the key is removed.
/// 
/// Thus, the terrain objects above which the player is standing are stored. These are referenced for physics calculations and interactions, such as 
/// what height a player will land from a jump at, whether they just walked off a cliff, etc.
/// </summary>
public class EntityPhysics : MonoBehaviour
{
    [SerializeField] private NavigationManager navManager;
    [SerializeField] private float playerSpriteZOffset;
    [SerializeField] private GameObject characterSprite;
    [SerializeField] private GameObject environmentHandler;
    [SerializeField] private GameObject handlerObject;
    [SerializeField] private GameObject FirstShadow;
    [SerializeField] private float gravity;
    [SerializeField] private float speed;
    [SerializeField] private float MaxHP;
    private float currentHP;
    private bool hasBeenHit;




    private Rigidbody2D PlayerRigidBody;
    private EntityHandler entityHandler;
    private KeyValuePair<Vector2, EnvironmentPhysics> lastFootHold;
    private EnvironmentPhysics currentNavEnvironmentObject;


    Dictionary<GameObject, KeyValuePair<float, float>> TerrainTouching; //each element of terrain touching the collider
    Dictionary<int, EnvironmentPhysics> TerrainTouched;
    //         ^ instanceID       ^bottom   ^ topheight
    Dictionary<int, KeyValuePair<float, GameObject>> Shadows;
    //          ^ instanceID       ^ height    ^ shadowobject 

    private float entityElevation;
    private float entityHeight;
    public float ZVelocity;


    void Start()
    {
        entityElevation = 20;
        entityHeight = 3;
        entityHandler = handlerObject.GetComponent<EntityHandler>();
        PlayerRigidBody = gameObject.GetComponent<Rigidbody2D>();
        TerrainTouching = new Dictionary<GameObject, KeyValuePair<float, float>>();
        TerrainTouched = new Dictionary<int, EnvironmentPhysics>();
        Shadows = new Dictionary<int, KeyValuePair<float, GameObject>>();
        currentHP = MaxHP;
        hasBeenHit = false;
    }

    void Update()
    {
        MoveCharacterPosition();
        if (entityElevation < -18)
        {
            WarpToPlatform();
        }
        UpdateEntityNavigationObject();
        hasBeenHit = false;
    }

    //======================================================| Terrain Collision management
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

    //=====================================================================| MOVEMENT 
    /// <summary>
    /// Calls MoveWithCollision but with Time.deltaTime and "speed" field accounted for 
    /// </summary>
    /// <param name="xInput"></param>
    /// <param name="yInput"></param>
    public void MoveCharacterPositionPhysics(float xInput, float yInput)
    {
        this.MoveWithCollision(xInput * speed * Time.deltaTime, yInput * speed * Time.deltaTime);
        //playerRigidBody.MovePosition(new Vector2(playerRigidBody.position.x + xInput * 0.3f, playerRigidBody.position.y + yInput * 0.3f));
    }

    /// <summary>
    /// Moves entity along a (somewhat) ballistic trajectory, and checks for headbutt collisions
    /// </summary>
    public void FreeFall()
    {
        //CheckHitHeadOnCeiling();
        entityElevation += ZVelocity;
        ZVelocity -= gravity;
        CheckHitHeadOnCeiling();
    }
    /// <summary>
    /// Returns true if, during the next frame, the player will fall into an object
    /// </summary>
    /// <returns></returns>
    public bool TestFeetCollision()
    {
        if (entityElevation + ZVelocity < GetMaxTerrainHeightBelow())
        {
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Changes position of character image as player moves. 
    /// </summary>
    private void MoveCharacterPosition()
    {
        //                           X: Horizontal position                    Y: Vertical position - accounts for height and depth               Z: Depth - order of object draw calls
        Vector3 coords = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + playerSpriteZOffset + entityElevation, gameObject.transform.position.y + environmentHandler.GetComponent<BoxCollider2D>().offset.y - environmentHandler.GetComponent<BoxCollider2D>().size.y / 2 + 0.4f);
        characterSprite.transform.position = coords;
        //playerCharacterSprite.transform.position = new Vector3(playerCharacterSprite.transform.position.x, playerCharacterSprite.transform.position.y, physicsobject.transform.position.y + physicsobject.GetComponent<BoxCollider2D>().offset.y + physicsobject.GetComponent<BoxCollider2D>().size.y / 2);
        //Vector2 tempvect = new Vector2(xInput, yInput);

        //move shadows
        foreach (KeyValuePair<int, KeyValuePair<float, GameObject>> entry in Shadows)
        {
            entry.Value.Value.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + entry.Value.Key, gameObject.transform.position.y + environmentHandler.GetComponent<BoxCollider2D>().offset.y - environmentHandler.GetComponent<BoxCollider2D>().size.y / 2 + 0.4f);
        }
    }
     /// <summary>
     /// If the entity is about to "hit their head" on the underside of a collider, set the ZVelocity to 0
     /// </summary>
    public void CheckHitHeadOnCeiling()
    {
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            if (entry.Value.GetBottomHeight() < entityHeight + entityElevation && entityElevation < entry.Value.GetBottomHeight() && ZVelocity > 0) ZVelocity = 0; // hit head on ceiling
            //Debug.Log("Testing if hit head");
        }
    }


    private bool PlayerWillCollide(float terrainBottom, float terrainTop, float playerBottom, float playerTop)
    {
        if (playerTop > terrainBottom && playerBottom < terrainTop)
            return true;
        return false;
    }

    /// <summary>
    /// Moves the entity in a direction, testing for collisions along that direction and acting accordingly to
    /// prevent clipping through objects, and allow the player to move right up against objects
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
        RaycastHit2D[] impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, new Vector2(2.0f, 1.2f), 0f, new Vector2(velocityX, velocityY), distance: boxCastDistance);
        bool NorthCollision = false;
        bool SouthCollision = false;
        bool EastCollision = false;
        bool WestCollision = false;
        float tempFract = 1.0f;
        //Debug.Log("BoxCastDistance:" + boxCastDistance);
        //Debug.Log("------------------------------------------");
        //Debug.Log("X:" + velocityX + "Y:" + velocityY);
        float bottomHeight;
        float topHeight;
        float playerElevation;
        float playerHeight;


        
        foreach (RaycastHit2D hit in impendingCollisions) //BoxCast in direction of motion
        {
            if (hit.transform.gameObject.tag == "Environment")
            {
                if (hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight() > entityElevation) // if the height of the terrain object is greater than the altitude of the player
                {
                    //Debug.Log("Player is about to move illegally!");
                    badCollisions.Add(hit);
                    //return;
                }
            }
        }
        if (badCollisions.Count > 0) //any problematic collisions?
        {
            foreach(KeyValuePair<GameObject, KeyValuePair<float, float>> entry in TerrainTouching)//is player currently colliding with anything
            {
                //Debug.Log("VALUE:" + entry.Value);

                bottomHeight = entry.Key.GetComponent<EnvironmentPhysics>().GetBottomHeight();
                topHeight = entry.Key.GetComponent<EnvironmentPhysics>().GetTopHeight();
                playerElevation = entityElevation;
                playerHeight = entityHeight;



                if (PlayerWillCollide(bottomHeight, topHeight, playerElevation, playerElevation + playerHeight)) //if given element is a wall in the way
                {
                    float playerEnvtHandlerYPos = environmentHandler.GetComponent<Transform>().position.y;
                    float playerEnvtHandlerYSize = environmentHandler.GetComponent<BoxCollider2D>().size.y;
                    float obstacleYPos = entry.Key.GetComponent<Transform>().position.y;
                    float obstacleYSize = entry.Key.GetComponent<BoxCollider2D>().size.y;
                    float playerEnvtHandlerXPos = environmentHandler.GetComponent<Transform>().position.x;
                    float playerEnvtHandlerXSize = environmentHandler.GetComponent<BoxCollider2D>().size.x;
                    float obstacleXPos = entry.Key.GetComponent<Transform>().position.x;
                    float obstacleXSize = entry.Key.GetComponent<BoxCollider2D>().size.x;
                    float obstacleYOffset = entry.Key.GetComponent<BoxCollider2D>().offset.y;

                    //entry.Key is the colliding terrainObject
                    //determine which direction the player is CURRENTLY having a collision in - North, South, East or West
                    //Debug.Log("Player is currently touching a wall");
                    //If player speed y component is positive, and left and right bounds are between right and left bounds, then there exists a Northern collision
                    if (playerEnvtHandlerYPos  + playerEnvtHandlerYSize / 2.0 < obstacleYPos + obstacleYOffset - obstacleYSize / 2.0) //player moving North (velocityY > 0)
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
                //first, boxcast along axis
                impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, new Vector2(2.0f, 1.2f), 0f, new Vector2(1, 0), distance: velocityX);
                foreach(RaycastHit2D hit in impendingCollisions)
                {
                    //check to see if the hit is an east or west wall (aka a problem) 
                    //=====| basically figure out if the entity hit by a box is a potential problem. Maybe if it's not currently being touched, since if it were we'd be in a corner and that'd be handled?
                    if ((hit.transform.gameObject.tag == "Environment" ) && //TODO: added in entity collision
                        ((hit.transform.position.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().offset.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().size.y / 2.0 > PlayerRigidBody.GetComponent<Transform>().position.y - (PlayerRigidBody.GetComponent<BoxCollider2D>().size.y * 0.6) / 2.0 &&
                           (hit.transform.position.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().offset.y - hit.transform.gameObject.GetComponent<BoxCollider2D>().size.y / 2.0 < PlayerRigidBody.GetComponent<Transform>().position.y + (PlayerRigidBody.GetComponent<BoxCollider2D>().size.y * 0.6) / 2))))
                    {
                        if (PlayerWillCollide(hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetBottomHeight(), hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight(), entityElevation, entityElevation + entityHeight))
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
                //first, boxcast along axis
                impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, new Vector2(2.0f, 1.2f), 0f, new Vector2(0, 1), distance: velocityY);
                foreach (RaycastHit2D hit in impendingCollisions)
                {
                    //check to see if the hit is a North or South wall (aka a problem) 
                    //maybe check to see if the hit is a non-east/west wall?
                    //if it were a east/west wall, player north and player south bounds would be within env south and north bounds, respectively
                    
                    if (hit.transform.gameObject.tag == "Environment" &&
                        (hit.transform.position.x + hit.transform.gameObject.GetComponent<BoxCollider2D>().size.x / 2.0 > PlayerRigidBody.GetComponent<Transform>().position.x - PlayerRigidBody.GetComponent<BoxCollider2D>().size.x / 2.0 &&
                           (hit.transform.position.x - hit.transform.gameObject.GetComponent<BoxCollider2D>().size.x / 2.0 < PlayerRigidBody.GetComponent<Transform>().position.x + PlayerRigidBody.GetComponent<BoxCollider2D>().size.x / 2)))
                    {
                        if (PlayerWillCollide(hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetBottomHeight(), hit.transform.gameObject.GetComponent<EnvironmentPhysics>().GetTopHeight(), entityElevation, entityElevation + entityHeight))
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

                foreach(RaycastHit2D hit in impendingCollisions)
                {
                    if (hit.transform.gameObject.tag == "Environment" && hit.fraction < tempFract && hit.distance > 0 )
                    {
                        tempFract = hit.fraction;
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
        

        PlayerRigidBody.MovePosition(new Vector2(PlayerRigidBody.position.x + velocityX, PlayerRigidBody.position.y + velocityY));

    }

    //=====================================================================| Terrain Management

    public void AddTerrainTouched(int terrainInstanceID, EnvironmentPhysics environment)
    {
        if (TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched already contains ID " + terrainInstanceID);
        }
        else
        {
            TerrainTouched.Add(terrainInstanceID, environment);
            Shadows.Add(terrainInstanceID, new KeyValuePair<float, GameObject>(environment.GetTopHeight(), Instantiate(FirstShadow, this.transform.parent)));
            Shadows[terrainInstanceID].Value.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + environment.GetTopHeight(), environment.GetTopHeight());
            Shadows[terrainInstanceID].Value.SetActive(true);
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
        Destroy(Shadows[terrainInstanceID].Value);
        Shadows.Remove(terrainInstanceID);
        //PrintTerrain();
    }

    public float GetMaxTerrainHeightBelow()
    {
        float max = -20;
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            if (entry.Value.GetTopHeight() > max && entityHeight + entityElevation > entry.Value.GetTopHeight()) max = entry.Value.GetTopHeight();
        }

        return max;
    }

    private void PrintTerrain()
    {
        Debug.Log("Terrain touching:");
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            Debug.Log("ID: " + entry.Key + "  heights:" + entry.Value.GetBottomHeight() + " " + entry.Value);
        }
    }

    public void SavePosition()
    {
        EnvironmentPhysics temp = null;
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            if (entry.Value.GetTopHeight() == entityElevation)
            {
                temp = entry.Value;
            }
        }
        if (temp != null)
        {
            lastFootHold = new KeyValuePair<Vector2, EnvironmentPhysics>(gameObject.GetComponent<Rigidbody2D>().position, temp);
        }
        else
        {
            Debug.Log("ALERT!!!");
        }

    }

    private void WarpToPlatform()
    {
        //Debug.Log(lastFootHold.Key);
        /*
        physicsobject.GetComponent<Rigidbody2D>().MovePosition(lasttouched.Key);
        PlayerElevation = lasttouched.Value.GetTopHeight() + 5;
        */
        Vector2 terrainsize = lastFootHold.Value.GetComponent<BoxCollider2D>().size;
        Vector2 terrainpos = lastFootHold.Value.GetComponent<Transform>().position;
        Vector2 terrainoffset = lastFootHold.Value.GetComponent<BoxCollider2D>().offset;
        float terrainheight = lastFootHold.Value.GetTopHeight();
        //Vector2 playerpos = physicsobject.GetComponent<Transform>().position;
        Vector2 warpcoordinates = lastFootHold.Key;


        //Debug.Log("Player warping");
        //if lastfoothold player center is outside bounds of collider
        //Debug.Log("destination center:" + lastFootHold.Key);
        //Debug.Log("Terrain position:" + terrainpos);
        //Debug.Log("Terrain Size:" + terrainsize);
        if (lastFootHold.Key.x < terrainpos.x + terrainoffset.x - terrainsize.x / 2)
        {
            warpcoordinates.x = terrainpos.x + terrainoffset.x - terrainsize.x / 2;
            //Debug.Log("Position too far to the left");
        }
        else if (lastFootHold.Key.x > terrainpos.x + terrainoffset.x + terrainsize.x / 2)
        {
            warpcoordinates.x = terrainpos.x + terrainoffset.x + terrainsize.x / 2;
            //Debug.Log("Position too far to the right");

        }
        if (lastFootHold.Key.y < terrainpos.y + terrainoffset.y - terrainsize.y / 2)
        {
            warpcoordinates.y = terrainpos.y + terrainoffset.y - terrainsize.y / 2;
            //Debug.Log("Position too far south");
        }
        else if (lastFootHold.Key.y > terrainpos.y + terrainoffset.y + terrainsize.y / 2)
        {
            warpcoordinates.y = terrainpos.y + terrainoffset.y + terrainsize.y / 2;
            //Debug.Log("Position too far north");

        }
        //Debug.Log("WARPING HERE:" + warpcoordinates);
        ZVelocity = 0;
        // - - - For some reason, MovePosition() wasnt working for the test Punching Bag NPC. 
        //gameObject.GetComponent<Rigidbody2D>().MovePosition(warpcoordinates);
        gameObject.GetComponent<Rigidbody2D>().position = warpcoordinates;
        entityElevation = terrainheight + 0; //maybe have the player fall from a great height to reposition them?
    }

    private void UpdateEntityNavigationObject()
    {
        List<EnvironmentPhysics> objectsbelow = new List<EnvironmentPhysics>();
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched) //get list of all colliders touching point
        {
            if (entry.Value.gameObject.GetComponent<BoxCollider2D>().OverlapPoint(this.gameObject.GetComponent<BoxCollider2D>().offset + (Vector2)gameObject.transform.position)) //if point at center of entity collider overlaps
            {
                objectsbelow.Add(entry.Value);
                //Debug.Log("Point!" + entry.Value.gameObject.GetInstanceID());
            }
        }
        float max = float.NegativeInfinity;
        EnvironmentPhysics tempphys = null;
        foreach (EnvironmentPhysics physobj in objectsbelow)
        {
            if (physobj.GetTopHeight() > max && entityHeight + entityElevation > physobj.GetTopHeight())
            {
                max = physobj.GetTopHeight();
                tempphys = physobj;
            }
        }
        if (tempphys == null || tempphys == currentNavEnvironmentObject)
        {
            return;
        }
        //Debug.Log("Updating point!!!");
        //Debug.Log(this.handlerObject);
        if (navManager.entityChangePositionDelegate != null)
            navManager.entityChangePositionDelegate(this.gameObject, tempphys);
        currentNavEnvironmentObject = tempphys;
    }


    /// <summary>
    /// Deal this entity damage, causing them to flash and lose health
    /// </summary>
    /// <param name="damage">Quantity of health to subtract from the entity</param>
    public void Inflict(float damage)
    {
        hasBeenHit = true;
        currentHP -= damage;
        StartCoroutine(TakeDamageFlash());
        
        if (currentHP <= 0)
        {
            GameObject.Destroy(gameObject.transform.parent.gameObject); //TODO - this is an awful way of dealing with death
        }
    }

    /// <summary>
    /// Deal this entity damage, as well as push them in a direction
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="direction"></param>
    public void Inflict(float damage, Vector2 direction, float force)
    {
        //Debug.Log(direction);
        Debug.Log(gameObject.GetComponent<Rigidbody2D>().position);
        MoveWithCollision(direction.x * force, direction.y * force);
        Inflict(damage);
    }

    IEnumerator TakeDamageFlash()
    {
        Debug.Log("TakeDamageFlash entered");
        entityHandler.JustGotHit();
        characterSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 1);
        for (float i = 0; i < 2; i++)
        {
            //characterSprite.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0);
            characterSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", new Color(1, 1, 1, 1));
            yield return new WaitForSeconds(0.05f);
            //characterSprite.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
            characterSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", new Color(1, 0, 0, 1));
            yield return new WaitForSeconds(0.05f);
        }

        characterSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 0);
    }

    //===============================================================| getters and setters
    public EnvironmentPhysics getCurrentNavObject()
    {
        return currentNavEnvironmentObject;
    }

    public float GetEntityHeight()
    {
        return entityHeight;
    }
    public float GetEntityElevation()
    {
        return entityElevation;
    }
    public void SetEntityElevation(float e)
    {
        entityElevation = e;
    }
    public float GetCurrentHealth()
    {
        return currentHP;
    }
    public float GetMaxHealth()
    {
        return MaxHP;
    }
}