using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class ExaltLevelSpritesheetPostprocessor : AssetPostprocessor
{
    //private static Sprite[] HeldSprites;

    void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
    {
        EditorApplication.update += UpdateSceneDelayed;

    }

    void OnPostprocessTexture(Texture2D texture)
    {
        return;
        string filePath = EditorPrefs.GetString("ExaltTopSpritesheetPath");
        if (assetImporter.assetPath != filePath)
        {
            return;
        }

        // --- split into subregions
        List<SpriteMetaData> newSpriteMetaData = new List<SpriteMetaData>();
        TextureImporter texImporter = assetImporter as TextureImporter;

        for (int i = 0; i < EnvironmentSpritemapGenerator.SelectedSpritesheetRegions.Count; i++)
        {
            SpriteMetaData smd = new SpriteMetaData();
            smd.rect = new Rect(EnvironmentSpritemapGenerator.SelectedSpritesheetRegions[i].min, EnvironmentSpritemapGenerator.SelectedSpritesheetRegions[i].size);
            smd.pivot = new Vector2(0.5f, 0.5f);
            smd.name = i + "";
            newSpriteMetaData.Add(smd);
        }
        texImporter.spritesheet = newSpriteMetaData.ToArray();
        AssetDatabase.Refresh();
    }

    void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;
        importer.spritePixelsPerUnit = 16;
        importer.maxTextureSize = 8192;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        string filePath = EditorPrefs.GetString("ExaltTopSpritesheetPath");
        if (importer.assetPath == filePath)
        {
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true;


            List<SpriteMetaData> newSpriteMetaData = new List<SpriteMetaData>();
            for (int i = 0; i < EnvironmentSpritemapGenerator.SelectedSpritesheetRegions.Count; i++)
            {
                SpriteMetaData smd = new SpriteMetaData();
                smd.rect = new Rect(EnvironmentSpritemapGenerator.SelectedSpritesheetRegions[i].min, EnvironmentSpritemapGenerator.SelectedSpritesheetRegions[i].size);
                smd.pivot = new Vector2(0.5f, 0.5f);
                smd.name = i + "";
                newSpriteMetaData.Add(smd);
            }
            importer.spritesheet = newSpriteMetaData.ToArray();
            //AssetDatabase.Refresh();
        }
    }

    void UpdateSceneDelayed()
    {
        string filePath = EditorPrefs.GetString("ExaltTopSpritesheetPath");
        if (assetImporter.assetPath != filePath)
        {
            EditorApplication.update -= UpdateSceneDelayed;
            return;
        }

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(filePath).OfType<Sprite>().ToArray();

        if (sprites.Length != EnvironmentSpritemapGenerator.SelectedSpritesheetRegions.Count || sprites.Length != EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList.Count)
        {
            Debug.LogError("Mismatch between number of sprites and number of regions!");
            //EditorApplication.update -= UpdateSceneDelayed;
            return;
        }


        for (int i = 0; i < EnvironmentSpritemapGenerator.SelectedSpritesheetRegions.Count; i++)
        {
            Undo.RecordObject(EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList[i].TopSprite, "Assign spritesheet sprite to environment physics top sprite");
            EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList[i].TopSprite.sprite = sprites[i];
        }
        EditorApplication.update -= UpdateSceneDelayed;
    }
}
