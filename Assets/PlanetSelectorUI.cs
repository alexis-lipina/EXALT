using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSelectorUI : MonoBehaviour
{
    [SerializeField] RestPlatform restPlatform;
    [SerializeField] SpriteRenderer SunSprite;
    [SerializeField] SpriteRenderer Planet1Sprite;
    [SerializeField] SpriteRenderer Planet2Sprite;
    [SerializeField] SpriteRenderer Planet3Sprite;
    [SerializeField] SpriteRenderer Planet4Sprite;

    public int HoveredPlanetIndex = 1;

    List<SpriteRenderer> Planets;
    //List<int> VisiblePlanetIndexes;

    // input nonsense
    private float InputScalarThreshold = 0.6f; // how high directional input needs to be in a direction to interpret it as that directions push
    private bool HasExceededThreshold = false; // set to true when scalar threshold has been exceeded and input has been processed. We will wait until the value has dropped below threshold before listening


    // Start is called before the first frame update
    void Start()
    {
        Planets = new List<SpriteRenderer>();
        Planets.Add(SunSprite);
        Planets.Add(Planet1Sprite);
        Planets.Add(Planet2Sprite);
        Planets.Add(Planet3Sprite);
        Planets.Add(Planet4Sprite);

        UpdateHoveredPlanet();
    }

    void Update()
    {
        if (restPlatform.IsActivated)
        {
            Debug.Log("Processing directional input");
            Vector2 dirInput = restPlatform.InputDirection;
            if (Mathf.Abs(dirInput.x) < InputScalarThreshold)
            {
                HasExceededThreshold = false;
            }
            else if (!HasExceededThreshold)
            {
                HasExceededThreshold = true;
                if (dirInput.x > 0)
                {
                    HoverRight();
                    Debug.Log("Hover Right");
                }
                else
                {
                    HoverLeft();
                    Debug.Log("Hover Left");
                }
            }
        }
    }

    public void HoverRight()
    {
        HoveredPlanetIndex++;
        HoveredPlanetIndex = Mathf.Clamp(HoveredPlanetIndex, 0, Planets.Count - 1);
        UpdateHoveredPlanet();
    }

    public void HoverLeft()
    {
        HoveredPlanetIndex--;
        HoveredPlanetIndex = Mathf.Clamp(HoveredPlanetIndex, 0, Planets.Count - 1);
        UpdateHoveredPlanet();
    }

    public void SelectHoveredPlanet()
    {
        CampaignSaveSingleton.GetInstance()._data.FireMemory_PlanetIndex = HoveredPlanetIndex;
        UpdateHoveredPlanet();
        CampaignSaveSingleton.GetInstance().SaveCurrentState();
    }

    void UpdateHoveredPlanet()
    {
        for (int i = 0; i < Planets.Count; i++)
        {
            if (i == HoveredPlanetIndex)
            {
                Planets[i].color = Color.white;
            }
            else if (i == CampaignSaveSingleton.GetInstance()._data.FireMemory_PlanetIndex)
            {
                Planets[i].color = new Color(1.0f, 0.0f, 0.5f, 1.0f);
            }
            else
            {
                Planets[i].color = Color.grey;
            }
        }
    }
}
