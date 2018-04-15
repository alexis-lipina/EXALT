using System;
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
        Debug.Log("Camera Jolt!");
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
        Debug.Log("Camera shaking!");
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

    
}

