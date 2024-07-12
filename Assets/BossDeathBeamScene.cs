using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeathBeamScene : MonoBehaviour
{
    private CameraScript _camera;
    [SerializeField] private AnimationCurve _cameraYCurve;
    [SerializeField] private AnimationCurve _cameraXCurve;
    [SerializeField] private float _duration = 8.0f;
    [SerializeField] private float _exitLevelDuration = 8.0f;
    [SerializeField] ExitVolume exit;

    [SerializeField] Texture2D PP_Black;
    [SerializeField] Texture2D PP_White;


    public void PlayDeathScene()
    {
        StartCoroutine(DeathSceneCoroutine());
        StartCoroutine(ChangeSceneEarlyCoroutine());
        _camera.SetPostProcessParam("_ShatterMaskTex", PP_White);
        _camera.SetPostProcessParam("_CrackTex", PP_Black);
        _camera.SetPostProcessParam("_OffsetTex", PP_Black);
    }

    IEnumerator DeathSceneCoroutine()
    {
        float timer = 0.0f;
        _camera = GameObject.FindObjectOfType<CameraScript>();
        Vector3 cameraStartPosition = _camera.transform.position;

        while (timer < _duration)
        {
            _camera.transform.position = cameraStartPosition + new Vector3(_cameraXCurve.Evaluate(timer), _cameraYCurve.Evaluate(timer), 0);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator ChangeSceneEarlyCoroutine()
    {
        yield return new WaitForSeconds(_duration - 1.0f);
        FadeTransition.Singleton.SetFadeColor("1,1,1,1");
        exit.ChangeLevel();
    }

    // Start is called before the first frame update
    void Start()
    {
    }
}
