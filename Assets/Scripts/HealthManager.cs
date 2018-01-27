using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour {

    [SerializeField] private float MaxHP;
    [SerializeField] private GameObject CharacterSpriteObject;
    private float currentHP;
    private SpriteRenderer CharacterSpriteRenderer;



	// Use this for initialization
	void Start ()
    {
        currentHP = MaxHP;
        CharacterSpriteRenderer = CharacterSpriteObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}


    public void Inflict(float damage)
    {
        currentHP -= damage;
        Debug.Log("Ow!!");
        StartCoroutine(TakeDamageFlash());
    }

    /*
    //In case I feel like making enemies recoil or pushed back from damage - would be neat
    public void Inflict(float damage, Vector2 force)
    {
        currentHP -= damage;
        Debug.Log("Ow!!");
        StartCoroutine(TakeDamageFlash());
    }
    */

    //Test coroutine for damage flash - flash black and red for a bit, then reset to normal
    IEnumerator TakeDamageFlash()
    {
        Debug.Log("TakeDamageFlash entered");

        for (float i = 0; i < 2; i++)
        {
            CharacterSpriteRenderer.color = new Color(1, 0, 0);
            yield return new WaitForSeconds(0.05f);
            CharacterSpriteRenderer.color = new Color(0, 0, 0);
            yield return new WaitForSeconds(0.05f);
        }
        CharacterSpriteRenderer.color = new Color(1, 1, 1);
    }
}
