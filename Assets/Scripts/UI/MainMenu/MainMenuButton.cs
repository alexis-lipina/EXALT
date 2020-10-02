using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class MainMenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Button _button;
    private Image _image;

    private Coroutine _currentGlowCoroutine = null;
    private float _glowAlpha = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _image.material = new Material(_image.material);
        _image.material.SetFloat("_Opacity", 0.0f);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        _button.Select();
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (eventData.selectedObject == gameObject)
        {
            Debug.Log("Epic");
            if (_currentGlowCoroutine != null) StopCoroutine(_currentGlowCoroutine);
            _currentGlowCoroutine = StartCoroutine(OnSelectGlow());
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (eventData.selectedObject == gameObject)
        {
            Debug.Log("Unepic");
            if (_currentGlowCoroutine != null) StopCoroutine(_currentGlowCoroutine);
            _currentGlowCoroutine = StartCoroutine(OnDeselectGlow());
        }
    }

    IEnumerator OnSelectGlow()
    {
        while (_glowAlpha < 1.0f)
        {
            yield return null;
            _glowAlpha += Time.deltaTime * 10.0f;
            _image.material.SetFloat("_Opacity", _glowAlpha);
        }
        while (true)
        {
            while (_glowAlpha > 0.6f)
            {
                yield return null;
                _glowAlpha -= Time.deltaTime * 0.2f;
                _image.material.SetFloat("_Opacity", _glowAlpha);
            }
            while (_glowAlpha < 0.9f)
            {
                yield return null;
                _glowAlpha += Time.deltaTime * 0.2f;
                _image.material.SetFloat("_Opacity", _glowAlpha);
            }
        }
    }

    IEnumerator OnDeselectGlow()
    {
        while (_glowAlpha > 0.0f)
        {
            yield return null;
            _glowAlpha -= Time.deltaTime * 3.0f;
            _image.material.SetFloat("_Opacity", _glowAlpha);
        }
    }

    void OnEnable()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            OnSelect(new BaseEventData(EventSystem.current));
        }
    }
    void OnDisable()
    {
        if (_currentGlowCoroutine != null)
        {
            StopCoroutine(_currentGlowCoroutine);
        }
        _glowAlpha = 0.0f;
        _image.material.SetFloat("_Opacity", _glowAlpha);
    }
}
