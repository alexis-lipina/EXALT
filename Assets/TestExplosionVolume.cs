using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriggerVolume))]


public class TestExplosionVolume : MonoBehaviour
{
    [SerializeField] private TriggerVolume _explosionVolume;
    [SerializeField] private Animator _animation;
    private float _duration;
    private float _timer;
    private int _framesElapsed;
    private bool _exploding;
	
	void Start ()
    {
        _exploding = false;
        _duration = 0.0f;
        AnimationClip[] clips = _animation.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            
            if (clip.name == "TestBlueBomb")
            {
                _duration = clip.length;
            }
        }

        _timer = 0.0f;
        _framesElapsed = 0;

        _animation.StopPlayback();

	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_exploding)
        {
            _timer += Time.deltaTime;
            if (_timer > _duration)
            {
                _exploding = false; //TODO : Implement Object Pooling for Explosions?
                Debug.Log("End explosion");
            }

            if (_framesElapsed == 2)
            {
                DamageHitEnemies();
            }
            ++_framesElapsed;
        }
        else
        {           
            _timer = 0.0f;
            _framesElapsed = 0;
        }
	}

    private void DamageHitEnemies()
    {
        GameObject[] hitObjects = _explosionVolume.TouchingObjects.ToArray();

        for (int i = 0; i < hitObjects.Length; i++)
        {
            hitObjects[i].GetComponent<EntityPhysics>().Inflict(5.0f);
        }
    }

    private void SetPosition()
    {
        Vector3 coords = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + /*_spriteZOffset +*/ _explosionVolume.GetBottomHeight(), gameObject.transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y - gameObject.GetComponent<BoxCollider2D>().size.y / 2 + 0.39f); //change here
        _animation.gameObject.transform.position = coords;
    }


    public void Detonate()
    {
        Start();
        _animation.enabled = true;
        _animation.StartPlayback();
        _animation.Play("TestBlueBomb");
        DamageHitEnemies();
    }
}
