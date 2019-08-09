// Draw lines to the connected game objects that a script has.
// If the target object doesnt have any game objects attached
// then it draws a line from the object to (0, 0, 0).

using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[CustomEditor(typeof(EnvironmentPhysics)), CanEditMultipleObjects]
class NavMeshLineRenderer  : Editor
{
    void OnSceneGUI()
    {
        EnvironmentPhysics connectedObjects = target as EnvironmentPhysics;
        if (connectedObjects.getNeighbors() == null)
            return;

        Vector3 center = connectedObjects.transform.position;
        Handles.color = Color.white;
        Handles.CircleHandleCap(0, connectedObjects.gameObject.transform.position + Vector3.down * 3f, Quaternion.identity, 1, EventType.Repaint);
        Handles.color = Color.black;
        Handles.CircleHandleCap(0, connectedObjects.gameObject.transform.position + Vector3.down * 3f, Quaternion.identity, 0.9f, EventType.Repaint);


        Handles.color = Color.magenta;

        //Color[] colors = new Color[] { Color.red, Color.yellow, Color.green, Color.blue, Color.magenta};

        for (int i = 0; i < connectedObjects.getNeighbors().Count; i++)
        {
            GameObject connectedObject = connectedObjects.getNeighbors()[i].gameObject;
            if (connectedObject)
            {
                //Handles.DrawLine(center, connectedObject.transform.position);
                Handles.ArrowHandleCap(0, center + Vector3.down * 3f, Quaternion.LookRotation(connectedObject.transform.position- center), (center- connectedObject.transform.position).magnitude, EventType.Repaint);
            }
            else
            {
                Handles.DrawLine(center, Vector3.zero);
            }
        }
    }
}
#endif