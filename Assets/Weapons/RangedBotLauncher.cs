using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedBotLauncher : Weapon
{
    void Awake()
    {
        _timeOfLastShot = Time.time;
        _bulletPoolMaxCount = 3;
        _timeBetweenShots = 0.2f;
        _bulletPrefabName = "RangedBot_Projectile";
    }



    public override GameObject FireBullet(Vector2 direction)
    {
        GameObject tempBullet = GetFromPool();
        tempBullet.GetComponentInChildren<BulletHandler>().MoveDirection = direction.normalized;
        tempBullet.SetActive(true);
        tempBullet.GetComponentInChildren<AudioSource>().Play();
        _timeOfLastShot = Time.time;
        return tempBullet;
    }
}
