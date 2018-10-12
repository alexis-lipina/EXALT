using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(DynamicPhysics))]


public class ShadowManager : MonoBehaviour
{
    [SerializeField] private GameObject _shadowPrefab;
    private DynamicPhysics _physics;
    Dictionary<GameObject, KeyValuePair<float, float>> _terrainTouched; //current terrain below player, ref to the one in DynamicPhysics
    //                                  bottom, top
    protected Dictionary<int, KeyValuePair<float, GameObject>> Shadows;

    protected List<List<GameObject>> shadowArray;
    /*    0   1   2
     * 0 [ ] [ ] [ ]
     * 1 [ ] [ ] [ ]
     * 2 [ ] [ ] [ ]
     */

    Vector2 _currentPlayerPos;
    float _currentPlayerElevation;



    //UpdateSlices fields
    List<float> horizontalLines;
    List<float> verticalLines; //these are x values that define vertical lines passing through the sprite






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
        _currentPlayerElevation = _physics.GetBottomHeight();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if player has moved, update shadows
		if (_currentPlayerPos != (Vector2)transform.position || _currentPlayerElevation != _physics.GetBottomHeight())
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
    void UpdateSlices()
    {
        Profiler.BeginSample("Other Stuff");
        horizontalLines.Clear();
        verticalLines.Clear(); 
        Bounds tempBounds;
        Vector3 tempPos;
        Bounds entityBounds = GetComponent<Collider2D>().bounds;
        //Look at terrainTouched, get all lines which are within the sprite/collider, exclude overlaps
        foreach(GameObject obj in _terrainTouched.Keys)
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
        //Debug.Log(_terrainTouched.Count);
        
        //Debug.Log(horizontalLines.Count);
        //Debug.Log(verticalLines.Count);
        

        //determine the dimensions of the 2D array that would be made with that many divisions (vertlines+1, horizlines+1)

        //resize current array, disable/enable elements, or something else to accommodate the new dimensions if they are different than the current ones
        for (int i = 0; i < shadowArray.Count; i++) //i traverses height of array, j traverses width
        {
            for (int j = 0; j < shadowArray[i].Count; j++)
            {
                if (i <= verticalLines.Count && j <= horizontalLines.Count)//enable
                {
                    shadowArray[j][i].GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    shadowArray[j][i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }
        //Add worldspace left and right of entity's bounding box
        horizontalLines.Add(entityBounds.min.y);
        horizontalLines.Add(entityBounds.max.y);
        verticalLines.Add(entityBounds.min.x);
        verticalLines.Add(entityBounds.max.x);
        horizontalLines.Sort();
        verticalLines.Sort();

        Profiler.EndSample();
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
                        if (envtPhysics.GetTopHeight() < _physics.GetBottomHeight() + 0.25f) //if collider is less than player height
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
                if (highestCollider == null)
                {
                    shadowArray[i][j].GetComponent<Renderer>().enabled = false;
                    //Debug.LogError("ERROR!!! No valid collider detected!"); //realizing that this will probably happen if there's ever a pit the player jumps over or something of the sort. TODO then!!!
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
                    rect = new Vector4(minx, miny, maxx, maxy);

                    // min x , min y , max x , max y 
                    //Send shadowArray at index i, j the rectangle to render and the height at which to render.
                    //Debug.Log("Expected Coord : " + new Vector2(i, j));
                    //Debug.Log(horizontalLines.Count - 2 - i);

                    Vector3 newpos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + highestCollider.GetComponent<EnvironmentPhysics>().GetTopHeight(), gameObject.transform.position.y - 0.58f + (miny * 1.2f));
                    Debug.DrawLine(new Vector3(transform.position.x - 1f, gameObject.transform.position.y - 0.6f - (-miny * 1.2f)), new Vector3(transform.position.x + 1f, gameObject.transform.position.y - 0.6f - (-miny * 1.2f)));
                    shadowArray[i][j].GetComponent<ShadowHandler>().UpdateShadow(newpos, highestCollider.GetComponent<EnvironmentPhysics>().GetTopHeight(), rect);
                }
            }
        }
        Profiler.EndSample();

    }
}
