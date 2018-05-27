using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGunRed : Weapon
{


    // Use this for initialization
    void Start()
    {
        _timeBetweenShots = 0.5f;
        _bulletPrefabName = "RedBullet";
    }



    public override GameObject FireBullet(Vector2 direction)
    {
        _bulletPrefabName = "RedBullet";
        GameObject tempBullet = Instantiate(Resources.Load("Prefabs/Bullets/" + _bulletPrefabName)) as GameObject;
        tempBullet.GetComponentInChildren<BulletHandler>().MoveDirection = direction.normalized;


        tempBullet.SetActive(true);
        return tempBullet;
    }
}
