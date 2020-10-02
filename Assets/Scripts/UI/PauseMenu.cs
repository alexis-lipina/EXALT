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
    [SerializeField] private Button _defaultSelectedButton;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private GameObject _energyBar;
    [Space(10)]
    [SerializeField] private ControlMenuManager controlMenuManager;
    [SerializeField] private AccessibilityMenu accessibilityMenuManager;

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

    public void AccessibilityPressed()
    {
        gameObject.SetActive(false);
        accessibilityMenuManager.SourceUIMenu = gameObject;
        accessibilityMenuManager.gameObject.SetActive(true);
    }

    public void ControlsPressed()
    {
        gameObject.SetActive(false);
        controlMenuManager.Source_Menu = gameObject;
        controlMenuManager.gameObject.SetActive(true);
    }

    public void QuitToMainMenuPressed()
    {
        //quit
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MainMenu");
    }

    // ===========================| Pause stuff
    void OnEnable()
    {
        StartCoroutine(OnPause());
    }

    private IEnumerator OnPause()
    {
        Debug.Log("SETTING THE SELECTED OBJECT");
        EventSystem.current.SetSelectedGameObject(null, new BaseEventData(EventSystem.current));
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(_defaultSelectedButton.gameObject, new BaseEventData(EventSystem.current));
    }
}
