using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullVFXHandler : MonoBehaviour
{

    private float _lifetimeTimer = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer < 0) Destroy(gameObject);
    }
}
