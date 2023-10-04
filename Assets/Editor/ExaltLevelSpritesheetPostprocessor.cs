using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExaltLevelSpritesheetPostprocessor : AssetPostprocessor
{
    void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
    {
        Debug.Log("Sprites: " + sprites.Length);
        Debug.Log(EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList.Count);
    }

    void OnPostprocessTexture(Texture2D texture)
    {
        return;
        Debug.Log("Texture2D: (" + texture.width + "x" + texture.height + ")");
        string filePath = EditorPrefs.GetString("ExaltTopSpritesheetPath");
        //Debug.Log(EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList.Count);

        if (AssetDatabase.GetAssetPath(texture) != filePath)
        {
            return;
        }

        // --- split into subregions
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
        AssetDatabase.Refresh();
    }
}
