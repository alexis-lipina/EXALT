using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

// Controls pretty much everything that goes on in the main menu that isn't UI stuff not unique to the main menu
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _foregroundTerrain;
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private float _cinematicPanDuration;
    //[SerializeField] private float _cinematicSpeed_Fast;
    [SerializeField] private float _uiFadeInSpeed;
    [SerializeField] private AnimationCurve _uiCurve;
    [SerializeField] private float _timeBetweenMusicAndUI;

    [SerializeField] private Image _titleGlow;
    [SerializeField] private Image[] _uiToFadeIn;

    [SerializeField] private Vector3 cameraStart;
    [SerializeField] private Vector3 cameraEnd;
    [SerializeField] private Vector3 foregroundStart;
    [SerializeField] private Vector3 foregroundEnd;

    [SerializeField] private Button _defaultSelectedButton;
    [SerializeField] private Button[] _buttonsToEnableWhenDone;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("Canvas").GetComponent<CanvasScaler>().scaleFactor = AccessibilityOptionsSingleton.GetInstance().UIScale;
        StartCoroutine(StartupAnimation());
    }

    IEnumerator StartupAnimation()
    {
        //fade in looking up at the sky
        _camera.transform.position = cameraStart;
        foreach (Image img in _uiToFadeIn)
        {
            img.color = new Color(1, 1, 1, 0);
        }
        foreach (var button in _buttonsToEnableWhenDone)
        {
            button.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(.2f);
        StartCoroutine(DelayStartMusic());

        //slowly pan down
        float interpVal = 0;
        while (interpVal < _cinematicPanDuration)
        {
            yield return null;
            interpVal += Time.deltaTime;
            _camera.transform.position = Vector3.Lerp(cameraStart, cameraEnd, _animationCurve.Evaluate(interpVal / _cinematicPanDuration));
            _foregroundTerrain.transform.position = Vector3.Lerp(foregroundStart, foregroundEnd, _animationCurve.Evaluate(interpVal));
        }

        foreach (var button in _buttonsToEnableWhenDone)
        {
            button.gameObject.SetActive(true);
        }
        _defaultSelectedButton.Select();

        //fade in UI
        interpVal = 0;
        while (interpVal < 1)
        {
            yield return null;
            interpVal += Time.deltaTime * _uiFadeInSpeed;
            foreach (Image img in _uiToFadeIn)
            {
                img.color = new Color(1, 1, 1, _uiCurve.Evaluate(interpVal));
            }
        }
        
        interpVal = 1;
        while (interpVal > 0)
        {
            interpVal -= Time.deltaTime;
            _titleGlow.color = new Color(1, 1, 1, _uiCurve.Evaluate(interpVal));
            yield return null;
        }
    }

    IEnumerator DelayStartMusic()
    {
        yield return new WaitForSeconds(_timeBetweenMusicAndUI);
        GetComponent<AudioSource>().Play();
    }
}
