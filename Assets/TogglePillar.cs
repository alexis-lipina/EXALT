using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePillar : MonoBehaviour
{
    [SerializeField] Switch _switch;
    [SerializeField] private EnvironmentPhysics _environmentPhysics;
    [SerializeField] private float OnTopHeight;
    [SerializeField] private float OffTopHeight;
    private float _desiredHeight;


    private bool _isOn;
    public bool IsOn {
        get
        {
            return _isOn;
        }
        set
        {
            _isOn = value;
            if (_isOn)
            {
                _desiredHeight = OnTopHeight;
            }
            else
            {
                _desiredHeight = OffTopHeight;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _desiredHeight = OffTopHeight;
    }

    // Update is called once per frame
    void Update()
    {
        IsOn = _switch.IsToggledOn;

        if (Mathf.Abs(_environmentPhysics.TopHeight - _desiredHeight) > 0.1f) //if not at desired height
        {
            float diff = Mathf.Lerp(_environmentPhysics.TopHeight, _desiredHeight, 0.1f) - _environmentPhysics.TopHeight;
            _environmentPhysics.TopHeight += diff;
            _environmentPhysics.BottomHeight += diff;
            _environmentPhysics.GetComponent<Transform>().position = new Vector3(gameObject.GetComponent<Transform>().position.x, gameObject.GetComponent<Transform>().position.y + diff, gameObject.GetComponent<Transform>().position.z);
            _environmentPhysics.GetComponent<BoxCollider2D>().offset = new Vector2(gameObject.GetComponent<BoxCollider2D>().offset.x, gameObject.GetComponent<BoxCollider2D>().offset.y - diff);
        }
    }
}
