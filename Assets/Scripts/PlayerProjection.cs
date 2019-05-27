using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjection : ShadowManager
{

    // Start is called before the first frame update
    void Start()
    {
        //copied from parent
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
    void Update()
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

    public void SetOpacity(float value)
    {
        if (shadowArray == null)
        {
            Start();
        }
        for (int i = 0; i < shadowArray.Count; i++)
        {
            for (int j = 0; j < shadowArray[i].Count; j++)
            {
                shadowArray[i][j].GetComponent<Renderer>().material.SetFloat("_Opacity", value);
            }
        }
    }

    public void SetColor(Color value)
    {
        if (shadowArray == null)
        {
            Start();
        }
        for (int i = 0; i < shadowArray.Count; i++)
        {
            for (int j = 0; j < shadowArray[i].Count; j++)
            {
                shadowArray[i][j].GetComponent<Renderer>().material.SetColor("_ColorOverride", value);
            }
        }
    }


    protected override void OnShadowInstantiated()
    {

    }
}
