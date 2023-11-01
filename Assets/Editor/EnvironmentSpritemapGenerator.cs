using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;


/// <summary>
/// Used to generate spritesheets for environment objects dynamically so I dont have to 
/// draw FUCKING RECTANGLES FOR HOURS FOR NO REASON.
/// </summary>
public class EnvironmentSpritemapGenerator
{
    public static List<EnvironmentPhysics> SelectedEnvtPhysicsList = new List<EnvironmentPhysics>();
    public static List<RectInt> SelectedSpritesheetRegions = new List<RectInt>();

    [MenuItem("EXALT Tools/Generate Environment TOP Spritemap from Selection")]
    private static void GenerateTopSpritemapFromSelection()
    {
        // initialize our list of envt objects
        Vector2 minPos = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxPos = new Vector2(float.MinValue, float.MinValue);
        //List<EnvironmentPhysics> envtPhysicsList = new List<EnvironmentPhysics>();
        SelectedEnvtPhysicsList.Clear();
        SelectedSpritesheetRegions.Clear();
        bool errorOccurred = false;

        foreach (GameObject obj in Selection.objects)
        {
            EnvironmentPhysics envtPhys = obj.GetComponent<EnvironmentPhysics>();
            if (!envtPhys)
            {
                Debug.LogError("Whoa!! Found a GameObject with no EnvironmentPhysics when trying to export an environment spritemap!");
                return;
            }

            SelectedEnvtPhysicsList.Add(envtPhys);
            minPos = Vector2.Min(minPos, envtPhys.ObjectCollider.bounds.min);
            maxPos = Vector2.Max(maxPos, envtPhys.ObjectCollider.bounds.max);
        }
        float texWidthPixels = (maxPos.x - minPos.x) * 16;
        float texHeightPixels = (maxPos.y - minPos.y) * 16;

        if (!Mathf.Approximately(texWidthPixels, (int)texWidthPixels) || !Mathf.Approximately(texHeightPixels, (int)texHeightPixels))
        {
            Debug.LogError("Envt Spritemap Exporter Error! The bounds of the selected objects do NOT form a clean integer number of pixels");
            errorOccurred = true;
            return;
        }

        Texture2D newSpritemapTex = new Texture2D((int)texWidthPixels, (int)texHeightPixels, TextureFormat.RGBA32, false);
        Rect TextureWorldspaceBoundingRect = new Rect(minPos, new Vector2(texWidthPixels, texHeightPixels));

        // start drawing those rectangles
        Dictionary<Color, EnvironmentPhysics> ColorToEnvironment = new Dictionary<Color, EnvironmentPhysics>();
        Color thisRectColor = Color.red;
        SelectedSpritesheetRegions.Clear();
        foreach (EnvironmentPhysics phys in SelectedEnvtPhysicsList)
        {
            while (ColorToEnvironment.ContainsKey(thisRectColor)) // keep trying to get a new color that isn't already used
            {
                thisRectColor = new Color(Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f);
            }
            ColorToEnvironment.Add(thisRectColor, phys);

            Vector2Int objectBottomLeftCornerPosition = GetPixelPosition(phys.ObjectCollider.bounds.min, TextureWorldspaceBoundingRect);
            Vector2Int rectSizePixels = new Vector2Int(Mathf.RoundToInt(phys.ObjectCollider.bounds.size.x * 16), Mathf.RoundToInt(phys.ObjectCollider.bounds.size.y * 16)); // fuck unity
            int colorArraySize = rectSizePixels.x * rectSizePixels.y;

            Color[] colors = new Color[colorArraySize];
            for (int i = 0; i < colorArraySize; i++)
            {
                colors[i] = thisRectColor;
            }
            if (IsRectOverlapping(new RectInt(objectBottomLeftCornerPosition, rectSizePixels)))
            {
                // alert! overlap! bad!!!
                Debug.LogError("SPRITEMAP GENERATOR ERROR : Rect overlap detected with environment object : " + phys.gameObject.name);
                errorOccurred = true;
            }
            newSpritemapTex.SetPixels(objectBottomLeftCornerPosition.x, objectBottomLeftCornerPosition.y, rectSizePixels.x, rectSizePixels.y, colors);
            SelectedSpritesheetRegions.Add(new RectInt(objectBottomLeftCornerPosition, rectSizePixels));
        }

        // make folder for scene if missing
        string folderRelativePath = "/Art/_FinalCampaign/" + SelectedEnvtPhysicsList[0].gameObject.scene.name;
        string folderAbsolutePath = Application.dataPath + "/Art/_FinalCampaign/" + SelectedEnvtPhysicsList[0].gameObject.scene.name;
        if (!AssetDatabase.IsValidFolder($"Assets{folderRelativePath}"))
        {
            AssetDatabase.CreateFolder("Assets/Art/_FinalCampaign", SelectedEnvtPhysicsList[0].gameObject.scene.name);
        }

        // get a filename that wont conflict
        string fileName = SelectedEnvtPhysicsList[0].gameObject.scene.name + "_Top.png";
        int increment = 0;
        while (File.Exists(folderAbsolutePath + "/" + fileName))
        {
            fileName = fileName.Split('.')[0] + "_" + increment + ".png";
            increment++;
        }
        string fileRelativePath = "/Art/_FinalCampaign/" + SelectedEnvtPhysicsList[0].gameObject.scene.name + "/" + fileName;
        string fileAbsolutePath = folderAbsolutePath + "/" + fileName;
        byte[] data = newSpritemapTex.EncodeToPNG();
        File.WriteAllBytes(fileAbsolutePath, data);
        if (errorOccurred) return; // dont kick off import stuff if it fucked up
        EditorPrefs.SetString("ExaltTopSpritesheetPath", "Assets" + fileRelativePath);
        AssetDatabase.Refresh();
    }

