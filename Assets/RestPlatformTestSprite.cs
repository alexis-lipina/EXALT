using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RestPlatformTestSprite : MonoBehaviour
{
    public RestPlatform ListenedRestPlatform;
    bool IsChargingDisplay = false;
    float DisplayEnergy = 0.0f;
    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        DisplayEnergy = ListenedRestPlatform.CurrentChargeAmount;
        _spriteRenderer.material.SetFloat("_FadeInMask", DisplayEnergy);
        //Shader.SetGlobalFloat("_FadeInMask", DisplayEnergy);
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
