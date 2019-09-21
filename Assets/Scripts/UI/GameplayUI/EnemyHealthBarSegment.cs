using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBarSegment : MonoBehaviour
{
    [SerializeField] Sprite _onSprite;
    [SerializeField] Sprite _offSprite;
    [SerializeField] Sprite _flashSprite;
    [SerializeField] Sprite _darkSprite;
    private bool _currentlyOn = true;
    private bool _jiggleRunning;

    public void SetSegment(bool newIsOn)
    {
        if (newIsOn && !_currentlyOn)
        {
            GetComponent<SpriteRenderer>().sprite = _onSprite;
        }
        if (!newIsOn && _currentlyOn)
        {
            StartCoroutine(FlashOff());
        }
        _currentlyOn = newIsOn;
    }

    public void JiggleSegment()
    {
        if (_jiggleRunning) return;
    }

    private IEnumerator FlashOff()
    {
        transform.position = transform.position + new Vector3(0f, 0f, -10f);
        GetComponent<SpriteRenderer>().sprite = _flashSprite;
        yield return new WaitForSeconds(0.03f);
        GetComponent<SpriteRenderer>().sprite = _darkSprite;
        yield return new WaitForSeconds(0.03f);
        transform.position = transform.position + new Vector3(0f, 0f, 10f);
        GetComponent<SpriteRenderer>().sprite = _offSprite;
    }
}