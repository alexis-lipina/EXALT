﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGunGreen : Weapon
{

    
    void Awake()
    {
        _timeOfLastShot = Time.time;
        _bulletPoolMaxCount = 20;
        _timeBetweenShots = 0.5f;
        _bulletPrefabName = "GreenBullet";
	}



    public override GameObject FireBullet(Vector2 direction)
    {
        /*
        _bulletPrefabName = "GreenBullet";
        GameObject tempBullet = Instantiate(Resources.Load("Prefabs/Bullets/" + _bulletPrefabName)) as GameObject;
        tempBullet.GetComponentInChildren<BulletHandler>().MoveDirection = direction.normalized;

        
        tempBullet.SetActive(true);
        return tempBullet;
        */
        GameObject tempBullet = GetFromPool();
        tempBullet.GetComponentInChildren<BulletHandler>().MoveDirection = direction.normalized;
        tempBullet.SetActive(true);
        _timeOfLastShot = Time.time;
        return tempBullet;
    }
}
