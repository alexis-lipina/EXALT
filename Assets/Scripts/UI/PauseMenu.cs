using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{


    [SerializeField] private Button _resume;
    [SerializeField] private Button _options;
    [SerializeField] private Button _screenshot;
    [SerializeField] private Button _mainmenu;
    [SerializeField] private Button _quit;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private GameObject _energyBar;


    private bool _isPaused = false;
    private Player _controller;


    // Use this for initialization
    void Start ()
    {
        _controller = ReInput.players.GetPlayer(0);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (_controller.GetButtonDown("Pause"))
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                Time.timeScale = 0;
                _menuPanel.SetActive(true);
                _healthBar.SetActive(false);
                _energyBar.SetActive(false);
                EventSystem.current.SetSelectedGameObject(_resume.gameObject, new BaseEventData(EventSystem.current));
                _resume.Select();
                
            }
            else
            {
                Time.timeScale = 1;
                _menuPanel.SetActive(false);
                _healthBar.SetActive(true);
                _energyBar.SetActive(true);
            }
        }


    }


    //==============================| Button Press Methods

    public void ResumePressed()
    {
        Time.timeScale = 1;
        _menuPanel.SetActive(false);
    }

    public void OptionsPressed()
    {
        Time.timeScale = 1;
        //do something
        SceneManager.LoadScene("Demo_Arena_Empty");
    }

    public void QuitPressed()
    {
        //MainMenuPressed();
        //quit
        Application.Quit();
    }

    public void ScreenshotPressed()
    {
        //take screenshot, but for now its combat demo
        Time.timeScale = 1;
        SceneManager.LoadScene("Demo_Arena");
    }

    public void MainMenuPressed()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("SwitchBlocks");
        //go to main menu
    }
}
