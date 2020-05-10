using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used by all agents to pathfind around the scene.
/// </summary>
public class NavigationManager : MonoBehaviour
{
    List<int> visited;
    public delegate void UpdatePathfindDelegate(GameObject changedEntity, EnvironmentPhysics newDestination);
    public UpdatePathfindDelegate entityChangePositionDelegate;


    public Stack<Vector2> FindCoordinatePath(EnvironmentPhysics start, EnvironmentPhysics destination)
    {
        
        visited = new List<int>();
        //start at "start" physics object
        //Stack<Vector2> path = SearchPath(start, destination);
        //Stack<Vector2> path = NewSearchPath(start, destination);
        Stack<Vector2> path = AStarSearch(start, destination);
        if (path == null || path.Count == 0)
        {
            Debug.Log("Pathfind failed!");
        }
        Debug.Log(path);
        return path;        
    }

    public Stack<EnvironmentPhysics> FindPath(EnvironmentPhysics start, EnvironmentPhysics destination)
    {
        visited = new List<int>();
        Stack<EnvironmentPhysics> path = AStarSearchExceptItReturnsEnvironmentObjects(start, destination);
        if (path == null || path.Count == 0)
        {
            //comment this in for navmap debug
            //Debug.Log("Valid path not found"); 
        }
        //Debug.Log(path);
        return path;
    }
    /*
    private Stack<Vector2> SearchPath(EnvironmentPhysics start, EnvironmentPhysics destination)
    {
        Debug.Log("Searching in " + start);
        List<EnvironmentPhysics> neighbors = start.getNeighbors();
        Stack<Vector2> path = new Stack<Vector2>();
        

        foreach(EnvironmentPhysics obj in neighbors) //visit each neighbor node
        {
            
            if (!visited.Contains(obj.GetInstanceID()))// if that node hasnt already been visited
            {
                visited.Add(obj.GetInstanceID());
                if (obj == destination) //if neighbor node is a destination, get neighbor and this and return
                {
                    path.Push(obj.transform.position);
                    path.Push(start.transform.position);
                    return path;
                }

                path = SearchPath(obj, destination); // initiate new search within
                if (path.Count > 0) //if valid path (path found destination)
                {
                    path.Push(start.gameObject.transform.position); // add this node to the path
                    return path;
                }
                
            }
            path = new Stack<Vector2>();
        }
        
        return path;

    }
    */
    private Stack<Vector2> NewSearchPath(EnvironmentPhysics start, EnvironmentPhysics destination)
    {
        //Debug.Log("Searching in " + start);
        List<NavEdge> navedges = start.getNavEdges();
        Stack<Vector2> path = new Stack<Vector2>();
        //Debug.Log(navedges.Count);

        foreach (NavEdge edge in navedges) //visit each neighbor node
        {
            //Debug.Log("!!");
            if (!visited.Contains(edge.EnvironmentObject.gameObject.GetInstanceID()))// if that node hasnt already been visited
            {
                visited.Add(edge.EnvironmentObject.gameObject.GetInstanceID());
                if (edge.EnvironmentObject == destination) //if neighbor node is a destination, get neighbor and this and return
                {
                    path.Push(edge.Position);
                    path.Push(start.transform.position);
                    return path;
                }

                path = NewSearchPath(edge.EnvironmentObject, destination); // initiate new search within
                if (path.Count > 0) //if valid path (path found destination)
                {
                    path.Push(start.gameObject.transform.position); // add this node to the path
                    return path;
                }

            }
            path = new Stack<Vector2>();
        }

        return path;

    }

