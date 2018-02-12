using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class provides methods and fields for all entities in the game. 
/// Entities, in this case (and in most cases in this project) encompass
/// the player, enemies, and other agents
/// </summary>
public abstract class EntityHandler : MonoBehaviour
{
    [SerializeField] protected GameObject PhysicsObject;




    /// <summary>
    /// Contains the state machine switch statement and calls state methods
    /// </summary>
    protected abstract void ExecuteState();
    public abstract void setXYAnalogInput(float x, float y);

}
