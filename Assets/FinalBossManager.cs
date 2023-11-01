using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossManager : MonoBehaviour
{
    // player approaches boss. camera pulls back. short cutscene thing. maybe the ground splits and the ceiling blows off? Level shifts to a 
    [Header("Pre-boss fight")]
    [SerializeField] List<EnvironmentDynamicPhysics> PlatformsToSeparate;
    
    [Space(10)]
    [Header("During Boss Fight")]
    [SerializeField] List<FinalBossFragment> FourCornerCrystals;





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
