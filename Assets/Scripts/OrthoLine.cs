using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

/// <summary>
/// Allows for the creation of constant-width, dynamically modifiable lines which are axis-aligned.
/// Unity's LineRenderer, while functional in some situations, does not make clean-looking, dynamic 
/// lines with sharp angles. This should fix that.
/// </summary>
public class OrthoLine : MonoBehaviour
{
    List<Vector3> _points; //list of all points on a continuous series of line segments
    Vector3 test_Start;
    Vector3 test_End;
    [SerializeField] private float _lineThickness;

    public float LineThickness
    {
        get { return _lineThickness; }
        set { _lineThickness = value; }
    }

    private List<Vector3> verts;
    private List<int> tris;



    // Start is called before the first frame update
    void Start()
    {
        verts = new List<Vector3>();
        tris = new List<int>();
        GetComponent<MeshFilter>().mesh = new Mesh();
        _points = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Updates the line mesh to reflect the current list of points
    /// </summary>
    public void UpdateLineMesh()
    {

        //LEGACY - Just draws a line from start to end, only renders facing the camera if player aims to the right
        /*
        Debug.Log("Drawing Mesh!");
        Mesh linemesh = GetComponent<MeshFilter>().mesh;
        linemesh.Clear();

        //create vertices above and below both start and end
        Vector3[] verts = new Vector3[4];
        verts[0] = test_Start + new Vector3(0, -1, 0);
        verts[1] = test_Start + new Vector3(0, 1, 0);
        verts[2] = test_End + new Vector3(0, 1, 0);
        verts[3] = test_End + new Vector3(0, -1, 0);

        //create tris with said vertices
        int[] tris = { 0, 1, 3, 1, 2, 3 };

        //assign normals to orient toward the camera


        //update mesh
        linemesh.vertices = verts;
        linemesh.triangles = tris;

        GetComponent<MeshFilter>().mesh = linemesh;
        */



        //Render just the "points"

        Mesh linemesh = GetComponent<MeshFilter>().mesh;
        linemesh.Clear();
        /*
        verts.Clear();
        tris.Clear();
       */
        verts = new List<Vector3>();
        tris = new List<int>();



        //these have been replaced with lists
        //Vector3[] verts = new Vector3[_points.Count * 4]; //one quad per point
        //int[] tris = new int[_points.Count * 6]; //2 tris per point, 3 verts per tri

        float offset = _lineThickness * 0.5f;
        for (int i = 0; i < _points.Count; i++)
        {
            //legacy from when used arrays rather than lists
            /*
            verts[4*i]   = _points[i] + new Vector3(-offset, -offset, 0);
            verts[4*i+1] = _points[i] + new Vector3(-offset, offset, 0);
            verts[4*i+2] = _points[i] + new Vector3(offset, offset, 0);
            verts[4*i+3] = _points[i] + new Vector3(offset, -offset, 0);
            tris[6*i]   = i; //0 1 3 1 2 3
            tris[6*i+1] = i+1; 
            tris[6*i+2] = i+3; 
            tris[6*i+3] = i+1; 
            tris[6*i+4] = i+2; 
            tris[6*i+5] = i+3;
            */
            verts.Add(_points[i] + new Vector3(-offset, -offset, 0));
            verts.Add(_points[i] + new Vector3(-offset, offset, 0));
            verts.Add(_points[i] + new Vector3(offset, offset, 0));
            verts.Add(_points[i] + new Vector3(offset, -offset, 0));
            AddRect(4*i, 4*i + 1, 4*i + 2, 4*i + 3);
            
            //Add connection-rect to previous
            if (i>0)
            {
                //check which direction to draw the rect - affects which verts are used

                if (_points[i-1].x > _points[i].x) // draw to the right [3, 2, -3, -4]
                {
                    AddRect(i * 4 + 3, i * 4 + 2, i * 4 - 3, i * 4 - 4);
                }
                else if (_points[i-1].x < _points[i].x) //draw to the left [-1, -2, 1, 0]
                {
                    AddRect(i * 4 - 1, i * 4 - 2, i * 4 + 1, i * 4);
                }
                else if (_points[i-1].y > _points[i].y) //draw up [1, -4, -1, 2]
                {
                    AddRect(i * 4 + 1, i * 4 - 4, i * 4 - 1, i * 4 + 2);
                }
                else //draw down [-3, 0, 3, -2]
                {
                    AddRect(i * 4 - 3, i * 4, i * 4 + 3, i * 4 - 2);
                }
            }
            
        }

        linemesh.vertices = verts.ToArray();
        linemesh.triangles = tris.ToArray();

        //Debug.Log("Number of verts: " + verts.Count);
        //Debug.Log("Number of tris: " + tris.Count);

        GetComponent<MeshFilter>().mesh = linemesh;

    }

    private void AddTriangle(int index1, int index2, int index3)
    {
        tris.Add(index1);
        tris.Add(index2);
        tris.Add(index3);
    }
    private void AddRect(int bottomleft, int topleft, int topright, int bottomright)
    {
        AddTriangle(bottomleft, topleft, bottomright);
        AddTriangle(topleft, topright, bottomright);
    }

    

    public void TEST_SetStart(Vector3 start)
    {
        test_Start = start;
    }

    public void TEST_SetEnd(Vector3 end)
    {
        test_End = end;
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
