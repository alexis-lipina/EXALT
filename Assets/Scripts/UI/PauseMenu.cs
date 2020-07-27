using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


/// <summary>
/// Holy shit this is in such need of a rework, I did this horridly
/// </summary>
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
    [Space(10)]
    [SerializeField] private ControlMenuManager controlMenuManager;

    private bool _isCurrentlyPaused = false;
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
            if (_menuPanel.activeSelf && _isCurrentlyPaused || !_isCurrentlyPaused)
            {
                SetPaused(!_isCurrentlyPaused);
            }
        }
    }
    
    private void SetPaused(bool isToBePaused)
    {
        if (isToBePaused)
        {
            Time.timeScale = 0;
            _menuPanel.SetActive(true);
            _healthBar.SetActive(false);
            _energyBar.SetActive(false);
            //_resume.Select();
            StartCoroutine(OnPause());
        }
        else
        {
            Time.timeScale = 1;
            _menuPanel.SetActive(false);
            _healthBar.SetActive(true);
            _energyBar.SetActive(true);
        }
        _isCurrentlyPaused = isToBePaused;
    }

    //==============================| Button Press Methods

    public void ResumePressed()
    {
        SetPaused(false);
    }

    public void OptionsPressed()
    {
        //open options, but for now its tutorial level
        Time.timeScale = 1;
        //do something
        SceneManager.LoadScene("CombatTraining");
    }

    public void QuitPressed()
    {
        //MainMenuPressed();
        //quit
        Application.Quit();
    }

    public void ScreenshotPressed()
    {
        //take screenshot, but for now its arena
        Time.timeScale = 1;
        SceneManager.LoadScene("Demo_Arena");
    }

    public void MainMenuPressed()
    {
        //Time.timeScale = 1;
        //SceneManager.LoadScene("SwitchBlocks");

        //temporarily, this is the options button
        controlMenuManager.gameObject.SetActive(true);
        controlMenuManager.Source_Menu = gameObject;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        StartCoroutine(OnPause());
    }

    private IEnumerator OnPause()
    {
        EventSystem.current.SetSelectedGameObject(null, new BaseEventData(EventSystem.current));
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(_resume.gameObject, new BaseEventData(EventSystem.current));
    }
}
