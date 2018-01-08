using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FutureCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void MoveFutureCollider(float coordX, float coordY)
    {
        this.gameObject.transform.position = new Vector3(coordX, coordY, 0);

    }

    public Vector2 Execute(float playerX, float playerY, float futureX, float futureY) // perform function
    {
        float x = futureX;
        float y = futureY;
        this.gameObject.transform.position = new Vector3(futureX, futureY, 0);
        

        return new Vector2(x, y); //returns suggested change in x and y position (speed, sorta)
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.tag == "Environment")
        {
            //Debug.Log("Blep");
            
        }
    }
}
