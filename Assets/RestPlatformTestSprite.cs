using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestPlatformTestSprite : MonoBehaviour
{
    public RestPlatform ListenedRestPlatform;
    bool IsChargingDisplay = false;
    float DisplayEnergy = 0.0f;
    public float ChargeRate = 0.1f;
    public float DecayRate = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ListenedRestPlatform.IsActionPressed)
        {
            transform.localScale = new Vector3(0.95f * transform.localScale.x, 0.95f * transform.localScale.y, 0.95f * transform.localScale.z);
        }
        if (IsChargingDisplay)
        {
            DisplayEnergy += Time.deltaTime * ChargeRate;
        }
        else
        {
            DisplayEnergy -= Time.deltaTime * DecayRate;
        }
        DisplayEnergy = Mathf.Clamp(DisplayEnergy, 0, 1);
        Shader.SetGlobalFloat("_FadeInMask", DisplayEnergy);
        //GetComponent<SpriteRenderer>().color.a = 0.0f;
    }

    public void TestMove()
    {
        transform.position += new Vector3(ListenedRestPlatform.InputDirection.x, ListenedRestPlatform.InputDirection.y, 0); ;
    }

    public void ChargeDisplay()
    {
        IsChargingDisplay = true;
    }

    public void DecayDisplay()
    {
        IsChargingDisplay = false;
    }
}
