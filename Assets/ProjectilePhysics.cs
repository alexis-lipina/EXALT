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
   

    [SerializeField] private bool canDeflect;
    [SerializeField] private bool isAffectedByGravity;
    [SerializeField] private bool canPenetrate;
    [SerializeField] private bool canBeDamaged;



    private Rigidbody2D bulletRigidBody;

    
    List<PhysicsObject> EntitiesTouched;


    override protected void Awake()
    {
        base.Awake();
        EntitiesTouched = new List<PhysicsObject>();
        
    }

	
	// Update is called once per frame
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
        if (other.gameObject.tag == "Environment" && !TerrainTouching.ContainsKey(other.gameObject))
        {
            TerrainTouching.Add(other.gameObject, other.gameObject.GetComponent<EnvironmentPhysics>().getHeightData());
        }
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


    






}
