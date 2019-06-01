using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBarManager : MonoBehaviour
{
    [SerializeField] private Transform _healthBarSegmentPrefab;
    [SerializeField] private EntityPhysics _physics;
    [SerializeField] private float _spacing;

    private int _currentHp;
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
        int newHp = _physics.GetCurrentHealth();
        if (newHp == _currentHp) return;
        for (int i = 0; i < _segments.Length; i++)
        {
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
