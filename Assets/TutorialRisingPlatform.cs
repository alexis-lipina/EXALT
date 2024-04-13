using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRisingPlatform : MonoBehaviour
{
    public RestPlatform controllingRestPlatform;
    public CameraAttractor cameraAttractor;
    private MovingEnvironment _movingEnvironment;

    // Start is called before the first frame update
    void Start()
    {
        _movingEnvironment = GetComponent<MovingEnvironment>();
    }

    // Update is called once per frame
    void Update()
    {
        //_movingEnvironment.SetToElevation( Mathf.Lerp(-20, 0, controllingRestPlatform.CurrentChargeAmount), true);
        _movingEnvironment.SetToElevation( Mathf.SmoothStep(-20, 0, controllingRestPlatform.CurrentChargeAmount), true);
        foreach (var asdf in GetComponentsInChildren<SpriteRenderer>())
        {
            asdf.material.SetFloat("_Opacity", controllingRestPlatform.CurrentChargeAmount);
        }
        cameraAttractor.SetPullMagnitude(controllingRestPlatform.CurrentChargeAmount);
    }
}
