using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RisingPillarTest : MonoBehaviour {

    [SerializeField] private float _endHeight; //how high the top of the object should be at the end
    [SerializeField] private float _speed; //rate of change of z-position
    [SerializeField] private TriggerVolume _button;
    [SerializeField] private CameraScript _camera;


    private EnvironmentDynamicPhysics _environDynamicPhysics;
    private bool _isRising;




	// Use this for initialization
	void Awake ()
    {
        _isRising = true;
        _environDynamicPhysics = gameObject.GetComponent<EnvironmentDynamicPhysics>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_button.IsTriggered && _isRising)
        {
            Rise();
        }
	}

    void Rise()
    {
        _environDynamicPhysics.TopHeight = _environDynamicPhysics.TopHeight + _speed; //change physics parameters
        gameObject.GetComponent<Transform>().position = new Vector3(gameObject.GetComponent<Transform>().position.x, gameObject.GetComponent<Transform>().position.y + _speed, gameObject.GetComponent<Transform>().position.z);
        gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(gameObject.GetComponent<BoxCollider2D>().offset.x, gameObject.GetComponent<BoxCollider2D>().offset.y - _speed);
        if (_environDynamicPhysics.TopHeight > _endHeight )
        {
            _isRising = false;
            _environDynamicPhysics.TopHeight = _endHeight;
            _camera.Jolt(0.5f, Vector2.up);
        }
        else
        {
            _camera.Jolt(0.03f, new Vector2(0, Random.Range(-1, 1)));
        }
    }

    void Lower()
    {

    }
}
