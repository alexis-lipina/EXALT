using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleController : MonoBehaviour
{
    public float[,] grid;
    [SerializeField] private int dimX;
    [SerializeField] private int dimY;

    private float _totalTime; 


	// Use this for initialization
	void Start () {
        grid = new float[dimX, dimY];
        _totalTime = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        _totalTime += Time.deltaTime;
        if (_totalTime > 360) _totalTime -= 360;

        for (int i = 0; i < dimX; i++)
        {
            grid[i, 0] = Mathf.Sin(_totalTime + i*30)/4.0f;
        }

	}
}
