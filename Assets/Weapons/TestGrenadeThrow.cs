using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestGrenadeThrow : Weapon
{


    void Awake()
    {
        _timeOfLastShot = Time.time;
        _bulletPoolMaxCount = 5;
        _timeBetweenShots = 1f;
        _bulletPrefabName = "TestBlueBomb";

    }



    public override GameObject FireBullet(Vector2 direction)
    {
        GameObject tempBullet = GetFromPool();
        tempBullet.GetComponentInChildren<BulletHandler>().MoveDirection = direction.normalized;
        tempBullet.SetActive(true);
        _timeOfLastShot = Time.time;
        return tempBullet;
    }


}
