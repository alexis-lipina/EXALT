using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiringChamberShadowedGradients : MonoBehaviour
{
    [SerializeField] List<SpriteRenderer> ShadowedGradients;
    [SerializeField] SpriteRenderer BackWallShadow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideGradients()
    {
        foreach (SpriteRenderer renderer in ShadowedGradients)
        {
            renderer.enabled = false;
        }
    }
    
    public void ShowGradients()
    {
        foreach (SpriteRenderer renderer in ShadowedGradients)
        {
            renderer.enabled = true;
        }
    }

    public SpriteRenderer GetBackWallShadow()
    {
        return BackWallShadow;
    }
}
