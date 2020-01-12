using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// These sprites appear when the player is within the trigger volume, and fade otherwise
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class OcclusionSprite : MonoBehaviour
{
    [SerializeField] private TriggerVolume _trigger;

    private bool _playerIsWithin = false;
    private float _targetOpacity;
    private float _currentOpacity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_trigger.IsTriggered)
        {
            _targetOpacity = 1.0f;
        }
        else
        {
            _targetOpacity = 0.0f;
        }
        _currentOpacity = Mathf.Lerp(_currentOpacity, _targetOpacity, 0.1f);

        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, _currentOpacity);
    }
}
