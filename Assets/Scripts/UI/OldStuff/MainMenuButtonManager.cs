using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Rewired;

public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] private MainOptionsMenu _optionsMenuManager;
    [SerializeField] private MainPlayMenu _playMenuManager;
    [SerializeField] private Button _defaultSelectedButton;

    private bool controlsVisible;
    private bool _currentlyGamepad = false;
    private Player _player;


    // Use this for initialization
    void Start ()
    {
        _optionsMenuManager.gameObject.SetActive(false);
        _player = ReInput.players.GetPlayer(0);
    }

    public void OnEnable()
    {
        _defaultSelectedButton.Select();
    }

    void Update()
    {
        if (_player.controllers.GetLastActiveController() != null)
        {
            if (_currentlyGamepad && _player.controllers.GetLastActiveController().type == ControllerType.Mouse || _player.controllers.GetLastActiveController().type == ControllerType.Keyboard)
            {
                _currentlyGamepad = false;
            }
            if (!_currentlyGamepad && _player.controllers.GetLastActiveController().type == ControllerType.Joystick)
            {
                _currentlyGamepad = true;
                _defaultSelectedButton.Select();
            }
        }
    }

    public void PlayPressed()
    {
        //present play screen
        gameObject.SetActive(false);
        _playMenuManager.gameObject.SetActive(true);
        _playMenuManager.SourceMenu = this.gameObject;
    }
    public void OptionsPressed()
    {
        //present options screen
        gameObject.SetActive(false);
        _optionsMenuManager.gameObject.SetActive(true);
        _optionsMenuManager.SourceMenu = this.gameObject;
    }
    public void QuitPressed()
    {
        //probably should do an "are you sure?" type thing
        Application.Quit();
    }
}
