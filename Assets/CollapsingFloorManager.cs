using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsingFloorManager : MonoBehaviour
{
    [SerializeField] GameObject[] OrderedCollapseFolders;

    int CollapseCount = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerNextCollapse()
    {
        CollapsingPlatform[] platformsToCollapse = OrderedCollapseFolders[CollapseCount].GetComponentsInChildren<CollapsingPlatform>();

        foreach (CollapsingPlatform platform in platformsToCollapse)
        {
            platform.StartCollapse();
        }

        CollapseCount++;
    }
}
