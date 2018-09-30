using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	// Use this for initialization
	void Start ()
    {
        shadowArray = new List<List<GameObject>>();
        for (int i = 0; i < 3; i++)
        {
            shadowArray.Add(new List<GameObject>());
            for (int j = 0; j < 3; j++)
            {
                Debug.Log("bloop");
                shadowArray[i].Add(Instantiate(_shadowPrefab, gameObject.transform));
                shadowArray[i][j].transform.position = new Vector3(transform.position.x + i, transform.position.y + j, transform.position.z);
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
        List<float> horizontalLines = new List<float>();
        List<float> verticalLines = new List<float>(); //these are x values that define vertical lines passing through the sprite
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
                    shadowArray[i][j].SetActive(true);
                }
                else
                {
                    shadowArray[i][j].SetActive(false);
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

        Debug.Log("new");
        //send data to each new slice about its rect
        for (int i = 0; i < horizontalLines.Count-1; i++)//traverses up the rows
        {
            for (int j = 0; j < verticalLines.Count-1; j++)//traverses right along the columns
            {
                //Get the worldspace rectangle for the given segment
                KeyValuePair<Vector2, Vector2> rect = new KeyValuePair<Vector2, Vector2>(new Vector2(verticalLines[j], horizontalLines[i]), new Vector2(verticalLines[j+1], horizontalLines[i+1]));
                Debug.Log(rect);
                Debug.DrawLine(rect.Key, rect.Value, Color.cyan, 0.01f, false);
                //use the center of the rect and "cast down" to get the height of the segment
                Physics2D.OverlapPointAll();
            }
        }

    }
}
