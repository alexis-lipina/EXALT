using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAttractor : MonoBehaviour
{
    [Tooltip("If player enters this volume, this CameraAttractor will activate")]
    [SerializeField] private TriggerVolume _activateVolume;
    [SerializeField] private CameraScript _camera;
    [Tooltip("0 = no effect, 1 = equidistant")]
    [SerializeField] private float _pullMagnitude;

    [Space(10)]

    [SerializeField] bool UseCurve; // whether we want to use the attractioncurve method
    [SerializeField] AnimationCurve AttractionCurve;
    enum Axis { x, y, z };
    [SerializeField] Axis AttractionAxis;
    [SerializeField] EntityPhysics PlayerPhysics;
    float CurrentCurveValue;
    Vector3 TriggerSize;


    private bool _isPullingCamera = false;

    public float PullMagnitude
    {
        get 
        {
            if (UseCurve) return AttractionCurve.Evaluate(CurrentCurveValue);
            return _pullMagnitude; 
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        TriggerSize = new Vector3(
            _activateVolume.GetComponent<BoxCollider2D>().size.x,
            _activateVolume.GetComponent<BoxCollider2D>().size.y,
            _activateVolume.GetTopHeight() - _activateVolume.GetBottomHeight()
            );
    }

    // Update is called once per frame
    void Update()
    {
        //when player first enters volume
        if (_activateVolume.IsTriggered && !_isPullingCamera)
        {
            _isPullingCamera = true;
            _camera.AddAttractor(this);
        }
        else if (!_activateVolume.IsTriggered && _isPullingCamera)
        {
            _isPullingCamera = false;
            _camera.RemoveAttractor(this);
        }

        if (_isPullingCamera)
        {
            switch (AttractionAxis)
            {
                case Axis.x:
                    CurrentCurveValue = (PlayerPhysics.transform.position.x - _activateVolume.transform.position.x) / TriggerSize.x + 0.5f;
                    break;
                case Axis.y:
                    CurrentCurveValue = (PlayerPhysics.transform.position.y - _activateVolume.transform.position.y) / TriggerSize.y + 0.5f;
                    break;
                case Axis.z:
                    CurrentCurveValue = Mathf.InverseLerp(_activateVolume.GetBottomHeight(), _activateVolume.GetTopHeight(), PlayerPhysics.GetObjectElevation());
                    break;
            }
        }
    }
    public void SetPullMagnitude(float value)
    {
        _pullMagnitude = value;
    }
}
