using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFlash : MonoBehaviour
{
    [SerializeField] private GameObject _blammo;

    public void SetBlammoDirection(Vector2 direction)
    {
        _blammo.transform.up = direction;
    }
}
