using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash InstanceOfScreenFlash;


    private Coroutine _flashCoroutine = null;
    private Coroutine _hitPauseCoroutine = null;
    private float _hitPauseTimer;


    private void Awake()
    {
        if (!InstanceOfScreenFlash)
        {
            InstanceOfScreenFlash = this;
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Triggers a screen-flash.
    /// </summary>
    /// <param name="opacity">Starting opacity</param>
    /// <param name="decayRate">Amount to deprecate opacity by every 10 milliseconds.</param>
    public void PlayFlash(float opacity, float decayRate)
    {
        PlayFlash(opacity, decayRate, Color.white);
    }

    public void PlayFlash(float opacity, float decayRate, Color color)
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(Flash(opacity, decayRate, color));
    }

    public void PlayHitPause(float duration)
    {
        if (_hitPauseCoroutine != null)
        {
            if (_hitPauseTimer < duration)
            {
                _hitPauseTimer = duration;
            }
            return;
        }
        _hitPauseTimer = duration;
        _hitPauseCoroutine = StartCoroutine(HitPause());
    }

    IEnumerator HitPause() //TODO : DEPRECATED - hit pausing messes with combo timing which is hella bad, and honestly the game feels smoother without it
    {
        
        Time.timeScale = 0f;
        while (_hitPauseTimer > 0)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            _hitPauseTimer -= 0.01f;
        }
        Time.timeScale = 1f;
        _hitPauseCoroutine = null;
    }

    IEnumerator Flash(float opacity, float decayRate, Color color)
    {
        if (AccessibilityOptionsSingleton.GetInstance().IsFlashingEnabled)
        {
            while (opacity > 0)
            {
                color.a = opacity;
                GetComponent<Image>().color = color;
                opacity -= decayRate;
                yield return new WaitForSeconds(0.01f);
            }
            GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }
    }
}
