﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Provides framework for weapons the player can wield. Is not a MonoBehavior.
/// </summary>
public abstract class Weapon : ScriptableObject
{
    protected string _bulletPrefabName; //what the name of the bullet is in the Assets/Resources/Bullets folder
    protected float _timeBetweenShots;
    protected Dictionary<int, GameObject> _bulletPool;
    protected int _bulletPoolMaxCount;



    /// <summary>
    /// Returns a bullet object, already set up for release.
    /// </summary>
    ///  <param name="direction">Direction and magnitude (if applicable) of shot</param>
    /// <returns></returns>
    public abstract GameObject FireBullet(Vector2 direction);



    //======================================| BULLET POOL METHODS

    /// <summary>
    /// sets up the object pool of bullets
    /// </summary>
    public void PopulateBulletPool()
    {
        Debug.Log("Pool populating...");
        _bulletPool = new Dictionary<int, GameObject>();
        for (int i = 0; i < _bulletPoolMaxCount; i++)
        {
            GameObject tempBullet = Instantiate(Resources.Load("Prefabs/Bullets/" + _bulletPrefabName)) as GameObject;
            tempBullet.GetComponentInChildren<BulletHandler>().SourceWeapon = this;
            tempBullet.SetActive(false);
            _bulletPool.Add(tempBullet.GetInstanceID(), tempBullet);
        }
    }

    public GameObject GetFromPool()
    {
        foreach (KeyValuePair<int, GameObject> entry in _bulletPool)
        {
            if (!entry.Value.activeSelf)
            {
                return entry.Value;
            }
        }

        //if there are no more inactive bullets
        Debug.Log("Expanding bullet pool (" + _bulletPrefabName + ")");
        GameObject tempBullet = Instantiate(Resources.Load("Prefabs/Bullets/" + _bulletPrefabName)) as GameObject;
        tempBullet.GetComponentInChildren<BulletHandler>().SourceWeapon = this;
        _bulletPool.Add(tempBullet.GetInstanceID(), tempBullet);
        return tempBullet;
    }

    public void ReturnToPool(int instanceID)
    {
        if (_bulletPool.ContainsKey(instanceID))
        {
            if (_bulletPool[instanceID].activeSelf)
            {
                _bulletPool[instanceID].SetActive(false);
                _bulletPool[instanceID].GetComponentInChildren<BulletHandler>().ResetBullet();
            }
            else
            {
                Debug.Log("Attempting to return object to pool that is already in pool.");
            }
        }
        else
        {
            Debug.Log("Invalid InstanceID - Object not in pool.");
        }
    }
}