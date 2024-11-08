﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballLauncher : Weapon
{


    // Use this for initialization
    void Awake()
    {
        _timeOfLastShot = Time.time;
        _bulletPoolMaxCount = 10;
        _timeBetweenShots = 0.2f;
        _bulletPrefabName = "Fireball";
    }



    public override GameObject FireBullet(Vector2 direction)
    {
        GameObject tempBullet = GetFromPool();
        tempBullet.GetComponentInChildren<BulletHandler>().MoveDirection = direction.normalized;
        tempBullet.SetActive(true);
        tempBullet.GetComponentInChildren<ProjectilePhysics>().GetComponent<AudioSource>().clip = tempBullet.GetComponentInChildren<BulletHandler>().SpawnSFX;
        tempBullet.GetComponentInChildren<ProjectilePhysics>().GetComponent<AudioSource>().Play();
        //tempBullet.GetComponentInChildren<BulletHandler>().SourceWeapon = this;

        _timeOfLastShot = Time.time;
        return tempBullet;
    }
}