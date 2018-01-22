using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    public Transform player;
    public float smoothTime;
    private Vector3 velocity = Vector3.zero;
    void Update()
    {
        Vector3 targetPosition = player.TransformPoint(new Vector3(0, 0, -100));
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}

