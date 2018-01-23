using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour {

    [SerializeField] float MaxHP;
    float currentHP;



	// Use this for initialization
	void Start ()
    {
        currentHP = MaxHP;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}


    public void Inflict(float damage)
    {
        currentHP -= damage;
        Debug.Log("Ow!!");
    }
}
