using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningCinematicManager : MonoBehaviour
{
    [SerializeField] private CameraScript _camera;

    [SerializeField] private Transform _startPosition;
    [SerializeField] private Transform _endPosition;
    [SerializeField] private AnimationCurve _cameraMotionCurve;
    [SerializeField] private float _duration;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerHandler.PREVIOUS_SCENE == "NewGameMainMenu")
            StartCoroutine(CameraPanDown(_duration));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CameraPanDown(float duration)
    {
        float timer = 0;
        _camera.enabled = false;
        while (timer < duration)
        {
            Vector3 camPos = _camera.transform.position;
            camPos.x = _startPosition.position.x;
            camPos.y = Mathf.Lerp(_startPosition.position.y, _endPosition.position.y, _cameraMotionCurve.Evaluate(timer/duration));
            _camera.transform.position = camPos;
            yield return new WaitForSecondsRealtime(0.01f);
            timer += 0.01f;
        }
        _camera.enabled = true;
        Debug.Log("YO ITS DONE");
    }
}
