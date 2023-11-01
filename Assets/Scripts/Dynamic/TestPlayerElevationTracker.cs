using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerElevationTracker : MonoBehaviour
{
    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private EnvironmentPhysics _environmentPhysics;
    [SerializeField] private bool _trueIfPlatformFalseIfWall;
    [SerializeField] private bool _canChangeElevation = false;
    private SpriteRenderer spriteRenderer;
    private float _lerpedPlayerHeight;
    private Material mat;

    // Use this for initialization
    void Start ()
    {
        UpdateShader();
    }

    public void UpdateShader()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        if (_trueIfPlatformFalseIfWall)
        {
            //platform

            GetComponent<SpriteRenderer>().material.SetFloat("_PlatformElevation", _environmentPhysics.GetTopHeight());

            //changes field for all instances
            //gameObject.GetComponent<SpriteRenderer>().sharedMaterial.SetFloat("_PlatformElevation", _environmentPhysics.GetTopHeight());
        }
        else
        {
            //wall
            GetComponent<SpriteRenderer>().material.SetFloat("_TopElevation", _environmentPhysics.GetTopHeight());
            GetComponent<SpriteRenderer>().material.SetFloat("_BottomElevation", _environmentPhysics.GetBottomHeight());
            GetComponent<SpriteRenderer>().material.SetFloat("_TopSpriteRect", spriteRenderer.sprite.rect.yMin / spriteRenderer.sprite.texture.height);
            GetComponent<SpriteRenderer>().material.SetFloat("_BottomSpriteRect", spriteRenderer.sprite.rect.yMax / spriteRenderer.sprite.texture.height);
        }
    }

    private void Update()
    {
        if (_canChangeElevation) UpdateShader();
    }

    public void SetCanChangeElevation(bool canchange)
    {
        _canChangeElevation = canchange;
    }
}
