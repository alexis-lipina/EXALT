using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class EntranceVolume : MonoBehaviour
{
    [SerializeField] private float _elevation;
    [SerializeField] private string _source;
    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private CameraScript _camera;

    // Use this for initialization
    void Start()
    {
        if (PlayerHandler.PREVIOUS_SCENE == _source)
        {
            _playerPhysics.transform.position = transform.position;
            _playerPhysics.SetElevation(_elevation + 0.5f);
            StartCoroutine(KeepCameraInLine());
        }
    }

    /// <summary>
    /// Prevents weird camera behaviour from setup of player position + camera position
    /// </summary>
    /// <returns></returns>
    IEnumerator KeepCameraInLine()
    {
        int numFrames = 10;
        while (numFrames > 0)
        {
            Debug.Log("holding camera");
            _camera.transform.position = transform.position + new Vector3(0, _elevation, 0);
            yield return new WaitForEndOfFrame();
            --numFrames;
        }
    }
}
