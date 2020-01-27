using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

#if UNITY_EDITOR
public class SpritesheetSlicerByColor
{
    [MenuItem("Assets/Slice_By_Color")]
    // Start is called before the first frame update
    private static void SliceByColor()
    {
        //find all the rectangles by color
        List<RectInt> regions = new List<RectInt>();
        Texture2D spritesheet = (Texture2D)Selection.activeObject;
        Debug.Log("X : " + spritesheet.width + ", Y : " + spritesheet.height);

        // traverse row by row
        for (int y = 0; y <  spritesheet.height; y++)
        {
            for (int x = 0; x < spritesheet.width; x++)
            {
                //ignore pixels within existing regions
                if (IsPixelWithinExistingRegion(regions, new Vector2Int(x, y)))
                {
                    continue;
                }
                // filter out transparent pixels
                if (spritesheet.GetPixel(x, y).a > 0.5f)
                {
                    regions.Add( GetRegion( spritesheet, spritesheet.GetPixel(x,y), new Vector2Int(x, y) ) );
                }
            }
        }

        // print regions to be sliced
        foreach (RectInt region in regions)
        {
            Debug.Log(region);
        }


        List<SpriteMetaData> newSpriteMetaData = new List<SpriteMetaData>();

        for (int i = 0; i < regions.Count; i++)
        {
            SpriteMetaData smd = new SpriteMetaData();
            smd.rect = new Rect(regions[i].min, regions[i].size);
            smd.pivot = new Vector2(0.5f, 0.5f);
            smd.name = i + "";
            newSpriteMetaData.Add(smd);
        }


        TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spritesheet)) as TextureImporter;
        importer.spritesheet = newSpriteMetaData.ToArray();
    }

    /// <summary>
    /// Returns true if pixel coordinate is within any of the specified rectangles
    /// </summary>
    /// <returns></returns>
    private static bool IsPixelWithinExistingRegion(List<RectInt> regions, Vector2Int pixel)
    {
        foreach (RectInt region in regions)
        {
            if (region.Contains(pixel)) return true;
        }
        return false;
    }

    /// <summary>
    /// Finds the rectangular region of a given color on a spritesheet and returns that rectangular area
    /// </summary>
    /// <param name="spritesheet">Spritesheet to traverse</param>
    /// <param name="regionColor">Color (of upper left pixel) to sample for</param>
    /// <param name="upperLeft">Coordinate to start from</param>
    /// <returns></returns>
    private static RectInt GetRegion(Texture2D spritesheet, Color regionColor, Vector2Int upperLeft)
    {
        Vector2Int bottomRight = upperLeft; //bottomRight will iterate right and then down to the bounds of the rectangle

        // iterate right
        while (spritesheet.GetPixel(bottomRight.x, bottomRight.y) == regionColor)
        {
            bottomRight.x += 1;
            if (bottomRight.x > spritesheet.width)
            {
                bottomRight.x--;
                break;
            }
        }
                
        // iterate down
        while (spritesheet.GetPixel(bottomRight.x - 1, bottomRight.y) == regionColor)
        {
            bottomRight.y += 1;
            if (bottomRight.y > spritesheet.height)
            {
                bottomRight.y--;
                break;
            }
        }

        return new RectInt(upperLeft, bottomRight - upperLeft);

    }


    [MenuItem("Assets/Slice_By_Color", true)]
    private static bool NewMenuOptionValidation()
    {
        return Selection.activeObject is Texture;
    }
}
#endif