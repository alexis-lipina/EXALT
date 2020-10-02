using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ExaltText))]
public class ExaltTextButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        var images = GetComponent<ExaltText>().GetComponentsInChildren<Image>();
        foreach (Image i in images)
        {
            i.color = new Color(0, 1, 1);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        var images = GetComponent<ExaltText>().GetComponentsInChildren<Image>();
        foreach (Image i in images)
        {
            i.color = new Color(1, 1, 1);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
