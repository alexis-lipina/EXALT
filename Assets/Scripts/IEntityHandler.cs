using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This interface provides methods and fields for all entities in the game. 
/// Entities, in this case (and in most cases in this project) encompass
/// the player, enemies, and other agents
/// </summary>
interface IEntityHandler {

    /// <summary>
    /// Contains the state machine switch statement and calls state methods
    /// </summary>
    void ExecuteState();


}
