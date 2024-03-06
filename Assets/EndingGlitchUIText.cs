using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingGlitchUIText : MonoBehaviour
{
    [SerializeField] ExaltText exaltText;
    [SerializeField] Image backgroundImage;
    [SerializeField] public float delayAfterAppear = 0.5f;
    [SerializeField] public Color endBGColor;
    [SerializeField] public AudioClip _appearSound;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public AudioClip Appear() // should flash
    {
        StartCoroutine(AppearCoroutine());
        return _appearSound;
    }

    private IEnumerator AppearCoroutine()
    {
        // flash white
        //exaltText.enabled = true;

        backgroundImage.color = Color.white;
        foreach (Image character in exaltText.GetComponentsInChildren<Image>())
        {
            character.color = Color.black;
        }
        yield return new WaitForSecondsRealtime(0.05f);
        backgroundImage.color = endBGColor;
        foreach (Image character in exaltText.GetComponentsInChildren<Image>())
        {
            character.color = Color.white;
        }
    }
}
