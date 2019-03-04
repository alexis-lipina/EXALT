using UnityEngine;
using UnityEngine.Assertions;
/// <summary>
/// HOW TO USE:
/// 
/// Works best with 16x16 sprite 
/// 
/// 2D pixel import settings
///  + PPU : 16
///  + Mesh Type : Full Rect
///  + Sprite Editor Border Values
/// 
/// First child object should have the 9 sliced sprite for top
/// Second child object should have 9 sliced sprite for side
/// 
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(EnvironmentPhysics))]
public class ScalablePlatform : MonoBehaviour {

    [Header("X:width   Y:depth   Z:height")]
    [SerializeField] private Vector3 dimensions; //dimensions of the object
    [SerializeField] private float elevation; //height of bottom of object from world-plane zero
    [SerializeField] private SpriteDrawMode drawMode = SpriteDrawMode.Sliced;
    [SerializeField] private bool autoCollider = false; //automatically set collider offset

    private void OnDrawGizmosSelected()
    {
        //find the sprite renderers
        SpriteRenderer top = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer side = transform.GetChild(1).GetComponent<SpriteRenderer>();

        //human error checking
        Assert.IsNotNull(top, gameObject.name + " requires top section child object");
        Assert.IsNotNull(side, gameObject.name + " requires side section child object");
        Assert.IsFalse(drawMode == SpriteDrawMode.Simple, gameObject.name + "'s Draw mode can't be simple");

        //set renderer properties
        top.drawMode = drawMode;
        side.drawMode = drawMode;
        top.size = new Vector2(dimensions.x, dimensions.y);
        side.size = new Vector2(dimensions.x, dimensions.z);
        side.transform.localPosition = Vector2.down * ((dimensions.y / 2) + (dimensions.z / 2) - elevation);
        top.transform.localPosition = Vector2.up * elevation;

        //set collider properties
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(dimensions.x, dimensions.y);
        if (autoCollider) { boxCollider.offset = Vector2.down * (dimensions.z); }

        //GetComponent<EnvironmentPhysics>().environmentBottomHeight = elevation;
        //GetComponent<EnvironmentPhysics>().environmentTopHeight = dimensions.z + elevation;


    }
}