    [MenuItem("EXALT Tools/Generate Environment FRONT Spritemap from Selection")]
    private static void GenerateFrontSpritemapFromSelection()
    {
        // initialize our list of envt objects
        Vector2 minPos = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxPos = new Vector2(float.MinValue, float.MinValue);
        //List<EnvironmentPhysics> envtPhysicsList = new List<EnvironmentPhysics>();
        SelectedEnvtPhysicsList.Clear();
        SelectedSpritesheetRegions.Clear();
        bool errorOccurred = false;

        foreach (GameObject obj in Selection.objects)
        {
            EnvironmentPhysics envtPhys = obj.GetComponent<EnvironmentPhysics>();
            if (!envtPhys)
            {
                Debug.LogError("Whoa!! Found a GameObject with no EnvironmentPhysics when trying to export an environment spritemap!");
                return;
            }

            SelectedEnvtPhysicsList.Add(envtPhys);
            minPos = Vector2.Min(minPos, new Vector2(envtPhys.ObjectCollider.bounds.min.x, envtPhys.BottomHeight + envtPhys.ObjectCollider.bounds.min.y));
            maxPos = Vector2.Max(maxPos, new Vector2(envtPhys.ObjectCollider.bounds.max.x, envtPhys.TopHeight + envtPhys.ObjectCollider.bounds.min.y));
        }
        int texWidthPixels = Mathf.RoundToInt((maxPos.x - minPos.x) * 16);
        int texHeightPixels = Mathf.RoundToInt((maxPos.y - minPos.y) * 16);

        Texture2D newSpritemapTex = new Texture2D(texWidthPixels, texHeightPixels, TextureFormat.RGBA32, false);
        Rect TextureWorldspaceBoundingRect = new Rect(minPos, new Vector2(texWidthPixels, texHeightPixels));

        // start drawing those rectangles
        Dictionary<Color, EnvironmentPhysics> ColorToEnvironment = new Dictionary<Color, EnvironmentPhysics>();
        Color thisRectColor = Color.red;
        SelectedSpritesheetRegions.Clear();
        foreach (EnvironmentPhysics phys in SelectedEnvtPhysicsList)
        {
            while (ColorToEnvironment.ContainsKey(thisRectColor)) // keep trying to get a new color that isn't already used
            {
                thisRectColor = new Color(Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f);
            }
            ColorToEnvironment.Add(thisRectColor, phys);

            Vector2Int objectBottomLeftCornerPosition = GetPixelPosition(new Vector2(phys.ObjectCollider.bounds.min.x, phys.BottomHeight + phys.ObjectCollider.bounds.min.y), TextureWorldspaceBoundingRect);
            Vector2Int rectSizePixels = new Vector2Int(Mathf.RoundToInt(phys.ObjectCollider.bounds.size.x * 16), Mathf.RoundToInt(phys.ObjectHeight * 16)); // fuck unity
            int colorArraySize = rectSizePixels.x * rectSizePixels.y;

            Color[] colors = new Color[colorArraySize];
            for (int i = 0; i < colorArraySize; i++)
            {
                colors[i] = thisRectColor;
            }
            if (IsRectOverlapping(new RectInt(objectBottomLeftCornerPosition, rectSizePixels)))
            {
                // alert! overlap! bad!!!
                Debug.LogError("SPRITEMAP GENERATOR ERROR : Rect overlap detected with environment object : " + phys.gameObject.name);
                errorOccurred = true;
            }
            newSpritemapTex.SetPixels(objectBottomLeftCornerPosition.x, objectBottomLeftCornerPosition.y, rectSizePixels.x, rectSizePixels.y, colors);
            SelectedSpritesheetRegions.Add(new RectInt(objectBottomLeftCornerPosition, rectSizePixels));
        }

        // make folder for scene if missing
        string folderRelativePath = "/Art/_FinalCampaign/" + SelectedEnvtPhysicsList[0].gameObject.scene.name;
        string folderAbsolutePath = Application.dataPath + "/Art/_FinalCampaign/" + SelectedEnvtPhysicsList[0].gameObject.scene.name;
        if (!AssetDatabase.IsValidFolder($"Assets{folderRelativePath}"))
        {
            AssetDatabase.CreateFolder("Assets/Art/_FinalCampaign", SelectedEnvtPhysicsList[0].gameObject.scene.name);
        }

        // get a filename that wont conflict
        string fileName = SelectedEnvtPhysicsList[0].gameObject.scene.name + "_Front.png";
        int increment = 0;
        while (File.Exists(folderAbsolutePath + "/" + fileName))
        {
            fileName = fileName.Split('.')[0] + "_" + increment + ".png";
            increment++;
        }
        string fileRelativePath = "/Art/_FinalCampaign/" + SelectedEnvtPhysicsList[0].gameObject.scene.name + "/" + fileName;
        string fileAbsolutePath = folderAbsolutePath + "/" + fileName;
        byte[] data = newSpritemapTex.EncodeToPNG();
        File.WriteAllBytes(fileAbsolutePath, data);
        if (errorOccurred) return; // dont kick off import stuff if it fucked up
        EditorPrefs.SetString("ExaltFrontSpritesheetPath", "Assets" + fileRelativePath);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Should be done on the spritesheet immediately after
    /// </summary>
    /// <param name="filepath"></param>
    [MenuItem("EXALT Tools/Split Spritemap")]
    public static void SplitSpritemap()
    {
        // --- split into subregions
        /*
        List<SpriteMetaData> newSpriteMetaData = new List<SpriteMetaData>();
        Texture2D spritesheet = (Texture2D)Selection.activeObject;
        TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spritesheet)) as TextureImporter;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16;
        importer.SaveAndReimport();

        for (int i = 0; i < SelectedSpritesheetRegions.Count; i++)
        {
            SpriteMetaData smd = new SpriteMetaData();
            smd.rect = new Rect(SelectedSpritesheetRegions[i].min, SelectedSpritesheetRegions[i].size);
            smd.pivot = new Vector2(0.5f, 0.5f);
            smd.name = i + "";
            newSpriteMetaData.Add(smd);
        }
        importer.spritesheet = newSpriteMetaData.ToArray();
        AssetDatabase.Refresh();*/
    }

    /// <summary>
    /// Should be done on the spritesheet last
    /// </summary>
    /// <param name="filepath"></param>
    [MenuItem("EXALT Tools/Assign Subsprites to Objects")]
    public static void SplitAndAssign()
    {
        /*
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        for (int i = 0; i < SelectedSpritesheetRegions.Count; i++)
        {
            if (!SelectedEnvtPhysicsList[i] || !sprites[i]) break;
            SelectedEnvtPhysicsList[i].TopSprite.sprite = sprites[i]; /// TODO = left off here. TopSprite is set in Start(). Make this something that's lazy loaded
        }
        */
    }

    // --- VALIDATION METHODS

    [MenuItem("EXALT Tools/Generate Environment TOP Spritemap from Selection", true)]
    private static bool CanGenerateSpritemap()
    {
        foreach (Object obj in Selection.objects)
        {
            if (!(obj is GameObject)) return false;
            if (!((GameObject)obj).GetComponent<EnvironmentPhysics>())
            {
                return false;
            }
        }
        return Selection.objects.Length > 0;
    }

    [MenuItem("EXALT Tools/Generate Environment FRONT Spritemap from Selection", true)]
    private static bool CanGenerateFrontSpritemap()
    {
        foreach (Object obj in Selection.objects)
        {
            if (!(obj is GameObject)) return false;
            if (!((GameObject)obj).GetComponent<EnvironmentPhysics>())
            {
                return false;
            }
        }
        return Selection.objects.Length > 0;
    }

    [MenuItem("EXALT Tools/Split Spritemap", true)]
    private static bool CanSplitSpritemap()
    {
        return Selection.activeObject is Texture2D;
    }

    [MenuItem("EXALT Tools/Assign Subsprites to Objects", true)]
    private static bool CanAssignSubspritesToObject()
    {
        return Selection.activeObject is Texture2D;
    }

    // --- HELPER METHODS

    private static Vector2Int GetPixelPosition(Vector2 WorldSpacePosition, Rect TextureWorldspaceBoundingRect)
    {
        float x = Mathf.InverseLerp(TextureWorldspaceBoundingRect.min.x, TextureWorldspaceBoundingRect.max.x, WorldSpacePosition.x) * TextureWorldspaceBoundingRect.width * 16;
        float y = Mathf.InverseLerp(TextureWorldspaceBoundingRect.min.y, TextureWorldspaceBoundingRect.max.y, WorldSpacePosition.y) * TextureWorldspaceBoundingRect.height * 16;

        if (!Mathf.Approximately(x, (int)x) || !Mathf.Approximately(y, (int)y))
        {
            x = Mathf.RoundToInt(x);
            y = Mathf.RoundToInt(y); // fuck unity. fucking floating point shit I swear to god.
            Debug.LogError("Envt Spritemap Exporter Error! The bounds of an environment object do not produce clean pixel coordinates!");
        }

        return new Vector2Int((int)x, (int)y);
    }

    private static bool IsRectOverlapping(RectInt inRect)
    {
        foreach (RectInt iteratorRect in SelectedSpritesheetRegions)
        {
            if (inRect.xMax > iteratorRect.xMin && inRect.xMin < iteratorRect.xMax &&
                inRect.yMax > iteratorRect.yMin && inRect.yMin < iteratorRect.yMax)
            {
                return true;
            }
        }
        return false;
    }
}
