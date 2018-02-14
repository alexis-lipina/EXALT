using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used by all agents to pathfind around the scene.
/// </summary>
public class NavigationManager : MonoBehaviour
{
    List<int> visited;



    public Stack<Vector2> FindPath(EnvironmentPhysics start, EnvironmentPhysics destination)
    {
        visited = new List<int>();
        //start at "start" physics object
        Stack<Vector2> path = SearchPath(start, destination);
        if (path.Count == 0)
        {
            Debug.Log("Pathfind failed!");
        }
        Debug.Log(path);
        return path;
    }

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


}
