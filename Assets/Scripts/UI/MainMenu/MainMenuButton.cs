using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

[RequireComponent(typeof(Animator))]
public class MainMenuButton : MonoBehaviour
{

    [SerializeField] private Image GlowEffect;
    private Button _button;
    private bool isUsingMouse;

    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ReInput.players.GetPlayer(0).GetAxis("MouseSwitch") != 0)
        {
            isUsingMouse = true;
        }
        if (ReInput.players.GetPlayer(0).GetAxis("MoveVertical") != 0)
        {
            isUsingMouse = false;
        }
    }

}
