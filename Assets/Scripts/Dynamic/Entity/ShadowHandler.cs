using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is attached to every shadow object - each DynamicPhysics object casts shadows that use this code.
/// </summary>
public class ShadowHandler : MonoBehaviour
{
    private float _elevation;
    private KeyValuePair<Vector2, Vector2> _cullingRectangle; //describes the rectangle within which the shadow is to be drawn
    private Vector2 coord;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void UpdateShadow(Vector3 playerPos, float height, Vector4 rect)
    {
        //transform.position = new Vector3(playerPos.x, playerPos.y + height, playerPos.y + height);
        transform.localPosition = new Vector3(0, height, 0/*playerPos.z*/);
        transform.position = new Vector3(transform.position.x, transform.position.y, playerPos.z);
        //Debug.Log("Actual coord : " + coord, this);
        //Debug.Log("Shadow Height in shadow : " + height + " = " + transform.localPosition.y, this);
        //Debug.Log(playerPos.y + height);

        //send rect to shader
        gameObject.GetComponent<SpriteRenderer>().material.SetColor("_CullRect", new Color(rect.x, rect.y, rect.z, rect.w));
        gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_Elevation", height);
    }

    public void DebugCoordinateCuzImAwfulAtCoding(Vector2 coord)
    {
        this.coord = coord;
    }
}
