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
    [SerializeField] private bool forceDrawMode = true;

    [Space(5)]
    [Header("Editor Utilities")]
    [SerializeField] private bool fixTransform = false;

    [Space(5)]
    [Header("Custom Scaling Buttons")]
    [Space(10)]
    [SerializeField] private bool X_Up = false;
    [SerializeField] private bool X_Down = false;
    [Space(10)]
    [SerializeField] private bool Y_Up = false;
    [SerializeField] private bool Y_Down = false;
    [Space(10)]
    [SerializeField] private bool Z_Up = false;
    [SerializeField] private bool Z_Down = false;
    [Space(10)]
    [SerializeField] private bool Raise = false;
    [SerializeField] private bool Lower = false;

    public float TopHeight
    {
        get { return dimensions.z + elevation; }
    }
    public float BottomHeight
    {
        get { return elevation; }
    }

    private void OnDrawGizmosSelected()
    {
        
        //find the sprite renderers
        SpriteRenderer top = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer side = transform.GetChild(1).GetComponent<SpriteRenderer>();

        //human error checking
        Assert.IsNotNull(top, gameObject.name + " requires top section child object");
        Assert.IsNotNull(side, gameObject.name + " requires side section child object");
        //Assert.IsFalse(drawMode == SpriteDrawMode.Simple, gameObject.name + "'s Draw mode can't be simple");

        //set renderer properties
        if (forceDrawMode)
        {
            top.drawMode = drawMode;
            side.drawMode = drawMode;
        }
        else
        {
            top.drawMode = SpriteDrawMode.Simple;
            side.drawMode = SpriteDrawMode.Simple;
            top.transform.localScale = new Vector3(1, 1, 1);
            side.transform.localScale = new Vector3(1, 1, 1);
        }
        top.size = new Vector2(dimensions.x, dimensions.y);
        side.size = new Vector2(dimensions.x, dimensions.z);
        side.transform.localPosition = Vector2.down * ((dimensions.y / 2) + (dimensions.z / 2) - elevation);
        top.transform.localPosition = Vector2.up * elevation;

        //set collider properties
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(dimensions.x, dimensions.y);
        if (autoCollider) { boxCollider.offset = Vector2.down * (dimensions.z); }


    }

    private void OnValidate()
    {
        #region BUTTONS
        if (X_Up)
        {
            dimensions.x++;
            Debug.Log("X++");
            X_Up = false;
        }
        if (X_Down)
        {
            dimensions.x--;
            Debug.Log("X--");
            X_Down = false;
        }
        if (Y_Up)
        {
            dimensions.y++;
            Debug.Log("Y++");
            Y_Up = false;
        }
        if (Y_Down)
        {
            dimensions.y--;
            Debug.Log("Y--");
            Y_Down = false;
        }
        if (Z_Up)
        {
            dimensions.z++;
            Debug.Log("Z++");
            Z_Up = false;
        }
        if (Z_Down)
        {
            dimensions.z--;
            Debug.Log("Z--");
            Z_Down = false;
        }
        if (Raise)
        {
            elevation++;
            Debug.Log("Elevation++");
            Raise = false;
        }
        if (Lower)
        {
            elevation--;
            Debug.Log("Elevation--");
            Lower = false;
        }

        if (fixTransform)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);
        #endregion
    }
}
