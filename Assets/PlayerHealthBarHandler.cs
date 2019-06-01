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

    private int _currentPlayerHealth = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentPlayerHealth == _playerPhysics.GetCurrentHealth()) return; //short circuit if no changes

        _currentPlayerHealth = _playerPhysics.GetCurrentHealth();
        for (int i = 0; i < _healthBarSegments.Count; i++)
        {
            if (i < _currentPlayerHealth)
            {
                _healthBarSegments[i].sprite = _onSprite;
            }
            else if (i == _currentPlayerHealth)
            {
                StartCoroutine(TurnOff(_healthBarSegments[i]));
            }
        }
    }

    IEnumerator TurnOff(Image segment)
    {
        segment.sprite = _flashSprite;
        float originalHeight = segment.GetComponent<RectTransform>().rect.height;

        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight * 100f);
        segment.sprite = _flashSprite;

        yield return new WaitForSeconds(0.02f);

        segment.sprite = _darkSprite;

        yield return new WaitForSeconds(0.02f);

        segment.sprite = _flashSprite;

        yield return new WaitForSeconds(0.02f);

        segment.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalHeight);
        segment.sprite = _darkSprite;

        yield return new WaitForSeconds(0.02f);
        segment.sprite = _offSprite;
    }
}
