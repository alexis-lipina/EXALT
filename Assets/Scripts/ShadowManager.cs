using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(DynamicPhysics))]


public class ShadowManager : MonoBehaviour
{
    protected const float HEIGHT_TOLERANCE = 0.5f; //in case player is on moving object
    [SerializeField] protected GameObject _shadowPrefab;
    protected DynamicPhysics _physics;
    protected Dictionary<GameObject, KeyValuePair<float, float>> _terrainTouched; //current terrain below player, ref to the one in DynamicPhysics
    //                                  bottom, top

    protected List<List<GameObject>> shadowArray;
    //      i references row, j references column
    /*    0   1   2
     * 0 [ ] [ ] [ ]
     * 1 [ ] [ ] [ ]
     * 2 [ ] [ ] [ ]
     */

    protected Vector2 _currentPlayerPos;
    protected float _currentPlayerElevation;



    //UpdateSlices fields
    protected List<float> horizontalLines;
    protected List<float> verticalLines; //these are x values that define vertical lines passing through the sprite






    void Start ()
    {
        horizontalLines = new List<float>();
        verticalLines = new List<float>();
        shadowArray = new List<List<GameObject>>();
        for (int i = 0; i < 3; i++)
        {
            shadowArray.Add(new List<GameObject>());
            for (int j = 0; j < 3; j++)
            {
                //Debug.Log("bloop");
                shadowArray[i].Add(Instantiate(_shadowPrefab, gameObject.transform));
                shadowArray[i][j].GetComponent<ShadowHandler>().DebugCoordinateCuzImAwfulAtCoding(new Vector2(i, j));
                //shadowArray[i][j].transform.position = new Vector3(transform.position.x + i, transform.position.y + j, transform.position.z);
            }
        }
        _physics = GetComponent<DynamicPhysics>();
        _terrainTouched = _physics.TerrainTouching;
        _currentPlayerPos = transform.position;
        _currentPlayerElevation = _physics.GetBottomHeight() + HEIGHT_TOLERANCE;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if player has moved, update shadows
		if (_currentPlayerPos != (Vector2)transform.position || _currentPlayerElevation != _physics.GetBottomHeight() + HEIGHT_TOLERANCE)
        {
            //Debug.Log("Updating");
            //Profiler.BeginSample();
            UpdateSlices();
            //update members
        }
        else //if any DynamicEnvironmentPhysics object are touched rn
        {
            //TODO
        }
	}


