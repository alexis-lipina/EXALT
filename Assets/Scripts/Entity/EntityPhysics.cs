using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class handles entity interaction with environment, as well as other entities.
/// 
/// TERRAIN TRAVERSAL
/// An area of terrain has a 2D Box Collider associated with it, which defines the area, and the terrain object stores the height of the 
/// terrain surface. Also, within the PlayerHandler is stored a HashMap which stores each terrain object it is currently above. When a 
/// player's collider enters a new terrain collider, a new entry is added to this HashMap, with the terrain object's InstanceID as the key, 
/// and the terrain object's height as the value. When the player collider exits a terrain collider, the entry in the HashMap with the 
/// terrain object's InstanceID as the key is removed.
/// 
/// EXTENSION OF PHYSICSOBJECT
/// This class extends PhysicsObject because I intend all objects which can be expressed as "physical" objects in the game world to inherit 
/// from the same class so adding functionality and interactivity is streamlined. Although PhysicsObject uses the bottomHeight and topHeight,
/// which makes more sense for testing for collision, it makes more sense for these objects to use a height-elevation system, since no matter
/// how an object moves, *generally* the height (from head to toe) is preserved. 
/// </summary>
public class EntityPhysics : DynamicPhysics
{

    [SerializeField] private NavigationManager navManager;
    [SerializeField] private EntityHandler entityHandler;
    
    [SerializeField] private float MaxHP;
    private float currentHP;
    private bool hasBeenHit;


    private KeyValuePair<Vector2, EnvironmentPhysics> lastFootHold;
    private EnvironmentPhysics currentNavEnvironmentObject;

    List<PhysicsObject> EntitiesTouched;

    //private float entityElevation; //replaced with bottomHeight



    public NavigationManager NavManager
    {
        set { navManager = value; }
        get { return navManager; }
    }



    override protected void Awake()
    {
        base.Awake();
        EntitiesTouched = new List<PhysicsObject>();
        currentHP = MaxHP;
        hasBeenHit = false;
    }

    void Update()
    {
        MoveCharacterPosition();
        if (bottomHeight < -18)
        {
            WarpToPlatform();
        }
        UpdateEntityNavigationObject();
        hasBeenHit = false;
        //Entity collision 
        HandleTouchedEntities();

    }

    /// <summary>
    /// Makes sure EntitiesTouched is kept up to date
    /// </summary>
    protected void HandleTouchedEntities()
    {
        Collider2D[] touchingCollider = new Collider2D[10]; //TODO : arbitrary max number of collisions
        gameObject.GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D(), touchingCollider); //TODO : Use layer masking to only get entities
        bool[] indicesToRemove = new bool[EntitiesTouched.Count]; //true if needs to be removed, false if not

        for (int i = 0; i < indicesToRemove.Length; i++)//array initialization
        {
            indicesToRemove[i] = true;
        }

        foreach (Collider2D touchedcollider in touchingCollider) //add new entities, mark entities for deletion with indicesToRemove
        {
            if (touchedcollider != null)
            {
                if (touchedcollider.gameObject.tag == "Friend" || touchedcollider.gameObject.tag == "Enemy")
                {
                    PhysicsObject touchedPhysicsObject = touchedcollider.gameObject.GetComponent<PhysicsObject>();
                    
                    if (!EntitiesTouched.Contains(touchedPhysicsObject)) //handle new objects
                    {
                        //Debug.Log("<color=red>Entering object: </color>" + touchedPhysicsObject.GetInstanceID());
                        EntitiesTouched.Add(touchedPhysicsObject);
                    }
                    else //mark still-touching objects for retention
                    {
                        indicesToRemove[EntitiesTouched.IndexOf(touchedPhysicsObject)] = false;
                        //only sets false those that are still touching
                        //will always be within array index bounds because new list elements are added to the end
                    }
                }
            }
        }
        for (int j = indicesToRemove.Length - 1; j > -1; j--) //regresses back from end, so the changing list size doesnt mess up anything
        {
            if (indicesToRemove[j])
            {
                EntitiesTouched.RemoveAt(j);
            }
        }
    }

    

    /// <summary>
    /// Move in a direction, but get pushed away by entities using extrusion method.
    /// 
    /// Probably should do this before MoveWithCollision (which tests collision with static objects) because 
    /// improper collision with other entities is less egregious than improper collision with static objects
    /// </summary>
    /// <param name="x"></param>
    /// <returns>New velocity adjusted</returns>
    public Vector2 MoveAvoidEntities(Vector2 velocity)
    {
        
        if (EntitiesTouched.Count > 0)
        {
            foreach (PhysicsObject entity in EntitiesTouched)
            {
                if (entity.GetBottomHeight() < this.GetTopHeight() && entity.GetTopHeight() > this.GetBottomHeight())
                {
                    //get the location relative to this objects location
                    Vector2 amountOfForceToAdd = new Vector2(entity.GetComponent<Transform>().position.x - gameObject.GetComponent<Transform>().position.x, entity.GetComponent<Transform>().position.y - gameObject.GetComponent<Transform>().position.y);
                    amountOfForceToAdd.Normalize(); //TODO - just a velocity of 1, might want different force strengths
                    velocity = new Vector2(velocity.x - amountOfForceToAdd.x , velocity.y - amountOfForceToAdd.y);
                }
            }
        }
        return velocity;
    }


    

    public void SavePosition()
    {
        EnvironmentPhysics temp = null;
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched)
        {
            if (entry.Value.GetTopHeight() == bottomHeight)
            {
                temp = entry.Value;
            }
        }
        if (temp != null && temp.IsSavePoint)
        {
            lastFootHold = new KeyValuePair<Vector2, EnvironmentPhysics>(gameObject.GetComponent<Rigidbody2D>().position, temp);
        }
        else
        {
            Debug.Log("ALERT!!! No Save!");
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
        bottomHeight = terrainheight + 0; //maybe have the player fall from a great height to reposition them?
    }

    /// <summary>
    /// Updates which navigation object (environment object) this entity is on
    /// </summary>
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
            if (physobj.GetTopHeight() > max && _objectHeight + bottomHeight > physobj.GetTopHeight())
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
        //Debug.Log(gameObject.GetComponent<Rigidbody2D>().position);
        MoveWithCollision(direction.x * force, direction.y * force);
        Inflict(damage);
        Debug.Log("Ow:" + direction.x * force);
    }

    IEnumerator TakeDamageFlash()
    {
        //Debug.Log("TakeDamageFlash entered");
        entityHandler.JustGotHit();
        _objectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 1);
        for (float i = 0; i < 2; i++)
        {
            //characterSprite.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0);
            _objectSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", new Color(1, 1, 1, 1));
            yield return new WaitForSeconds(0.05f);
            //characterSprite.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
            _objectSprite.GetComponent<SpriteRenderer>().material.SetColor("_MaskColor", new Color(1, 0, 0, 1));
            yield return new WaitForSeconds(0.05f);
        }

        _objectSprite.GetComponent<SpriteRenderer>().material.SetFloat("_MaskOn", 0);
    }

    //===============================================================| getters and setters
    public EnvironmentPhysics GetCurrentNavObject()
    {
        return currentNavEnvironmentObject;
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