using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailFollower : MonoBehaviour
{

    [SerializeField] bool _followsXAxis = false;
    [SerializeField] bool _followsYAxis = false;
    [SerializeField] Transform _target;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 futurePosition = transform.position;

        if (_followsXAxis)
        {
            futurePosition.x = _target.position.x;
        }
        if (_followsYAxis)
        {
            futurePosition.y = _target.position.y;
        }
        transform.position = futurePosition;
    }
}
