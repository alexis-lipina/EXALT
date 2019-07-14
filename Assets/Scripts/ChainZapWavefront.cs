using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Objects of this class represent a "shockwave" or "wavefront" for the chain-lightning attack, 
/// in order to prevent chaining to an already-hit enemy, but allow multiple wavefronts to exist 
/// concurrently with different "already hit" enemy lists. 
/// 
/// ...basically this just keeps track of already hit enemies for a given wavefront
/// 
public class ChainZapWavefront
{
    // instance variables
    public List<int> AlreadyHit;
    //public List<GameObject> ZapNodes;
}
