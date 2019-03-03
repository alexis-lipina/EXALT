using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash InstanceOfScreenFlash;


    private Coroutine _coroutine = null;

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
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(Flash(opacity, decayRate));
    }

    IEnumerator Flash(float opacity, float decayRate)
    {
        while (opacity > 0)
        {
            GetComponent<Image>().color = new Color(1, 1, 1, opacity);
            opacity -= decayRate;
            yield return new WaitForSeconds(0.01f);
        }
        GetComponent<Image>().color = new Color(1, 1, 1, 0);
    }
}
