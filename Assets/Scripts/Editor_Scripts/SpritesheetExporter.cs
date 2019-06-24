using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
public class SpritesheetSlicer// : MonoBehaviour
{
    [MenuItem("Assets/Slice_And_Export_Spritesheet")]
    private static void ExportSheet()
    {
        string spritesheet = AssetDatabase.GetAssetPath(Selection.activeObject);
        string folderpath = spritesheet.Substring( 0, spritesheet.LastIndexOf('/') + 1 ) + Selection.activeObject.name + "_Sprites";
        Debug.Log(folderpath);
        var folder = Directory.CreateDirectory(folderpath);

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritesheet).OfType<Sprite>().ToArray();

        foreach (Sprite s in sprites) //code courtesy of https://answers.unity.com/questions/683772/export-sprite-sheets.html
        {
            Texture2D tex = s.texture;
            //Rect r = s.textureRect;
            Rect r = s.rect;
            Texture2D subtex = tex.CropTexture((int)r.x, (int)r.y, (int)r.width, (int)r.height);
            byte[] data = subtex.EncodeToPNG();
            File.WriteAllBytes(folderpath + "/" + s.name + "_Single.png", data);
            Debug.Log("Created asset " + s.name + ".png");
            //AssetDatabase.CreateAsset(subtex, folderpath + "/" + s.name + ".png");
        }
        Debug.Log("Spritesheet <color=cyan>" + spritesheet + "</color> member sprites have been successfully generated");
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Slice_And_Export_Spritesheet", true)]
    private static bool NewMenuOptionValidation()
    {
        return Selection.activeObject is Texture;
    }
}
#endif