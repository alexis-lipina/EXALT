using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CameraScript : MonoBehaviour
{
    private const float OFFSET_MAGNITUDE_X = 16f * 0.75f;
    private const float OFFSET_MAGNITUDE_Y = 9f * 0.75f;
    [SerializeField] private EntityPhysics _playerPhysics;
    public Transform player;
    public float smoothTime;
    public float lerpAmount = 0.5f;
    //[SerializeField] private InputHandler input;
    private Player controller;
    private Vector3 velocity = Vector3.zero;
    private bool _isUsingCursor;
    private Vector2 _cursorWorldPos;
    private Camera _camera;
    [SerializeField] AnimationCurve _cameraSizeChangeEaseCurve;
    [SerializeField] List<Material> _postProcessMaterials;

    private List<CameraAttractor> _currentAttractors;
    public bool TrackPlayer = true;

    public bool IsUsingMouse
    {
        get { return _isUsingCursor; }
        set { _isUsingCursor = value; }
    }

    public void SetCameraSizeImmediate(float newSize)
    {
        gameObject.transform.localScale = Vector3.one * newSize;
        newSize *= 16.875f;
        _camera.orthographicSize = newSize;
    }

    public void AddAttractor(CameraAttractor attractor)
    {
        if (!_currentAttractors.Contains(attractor))
        {
            _currentAttractors.Add(attractor);
        }
        else
        {
            Debug.LogWarning("AddAttractor() already in list!");
        }
    }

    public void RemoveAttractor(CameraAttractor attractor)
    {
        if (_currentAttractors.Contains(attractor))
        {
            _currentAttractors.Remove(attractor);
        }
        else
        {
            Debug.LogWarning("RemoveAttractor() attempt failed : CameraAttractor not in list!");
        }
    }


    private void Awake()
    {
        controller = ReInput.players.GetPlayer(0);
        _currentAttractors = new List<CameraAttractor>();
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 targetPosition = UpdateTargetPosition();
        foreach (CameraAttractor attractor in _currentAttractors)
        {
            if (!attractor) continue;
            targetPosition = (targetPosition + attractor.transform.position * attractor.PullMagnitude) / (attractor.PullMagnitude + 1f);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        //transform.position = Vector3.Lerp(transform.position, targetPosition, lerpAmount);
    }

    /// <summary>
    /// Updates the position the camera follows.
    /// </summary>
    Vector3 UpdateTargetPosition()
    {
        if (!TrackPlayer) { return transform.position; }
        Vector3 pos = player.TransformPoint(new Vector3(0f, 0f, -300f));
        Vector2 offset;
        if (_isUsingCursor)
        {
            offset = new Vector2(_cursorWorldPos.x, _cursorWorldPos.y - _playerPhysics.GetBottomHeight() - 1f) - (Vector2)_playerPhysics.GetComponent<Transform>().position;
            offset *= 0.05f;
            if (offset.sqrMagnitude > 1f)
            {
                offset.Normalize();
            }
        }
        else
        {
            offset = controller.GetAxis2DRaw("LookHorizontal", "LookVertical"); //moves camera in direction the stick is pointing
            if (offset.magnitude > 0.1f)
            {
                offset = controller.GetAxis2DRaw("LookHorizontal", "LookVertical");
            }
            else
            {
                offset = controller.GetAxis2DRaw("MoveHorizontal", "MoveVertical");
            }
        }
        pos.Set(pos.x + offset.x * OFFSET_MAGNITUDE_X, pos.y + offset.y * OFFSET_MAGNITUDE_Y, pos.z);
        return pos;
    }

    //------------| Camera Effects (wrapper methods for coroutines)
    
    /// <summary>
    /// Vibrates the camera.
    /// </summary>
    /// <param name="intensity">Max distance the camera is allowed to move</param>
    /// <param name="repetitions">How often the camera's position is moved</param>
    /// <param name="timeBetweenJolts">Time between position adjustments</param>
    public void Shake(float intensity, int repetitions, float timeBetweenJolts)
    {
        //Debug.Log("Camera shaking!");

        StartCoroutine(CameraShake(intensity, repetitions, timeBetweenJolts));
    }

    /// <summary>
    /// Jolt camera in a random direction
    /// </summary>
    /// <param name="intensity">Distance camera is moved</param>
    public void Jolt(float intensity)
    {
        System.Random rand = new System.Random();
        Jolt(intensity, new Vector2((float)(rand.NextDouble()*2.0-1.0), (float)(rand.NextDouble()*2.0-1.0)));
    }

    public void Jolt(float intensity, Vector2 direction)
    {
        intensity *= AccessibilityOptionsSingleton.GetInstance().ScreenshakeAmount;
        //Debug.Log("Camera Jolt!");
        if (direction.magnitude == 0)
        {
            Jolt(intensity);
            return;
        }
        Vector3 originalpos = gameObject.GetComponent<Transform>().position;
        gameObject.GetComponent<Transform>().position = new Vector3(originalpos.x + direction.normalized.x*intensity, originalpos.y + direction.normalized.y*intensity, originalpos.z);
    }


    //-----------| Coroutines 

    IEnumerator CameraShake(float intensity, int repetitions, float timeBetweenJolts)
    {
        //Debug.Log("Camera shaking!");
        intensity *= AccessibilityOptionsSingleton.GetInstance().ScreenshakeAmount;
        System.Random rand = new System.Random();
        Vector3 originalpos = gameObject.GetComponent<Transform>().position;
        Vector3 newpos = originalpos;
        for (float i = 0; i < repetitions; i++)
        {
            originalpos = gameObject.GetComponent<Transform>().position;
            gameObject.GetComponent<Transform>().position = new Vector3(originalpos.x + (float)(rand.NextDouble()*2-1)*intensity, originalpos.y + (float)(rand.NextDouble() * 2 - 1)*intensity, originalpos.z);
            yield return new WaitForSeconds(timeBetweenJolts);
        }
    }

    public void UpdateMousePosition(Vector2 position)
    {
        _isUsingCursor = true;
        _cursorWorldPos = position;
    }

    public void SmoothToSizeSimple(float TargetSize)
    {
        SmoothToSize(TargetSize, 1.0f);
    }

    public void SmoothToSize(float newSize, float duration)
    {
        //StopAllCoroutines();
        StartCoroutine(SmoothToSizeCoroutine(newSize, duration));
    }

    IEnumerator SmoothToSizeCoroutine(float newSize, float duration)
    {
        float scale = newSize;
        newSize *= 16.875f;
        float timer = 0.0f;
        float oldSize = _camera.orthographicSize;
        while (timer < duration)
        {
            float currentNewSize = Mathf.Lerp(oldSize, newSize, _cameraSizeChangeEaseCurve.Evaluate(timer / duration));
            _camera.orthographicSize = currentNewSize;
            gameObject.transform.localScale = new Vector3(1, 1, 1) * scale;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _camera.orthographicSize = newSize; 
        gameObject.transform.localScale = new Vector3(1, 1, 1) * scale;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Graphics.Blit(source, destination);
        //Graphics.Blit(destination, source);
        //Graphics.Blit(source, destination);
        //Graphics.Blit(destination, source);

        //RenderTexture buffer = new RenderTexture(source.descriptor);
        RenderTexture temp = RenderTexture.GetTemporary(source.descriptor);
        //int index = 0;
        //Graphics.Blit(source, doubleBuffer[index]);
        int toggle = -1;
        //Graphics.Blit(source, temp);

        foreach (Material ppm in _postProcessMaterials)
        {
            Graphics.Blit(source, temp, ppm);
            Graphics.Blit(temp, source);
            toggle *= -1;
        }
        RenderTexture.ReleaseTemporary(temp);
        Graphics.Blit(temp, destination);

        // it inverts over the x (vertically) every other time if I don't run the below nightmare. I don't know why. blitting back and forth just makes it invert?
        //Graphics.Blit(source, destination, new Vector2(1.0f, toggle), new Vector2(0.0f, toggle == -1 ? 1.0f : 0.0f)); 
    }

    public void SetPostProcessParam(string param, Texture2D tex)
    {
        foreach (Material ppm in _postProcessMaterials)
        {
            ppm.SetTexture(param, tex);
        }
    }
    public void SetPostProcessParam(string param, float value)
    {
        foreach (Material ppm in _postProcessMaterials)
        {
            ppm.SetFloat(param, value);
        }
    }
}

