using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test_UIEnemyHealth : MonoBehaviour {

    [SerializeField] private EntityPhysics _entityCollider;
    [SerializeField] private string _name;
    private Text _text;


	// Use this for initialization
	void Start ()
    {
        _text = this.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        _text.text = name + " : " + _entityCollider.GetCurrentHealth() + "/" + _entityCollider.GetMaxHealth();
	}
}
