using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPillarTest : MonoBehaviour
{
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _collapsingSprite;
    [SerializeField] private TriggerVolume _trigger;
    [SerializeField] float _timeToCollapse;
    [SerializeField] float _timeToReturn;
    private bool _isTouched;
    private bool _isCollapsed;
    private float _timer;


    // Use this for initialization
    void Start ()
    {
        _timer = 0.0f;
        _isCollapsed = false;

	}
	
	// Update is called once per frame
	void Update ()
    {
		if (_trigger.IsTriggered && !_isTouched) //determine whether triggered to begin collapse
        {
            _isTouched = true;
            gameObject.GetComponent<SpriteRenderer>().sprite = _collapsingSprite;
        }

        if (_isTouched && !_isCollapsed) //determine whether to collapse
        {
            if (_timer < _timeToCollapse)
            {
                _timer += Time.deltaTime;
            }
            else //collapse
            {
                _isCollapsed = true;
                _isTouched = false;
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
                gameObject.GetComponent<EnvironmentPhysics>().enabled = false;
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                Debug.Log("<color=red>Deactivated</color>");
                _timer = 0.0f;
            }
        }
        if (_isCollapsed) //determine if needs to be restored
        {
            if (_timer < _timeToReturn)
            {
                _timer += Time.deltaTime;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = _defaultSprite;
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
                gameObject.GetComponent<EnvironmentPhysics>().enabled = true;
                gameObject.GetComponent<BoxCollider2D>().enabled = true;
                Debug.Log("<color=green>Activated</color>");
                _timer = 0.0f;
                _isCollapsed = false;
                _isTouched = false;
            }
        }
	}
}
