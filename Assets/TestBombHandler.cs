using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBombHandler : BulletHandler
{
    [SerializeField] private Sprite _grenadeSprite;
    [SerializeField] private TriggerVolume _explosionVolume;
    private float _timer;
    private float _fuse = 1f;
    private bool _hasExploded;
    private float _explosionDuration = 0.4f;

    protected override void Start()
    {
        base.Start();
        _timer = 0.0f;
        _hasExploded = false;
        _projectilePhysics.enabled = true;
        _projectilePhysics.ObjectSprite.GetComponent<Animator>().enabled = false;
        _projectilePhysics.ObjectSprite.GetComponent<SpriteRenderer>().sprite = _grenadeSprite;
    }

	protected override void Update()
    {
        base.Update();

        _explosionVolume.MoveBottom(_projectilePhysics.GetBottomHeight());
        if (_timer > _fuse && !_hasExploded)
        {
            //Boom
            BlowUp();

        }
        _timer += Time.deltaTime;

        if (_hasExploded && _timer > _fuse + _explosionDuration)
        {
            SourceWeapon.ReturnToPool(transform.parent.gameObject.GetInstanceID());
        }
    }

    private void BlowUp()
    {
        //Debug.Log("<color=orange>BOOM</color>");
        //_explosion.Detonate();
        _projectilePhysics.ObjectSprite.GetComponent<Animator>().enabled = true;
        _projectilePhysics.enabled = false;
        _hasExploded = true;
        
        if (_explosionVolume.IsTriggered)
        {
            Debug.Log("<color=orange>Damaging SOMEONE</color>");
            GameObject[] damagedEnemies = _explosionVolume.TouchingObjects.ToArray();
            for (int i = 0; i < damagedEnemies.Length; i++)
            {
                damagedEnemies[i].GetComponent<EntityPhysics>().Inflict(1.0f);
            }
        }
    }
}
