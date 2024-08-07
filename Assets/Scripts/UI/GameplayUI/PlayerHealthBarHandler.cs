using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarHandler : MonoBehaviour
{
    [SerializeField] private List<Image> _healthBarSegments;
    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;
    [SerializeField] private Sprite _flashSprite;
    [SerializeField] private Sprite _darkSprite;
    [SerializeField] private List<Sprite> _shatterSprites; // rightmost

    private int _currentPlayerHealth = 5;
    private static float TimeToFadeOut = 3.0f;
    private float originalCellHeight;

    private List<Coroutine> HealthCellCoroutines;

    private void Awake()
    {
        originalCellHeight =_healthBarSegments[0].GetComponent<RectTransform>().rect.height;
        HealthCellCoroutines = new List<Coroutine>() { null, null, null, null, null};
    }

    // Update is called once per frame
    void Update()
    {
        //fade in/out for combat
        if (((PlayerHandler)_playerPhysics.Handler).ForceUIVisible)
        {
            foreach (Image segment in _healthBarSegments)
            {
                segment.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
        else if ( ((PlayerHandler)_playerPhysics.Handler).TimeSinceCombat > TimeToFadeOut )
        {
            const float lerpRate = 1.0f;
            float newSegmentAlpha = Mathf.Lerp(1.0f, 0.0f, (((PlayerHandler)_playerPhysics.Handler).TimeSinceCombat - TimeToFadeOut) * lerpRate);
            foreach (Image segment in _healthBarSegments)
            {
                segment.color = new Color(1.0f, 1.0f, 1.0f, newSegmentAlpha);
            }
        }
        else
        {
            foreach (Image segment in _healthBarSegments)
            {
                segment.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(segment.color.a, 1.0f, 0.2f));
            }
        }

        if (_currentPlayerHealth == _playerPhysics.GetCurrentHealth()) return; //short circuit if no changes
        int _previousPlayerHealth = _currentPlayerHealth;
        _currentPlayerHealth = _playerPhysics.GetCurrentHealth();
        for (int i = 0; i < _healthBarSegments.Count; i++)
        {
            if (i < _currentPlayerHealth && i >= _previousPlayerHealth)
            {
                if (HealthCellCoroutines[i] != null)
                {
                    StopCoroutine(HealthCellCoroutines[i]);
                }
                HealthCellCoroutines[i] = StartCoroutine(TurnOn(_healthBarSegments[i]));
            }
            else if (i >= _currentPlayerHealth && i < _previousPlayerHealth)
            {
                if (HealthCellCoroutines[i] != null)
                {
                    StopCoroutine(HealthCellCoroutines[i]);
                }
                HealthCellCoroutines[i] = StartCoroutine(TurnOff(_healthBarSegments[i]));
            }
        }
    }

    IEnumerator TurnOff(Image segment)
    {
        segment.sprite = _flashSprite;
        originalCellHeight = segment.GetComponent<RectTransform>().rect.height;

        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalCellHeight * 100f);
        segment.sprite = _flashSprite;

        yield return new WaitForSeconds(0.02f);

        segment.sprite = _darkSprite;

        yield return new WaitForSeconds(0.02f);

        segment.sprite = _flashSprite;

        yield return new WaitForSeconds(0.02f);

        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalCellHeight);
        segment.sprite = _darkSprite;

        yield return new WaitForSeconds(0.02f);
        segment.sprite = _offSprite;
    }

    IEnumerator TurnOn(Image segment)
    {
        segment.sprite = _flashSprite;

        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalCellHeight * 100f);
        segment.sprite = _flashSprite;

        yield return new WaitForSeconds(0.04f);

        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalCellHeight * 2f);
        segment.sprite = _flashSprite;

        yield return new WaitForSeconds(0.04f);

        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalCellHeight);
        segment.sprite = _onSprite;
    }

    public void ShatterHealthBarSegment(int NewMax)
    {
        _currentPlayerHealth--;
        for (int i = 0; i < _healthBarSegments.Count; i++)
        {
            if (i < NewMax)
            {
                
            }
            else
            {
                _healthBarSegments[i].sprite = _shatterSprites[i];
                _healthBarSegments[i].color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
        }
    }
}