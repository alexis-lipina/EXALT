using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestGunBlue : Weapon
{


    void Awake()
    {
        _timeOfLastShot = Time.time;
        _bulletPoolMaxCount = 20;
        _timeBetweenShots = 0.2f;
        _bulletPrefabName = "BlueBullet";
        
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
