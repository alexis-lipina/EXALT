using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [SerializeField] private string _command;
    [SerializeField] private Color DEFAULT_COLOR;
    [SerializeField] private Color SELECT_COLOR;
    [SerializeField] private Color CLICK_COLOR;

    [SerializeField] private Color DEFAULT_TEXT_COLOR;
    [SerializeField] private Color SELECTED_TEXT_COLOR;
    [SerializeField] private Color CLICK_TEXT_COLOR;


    [SerializeField] private Image _text;
	// Use this for initialization
	void Start ()
    {
        Debug.Log("run");
        gameObject.GetComponent<Image>().color = DEFAULT_COLOR;
        _text.color = DEFAULT_TEXT_COLOR;
	}


    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Button>().Select();
    }

    public void OnSelect(BaseEventData eventData)
    {
        //Debug.Log("I've been selected!");
        gameObject.GetComponent<Image>().color = SELECT_COLOR;
        _text.color = SELECTED_TEXT_COLOR;
    }

    public void OnDeselect(BaseEventData data)
    {
        _text.color = DEFAULT_TEXT_COLOR;
        gameObject.GetComponent<Image>().color = DEFAULT_COLOR;
    }

    public void Pressed()
    {
        Debug.Log("Click!");
        gameObject.GetComponent<Image>().color = CLICK_COLOR;
        _text.color = CLICK_TEXT_COLOR;
    }

}
