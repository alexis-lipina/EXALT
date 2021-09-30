using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains settings that will be handled on a per-level basis. Includes this level's "gradient color" settings.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField] private Color depthsColor;
    [SerializeField] private Color heightsColor;
    [SerializeField] private float elevationOffset = 30.0f;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Setting color");
        Shader.SetGlobalColor("_HighColor", heightsColor);
        Shader.SetGlobalColor("_LowColor", depthsColor);
        Shader.SetGlobalFloat("_MaxElevationOffset", elevationOffset);
        //Shader.SetGlobalColor("_HighColor", heightsColor);
        //Shader.SetGlobalColor("_LowColor", depthsColor);


    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
