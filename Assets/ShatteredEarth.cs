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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        TopSprite.transform.localScale = new Vector3(Width, Height, 1);
        FrontSprite.transform.localScale = new Vector3(Width, 1, 1);

        TopSprite.transform.localPosition = new Vector3(0, Height * 0.5f / 16.0f, 0);
        FrontSprite.transform.localPosition = new Vector3(0, GradientSpriteHeight / -32.0f, 0);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.y);

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
    }

}
