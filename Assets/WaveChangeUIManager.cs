using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveChangeUIManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup WaveUIGroup;
    [SerializeField] private List<Image> WaveCounters; // FROM 1 TO MAX (inclusive)
    [SerializeField] private Image WaveNumber;

    [SerializeField] private Sprite FilledCounterSprite;
    [SerializeField] private Sprite EmptyCounterSprite;
    [SerializeField] private List<Sprite> WaveNumberSprites; // FROM 0 TO MAX (inclusive)

    const float FADE_IN_RATE = 1.0f; // per second
    const float FADE_OUT_RATE = 1.0f; // per second

    // Start is called before the first frame update
    void Start()
    {
        // setup default starting condition (0)
        foreach(Image i in WaveCounters)
        {
            i.sprite = EmptyCounterSprite;
        }
        WaveNumber.sprite = WaveNumberSprites[0];
        WaveUIGroup.alpha = 0.0f;
    }

    public void PlayWaveChange(int newWaveNumber)
    {
        StartCoroutine(WaveChange(newWaveNumber));
    }

    IEnumerator WaveChange(int newWaveNumber)
    {
        // fade in everything
        WaveUIGroup.alpha = 0;
        while (WaveUIGroup.alpha < 1)
        {
            WaveUIGroup.alpha += FADE_IN_RATE * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        WaveUIGroup.alpha = 1.0f;

        // play the transition
        yield return new WaitForSeconds(1.0f);
        ScreenFlash.InstanceOfScreenFlash.PlayFlash(1.0f, 0.1f);
        // set counters
        for (int i = 0; i < WaveCounters.Count; i++)
        {
            if (i < newWaveNumber)
            {
                WaveCounters[i].sprite = FilledCounterSprite;
            }
        }
        // set number
        if (newWaveNumber < WaveNumberSprites.Count)
        {
            WaveNumber.sprite = WaveNumberSprites[newWaveNumber];
        }

        yield return new WaitForSeconds(1.0f);

        // fade out everything
        while (WaveUIGroup.alpha > 0)
        {
            WaveUIGroup.alpha -= FADE_OUT_RATE * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        WaveUIGroup.alpha = 0.0f;
    }
}
