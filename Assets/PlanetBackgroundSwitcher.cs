using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetBackgroundSwitcher : MonoBehaviour
{
    [SerializeField] Sprite SunSprite;
    [SerializeField] Sprite Planet1Sprite;
    [SerializeField] Sprite Planet2Sprite;
    [SerializeField] Sprite Planet3Sprite;
    [SerializeField] Sprite Planet4Sprite;
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdatePlanetSprite();
    }
    private void Update()
    {
        UpdatePlanetSprite();
    }

    void UpdatePlanetSprite()
    {
        switch (CampaignSaveSingleton.GetInstance()._data.FireMemory_PlanetIndex)
        {
            case 0:
                spriteRenderer.sprite = SunSprite;
                break;
            case 1:
                spriteRenderer.sprite = Planet1Sprite;
                break;
            case 2:
                spriteRenderer.sprite = Planet2Sprite;
                break;
            case 3:
                spriteRenderer.sprite = Planet3Sprite;
                break;
            case 4:
                spriteRenderer.sprite = Planet4Sprite;
                break;
        }
    }
}
