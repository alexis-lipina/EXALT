using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] private Button PlayDemoButton;
    [SerializeField] private Button ControlsButton;
    [SerializeField] private Button QuitButton;

    [SerializeField] private GameObject ControlsImage;

    private bool controlsVisible;

    // Use this for initialization
    void Start ()
    {
        EventSystem.current.SetSelectedGameObject(PlayDemoButton.gameObject);
        controlsVisible = false;
	}
	
	public void PlayDemoPressed()
    {
        SceneManager.LoadScene("WaterfallArena", LoadSceneMode.Single);
        //begin game
        
    }
    public void ControlsPressed()
    {
        //Show Controls if they arent already visible
        if (!controlsVisible)
        {
            ControlsImage.SetActive(true);
            controlsVisible = true;
        }
        else
        {
            ControlsImage.SetActive(false);
            controlsVisible = false;
        }

    }
    public void QuitPressed()
    {
        //Application.Quit();
        SceneManager.LoadScene("TitleScreen");
    }

    void Update()
    {
        if (PlayDemoButton.isActiveAndEnabled && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            PlayDemoPressed();
        }
    }
}
