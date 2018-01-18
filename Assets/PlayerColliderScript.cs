using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
public class PlayerColliderScript : MonoBehaviour
{
    [SerializeField] private GameObject playerEnvironmentHandler;
    [SerializeField] private GameObject playerHandlerObject;
    private PlayerHandler playerHandler;
    private Rigidbody2D PlayerRigidBody;
    Dictionary<GameObject, KeyValuePair<float, float>> TerrainTouching; //each element of terrain touching the collider


    void Start()
    {
        playerHandler = playerHandlerObject.GetComponent<PlayerHandler>();
        PlayerRigidBody = gameObject.GetComponent<Rigidbody2D>();
        TerrainTouching = new Dictionary<GameObject, KeyValuePair<float, float>>();
    }

    //======================================================| Terrain Collision management
    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Entering!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Blep");
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!TerrainTouching.ContainsKey(other.gameObject))
        {
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Exiting!");
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Hurk");
           TerrainTouching.Remove(other.gameObject);
        }
    }

    //=====================================================================| MOVEMENT 

    public void MoveWithCollisionTwo(float velocityX, float velocityY)
    {
        //boxcast along movement path, as normal
        //if no collisions with high environment, move there
        //if yes collisions with high environment...
        //    Find boxcast-wall collision with shortest distance
        //    Save the point of collision, but 0.1 (or something) before the wall
        //    Subtract the traveled distance from  the original distance, and based on the remaining x and y components, fire off two more boxcasts in the cardinal directions
        //    Deal with both - if one of them goes further than 0.1 without collision, move along that axis until limit or 
    }

    public bool playerWillCollide(float terrainBottom, float terrainTop, float playerBottom, float playerTop)
    {
        if (playerTop > terrainBottom && playerBottom < terrainTop)
            return true;
        return false;
    }


    public void MoveWithCollision(float velocityX, float velocityY)
    {
        float boxCastDistance = Mathf.Sqrt(velocityX * velocityX + velocityY * velocityY);
        List<RaycastHit2D> badCollisions = new List<RaycastHit2D>();
        GameObject tempEnvironmentObject;
        RaycastHit2D[] impendingCollisions = Physics2D.BoxCastAll(this.gameObject.transform.position, new Vector2(2.0f, 1.2f), 0f, new Vector2(velocityX, velocityY), distance: boxCastDistance);
        bool NorthCollision = false;
        bool SouthCollision = false;
        bool EastCollision = false;
        bool WestCollision = false;
        Vector2 tempEnviroPos;
        Vector2 tempPlayerPos;
        float tempFract = 1.0f;
        //Debug.Log("BoxCastDistance:" + boxCastDistance);
        Debug.Log("------------------------------------------");
        //Debug.Log("X:" + velocityX + "Y:" + velocityY);
        foreach(RaycastHit2D hit in impendingCollisions) //BoxCast in direction of motion
        {
            if (hit.transform.gameObject.tag == "Environment")
            {
                if (hit.transform.gameObject.GetComponent<EnvironmentPhysics>().getTopHeight() > playerHandler.getPlayerElevation()) // if the height of the terrain object is greater than the altitude of the player
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
                if (playerWillCollide(entry.Key.GetComponent<EnvironmentPhysics>().getBottomHeight(), entry.Key.GetComponent<EnvironmentPhysics>().getTopHeight(), playerHandler.getPlayerElevation(), playerHandler.getPlayerElevation() + playerHandler.getPlayerHeight())) //if given element is a wall in the way
                {
                    //entry.Key is the colliding terrainObject
                    //determine which direction the player is CURRENTLY having a collision in - North, South, East or West
                    //Debug.Log("Player is currently touching a wall");
                    //Debug.Log("PlayerGonnaCollide!!!");
                    //If player speed y component is positive, and left and right bounds are between right and left bounds, then there exists a Northern collision
                    if (playerEnvironmentHandler.GetComponent<Transform>().position.y  + (playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.y * 0.6) / 2.0 < entry.Key.GetComponent<Transform>().position.y - entry.Key.GetComponent<BoxCollider2D>().size.y / 2.0) //player moving North (velocityY > 0)
                    {
                        if (entry.Key.GetComponent<Transform>().position.x + entry.Key.GetComponent<BoxCollider2D>().size.x / 2.0 > playerEnvironmentHandler.GetComponent<Transform>().position.x - playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.x / 2.0 && //if player left bound to left of terrain right bound
                           entry.Key.GetComponent<Transform>().position.x - entry.Key.GetComponent<BoxCollider2D>().size.x / 2.0 < playerEnvironmentHandler.GetComponent<Transform>().position.x + playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.x / 2.0)  //if player right bound is to right of terrain left bound
                        {
                            NorthCollision = true;
                            //Debug.Log("NorthCollision");
                        }
                    }
                    else if (playerEnvironmentHandler.GetComponent<Transform>().position.y - (playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.y * 0.6) / 2.0 > entry.Key.GetComponent<Transform>().position.y + entry.Key.GetComponent<BoxCollider2D>().offset.y + entry.Key.GetComponent<BoxCollider2D>().size.y / 2.0) //player moving South (velocityY < 0) / player lower bound above box upper bound
                    {
                        Debug.Log("Here!");
                        if (entry.Key.GetComponent<Transform>().position.x + entry.Key.GetComponent<BoxCollider2D>().size.x / 2.0 > playerEnvironmentHandler.GetComponent<Transform>().position.x - playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.x / 2.0 && //if player left bound to left of terrain right bound
                           entry.Key.GetComponent<Transform>().position.x - entry.Key.GetComponent<BoxCollider2D>().size.x / 2.0 < playerEnvironmentHandler.GetComponent<Transform>().position.x + playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.x / 2.0)  //if player right bound is to right of terrain left bound
                        {
                            SouthCollision = true;
                            //Debug.Log("SouthCollision");
                        }
                    }
                    if (playerEnvironmentHandler.GetComponent<Transform>().position.x + playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.x / 2.0 < entry.Key.GetComponent<Transform>().position.x - entry.Key.GetComponent<BoxCollider2D>().size.x / 2.0) //player moving East (velocityX > 0) / player to left
                    {
                        if (entry.Key.GetComponent<Transform>().position.y + entry.Key.GetComponent<BoxCollider2D>().offset.y + entry.Key.GetComponent<BoxCollider2D>().size.y / 2.0 > playerEnvironmentHandler.GetComponent<Transform>().position.y - playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.y / 2.0 && //if player south bound to south of terrain north bound
                           entry.Key.GetComponent<Transform>().position.y + entry.Key.GetComponent<BoxCollider2D>().offset.y - entry.Key.GetComponent<BoxCollider2D>().size.y / 2.0 < playerEnvironmentHandler.GetComponent<Transform>().position.y + playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.y / 2.0)  //if player north bound is to north of terrain south bound
                        {
                            EastCollision = true;
                            //Debug.Log("EastCollision");
                        }
                    }
                    else if (playerEnvironmentHandler.GetComponent<Transform>().position.x - playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.x / 2.0 > entry.Key.GetComponent<Transform>().position.x + entry.Key.GetComponent<BoxCollider2D>().size.x / 2.0) //player moving West (velocityX < 0)
                    {
                        if (entry.Key.GetComponent<Transform>().position.y + entry.Key.GetComponent<BoxCollider2D>().offset.y + entry.Key.GetComponent<BoxCollider2D>().size.y / 2.0 > playerEnvironmentHandler.GetComponent<Transform>().position.y - playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.y / 2.0 && //if player south bound to south of terrain north bound
                           entry.Key.GetComponent<Transform>().position.y + entry.Key.GetComponent<BoxCollider2D>().offset.y - entry.Key.GetComponent<BoxCollider2D>().size.y / 2.0 < playerEnvironmentHandler.GetComponent<Transform>().position.y + playerEnvironmentHandler.GetComponent<BoxCollider2D>().size.y / 2.0)  //if player north bound is to north of terrain south bound
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
                    //=====| !!! okay dude, basically figure out if the entity hit by a box is a potential problem. Maybe if it's not currently being touched, since if it were we'd be in a corner and that'd be handled?
                    if (hit.transform.gameObject.tag == "Environment" && 
                        ((hit.transform.position.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().offset.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().size.y / 2.0 > PlayerRigidBody.GetComponent<Transform>().position.y - (PlayerRigidBody.GetComponent<BoxCollider2D>().size.y * 0.6) / 2.0 &&
                           (hit.transform.position.y + hit.transform.gameObject.GetComponent<BoxCollider2D>().offset.y - hit.transform.gameObject.GetComponent<BoxCollider2D>().size.y / 2.0 < PlayerRigidBody.GetComponent<Transform>().position.y + (PlayerRigidBody.GetComponent<BoxCollider2D>().size.y * 0.6) / 2))))
                    {
                        if (playerWillCollide(hit.transform.gameObject.GetComponent<EnvironmentPhysics>().getBottomHeight(), hit.transform.gameObject.GetComponent<EnvironmentPhysics>().getTopHeight(), playerHandler.getPlayerElevation(), playerHandler.getPlayerElevation() + playerHandler.getPlayerHeight()))
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
                        if (playerWillCollide(hit.transform.gameObject.GetComponent<EnvironmentPhysics>().getBottomHeight(), hit.transform.gameObject.GetComponent<EnvironmentPhysics>().getTopHeight(), playerHandler.getPlayerElevation(), playerHandler.getPlayerElevation() + playerHandler.getPlayerHeight()))
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


}
