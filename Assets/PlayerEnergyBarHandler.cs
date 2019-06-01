using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergyBarHandler : MonoBehaviour
{
    [SerializeField] private List<Image> _energyBarSegments;
    [SerializeField] private PlayerHandler _playerHandler;
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;
    [SerializeField] private Sprite _flashSprite;
    [SerializeField] private Sprite _flareSprite;
    [SerializeField] private Sprite _darkSprite;

    private int _currentPlayerEnergy = 12;



    // Update is called once per frame
    void Update()
    {
        if (_currentPlayerEnergy == _playerHandler.CurrentEnergy) return;
        _currentPlayerEnergy = _playerHandler.CurrentEnergy;
        for (int i = 0; i < _energyBarSegments.Count; i++)
        {
            if (i < _currentPlayerEnergy && _energyBarSegments[i].sprite != _onSprite)
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
        segment.sprite = _onSprite;
    }

    IEnumerator TurnOff(Image segment)
    {
        //float originalHeight = segment.GetComponent<RectTransform>().rect.height;
        //segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight * 100f);
        segment.sprite = _flashSprite;

        yield return new WaitForSeconds(0.02f);

        segment.sprite = _darkSprite;

        //yield return new WaitForSeconds(0.02f);

        //segment.sprite = _flashSprite;

        //yield return new WaitForSeconds(0.02f);

        //segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight);
        //segment.sprite = _darkSprite;

        yield return new WaitForSeconds(0.02f);
        segment.sprite = _offSprite;
    }
}
