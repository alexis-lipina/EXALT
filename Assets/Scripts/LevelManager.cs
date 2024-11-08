﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains settings that will be handled on a per-level basis. Includes this level's "gradient color" settings.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField] private Color depthsColor;
    [SerializeField] private Color heightsColor;
    public float elevationOffset = 30.0f;
    [SerializeField] public float killPlaneElevation = -20.0f;
    [SerializeField] Canvas _canvas;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Setting color");
        Shader.SetGlobalColor("_HighColor", heightsColor);
        Shader.SetGlobalColor("_LowColor", depthsColor);
        Shader.SetGlobalFloat("_MaxElevationOffset", elevationOffset);
        _canvas.gameObject.SetActive(true); // in case its been disabled
        EntityPhysics.KILL_PLANE_ELEVATION = killPlaneElevation;

        //Shader.SetGlobalColor("_HighColor", heightsColor);
        //Shader.SetGlobalColor("_LowColor", depthsColor);


    }


    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
