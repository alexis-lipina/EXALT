using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentDeathShatterController : MonoBehaviour
{
    [SerializeField] bool previewDirection;
    [SerializeField] GameObject ShatterDirectionSprite;
    [SerializeField] float ShatterSpriteOffset = -1.0f; // offsets the shatter sprite this amount backwards after it's rotated into position. Handy if you want its center for calculations to be behind its pivot.
    [SerializeField] List<GameObject> ShatterFragments;
    [SerializeField] AnimationCurve ShatterFragmentPositionOffsetByAngle; // amount to offset the position of the fragment, using the dot product of the relative position of the fragment & direction sprite with shatter direction 
    [SerializeField] AnimationCurve ShatterFragmentNormalizedRotationByAngle; // 0...1 for lerp, where 0 = fragment stays in its orientation, and 1 = fragment rotation is aligned with attack angle
    [SerializeField] AnimationCurve LerpOffsetByProximity; // biases lerps for rotation and position offset by distance
    
    List<Vector2> ShatterFragmentPositions;
    

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        ShatterInternal();

    }

    public void Shatter(Vector2 direction)
    {
        previewDirection = true; // delete this stupid variable lmao
        if (direction.sqrMagnitude == 0.0f) direction = Vector2.right;
        direction.Normalize();
        direction.y = Mathf.Clamp(direction.y, -0.5f, 0.5f);

        ShatterDirectionSprite.transform.up = direction;

        //ShatterDirectionSprite.transform.up = Vector2.right;
        ShatterInternal();
    }

    void ShatterInternal()
    {
        if (ShatterFragmentPositions == null)
        {
            ShatterFragmentPositions = new List<Vector2>();
        }
        if (previewDirection)
        {
            ShatterFragmentPositions.Clear();
            Debug.Log("Called!");
            ShatterDirectionSprite.transform.position += ShatterDirectionSprite.transform.up * ShatterSpriteOffset; // direction sprite pivots around the center of optimal shatter location, then moves back a bit.
            foreach (GameObject obj in ShatterFragments)
            {
                Vector2 offset = obj.transform.position - ShatterDirectionSprite.transform.position;
                Vector2 offsetNormalized = offset;
                offsetNormalized.Normalize();
                float dotProd = Vector2.Dot(offsetNormalized, ShatterDirectionSprite.transform.up);
                float crossSign = Mathf.Sign(Vector3.Cross(offset, ShatterDirectionSprite.transform.up).z); // 1 if to the left of the attack vector, -1 if to the right
                float eastWestSign = Mathf.Sign(ShatterDirectionSprite.transform.up.x); // 1 if attack is aiming east, -1 if aiming west
                // objects above the shatter line should have their up vector tilt toward the OPPOSITE direction of the shatter line
                // objects below should tilt IN THE DIRECTION of the shatter line.
                Vector2 NewDesiredUp = ShatterDirectionSprite.transform.up * crossSign * eastWestSign;
                ShatterFragmentPositions.Add(obj.transform.position);
                if (dotProd > 0)//for objects in front of the hit direction
                {   
                    obj.transform.up = Vector2.Lerp(obj.transform.up, NewDesiredUp, ShatterFragmentNormalizedRotationByAngle.Evaluate(LerpOffsetByProximity.Evaluate(offset.magnitude) * dotProd));
                    //obj.transform.up = Vector2.Lerp(obj.transform.up, NewDesiredUp, ShatterFragmentNormalizedRotationByAngle.Evaluate(LerpOffsetByProximity.Evaluate(offset.magnitude) * dotProd));
                }
                else
                {
                    obj.transform.up = Vector2.Lerp(obj.transform.up, NewDesiredUp, -ShatterFragmentNormalizedRotationByAngle.Evaluate(LerpOffsetByProximity.Evaluate(offset.magnitude) * dotProd));
                }
                obj.transform.position += ShatterDirectionSprite.transform.up * ShatterFragmentPositionOffsetByAngle.Evaluate(LerpOffsetByProximity.Evaluate(offset.magnitude) * dotProd);
                //obj.transform.position += (Vector3)NewDesiredUp * ShatterFragmentPositionOffsetByAngle.Evaluate(LerpOffsetByProximity.Evaluate(offset.magnitude) * dotProd);
            }
        } 
        else
        {
            // restore them to their original position + orientation
            for (int i = 0; i < ShatterFragments.Count; i++)
            {
                ShatterFragments[i].transform.position = ShatterFragmentPositions[i] * new Vector3(1, 1, 0);
                ShatterFragments[i].transform.up = Vector2.up;
            }
        }
        
    }
}
