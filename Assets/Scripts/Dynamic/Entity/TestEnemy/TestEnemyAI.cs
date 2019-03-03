using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity AI Class
/// This class exerts control over the EntityHandler class in 
/// much the same way as the InputHandler exerts control over
/// the PlayerHandler class, except in this case rather than 
/// receiving user input, parsing it and passing it to the 
/// handler class, this one includes (or, will include) a
/// simple artificial intelligence which determines what input
/// to send to the EntityHandler.
/// 
/// In future implementations, this might instead serve as a middleman between
/// an "overlord" AI, but who knows. Who knows indeed.
/// </summary>
public class TestEnemyAI : EntityAI
{
    //[SerializeField] EntityHandler handler;
    public GameObject target;
    [SerializeField] float detectionRange;
    private Stack<Vector2> coordpath;
    private Stack<EnvironmentPhysics> path;
    private TestEnemyHandler testhandler;
    private Vector2 _moveDirection;

    private bool pathfound;

    void Start()
    {
        _moveDirection = Vector2.zero;
        pathfound = false;
        //TestAwfulPathfindingSystem();
        navManager.entityChangePositionDelegate += CheckForPathUpdate;
        testhandler = (TestEnemyHandler) handler;
    }

    // Update is called once per frame
    void Update()
    {
        //----Test of god-awful pathfinding system
        if (path == null || !TargetInDetectionRange())
        {
            //do nothing
            testhandler.SetXYAnalogInput(0, 0);
        }
        else
        {
            if (path.Count == 0)
            {
                //Debug.Log("Moving toward target!");
                MoveToAttackTarget();
            }
            else
            {
                //peek, test if collider is overlapping other
                //if overlap, pop and exit
                //if no overlap, movetowardpoint
                EnvironmentPhysics dest = path.Peek();
                //if (entityPhysics.GetComponent<BoxCollider2D>().IsTouching(dest.GetComponent<BoxCollider2D>()))

                // below checks if contains both bottom-left and top-right - doesnt work for objects which are smaller than the size of the entity
                //if (dest.GetComponent<BoxCollider2D>().OverlapPoint(entityPhysics.GetComponent<BoxCollider2D>().bounds.min) && dest.GetComponent<BoxCollider2D>().OverlapPoint(entityPhysics.GetComponent<BoxCollider2D>().bounds.max))
                if (dest.GetComponent<BoxCollider2D>().OverlapPoint(entityPhysics.GetComponent<BoxCollider2D>().bounds.center))
                { 
                        path.Pop();
                }
                else
                {
                    //Debug.Log(dest);
                    MoveTowardPoint(new Vector2(dest.transform.position.x, dest.transform.position.y + dest.GetComponent<BoxCollider2D>().offset.y));
                    if (path.Peek().GetTopHeight() > handler.GetEntityPhysics().GetObjectElevation() + 1) //Needs to jump
                    {
                        testhandler.gameObject.GetComponent<TestEnemyHandler>().SetJumpPressed(true);
                    }
                    else { testhandler.gameObject.GetComponent<TestEnemyHandler>().SetJumpPressed(false); }
                }
            }
        }
    }

    public void SetPath(EnvironmentPhysics source)
    {
        Debug.Log(source);
        Debug.Log(target.GetComponent<EntityPhysics>().GetCurrentNavObject());
        path = navManager.FindPath(source, target.GetComponent<EntityPhysics>().GetCurrentNavObject());
    }

    //=====================| AI Methods
    private void MoveTowardTarget()
    {
        Vector2 direction = new Vector2(target.transform.position.x - entityPhysics.transform.position.x, target.transform.position.y - entityPhysics.transform.position.y);
        if (direction.magnitude > 2)
        {
            testhandler.SetXYAnalogInput(direction.normalized.x, direction.normalized.y);
        }
        else
        {
            testhandler.SetXYAnalogInput(0, 0);
        }
    }

    private void MoveTowardPoint(Vector2 destination)
    {
        Vector2 direction = new Vector2(destination.x - entityPhysics.transform.position.x, destination.y - entityPhysics.transform.position.y);
        _moveDirection = Vector2.Lerp(_moveDirection, direction.normalized, 0.2f);
        //Debug.Log("<color=blue>HERE</color>");
        testhandler.SetXYAnalogInput(_moveDirection.x, _moveDirection.y);
    }

    private void MoveToAttackTarget()
    {

        Vector2 direction = new Vector2(target.transform.position.x - entityPhysics.transform.position.x, target.transform.position.y - entityPhysics.transform.position.y);
        if (direction.magnitude > 4)
        {
            testhandler.SetXYAnalogInput(direction.normalized.x, direction.normalized.y);
        }
        else
        {
            testhandler.SetXYAnalogInput(direction.normalized.x, direction.normalized.y);
            testhandler.SetAttackPressed(true);
        }
    }

    // =================| Update path if target changes touched nav
    private void CheckForPathUpdate(GameObject obj, EnvironmentPhysics newDestination)
    {
        if (obj == target)
        {
            //Debug.Log("Success!!!");
            //recalculate path
            path = navManager.FindPath(handler.GetEntityPhysics().GetCurrentNavObject(), newDestination);
        }
    }

    private bool TargetInDetectionRange()
    {
        return Vector2.Distance(target.transform.position, entityPhysics.transform.position) < detectionRange;
    }
}