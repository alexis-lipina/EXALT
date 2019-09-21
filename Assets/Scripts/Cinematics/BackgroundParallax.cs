using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used for backgrounds and cinematics involving background elements which must move with the camera to give a parallax-like effect
/// </summary>
public class BackgroundParallax : MonoBehaviour
{
    [SerializeField] private CameraScript _camera;
    [Tooltip("Affects the strength of the parallax effect : 0 = no effect, n > 0 = 'farther back'")]
    [SerializeField] private Vector2 _strength;
    [SerializeField] private Vector2 _offset;

    private Vector3 _originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        _originalPosition = transform.position + new Vector3(_offset.x, _offset.y, 0f);
    }

    // Update is called once per frame
    void Update()
    {

        //Perform parallax motion
        //when camera is dead-on on top of the object's original position the object should be at its original position
        //when the camera moves, the object is slightly pulled in the direction of the camera to reflect its Depth field
        Vector3 parallax = _camera.transform.position - _originalPosition;
        parallax = new Vector3(parallax.x * _strength.x, parallax.y * _strength.y, 0f);
        transform.position = _originalPosition + parallax;
    }
}
