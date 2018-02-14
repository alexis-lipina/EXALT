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
    [SerializeField] GameObject player;
    [SerializeField] GameObject punchingBag; // physics object
    [SerializeField] NavigationManager navManager;
    [SerializeField] EnvironmentPhysics testStart;
    [SerializeField] EnvironmentPhysics testEnd;

    private Stack<Vector2> path;

	void Start()
    {
        TestAwfulPathfindingSystem();
    }

	// Update is called once per frame
	void Update ()
    {
        //MoveTowardPlayer();


        //----Test of god-awful pathfinding system
        if(path.Count == 0)
        {
            //do nothing
            handler.setXYAnalogInput(0, 0);
        }
        else
        {
            
            //peek, test if collider is overlapping point
            //if overlap, pop and exit
            //if no overlap, movetowardpoint
            Vector2 dest = path.Peek();
            if ( punchingBag.GetComponent<BoxCollider2D>().OverlapPoint(dest) )
            {
                path.Pop();
            }
            else
            {
                Debug.Log(dest);
                MoveTowardPoint(dest);
            }
        }
    }

    //=====================| AI Methods
    private void MoveTowardPlayer()
    {
        Vector2 direction = new Vector2(player.transform.position.x - punchingBag.transform.position.x, player.transform.position.y - punchingBag.transform.position.y);
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


}
