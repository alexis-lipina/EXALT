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
public class EntityAI : MonoBehaviour
{
    [SerializeField] EntityHandler handler;
    [SerializeField] GameObject target;
    [SerializeField] GameObject punchingBag; // physics object
    [SerializeField] NavigationManager navManager;
    [SerializeField] EnvironmentPhysics testStart;
    [SerializeField] EnvironmentPhysics testEnd;
    [SerializeField] float detectionRange;

    private Stack<Vector2> coordpath;
    private Stack<EnvironmentPhysics> path;
    private bool pathfound;
	void Start()
    {
        pathfound = false;
        //TestAwfulPathfindingSystem();
        navManager.entityChangePositionDelegate += CheckForPathUpdate;
    }

	// Update is called once per frame
	void Update ()
    {
        //MoveTowardPlayer();
        if (pathfound == false)
        {
            TestAwfulPathfindingSystem();
            pathfound = true;
        }

        //----Test of god-awful pathfinding system
        if(path == null || !TargetInDetectionRange())
        {
            //do nothing
            handler.setXYAnalogInput(0, 0);
        }
        else
        {
            if (path.Count == 0)
            {
                Debug.Log("Moving toward target!");
                MoveTowardTarget();
            }
            else
            {
                //peek, test if collider is overlapping other
                //if overlap, pop and exit
                //if no overlap, movetowardpoint
                EnvironmentPhysics dest = path.Peek();
                if (punchingBag.GetComponent<BoxCollider2D>().IsTouching(dest.GetComponent<BoxCollider2D>()))
                {
                    path.Pop();
                }
                else
                {
                    //Debug.Log(dest);
                    MoveTowardPoint(new Vector2(dest.transform.position.x, dest.transform.position.y + dest.GetComponent<BoxCollider2D>().offset.y));
                    if (path.Peek().getTopHeight() > handler.getEntityPhysics().GetEntityElevation()) //Needs to jump
                    {
                        handler.gameObject.GetComponent<PunchingBagHandler>().SetJumpPressed(true);
                    }
                    else { handler.gameObject.GetComponent<PunchingBagHandler>().SetJumpPressed(false); }
                }
            }
        }
    }

    //=====================| AI Methods
    private void MoveTowardTarget()
    {
        Vector2 direction = new Vector2(target.transform.position.x - punchingBag.transform.position.x, target.transform.position.y - punchingBag.transform.position.y);
        if (direction.magnitude > 2)
        {
            handler.setXYAnalogInput(direction.normalized.x, direction.normalized.y);
        }
        else
        {
            handler.setXYAnalogInput(0, 0);
        }
    }

    private void TestAwfulPathfindingSystem()
    {
        path = navManager.FindPath(testStart, testEnd);
    }

    private void MoveTowardPoint(Vector2 destination)
    {
        Vector2 direction = new Vector2(destination.x - punchingBag.transform.position.x, destination.y - punchingBag.transform.position.y);
        handler.setXYAnalogInput(direction.normalized.x, direction.normalized.y);
    }

    // =================| Update path if target changes touched nav
    private void CheckForPathUpdate(GameObject obj, EnvironmentPhysics newDestination)
    {
        if (obj == target)
        {
            Debug.Log("Success!!!");
            //recalculate path
            path = navManager.FindPath(handler.getEntityPhysics().getCurrentNavObject(), newDestination);
        }
    }

    private bool TargetInDetectionRange()
    {
        return Vector2.Distance(target.transform.position, punchingBag.transform.position) < detectionRange;
    }

}
