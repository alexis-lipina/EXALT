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
public class PathfindingAI : EntityAI
{
    //[SerializeField] EntityHandler handler;
    public GameObject target;
    [SerializeField] float detectionRange;
    private Stack<EnvironmentPhysics> path;
    private EntityHandler testhandler;
    private Vector2 _moveDirection;

    private bool pathfound;

    void Start()
    {
        _moveDirection = Vector2.zero;
        pathfound = false;
        //TestAwfulPathfindingSystem();
        navManager.entityChangePositionDelegate += CheckForPathUpdate;
        testhandler = (EntityHandler) handler;
    }

    // Update is called once per frame
    void Update()
    {
        if (path == null || !TargetInDetectionRange() || target.GetComponent<EntityPhysics>().GetCurrentHealth() <= 0)
        {
            //do nothing
            testhandler.SetXYAnalogInput(0, 0);
        }
        else
        {
            // Enemy is pursuing player
            ( (PlayerHandler)target.GetComponent<EntityPhysics>().Handler ).TimeSinceCombat = 0.0f;

            //shortcut for ranged enemy if player is in line of sight
            //if (handler is RangedEnemyHandler && Mathf.Abs(target.GetComponent<EntityPhysics>().GetBottomHeight() - entityPhysics.GetBottomHeight()) < 0.5f) //Old sometimes-inaccurate one
            if (handler is RangedEnemyHandler && target.GetComponent<EntityPhysics>().GetBottomHeight() < entityPhysics.GetBottomHeight() + 2.0f && target.GetComponent<EntityPhysics>().GetTopHeight() > entityPhysics.GetBottomHeight() + 2.0f)
            {
                Vector2 offset = target.transform.position - entityPhysics.transform.position;
                RaycastHit2D[] hits = Physics2D.RaycastAll(entityPhysics.transform.position, offset, offset.magnitude);
                bool isObstruction = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.transform.gameObject.tag == "Environment")
                    {
                        if (hit.transform.gameObject.GetComponent<EnvironmentPhysics>().TopHeight > entityPhysics.GetBottomHeight() + 2f && hit.transform.gameObject.GetComponent<EnvironmentPhysics>().BottomHeight < entityPhysics.GetBottomHeight() + 2f)
                        {
                            isObstruction = true;
                        }
                    }
                }
                if (!isObstruction)
                {
                    offset.Normalize();
                    ((RangedEnemyHandler)testhandler).SetXYAnalogInput(offset.x, offset.y);
                    ((RangedEnemyHandler)testhandler).SetAttackPressed(true);
                    return;
                }
            }
            
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
                        if (testhandler is SwordEnemyHandler) ((SwordEnemyHandler)testhandler).SetJumpPressed(true);
                        else if (testhandler is RangedEnemyHandler) ((RangedEnemyHandler)testhandler).SetJumpPressed(true);
                    }
                    else if (NeedsToJump())//jump to other platform
                    {
                        if (testhandler is SwordEnemyHandler) ((SwordEnemyHandler)testhandler).SetJumpPressed(true);
                        else if (testhandler is RangedEnemyHandler) ((RangedEnemyHandler)testhandler).SetJumpPressed(true);
                    }
                    else
                    {
                        if (testhandler is SwordEnemyHandler) ((SwordEnemyHandler)testhandler).SetJumpPressed(false);
                        else if (testhandler is RangedEnemyHandler) ((RangedEnemyHandler)testhandler).SetJumpPressed(false);
                    }
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
            if (testhandler is SwordEnemyHandler) ((SwordEnemyHandler)testhandler).SetAttackPressed(true);
            else if (testhandler is RangedEnemyHandler) ((RangedEnemyHandler)testhandler).SetAttackPressed(true);
        }
    }

    private bool NeedsToJump()
    {
        Debug.Log("got here");
        //check if bounds of this collider are too far from the other
        Debug.Log(testhandler.GetEntityPhysics().GetCurrentNavObject().name);

        Collider2D[] objectsbelow = Physics2D.OverlapPointAll(testhandler.GetEntityPhysics().transform.position);
        float max = -1000;
        EnvironmentPhysics tempphys = null;

        foreach (Collider2D physobj in objectsbelow)
        {
            if (physobj.GetComponent<EnvironmentPhysics>())
            {
                if (physobj.GetComponent<EnvironmentPhysics>().GetTopHeight() > max && testhandler.GetEntityPhysics().GetTopHeight() > physobj.GetComponent<EnvironmentPhysics>().GetTopHeight())
                {
                    max = physobj.GetComponent<EnvironmentPhysics>().GetTopHeight();
                    tempphys = physobj.GetComponent<EnvironmentPhysics>();
                }
            }
        }

        //continue here, I was gettin somewhere

        return testhandler.GetEntityPhysics().GetBottomHeight() - max > 3;

        /*
        bool _willJump = testhandler.GetEntityPhysics().GetCurrentNavObject().GetComponent<BoxCollider2D>().Distance(path.Peek().GetComponent<BoxCollider2D>()).distance > 2;
        if (_willJump)
        {
            Debug.Log("Separation between : " + testhandler.GetEntityPhysics().GetCurrentNavObject().gameObject.name + " and " + path.Peek().name);
            Debug.Log(testhandler.GetEntityPhysics().GetCurrentNavObject().GetComponent<BoxCollider2D>().Distance(path.Peek().GetComponent<BoxCollider2D>()).distance);
        }

        return _willJump;
        */

        /*
        //                                                               Is the center of the entity physics outside the environment collider,                     
        return !testhandler.GetEntityPhysics().GetCurrentNavObject().GetComponent<BoxCollider2D>().OverlapPoint(testhandler.GetEntityPhysics().transform.position) 
            && Vector2.Dot(_moveDirection, testhandler.GetEntityPhysics().GetCurrentNavObject().GetComponent<BoxCollider2D>().bounds.center - testhandler.GetEntityPhysics().transform.position) < 0;
            */
    }//     and       is the movement vector away from the center of the environment collider

    private void JumpToPlatform()
    {
        
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

    public void SetDetectionRange(float value)
    {
        detectionRange = value;
    }
}