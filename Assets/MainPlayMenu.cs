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
        SceneManager.LoadScene("Demo_Arena");
    }

    public void DemoStoryButtonPressed()
    {
        SceneManager.LoadScene("CampaignStart");
    }

    public void FinalBossButtonPressed()
    {
        SceneManager.LoadScene("FinalBoss");
    }

    public void OnBackToMainMenuPressed()
    {
        gameObject.SetActive(false);
        SourceMenu.SetActive(true);
    }
}
