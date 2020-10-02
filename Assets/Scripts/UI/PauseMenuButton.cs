using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [SerializeField] private Color NORMAL_COLOR;
    [SerializeField] private Color SELECT_COLOR;

	// Use this for initialization
	void Start ()
    {
        foreach (Image i in GetComponentsInChildren<Image>())
        {
            i.color = NORMAL_COLOR;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Button>().Select();
    }

    public void OnSelect(BaseEventData eventData)
    {
        foreach (Image i in GetComponentsInChildren<Image>())
        {
            i.color = SELECT_COLOR;
        }
    }

    public void OnDeselect(BaseEventData data)
    {
        foreach (Image i in GetComponentsInChildren<Image>())
        {
            i.color = NORMAL_COLOR;
        }
    }
}
