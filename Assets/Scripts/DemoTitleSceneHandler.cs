using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class DemoTitleSceneHandler : MonoBehaviour
{
    float alpha;
    bool isIncreasing;


    void Start()
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject);
        Debug.Log("Start!");
        StartCoroutine(FlashStartButton());
        alpha = 0;
        isIncreasing = true;
    }

    public void ContinuePressed()
    {
        Debug.Log("Press!!!");
        SceneManager.LoadScene("FirstLevel");
    }

    IEnumerator FlashStartButton()
    {
        Debug.Log("Flashing button!");
       
        do
        {

            if (isIncreasing)
            {
                this.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                alpha += 0.01f; 
                if (alpha > 0.9f)
                {
                    isIncreasing = false;
                }
            }
            else
            {
                this.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                alpha -= 0.01f;
                if (alpha < 0.1f)
                {
                    isIncreasing = true;
                }

            }

            yield return new WaitForEndOfFrame();
            //yield return new WaitForSeconds(0.01f);
            


            /*
            this.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(1.0f);
            this.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(1.0f);
            */
            
        }
        while (true);
    }
}
