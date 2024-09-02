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

    [Space(10)]
    [SerializeField] private bool alignToGrid = false;

    [Space(10)]
    [SerializeField] private bool addNearbyEnvironmentAsNeighbors = false;

    [Space(5)]
    [Header("Custom Scaling Buttons")]
    [Space(10)]
    [SerializeField] private float Delta = 1.0f;
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
        //return; // improve editor perf
        //find the sprite renderers
        //SpriteRenderer top = transform.GetChild(0).GetComponent<SpriteRenderer>();
        //SpriteRenderer side = transform.GetChild(1).GetComponent<SpriteRenderer>();

        //human error checking
        Assert.IsNotNull(transform.GetChild(0).GetComponent<SpriteRenderer>(), gameObject.name + " requires top section child object");
        Assert.IsNotNull(transform.GetChild(1).GetComponent<SpriteRenderer>(), gameObject.name + " requires side section child object");
        //Assert.IsFalse(drawMode == SpriteDrawMode.Simple, gameObject.name + "'s Draw mode can't be simple");

        //set renderer properties
        if (forceDrawMode)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().drawMode = drawMode;
            transform.GetChild(1).GetComponent<SpriteRenderer>().drawMode = drawMode;
        }
        else
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().drawMode = SpriteDrawMode.Simple;
            transform.GetChild(1).GetComponent<SpriteRenderer>().drawMode = SpriteDrawMode.Simple;
            transform.GetChild(0).GetComponent<SpriteRenderer>().transform.localScale = new Vector3(1, 1, 1);
            transform.GetChild(1).GetComponent<SpriteRenderer>().transform.localScale = new Vector3(1, 1, 1);
        }
        transform.GetChild(0).GetComponent<SpriteRenderer>().size = new Vector2(dimensions.x, dimensions.y);
        transform.GetChild(1).GetComponent<SpriteRenderer>().size = new Vector2(dimensions.x, dimensions.z);
        transform.GetChild(1).GetComponent<SpriteRenderer>().transform.localPosition = Vector2.down * ((dimensions.y / 2) + (dimensions.z / 2) - elevation);
        transform.GetChild(0).GetComponent<SpriteRenderer>().transform.localPosition = Vector2.up * elevation;

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
            dimensions.x += Delta;
            Debug.Log("X++");
            X_Up = false;
        }
        if (X_Down)
        {
            dimensions.x -= Delta;
            Debug.Log("X--");
            X_Down = false;
        }
        if (Y_Up)
        {
            dimensions.y += Delta;
            Debug.Log("Y++");
            Y_Up = false;
        }
        if (Y_Down)
        {
            dimensions.y -= Delta;
            Debug.Log("Y--");
            Y_Down = false;
        }
        if (Z_Up)
        {
            dimensions.z += Delta;
            Debug.Log("Z++");
            Z_Up = false;
        }
        if (Z_Down)
        {
            dimensions.z -= Delta;
            Debug.Log("Z--");
            Z_Down = false;
        }
        if (Raise)
        {
            elevation += Delta;
            Debug.Log("Elevation++");
            Raise = false;
        }
        if (Lower)
        {
            elevation -= Delta;
            Debug.Log("Elevation--");
            Lower = false;
        }

        if (fixTransform)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);
        }
        if (alignToGrid)
        {
            //adjust position to be on the grid
            Vector3 realignedPosition = transform.position;
            int step_size = 8;
            realignedPosition *= step_size;
            realignedPosition = new Vector3(Mathf.Round(realignedPosition.x), Mathf.Round(realignedPosition.y), Mathf.Round(realignedPosition.z));
            realignedPosition /= step_size;
            transform.position = realignedPosition;
            alignToGrid = false;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + gameObject.GetComponent<BoxCollider2D>().offset.y + gameObject.GetComponent<BoxCollider2D>().size.y / 2);
        if (addNearbyEnvironmentAsNeighbors)
        {
            Collider2D[] nearbyCollidersX = Physics2D.OverlapBoxAll(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size + new Vector3(-0.25f, 0.25f, 0.0f), 0f);
            Collider2D[] nearbyCollidersY = Physics2D.OverlapBoxAll(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size + new Vector3(0.25f, -0.25f, 0.0f), 0f);
            foreach (Collider2D nearcollider in nearbyCollidersX)
            {
                if (nearcollider.gameObject == gameObject) continue;
                if (nearcollider.GetComponent<EnvironmentPhysics>())
                {
                    GetComponent<EnvironmentPhysics>().AddNeighbor(nearcollider.gameObject);
                }
            }

            foreach (Collider2D nearcollider in nearbyCollidersY)
            {
                if (nearcollider.gameObject == gameObject) continue;
                if (nearcollider.GetComponent<EnvironmentPhysics>())
                {
                    GetComponent<EnvironmentPhysics>().AddNeighbor(nearcollider.gameObject);
                }
            }

            addNearbyEnvironmentAsNeighbors = false;
            OnValidate();
        }
        Shader.SetGlobalFloat("_TopSpriteRect", 0);
        Shader.SetGlobalFloat("_BottomSpriteRect", 1);
        //GetComponent<EnvironmentPhysics>().BottomHeight = BottomHeight;
        //GetComponent<EnvironmentPhysics>().TopHeight = TopHeight;
        #endregion
    }
}
