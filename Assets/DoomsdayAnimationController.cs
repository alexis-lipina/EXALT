using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the background animation in the "doomsday" scenario on the CollapsingBalconyLedge scene
/// </summary>
public class DoomsdayAnimationController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //=================================| STAGES |===================================
     
    /// <summary>
    /// Looping, default animation 
    /// </summary>
    /// <returns></returns>
    private IEnumerator OpeningLoop()
    {
        yield return new WaitForEndOfFrame();
    }
}
