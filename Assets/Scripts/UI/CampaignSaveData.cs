using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CampaignSaveData
{
    public string CurrentLevelName = "";
    public string PreviousLevelName = ""; 
    public string PreviousDoorName = ""; 

    // === Misc campaign state info, ordered by sequence

    public int VoidMemory_Day = 1; // there are 3 days of the memory. 1 = first day, 2 = second day, 3 = third day
    public bool VoidMemory_HasDesignedThing = false; // whether player has done the "design" puzzle in the void memory for the current day.

    public int FireMemory_PlanetIndex = 1; // 0 = sun, 1 = first/barren, 2 = life, 3 = civilization, 4 = final
}
