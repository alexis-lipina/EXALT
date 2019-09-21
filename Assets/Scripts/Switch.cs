using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    private bool _isToggledOn;
    public bool IsToggledOn {
        get
        {
            return _isToggledOn;
        }
        protected set
        {
            if (value == _isToggledOn) return; //short circuit
            if (value)
            {
                _frontSpriteRenderer.sprite = _frontSprite_On;
                _topSpriteRenderer.sprite = _topSprite_On;
                _glowRenderer.enabled = true;
            }
            else
            {
                _frontSpriteRenderer.sprite = _frontSprite_Off;
                _topSpriteRenderer.sprite = _topSprite_Off;
                _glowRenderer.enabled = false;
            }
            _isToggledOn = value;
        }
    }

    [SerializeField] protected bool _defaultState;
    [SerializeField] protected SwitchPhysics _switchPhysics;
    protected bool _switchPhysicsPreviousState;

    [SerializeField] protected SpriteRenderer _frontSpriteRenderer;
    [SerializeField] protected SpriteRenderer _topSpriteRenderer;
    [SerializeField] protected SpriteRenderer _glowRenderer;

    [SerializeField] protected Sprite _frontSprite_On;
    [SerializeField] protected Sprite _frontSprite_Off;
    [SerializeField] protected Sprite _topSprite_On;
    [SerializeField] protected Sprite _topSprite_Off;

    protected virtual void Start()
    {
        IsToggledOn = _defaultState;
        _glowRenderer.enabled = _defaultState;
    }

}
