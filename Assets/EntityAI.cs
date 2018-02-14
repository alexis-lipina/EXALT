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
public class EntityAI : MonoBehaviour
{
    [SerializeField] EntityHandler handler;
    [SerializeField] GameObject player;
    [SerializeField]
    GameObject punchingBag;

    private int step;


    void Start () {
        step = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        MoveTowardPlayer();
    }

    //=====================| AI Methods
    private void MoveTowardPlayer()
    {
        Vector2 direction = new Vector2(player.transform.position.x - punchingBag.transform.position.x, player.transform.position.y - punchingBag.transform.position.y);
        if (direction.magnitude > 2)
        {
            handler.setXYAnalogInput(direction.normalized.x, direction.normalized.y);
        }
        else
        {
            handler.setXYAnalogInput(0, 0);
        }
    }
}
