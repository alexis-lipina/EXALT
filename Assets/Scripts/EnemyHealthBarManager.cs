using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBarManager : MonoBehaviour
{
    [SerializeField] private Transform _healthBarSegmentPrefab;
    [SerializeField] private EntityPhysics _physics;
    [SerializeField] private float _spacing;

    private int _currentHp;
    private ElementType _currentShield = ElementType.NONE;
    private GameObject[] _segments;

    private void Start()
    {
        SetupUI();
    }
    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        // Update shielding if applicable
        if (_currentShield != _physics.Handler.GetShield())
        {
            Debug.Log("Shielding segment!");
            _currentShield = _physics.Handler.GetShield();
            foreach (GameObject seg in _segments)
            {
                seg.GetComponent<EnemyHealthBarSegment>().SetShieldSegment(_currentShield);
            }
        }

        // Update HP count

        int newHp = _physics.GetCurrentHealth();
        if (newHp == _currentHp) return; // early return for no HP change
        for (int i = 0; i < _segments.Length; i++)
        {
            if (newHp == 0)
            {
                _segments[i].GetComponent<SpriteRenderer>().enabled = false;
            }
            else if (newHp > 0 && _currentHp == 0)
            {
                _segments[i].GetComponent<SpriteRenderer>().enabled = true;
            }
            if (i < newHp)
            {
                _segments[i].GetComponent<EnemyHealthBarSegment>().SetSegment(true);
            }
            else
            {
                _segments[i].GetComponent<EnemyHealthBarSegment>().SetSegment(false);
            }
        }
        _currentHp = newHp;


        
    }

    private void SetupUI()
    {
        if (_segments != null && _segments.Length > 0)
        {
            for (int i = 0; i < _segments.Length; i++)
            {
                // clear if for some reason isnt
                if (_segments[i] == null) continue;
                Destroy(_segments[i]);
            }
        }
        _segments = new GameObject[_physics.GetMaxHealth()];
        
        for (int i = 0; i < _physics.GetMaxHealth(); i++)
        {
            _segments[i] = Instantiate(_healthBarSegmentPrefab, this.transform).gameObject;

            // position elements in a neat row
            _segments[i].transform.position = transform.position + new Vector3( i * _spacing - _physics.GetMaxHealth() * 0.5f * _spacing , 0.0f, -0.1f); 
        }
        _currentHp = _physics.GetCurrentHealth();
    }
}
