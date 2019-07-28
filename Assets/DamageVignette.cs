using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    [SerializeField] private EntityPhysics playerPhysics;
    private int previousHealth;
    private Coroutine currentCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
        while (true)
        {
            yield return ExponentialDecayOpacity(0.4f, 0.25f, 0.4f, 0.3f);
            yield return ExponentialDecayOpacity(0.25f, 0.4f, 0.05f, 0.5f);
            yield return ExponentialDecayOpacity(0.4f, 0.25f, 0.1f, 0.3f);
            yield return ExponentialDecayOpacity(0.25f, 0.4f, 0.05f, 0.5f);
        }
    }

    IEnumerator MajorPulse()
    {
        while (true)
        {
            yield return ExponentialDecayOpacity(0.8f, 0.5f, 0.4f, 0.3f);
            yield return ExponentialDecayOpacity(0.5f, 0.8f, 0.05f, 0.5f);
            yield return ExponentialDecayOpacity(0.8f, 0.5f, 0.1f, 0.3f);
            yield return ExponentialDecayOpacity(0.5f, 0.8f, 0.05f, 0.5f);

        }
    }

    IEnumerator DecayOpacity(float start, float end, float duration)
    {
        GetComponent<Image>().color = new Color(1, 1, 1, start);
        float timer = duration;
        while (timer > 0)
        {
            GetComponent<Image>().color = new Color(1, 1, 1, Mathf.Lerp(start, end, duration-timer/duration));
            timer -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator ExponentialDecayOpacity(float start, float end, float duration, float scale)
    {
        GetComponent<Image>().color = new Color(1, 1, 1, start);
        float timer = duration;
        GetComponent<Image>().color = new Color(1, 1, 1, start);
        while (timer > 0)
        {
            GetComponent<Image>().color = new Color(1, 1, 1, Mathf.Lerp(GetComponent<Image>().color.a, end, scale));
            timer -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
