using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows for the creation of constant-width, dynamically modifiable lines which are axis-aligned.
/// Unity's LineRenderer, while functional in some situations, does not make clean-looking, dynamic 
/// lines with sharp angles. This should fix that.
/// </summary>
public class OrthoLine : MonoBehaviour
{
    List<Vector3> _points; //list of all points on a continuous series of line segments


    // Start is called before the first frame update
    void Start()
    {
        _points = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Updates the line mesh to reflect the current list of points
    /// </summary>
    void UpdateLineMesh()
    {

    }



    //================| Data Manipulation Methods

    /// <summary>
    /// Empties the list of points
    /// </summary>
    public void ClearPoints()
    {
        _points.Clear();
    }
    /// <summary>
    /// Adds point to end of list
    /// </summary>
    /// <param name="point"></param>
    public void AddPoint(Vector3 point)
    {
        _points.Add(point);
    }

    /// <summary>
    /// Sets list of points to a pre-built list
    /// </summary>
    /// <param name="points"></param>
    public void SetPoints(List<Vector3> points)
    {
        _points = points;
    }
}
