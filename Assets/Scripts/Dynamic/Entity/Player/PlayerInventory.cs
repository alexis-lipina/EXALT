using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Keeps track of what the player has in their inventory.
/// </summary>
public class PlayerInventory : MonoBehaviour
{

    private Weapon _weaponNorth;
    private Weapon _weaponSouth;
    private Weapon _weaponEast;
    private Weapon _weaponWest;

    void Awake()
    {
        _weaponEast = ScriptableObject.CreateInstance<TestGunGreen>();
        _weaponNorth = ScriptableObject.CreateInstance<TestGrenadeThrow>();
        _weaponSouth = ScriptableObject.CreateInstance<FireballLauncher>();
        _weaponWest = ScriptableObject.CreateInstance<TestGunRed>();
        //Debug.Log(_weaponNorth);
    }

    /// <summary>
    /// Returns a weapon in that equip spot. Requires a cardinal input.
    /// </summary>
    /// <param name="dir">NORTH, SOUTH, EAST, WEST</param>
    /// <returns>Weapon in that equip spot, if one exists.</returns>
    public Weapon GetWeapon(string dir)
    {
        /*
        Debug.Log("East: " + _weaponEast);
        Debug.Log("West: " + _weaponWest);
        Debug.Log("North: " + _weaponNorth);
        Debug.Log("South: " + _weaponSouth);
        */
        switch (dir)
        {
            case "NORTH": return _weaponNorth;
            case "SOUTH": return _weaponSouth;
            case "EAST": return _weaponEast;
            case "WEST": return _weaponWest;
            default:
                Debug.Log("Error - Improper argument \"" + dir + "\" for GetWeapon ");
                return null;
        }
    }

}
