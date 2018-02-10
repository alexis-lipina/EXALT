using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHandler : MonoBehaviour
{
    [SerializeField] private GameObject entityCharacterSprite;
    [SerializeField] private GameObject entityPhysicsObject;
    [SerializeField] private GameObject entityEnvironmentHandlerObject;
    [SerializeField] private GameObject firstShadow;
    [SerializeField] private float entitySpriteZOffset;

    private EntityColliderScript entityCollider;
    private Rigidbody2D entityRigidBody;


    enum entityState { IDLE, RUN, JUMP };

    private entityState CurrentState;

    private float entityElevation;
    private float entityHeight;
    private float entityRunSpeed;
    private float xInput;
    private float yInput;
    private float JumpImpulse;
    private float ZVelocity;

    Dictionary<int, EnvironmentPhysics> TerrainTouched;
    //         ^ instanceID       ^bottom   ^ topheight
    Dictionary<int, KeyValuePair<float, GameObject>> Shadows;
    //          ^ instanceID       ^ height    ^ shadowobject 


    void Start()
    {

        CurrentState = entityState.IDLE;
        entityElevation = 0;
        entityHeight = 3;
        JumpImpulse = 0.6f;
        entityRigidBody = entityPhysicsObject.GetComponent<Rigidbody2D>();
        TerrainTouched = new Dictionary<int, EnvironmentPhysics>();
        //TerrainTouched.Add(666, new KeyValuePair<float, float>(0.0f, -20.0f));
        entityCollider = entityPhysicsObject.GetComponent<EntityColliderScript>();
        Shadows = new Dictionary<int, KeyValuePair<float, GameObject>>();
        //Shadows.Add(firstShadow.GetInstanceID(), new KeyValuePair<float, GameObject>(0.0f, firstShadow));


    }


    void Update()
    {
        //---------------------------| Manage State Machine |
        switch (CurrentState)
        {
            case (entityState.IDLE):
                entityIdle();
                break;
            case (entityState.RUN):
                entityRun();
                break;
            case (entityState.JUMP):
                entityJump();
                break;
        }

        //updateHeight();
        moveCharacterPosition();
        //reset button presses
        //FollowingCamera.transform.position = new Vector3(entityCharacterSprite.transform.position.x, entityCharacterSprite.transform.position.y, -100);
    }

    //================================================================================| STATE METHODS |
    private void entityIdle()
    {
        if(xInput > 0 || yInput > 0)
        {
            CurrentState = entityState.RUN;
        }
        //do stuff
    }
    private void entityRun()
    {
        moveCharacterPositionPhysics();

        //-------| Z Azis Traversal 
        float maxheight = -20;
        foreach (KeyValuePair<int, EnvironmentPhysics> entry in TerrainTouched) // handles falling if entity is above ground
        {
            if (entry.Value.getTopHeight() > maxheight && entityHeight + entityElevation > entry.Value.getTopHeight()) maxheight = entry.Value.getTopHeight();
        }
        if (entityElevation > maxheight)
        {
            ZVelocity = 0;
            CurrentState = entityState.JUMP;
        }
        else
        {
            entityElevation = maxheight;
        }
        //------------------------------------------------| STATE CHANGE
        //Debug.Log("X:" + xInput + "Y:" + yInput);
        if (Mathf.Abs(xInput) < 0.1 && Mathf.Abs(yInput) < 0.1)
        {
            //Debug.Log("RUN -> IDLE");
            CurrentState = entityState.IDLE;
        }
    }
    private void entityJump()
    {
        //do even more stuff
    }
    /// <summary>
    /// 
    /// </summary>
    private void moveCharacterPositionPhysics()
    {
        entityCollider.MoveWithCollision(xInput * 20f * Time.deltaTime, yInput * 20f * Time.deltaTime);
        //entityRigidBody.MovePosition(new Vector2(entityRigidBody.position.x + xInput * 0.3f, entityRigidBody.position.y + yInput * 0.3f));
    }

    /// <summary>
    /// Changes position of character image as entity moves. 
    /// </summary>
    private void moveCharacterPosition()
    {
        //                           X: Horizontal position                    Y: Vertical position - accounts for height and depth               Z: Depth - order of object draw calls
        Vector3 coords = new Vector3(entityPhysicsObject.transform.position.x, entityPhysicsObject.transform.position.y + entitySpriteZOffset + entityElevation, entityPhysicsObject.transform.position.y + entityEnvironmentHandlerObject.GetComponent<BoxCollider2D>().offset.y - entityEnvironmentHandlerObject.GetComponent<BoxCollider2D>().size.y / 2 + 0.4f);
        entityCharacterSprite.transform.position = coords;
        //entityCharacterSprite.transform.position = new Vector3(entityCharacterSprite.transform.position.x, entityCharacterSprite.transform.position.y, entityPhysicsObject.transform.position.y + entityPhysicsObject.GetComponent<BoxCollider2D>().offset.y + entityPhysicsObject.GetComponent<BoxCollider2D>().size.y / 2);
        //Vector2 tempvect = new Vector2(xInput, yInput);



        //move shadows
        foreach (KeyValuePair<int, KeyValuePair<float, GameObject>> entry in Shadows)
        {
            entry.Value.Value.transform.position = new Vector3(entityPhysicsObject.transform.position.x, entityPhysicsObject.transform.position.y + entry.Value.Key, entityPhysicsObject.transform.position.y + entityEnvironmentHandlerObject.GetComponent<BoxCollider2D>().offset.y - entityEnvironmentHandlerObject.GetComponent<BoxCollider2D>().size.y / 2 + 0.4f);
        }
    }

    //================================================================================| SETTERS FOR INPUT |
    

    public float getEntityElevation()
    {
        return entityElevation;
    }
    public float getentityHeight()
    {
        return entityHeight;
    }



    public void setXYAnalogInput(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    public void addTerrainTouched(int terrainInstanceID, EnvironmentPhysics environment)
    {
        
        if (TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched already contains ID " + terrainInstanceID);
        }
        else
        {
            TerrainTouched.Add(terrainInstanceID, environment);
            Shadows.Add(terrainInstanceID, new KeyValuePair<float, GameObject>(environment.getTopHeight(), Instantiate(firstShadow, this.transform.parent)));
            Shadows[terrainInstanceID].Value.SetActive(true);
            Shadows[terrainInstanceID].Value.transform.position = new Vector3(entityPhysicsObject.transform.position.x, entityPhysicsObject.transform.position.y + environment.getTopHeight(), environment.getTopHeight());
        }

    }
    public void removeTerrainTouched(int terrainInstanceID)
    {
        if (!TerrainTouched.ContainsKey(terrainInstanceID)) //Debug lines
        {
            Debug.Log("TerrainTouched does not contain ID " + terrainInstanceID);
        }
        TerrainTouched.Remove(terrainInstanceID);
        Destroy(Shadows[terrainInstanceID].Value);
        Shadows.Remove(terrainInstanceID);
    }
}
