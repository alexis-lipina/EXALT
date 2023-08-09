using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMemoryPlanetfallExitVolume : MonoBehaviour
{
    ExitVolume exitVolume;

    // Start is called before the first frame update
    void Start()
    {
        exitVolume = GetComponent<ExitVolume>();
        switch (CampaignSaveSingleton.GetInstance()._data.FireMemory_PlanetIndex)
        {
            case 0:
                exitVolume.SetTargetLevel("Fire_SolarConfrontation");
                break;
            case 1:
                exitVolume.SetTargetLevel("Fire_Planet1");
                break;
            case 2:
                exitVolume.SetTargetLevel("Fire_Planet2");
                break;
            case 3:
                exitVolume.SetTargetLevel("Fire_Planet3");
                break;
            case 4:
                exitVolume.SetTargetLevel("Fire_Planet4");
                break;
        }
    }
}
