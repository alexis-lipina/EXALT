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

    private bool _isPullingCamera = false;

    public float PullMagnitude
    {
        get { return _pullMagnitude; }
    }

    // Start is called before the first frame update
    void Start()
    {
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
    }
}
