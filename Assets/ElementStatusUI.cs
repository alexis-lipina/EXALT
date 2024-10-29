using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementStatusUI : MonoBehaviour
{
    [SerializeField] private Sprite spriteVoid_Gamepad;
    [SerializeField] private Sprite spriteZap_Gamepad;
    [SerializeField] private Sprite spriteFire_Gamepad;
    [SerializeField] private Sprite spriteIchor_Gamepad;
    [SerializeField] private Sprite spriteVoid_KBM;
    [SerializeField] private Sprite spriteZap_KBM;
    [SerializeField] private Sprite spriteFire_KBM;
    [SerializeField] private Sprite spriteIchor_KBM;
    [SerializeField] private Sprite spriteIchorOvertaken1_KBM;
    [SerializeField] private Sprite spriteIchorOvertaken1_Gamepad;    
    [SerializeField] private Sprite spriteIchorOvertaken2_KBM;
    [SerializeField] private Sprite spriteIchorOvertaken2_Gamepad;
    [SerializeField] private Sprite spriteIchorOvertaken3_KBM;
    [SerializeField] private Sprite spriteIchorOvertaken3_Gamepad;
    [SerializeField] private Sprite spriteStormOvertaken_KBM;
    [SerializeField] private Sprite spriteStormOvertaken_Gamepad;
    [SerializeField] private PlayerHandler _playerHandler;
    private ElementType _currentElement = ElementType.ZAP;
    private bool _isUsingMouse = false;

    private static float TimeToFadeOut = 3.0f;

    // Update is called once per frame
    void Update()
    {
        //fade in/out for combat
        if (_playerHandler.ForceUIVisible)
        {
            GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        }
        else if (_playerHandler.TimeSinceCombat > TimeToFadeOut)
        {
            const float lerpRate = 1.0f;
            float newAlpha = Mathf.Lerp(1.0f, 0.0f, ((_playerHandler).TimeSinceCombat - TimeToFadeOut) * lerpRate);
            GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, newAlpha);
        }
        else
        {
            GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(GetComponent<Image>().color.a, 1.0f, 0.2f));
        }
        if (_playerHandler.OvertakenElement != ElementType.NONE)
        {
            switch (_playerHandler.OvertakenElement)
            {
                case ElementType.ICHOR:
                    switch (_playerHandler.OvertakenElementAmount)
                    {
                        case 1:
                            if (_isUsingMouse)
                            {
                                GetComponent<Image>().sprite = spriteIchorOvertaken1_KBM;
                            }
                            else
                            {
                                GetComponent<Image>().sprite = spriteIchorOvertaken1_Gamepad;
                            }
                            break;
                        case 2:
                            if (_isUsingMouse)
                            {
                                GetComponent<Image>().sprite = spriteIchorOvertaken2_KBM;
                            }
                            else
                            {
                                GetComponent<Image>().sprite = spriteIchorOvertaken2_Gamepad;
                            }
                            break;
                        case 3:
                            if (_isUsingMouse)
                            {
                                GetComponent<Image>().sprite = spriteIchorOvertaken3_KBM;
                            }
                            else
                            {
                                GetComponent<Image>().sprite = spriteIchorOvertaken3_Gamepad;
                            }
                            break;
                    }
                    break;
                case ElementType.ZAP:
                    if (_isUsingMouse)
                    {
                        GetComponent<Image>().sprite = spriteStormOvertaken_KBM;
                    }
                    else
                    {
                        GetComponent<Image>().sprite = spriteStormOvertaken_Gamepad;
                    }
                    break;
            }
        }

        if (_playerHandler.GetElementalAttunement() != _currentElement || _isUsingMouse != _playerHandler.IsUsingMouse)
        {
            _isUsingMouse = _playerHandler.IsUsingMouse;
            _currentElement = _playerHandler.GetElementalAttunement();
            switch (_currentElement)
            {
                case ElementType.FIRE:

                    if (_isUsingMouse)
                    {
                        GetComponent<Image>().sprite = spriteFire_KBM;
                    }
                    else
                    {
                        GetComponent<Image>().sprite = spriteFire_Gamepad;
                    }
                    break;
                case ElementType.VOID:
                    if (_isUsingMouse)
                    {
                        GetComponent<Image>().sprite = spriteVoid_KBM;
                    }
                    else
                    {
                        GetComponent<Image>().sprite = spriteVoid_Gamepad;
                    }
                    break;
                case ElementType.ZAP:
                    if (_isUsingMouse)
                    {
                        GetComponent<Image>().sprite = spriteZap_KBM;
                    }
                    else
                    {
                        GetComponent<Image>().sprite = spriteZap_Gamepad;
                    }
                    break;
                case ElementType.ICHOR:
                    if (_isUsingMouse)
                    {
                        GetComponent<Image>().sprite = spriteIchor_KBM;
                    }
                    else
                    {
                        GetComponent<Image>().sprite = spriteIchor_Gamepad;
                    }
                    break;
            }
        }
    }
}
