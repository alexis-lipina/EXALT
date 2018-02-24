using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] private Button PlayDemoButton;
    [SerializeField] private Button ControlsButton;
    [SerializeField] private Button QuitButton;



    // Use this for initialization
    void Start ()
    {
        EventSystem.current.SetSelectedGameObject(PlayDemoButton.gameObject);
	}
	
	public void PlayDemoPressed()
    {
        //begin game
    }
    public void ControlsPressed()
    {
        //Change to Controls scene
    }
    public void QuitPressed()
    {
        Application.Quit();
    }

    void Update()
    {
        if (PlayDemoButton.isActiveAndEnabled && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            PlayDemoPressed();
        }
    }
}
