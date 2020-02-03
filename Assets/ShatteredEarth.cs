using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class ShatteredEarth : MonoBehaviour
{
    [SerializeField] private int Width = 4; //in pixels
    [SerializeField] private int Height = 4; //in pixels
    [SerializeField] private int GradientSpriteHeight = 64; // in pixels

    [SerializeField] private SpriteRenderer TopSprite;
    [SerializeField] private SpriteRenderer FrontSprite;
    [SerializeField] private bool alignToGrid = false;
    [SerializeField] private bool scaleSprites = false;
    private const float density = 0.1f;


    public bool IsShattered;
    public Vector3 newPosition;

    private float startY;
    private float elevation; //height difference from normal
    private Color defaultColor;
    [SerializeField] private Color highColor;
    [SerializeField] private float maxElevationForColor;
    public float mass;

    public Vector3 GetBlockCenter()
    {
        return transform.localPosition + new Vector3(0, Height / 32.0f, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        defaultColor = TopSprite.GetComponent<SpriteRenderer>().color;
        startY = transform.localPosition.y;
        mass = Width * Height * density;

    }

    // Update is called once per frame
    void Update()
    {
        if (IsShattered)
        {
            elevation = transform.localPosition.y - startY;
            transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, 0.01f / mass);
            TopSprite.color = Color.Lerp(defaultColor, highColor, elevation / maxElevationForColor);
        }
    }

    private void OnDrawGizmosSelected()
    {        
        if (alignToGrid)
        {
            //adjust position to be on the grid
            Vector3 realignedPosition = transform.position;
            int step_size = 16;
            realignedPosition *= step_size;
            realignedPosition = new Vector3(Mathf.Round(realignedPosition.x), Mathf.Round(realignedPosition.y), Mathf.Round(realignedPosition.z));
            realignedPosition /= step_size;
            transform.position = realignedPosition;
            alignToGrid = false;
        }
        if (scaleSprites)
        {
            TopSprite.transform.localScale = new Vector3(Width, Height, 1);
            FrontSprite.transform.localScale = new Vector3(Width, 1, 1);

            TopSprite.transform.localPosition = new Vector3(0, Height * 0.5f / 16.0f, 0);
            FrontSprite.transform.localPosition = new Vector3(0, GradientSpriteHeight / -32.0f, 0);
        }
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.y);
    }

}
