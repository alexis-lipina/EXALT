using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackRestPlatform : MonoBehaviour
{
    [SerializeField] ZapFXController TopLightningBolt;
    [SerializeField] ZapFXController BottomLightningBolt;
    [SerializeField] List<ZapFXController> RandomAmbientLightningBolts;

    float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        /*
        if (timer > 1.0f)
        {
            timer = 0.0f;
            TopLightningBolt.SetupLine(Vector3.zero, new Vector3(0, 36, 0));
            TopLightningBolt.Play(0.5f);
            BottomLightningBolt.SetupLine(Vector3.zero, new Vector3(0, 36, 0));
            BottomLightningBolt.Play(0.5f);
            Debug.LogError("Lightning!");
        }*/
    }
}
