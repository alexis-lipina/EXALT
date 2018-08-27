using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwitcher : MonoBehaviour
{
    [SerializeField]    private Sprite _untriggeredSprite;
    [SerializeField]    private Sprite _triggeredSprite;

    private TriggerVolume _trigger;
    private SpriteRenderer _renderer;
    private bool _currentlyTriggered;



	// Use this for initialization
	void Awake()
    {
        _currentlyTriggered = false;
        _trigger = this.gameObject.GetComponent<TriggerVolume>();
        _renderer = this.gameObject.GetComponent<SpriteRenderer>();
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (_trigger.IsTriggered && !_currentlyTriggered)
        {
            _currentlyTriggered = true;
            _renderer.sprite = _triggeredSprite;
        }
        else if (!_trigger.IsTriggered && _currentlyTriggered)
        {
            _currentlyTriggered = false;
            _renderer.sprite = _untriggeredSprite;
        }
	}
}
