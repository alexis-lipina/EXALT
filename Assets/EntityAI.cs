using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity AI Class
/// This class exerts control over the EntityHandler class in 
/// much the same way as the InputHandler exerts control over
/// the PlayerHandler class, except in this case rather than 
/// receiving user input, parsing it and passing it to the 
/// handler class, this one includes (or, will include) a
/// simple artificial intelligence which determines what input
/// to send to the EntityHandler.
/// 
/// In future implementations, this might instead serve as a middleman between
/// an "overlord" AI, but who knows. Who knows indeed.
/// </summary>
public class EntityAI : MonoBehaviour {
    [SerializeField] EntityHandler handler;
    private int step;
	// Use this for initialization
	void Start () {
        step = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (step <= 12)
        {
            Debug.Log("AI moving East");
            handler.setXYAnalogInput(1f, 0f);
        }
        else if (step <= 24)
        {
            handler.setXYAnalogInput(0f, 1f);
        }
        else if (step <= 36)
        {
            handler.setXYAnalogInput(-1f, 0f);
        }
        else if (step <= 48)
        {
            handler.setXYAnalogInput(0f, -1f);
        }
        else
        {
            step = 0;
        }
        step++;
    }
}
