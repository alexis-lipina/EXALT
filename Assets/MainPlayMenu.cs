using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPlayMenu : MonoBehaviour
{
    public GameObject SourceMenu;

    private void OnEnable()
    {
        GetComponentInChildren<Button>().Select();
    }
    public void TutorialButtonPressed()
    {
        SceneManager.LoadScene("CombatTraining");
    }

    public void ArenaButtonPressed()
    {
        ((FadeTransition)GameObject.FindObjectOfType(typeof(FadeTransition))).FadeToScene("Demo_Arena", "");
    }

    public void DemoStoryButtonPressed()
    {
        ((FadeTransition)GameObject.FindObjectOfType(typeof(FadeTransition))).FadeToScene("HugeElevator", "");
    }

    public void FinalBossButtonPressed()
    {
        ((FadeTransition)GameObject.FindObjectOfType(typeof(FadeTransition))).FadeToScene("MonolithStart", "");
    }

    public void OnBackToMainMenuPressed()
    {
        gameObject.SetActive(false);
        SourceMenu.SetActive(true);
    }
}
