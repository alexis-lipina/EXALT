using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] private Button DogButton;
    [SerializeField] private Button ArenaButton;
    [SerializeField] private Button PillarButton;
    [SerializeField] private Button WavyButton;

    [SerializeField] private GameObject ControlsImage;

    private bool controlsVisible;

    // Use this for initialization
    void Start ()
    {
        EventSystem.current.SetSelectedGameObject(DogButton.gameObject);
        controlsVisible = false;
	}
	
	public void DogPressed()
    {
        SceneManager.LoadScene("WaterfallArenaNoEnemies", LoadSceneMode.Single);
        //begin game
        
    }
    public void ArenaPressed()
    {
        /*
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
        */
        //its not controls anymore...
        SceneManager.LoadScene("ArenaScene", LoadSceneMode.Single);
    }
    public void PillarPressed()
    {
        //Application.Quit();
        SceneManager.LoadScene("ElevatorRoom", LoadSceneMode.Single);
    }

    public void WavyPressed()
    {
        SceneManager.LoadScene("RipplingPillars", LoadSceneMode.Single);
    }

    void Update()
    {
        /*
        if (PlayDemoButton.isActiveAndEnabled && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            PlayDemoPressed();
        }
        */
    }
}
