using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition Singleton;
    private bool _hasBeganToExit = false;
    
    private void Awake()
    {
        Singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine( FadeInTransition() );
    }

    public void FadeToScene(string levelName)
    {
        if (!_hasBeganToExit)
        {
            StartCoroutine(FadeOutTransition(levelName));
            _hasBeganToExit = true;
        }
    }


    private IEnumerator FadeInTransition(float rate = 2f)
    {
        _hasBeganToExit = false;
        GetComponent<Image>().color = new Color(0, 0, 0, 1);
        float opacity = 1f;
        while (opacity > 0)
        {
            yield return new WaitForSeconds(0.01f);
            GetComponent<Image>().color = new Color(0, 0, 0, opacity);
            opacity -= 0.02f * rate;
        }
        GetComponent<Image>().color = new Color(0, 0, 0, 0);
    }

    private IEnumerator FadeOutTransition(string sceneName, float rate = 2f)
    {
        GetComponent<Image>().color = new Color(0, 0, 0, 0);
        float opacity = 0f;
        while (opacity < 1)
        {
            yield return new WaitForSeconds(0.01f);
            GetComponent<Image>().color = new Color(0, 0, 0, opacity);
            opacity += 0.02f * rate;
        }
        GetComponent<Image>().color = new Color(0, 0, 0, 1);
        PlayerHandler.PREVIOUS_SCENE = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }
}
