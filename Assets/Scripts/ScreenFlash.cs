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

    [SerializeField] private Texture2D GradientTex_Ichor;
    [SerializeField] private Texture2D GradientTex_Sol;
    [SerializeField] private Texture2D GradientTex_Rift;
    [SerializeField] private Texture2D GradientTex_Storm;
    [SerializeField] private AnimationCurve FlashIntensityCurve;


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
        GetComponent<Image>().color = new Color(1, 1, 1, 0);
    }

    /// <summary>
    /// Triggers a screen-flash.
    /// </summary>
    /// <param name="opacity">Starting opacity</param>
    /// <param name="decayRate">Amount to deprecate opacity by every 10 milliseconds.</param>
    public void PlayFlash(float maxIntensity, float duration)
    {
        PlayFlash(maxIntensity, duration, Color.white);
    }

    public void PlayFlash(float maxIntensity, float duration, Color color, ElementType element = ElementType.NONE)
    {
        if (!AccessibilityOptionsSingleton.GetInstance().IsFlashingEnabled)
        {
            return;
        }
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(Flash(maxIntensity, duration, color, element));
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

    IEnumerator Flash(float maxIntensity, float duration, Color color, ElementType element = ElementType.NONE)
    {
        if (element != ElementType.NONE)
        {
            switch (element)
            {
                case ElementType.ICHOR:
                    GetComponent<Image>().material.SetTexture("_ElementGradient", GradientTex_Ichor);
                    break;
                case ElementType.FIRE:
                    GetComponent<Image>().material.SetTexture("_ElementGradient", GradientTex_Sol);
                    break;
                case ElementType.VOID:
                    GetComponent<Image>().material.SetTexture("_ElementGradient", GradientTex_Rift);
                    break;
                case ElementType.ZAP:
                    GetComponent<Image>().material.SetTexture("_ElementGradient", GradientTex_Storm);
                    break;
            }
            GetComponent<Image>().color = Color.white;
        }
        else 
        {
            GetComponent<Image>().color = color;
        }

        float timer = 0.0f;
        while (timer < duration)
        {
            color.a = FlashIntensityCurve.Evaluate(timer / duration) * maxIntensity;
            GetComponent<Image>().color = color;
            timer += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        GetComponent<Image>().color = new Color(1, 1, 1, 0);
    }
}
