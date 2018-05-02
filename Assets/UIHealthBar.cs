using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{

    [SerializeField] private EntityPhysics _playerPhysics;
    [SerializeField] private Slider HealthBar;
    [SerializeField] private Image _barImage;

    void Start()
    {
        HealthBar.maxValue = (int)_playerPhysics.GetMaxHealth();
        HealthBar.value = _playerPhysics.GetCurrentHealth();
        //_barImage.material.SetFloat("_MaskOn", 1f);
    }

    public void UpdateBar(int health)
    {
        HealthBar.value = health;
        
        StartCoroutine(DamageFlash());
        
        Debug.Log("Bar should go down");
    }

    IEnumerator DamageFlash()
    {
        for (int i = 0; i < 2; i++)
        {
            _barImage.material.SetColor("_MaskColor", new Color(1, 1, 1, 1));
            yield return new WaitForSeconds(0.05f);
            _barImage.material.SetColor("_MaskColor", new Color(1, 0, 0, 1));
            yield return new WaitForSeconds(0.05f);
        }

    }
}
