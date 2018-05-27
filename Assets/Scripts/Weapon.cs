using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Provides framework for weapons the player can wield
/// </summary>
public abstract class Weapon : ScriptableObject
{
    protected string _bulletPrefabName; //what the name of the bullet is in the Assets/Resources/Bullets folder
    protected float _timeBetweenShots;




    /// <summary>
    /// Returns a bullet object, already set up for release.
    /// </summary>
    ///  <param name="direction">Direction and magnitude (if applicable) of shot</param>
    /// <returns></returns>
    public abstract GameObject FireBullet(Vector2 direction);

    
}
