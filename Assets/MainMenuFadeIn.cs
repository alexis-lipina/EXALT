using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuFadeIn : MonoBehaviour
{
    [SerializeField] float _speed;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerHandler.PREVIOUS_SCENE == "")
        {
            //StartCoroutine(PlayFade(_speed));
        }
    }

    IEnumerator PlayFade(float speed)
    {
        float opacity = 1f; ;
        Color col = GetComponent<Image>().material.color;
        col.a = opacity;
        GetComponent<Image>().material.color = col;
        while (opacity > 0)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            opacity -= 0.01f * speed;
            col.a = opacity;
            GetComponent<Image>().material.color = col;
        }
        GetComponent<Image>().enabled = false;
    }
}