    /// <summary>
    /// Handles effects of changes in level geometry and player position, specifically how each shadow sprite behaves : the rectangle it receives, its height, and the
    /// local data structure which references them all
    /// </summary>
    protected void UpdateSlices()
    {
        Profiler.BeginSample("SEG_1");
        horizontalLines.Clear();
        verticalLines.Clear(); 
        Bounds tempBounds;
        Vector3 tempPos;
        //Bounds entityBounds = new Bounds( GetComponent<Rigidbody2D>().position, GetComponent<BoxCollider2D>().size); //FORCE REFERENCE RIGIDBODY
        Bounds entityBounds = new Bounds( transform.position, GetComponent<BoxCollider2D>().size); //FORCE REFERENCE TRANSFORM
        //Bounds entityBounds = GetComponent<BoxCollider2D>().bounds;
        _terrainTouched = _physics.TerrainTouching; //if implement reset, maybe do this there instead?

        //Debug.Log("TerrainTouched: " + _terrainTouched.Count);

        //Look at terrainTouched, get all lines which are within the sprite/collider, exclude overlaps
        foreach (GameObject obj in _terrainTouched.Keys)
        {
            tempBounds = obj.GetComponent<Collider2D>().bounds;
            tempPos = tempBounds.min;

            if (entityBounds.min.x < tempPos.x && entityBounds.max.x > tempPos.x)
            {
                if (!verticalLines.Contains(tempPos.x))
                {
                    verticalLines.Add(tempPos.x);
                }
            }
            if (entityBounds.min.y < tempPos.y && entityBounds.max.y > tempPos.y)
            {
                if (!horizontalLines.Contains(tempPos.y))
                {
                    horizontalLines.Add(tempPos.y);
                }
            }
            tempPos = tempBounds.max;
            if (entityBounds.min.x < tempPos.x && entityBounds.max.x > tempPos.x)
            {
                if (!verticalLines.Contains(tempPos.x))
                {
                    verticalLines.Add(tempPos.x);
                }
            }
            if (entityBounds.min.y < tempPos.y && entityBounds.max.y > tempPos.y)
            {
                if (!horizontalLines.Contains(tempPos.y))
                {
                    horizontalLines.Add(tempPos.y);
                }
            }
        }
        // Debug.Log(transform.parent.name);
        // Debug.Log("Horizontal lines : " + horizontalLines.Count);
        // Debug.Log("Vertical lines : " + verticalLines.Count);
        // Debug.Log("Keys : " + _terrainTouched.Keys.Count);
        // Debug.Log(transform.position);
        // Debug.Log(GetComponent<Rigidbody2D>().position);
        Profiler.EndSample();
        Profiler.BeginSample("SEG_2");

        

        //Resize shadowarray depending on number of shadows needed
        if (verticalLines.Count + 1 > shadowArray[0].Count || horizontalLines.Count + 1 > shadowArray.Count)
        {
            //Debug.Log("EXPAND");
            shadowArray.Add(new List<GameObject>());
            for (int n = 0; n < shadowArray[0].Count; n++)
            {
                shadowArray[shadowArray.Count - 1].Add(Instantiate(_shadowPrefab, gameObject.transform));
            }
            for (int n = 0; n < shadowArray.Count; n++)
            {
                shadowArray[n].Add(Instantiate(_shadowPrefab, gameObject.transform));
            }
        } 
        //TODO : Add functionality to remove excess
        
        Profiler.EndSample();
        Profiler.BeginSample("SEG_3");

        //determine the dimensions of the 2D array that would be made with that many divisions (vertlines+1, horizlines+1)

        //disables/enables sprite objects depending on whether theyre needed or not
        for (int i = 0; i < shadowArray.Count; i++) //i traverses height of array, j traverses width
        {
            for (int j = 0; j < shadowArray[i].Count; j++)
            {
                //Debug.Log("boop i:" + i + "j:" + j);
                if (i <= verticalLines.Count && j <= horizontalLines.Count)
                {
                    shadowArray[j][i].GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    shadowArray[j][i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }
        Profiler.EndSample();
        Profiler.BeginSample("SEG_4");
        //Add worldspace left and right of entity's bounding box
        horizontalLines.Add(entityBounds.min.y);
        horizontalLines.Add(entityBounds.max.y);
        verticalLines.Add(entityBounds.min.x);
        verticalLines.Add(entityBounds.max.x);
        horizontalLines.Sort();
        verticalLines.Sort();
        Profiler.EndSample();

        //Profiler.EndSample();
        Profiler.BeginSample("Big For-Loop");
        
        //send data to each new slice about its rect
        for (int i = 0; i < horizontalLines.Count - 1; i++)//traverses up the rows
        {
            for (int j = 0; j < verticalLines.Count - 1; j++)//traverses right along the columns
            {
                //Get the worldspace rectangle for the given segment
                Vector4 rect = new Vector4(verticalLines[j], horizontalLines[i], verticalLines[j + 1], horizontalLines[i + 1]);
                //                                                min x,     min y,     max x,     max y
                //Debug.Log(rect);
                //Debug.DrawLine(rect.Key, rect.Value, Color.cyan, 0.01f, false);
                //use the center of the rect and "cast down" to get the height of the segment
                Collider2D highestCollider = null;
                //_terrainTouched.Count;
                //Collider2D[] collidersUnderRect;

                Collider2D[] collidersUnderRect = Physics2D.OverlapPointAll( ( new Vector2(rect.x, rect.y) + new Vector2(rect.z, rect.w) ) / 2.0f); //TODO : Make this nonalloc, hopefully better performance

                //Debug.Log( "Hey! Point : " + (new Vector2(rect.x, rect.y) + new Vector2(rect.z, rect.w)) / 2.0f);
                //Debug.Log("Num Colliders : " + collidersUnderRect.Length);
                for (int k = 0; k < collidersUnderRect.Length; k++)
                {
                    EnvironmentPhysics envtPhysics = collidersUnderRect[k].GetComponent<EnvironmentPhysics>();
                    //get collider with greatest height of all colliders that are less than player height
                    if (envtPhysics)
                    {
                        //Debug.Log("ENVIRONMENT!!!");
                        if (envtPhysics.GetTopHeight() < _physics.GetBottomHeight() + HEIGHT_TOLERANCE) //if collider is less than player height
                        {
                            if (highestCollider == null)
                            {
                                //Debug.Log("W O O O ! ! !");
                                highestCollider = collidersUnderRect[k];
                            }
                            else if (highestCollider.GetComponent<EnvironmentPhysics>().GetTopHeight() < envtPhysics.GetTopHeight())
                            {
                                highestCollider = collidersUnderRect[k];
                            }
                        }
                    }
                }

                //Debug.Log("X : " + i + "Y: " + j);

                if (highestCollider == null)
                {
                    shadowArray[i][j].GetComponent<Renderer>().enabled = false;
                    //Debug.Log("NOT DRAWING " + i + " " + j);
                }
                else
                {
                    //Debug.Log("Shadow Height : " + highestCollider.GetComponent<EnvironmentPhysics>().GetTopHeight());
                    //make 0-1 values of positions based on their location in the colllider

                    //subtract all by left value so left = 0
                    //divide value "between" by current right value

                    float minx = (rect.x - entityBounds.min.x) / (entityBounds.max.x - entityBounds.min.x);
                    float miny = (rect.y - entityBounds.min.y) / (entityBounds.max.y - entityBounds.min.y);
                    float maxx = (rect.z - entityBounds.min.x) / (entityBounds.max.x - entityBounds.min.x);
                    float maxy = (rect.w - entityBounds.min.y) / (entityBounds.max.y - entityBounds.min.y);
                    rect = new Vector4(minx, miny, maxx, maxy); //positions between 0 and 1 to denote where inside the sprite the shadow will crop

                    // min x , min y , max x , max y 
                    //Send shadowArray at index i, j the rectangle to render and the height at which to render.
                    //Debug.Log("Expected Coord : " + new Vector2(i, j));
                    //Debug.Log(horizontalLines.Count - 2 - i);

                    Vector3 newpos = new Vector3(gameObject.transform.position.x, 
                        gameObject.transform.position.y + highestCollider.GetComponent<EnvironmentPhysics>().GetTopHeight(), 
                        //gameObject.transform.position.y - entityBounds.size.y/2 + (miny * entityBounds.size.y) + 0.02f);
                        gameObject.transform.position.y - entityBounds.size.y/2 + (maxy * entityBounds.size.y ) - 0.001f);
                    Debug.DrawLine(new Vector3(transform.position.x - 1f, gameObject.transform.position.y - 0.6f - (-miny * 1.2f)), new Vector3(transform.position.x + 1f, gameObject.transform.position.y - 0.6f - (-miny * 1.2f)));
                    //Debug.Log(shadowArray[i][j]);
                    shadowArray[i][j].GetComponent<ShadowHandler>().UpdateShadow(newpos, highestCollider.GetComponent<EnvironmentPhysics>().GetTopHeight(), rect);
                }
            }
        }
        Profiler.EndSample();

    }


    protected virtual void OnShadowInstantiated()
    {

    }
}
