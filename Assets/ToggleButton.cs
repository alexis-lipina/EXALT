using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ToggleButton : MonoBehaviour
{
    [SerializeField] private Sprite OnSprite;
    [SerializeField] private Sprite OffSprite;

    private bool _isToggledOn = false;

    public void SetCurrentValue(bool value)
    {
        _isToggledOn = value;
        UpdateSprite();
    }

    public void Toggle()
    {
        SetCurrentValue(!_isToggledOn);
    }

    public bool GetCurrentValue()
    {
        return _isToggledOn;
    }

    void UpdateSprite()
    {
        if (_isToggledOn) GetComponent<Image>().sprite = OnSprite;
        else GetComponent<Image>().sprite = OffSprite;
    }
}
