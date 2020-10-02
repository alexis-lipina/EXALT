using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    [SerializeField] private EntityPhysics playerPhysics;
    private PlayerHandler _playerHandler;
    private int previousHealth;
    private Coroutine currentCoroutine;
    private AudioSource _heartBeat;

    // Start is called before the first frame update
    void Start()
    {
        _playerHandler = playerPhysics.Handler as PlayerHandler;
        _heartBeat = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!AccessibilityOptionsSingleton.GetInstance().IsFlashingEnabled)
        {
            GetComponent<Image>().enabled = false;
            return;
        }
        if (playerPhysics.GetCurrentHealth() != previousHealth)
        {
            if (playerPhysics.GetCurrentHealth() == 2)
            {
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                currentCoroutine = StartCoroutine(MildPulse());
            }
            else if (playerPhysics.GetCurrentHealth() == 1)
            {
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                currentCoroutine = StartCoroutine(MajorPulse());
            }
            else
            {
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            }
        }
        previousHealth = playerPhysics.GetCurrentHealth();
    }


    

    IEnumerator MildPulse()
    {
        _heartBeat.volume = 0.5f;
        while (true)
        {
            StartCoroutine(_playerHandler.VibrateDecay(0.05f, 0.1f));          // ba
            _heartBeat.Play();
            yield return ExponentialDecayOpacity(0.28f, 0.45f, 0.15f, 0.5f);   

            yield return ExponentialDecayOpacity(0.45f, 0.28f, 0.1f, 0.3f);    // -

            StartCoroutine(_playerHandler.VibrateDecay(0.05f, 0.05f));         // bum
            _heartBeat.Play();
            yield return ExponentialDecayOpacity(0.28f, 0.45f, 0.15f, 0.5f);   

            yield return ExponentialDecayOpacity(0.45f, 0.28f, 0.5f, 0.1f);    // ...
        }
    }

    IEnumerator MajorPulse()
    {
        _heartBeat.volume = 1.0f;
        while (true)
        {
            StartCoroutine(_playerHandler.VibrateDecay(0.3f, 0.05f));      // ba
            _heartBeat.Play();
            yield return ExponentialDecayOpacity(0.6f, 0.8f, 0.1f, 0.5f);  

            yield return ExponentialDecayOpacity(0.8f, 0.6f, 0.1f, 0.3f);  // -

            StartCoroutine(_playerHandler.VibrateDecay(0.3f, 0.01f));      // bum
            _heartBeat.Play();
            yield return ExponentialDecayOpacity(0.6f, 0.8f, 0.1f, 0.5f);  

            yield return ExponentialDecayOpacity(0.8f, 0.6f, 0.4f, 0.1f);  // ...

        }
    }

    IEnumerator DecayOpacity(float start, float end, float duration)
    {
        if (AccessibilityOptionsSingleton.GetInstance().LowHPVignette) GetComponent<Image>().color = new Color(1, 1, 1, start);
        float timer = duration;
        while (timer > 0)
        {
            if (AccessibilityOptionsSingleton.GetInstance().LowHPVignette) GetComponent<Image>().color = new Color(1, 1, 1, Mathf.Lerp(start, end, duration-timer/duration));
            timer -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator ExponentialDecayOpacity(float start, float end, float duration, float scale)
    {
        if (AccessibilityOptionsSingleton.GetInstance().LowHPVignette) GetComponent<Image>().color = new Color(1, 1, 1, start);
        float timer = duration;
        while (timer > 0)
        {
            if (AccessibilityOptionsSingleton.GetInstance().LowHPVignette) GetComponent<Image>().color = new Color(1, 1, 1, Mathf.Lerp(GetComponent<Image>().color.a, end, scale));
            timer -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
