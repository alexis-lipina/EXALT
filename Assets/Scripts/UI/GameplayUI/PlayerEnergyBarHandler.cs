using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergyBarHandler : MonoBehaviour
{
    [SerializeField] private List<Image> _energyBarSegments;
    [SerializeField] private PlayerHandler _playerHandler;
    [SerializeField] private Sprite _onSprite_Zap;
    [SerializeField] private Sprite _onSprite_Void;
    [SerializeField] private Sprite _onSprite_Fire;
    [SerializeField] private Sprite _onSprite_Ichor;
    [SerializeField] private Sprite _offSprite;
    [SerializeField] private Sprite _flashSprite;
    [SerializeField] private Sprite _flareSprite;
    [SerializeField] private Sprite _darkSprite;

    private int _currentPlayerEnergy = 12;

    private ElementType _currentPlayerElement = ElementType.NONE;

    // time since last "combat event" to fade out the UI
    private static float TimeToFadeOut = 3.0f;

    /// <summary>
    /// Get the current elemental sprite
    /// </summary>
    /// <returns></returns>
    private Sprite GetCurrentOnSprite()
    {
        switch (_currentPlayerElement)
        {
            case ElementType.FIRE:
                return _onSprite_Fire;
            case ElementType.VOID:
                return _onSprite_Void;
            case ElementType.ZAP:
                return _onSprite_Zap;
            case ElementType.ICHOR:
                return _onSprite_Ichor;
            default:
                return _onSprite_Zap;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //fade in/out for combat
        if ( _playerHandler.ForceUIVisible)
        {
            foreach (Image segment in _energyBarSegments)
            {
                segment.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
        else if (_playerHandler.TimeSinceCombat > TimeToFadeOut)
        {
            const float lerpRate = 1.0f;
            float newSegmentAlpha = Mathf.Lerp(1.0f, 0.0f, (_playerHandler.TimeSinceCombat - TimeToFadeOut) * lerpRate);
            foreach (Image segment in _energyBarSegments)
            {
                segment.color = new Color(1.0f, 1.0f, 1.0f, newSegmentAlpha);
            }
        }
        else
        {
            foreach (Image segment in _energyBarSegments)
            {
                segment.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(segment.color.a, 1.0f, 0.2f));
            }
        }

        // update element color
        if (_playerHandler.GetStyle() != _currentPlayerElement)
        {
            _currentPlayerElement = _playerHandler.GetStyle();
            for (int i = 0; i < _energyBarSegments.Count; i++)
            {
                if (i < _currentPlayerEnergy && _energyBarSegments[i].sprite != GetCurrentOnSprite())
                {
                    StartCoroutine(TurnOn(_energyBarSegments[i]));
                }
                else if (i >= _currentPlayerEnergy && _energyBarSegments[i].sprite != _offSprite)
                {
                    StartCoroutine(TurnOff(_energyBarSegments[i]));
                }
            }
        }

        //code to update player energy
        if (_currentPlayerEnergy == _playerHandler.CurrentEnergy) return;
        _currentPlayerEnergy = _playerHandler.CurrentEnergy;
        for (int i = 0; i < _energyBarSegments.Count; i++)
        {
            if (i < _currentPlayerEnergy && _energyBarSegments[i].sprite != GetCurrentOnSprite())
            {
                StartCoroutine(TurnOn(_energyBarSegments[i]));
            }
            else if (i >= _currentPlayerEnergy && _energyBarSegments[i].sprite != _offSprite)
            {
                StartCoroutine(TurnOff(_energyBarSegments[i]));
            }
        }
    }
    IEnumerator TurnOn(Image segment)
    {
        float originalHeight = segment.GetComponent<RectTransform>().rect.height;
        segment.sprite = _flareSprite;
        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight * 5);

        yield return new WaitForSeconds(0.02f);

        segment.sprite = _flashSprite;
        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight * 2);

        yield return new WaitForSeconds(0.02f);

        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight);
        segment.sprite = GetCurrentOnSprite();
    }

    IEnumerator TurnOff(Image segment)
    {
        segment.sprite = _flashSprite;

        yield return new WaitForSeconds(0.02f);

        segment.sprite = _darkSprite;

        yield return new WaitForSeconds(0.02f);
        segment.sprite = _offSprite;
    }
}
