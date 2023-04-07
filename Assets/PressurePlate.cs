using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A platform which is slightly raised and, when stood on by the player, lowers and fires a custom "pressed" event.  
/// </summary>
public class PressurePlate : MonoBehaviour
{

    MovingEnvironment movingEnvironment;

    // Start is called before the first frame update
    void Start()
    {
        movingEnvironment = GetComponent<MovingEnvironment>();
    }

    // Update is called once per frame
    void Update()
    {
        //movingEnvironment.StandingTrigger.On3DOverlapEvent += 

    }
}