    private Stack<Vector2> AStarSearch(EnvironmentPhysics start, EnvironmentPhysics destination)
    {
        if (start == null || destination == null)
        {
            return new Stack<Vector2>();
        }
        Dictionary<EnvironmentPhysics, float> frontier = new Dictionary<EnvironmentPhysics, float>(); //object, priority
        Dictionary<EnvironmentPhysics, EnvironmentPhysics> cameFrom = new Dictionary<EnvironmentPhysics, EnvironmentPhysics>();//leaf, source
        Dictionary<EnvironmentPhysics, float> costSoFar = new Dictionary<EnvironmentPhysics, float>();//object, priority
        List<NavEdge> neighbors = new List<NavEdge>();
        frontier.Add(start, 0);
        cameFrom.Add(start, null);
        costSoFar.Add(start, 0);

        EnvironmentPhysics current = null;
        //Debug.Log("About to die!");
        int iterations = 0;
        while(frontier.Count != 0 && iterations < 100) // TODO : Arbitrary cap on iterations
        {
            //Debug.Log("bloop");
            //get least expensive frontier, set to current
            float min = float.PositiveInfinity;
            foreach(KeyValuePair<EnvironmentPhysics, float> entry in frontier)
            {
                if (entry.Value < min)
                {
                    current = entry.Key;
                    min = entry.Value;
                }
            }
            //Debug.Log(min);


            //get neighbors of current
            neighbors = current.getNavEdges();
            if (current == destination) break; //PATH FOUND!!!
            foreach(NavEdge edge in neighbors)
            {
                //sets cost equal to cumulative cost of previous node plus cost to reach next node
                float tempcost = costSoFar[current] + edge.Distance;
                if (!costSoFar.ContainsKey(edge.EnvironmentObject) || tempcost < costSoFar[edge.EnvironmentObject] )
                {
                    costSoFar[edge.EnvironmentObject] = tempcost;
                    float tempPriority = tempcost + Vector2.Distance(edge.EnvironmentObject.gameObject.transform.position, destination.transform.position);
                    frontier[edge.EnvironmentObject] = tempPriority;
                    cameFrom[edge.EnvironmentObject] = current;
                }
            }
            frontier.Remove(current);
            iterations++;
        }
        Stack<Vector2> path = new Stack<Vector2>();
        if (iterations > 18) return null;
        iterations = 0;
        while (current != start && iterations < 20)//while current thing isnt start
        {
            //Debug.Log("nooo");
            path.Push(current.transform.position);
            current = cameFrom[current];
            iterations++;
        }
        if (iterations > 18) return null;
        return path;
    }


    /// <summary>
    /// This is the good one
    /// </summary>
    /// <param name="start"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    private Stack<EnvironmentPhysics> AStarSearchExceptItReturnsEnvironmentObjects(EnvironmentPhysics start, EnvironmentPhysics destination)
    {
        if (start == null || destination == null)
        {
            return new Stack<EnvironmentPhysics>();
        }
        Dictionary<EnvironmentPhysics, float> frontier = new Dictionary<EnvironmentPhysics, float>(); //object, priority
        Dictionary<EnvironmentPhysics, EnvironmentPhysics> cameFrom = new Dictionary<EnvironmentPhysics, EnvironmentPhysics>();//leaf, source
        Dictionary<EnvironmentPhysics, float> costSoFar = new Dictionary<EnvironmentPhysics, float>();//object, priority
        List<NavEdge> neighbors = new List<NavEdge>();
        frontier.Add(start, 0);
        cameFrom.Add(start, null);
        costSoFar.Add(start, 0);

        EnvironmentPhysics current = null;
        int iterations = 0;
        while (frontier.Count != 0 && iterations < 30)
        {
            //Debug.Log("bloop:" + iterations);

            //get least expensive frontier, set to current
            float min = float.PositiveInfinity;
            foreach (KeyValuePair<EnvironmentPhysics, float> entry in frontier)
            {
                if (entry.Value < min)
                {
                    current = entry.Key;
                    min = entry.Value;
                }
            }
            //Debug.Log(min);


            //get neighbors of current
            neighbors = current.getNavEdges();
            if (current == destination) break; //PATH FOUND!!!
            foreach (NavEdge edge in neighbors)
            {
                //sets cost equal to cumulative cost of previous node plus cost to reach next node
                float tempcost = costSoFar[current] + edge.Distance;
                if (!costSoFar.ContainsKey(edge.EnvironmentObject) || tempcost < costSoFar[edge.EnvironmentObject])
                {
                    costSoFar[edge.EnvironmentObject] = tempcost;
                    float tempPriority = tempcost + Vector2.Distance(edge.EnvironmentObject.gameObject.transform.position, destination.transform.position);
                    frontier[edge.EnvironmentObject] = tempPriority;
                    cameFrom[edge.EnvironmentObject] = current;
                }
            }
            frontier.Remove(current);
            iterations++;
        }
        Stack<EnvironmentPhysics> path = new Stack<EnvironmentPhysics>();
        if (iterations > 30) return null;
        iterations = 0;
        while (current != start && iterations < 30)//while current thing isnt start
        {
            //Debug.Log("nooo");
            path.Push(current);
            current = cameFrom[current];
            iterations++;
        }
        if (iterations > 30) return null;
        return path;
    }
}
