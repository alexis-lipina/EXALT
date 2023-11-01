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
        string topFilePath = EditorPrefs.GetString("ExaltTopSpritesheetPath");
        string frontFilePath = EditorPrefs.GetString("ExaltFrontSpritesheetPath");
        if (assetImporter.assetPath != frontFilePath && assetImporter.assetPath != topFilePath)
        {
            return;
        }
        EditorApplication.update += UpdateSceneDelayed;
    }

    void OnPostprocessTexture(Texture2D texture)
    {
        return;
        /*
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
        AssetDatabase.Refresh();*/
    }

    void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;
        if (importer.spritePixelsPerUnit == 100)
        {
            // first import
            importer.spritePixelsPerUnit = 16;
            importer.maxTextureSize = 8192;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }

        string topFilePath = EditorPrefs.GetString("ExaltTopSpritesheetPath");
        string frontFilePath = EditorPrefs.GetString("ExaltFrontSpritesheetPath");
        if (importer.assetPath == topFilePath || importer.assetPath == frontFilePath)
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
            return;
            //AssetDatabase.Refresh();
        }
    }

    void UpdateSceneDelayed()
    {
        string topFilePath = EditorPrefs.GetString("ExaltTopSpritesheetPath");
        string frontFilePath = EditorPrefs.GetString("ExaltFrontSpritesheetPath");

        if (assetImporter.assetPath != topFilePath && assetImporter.assetPath != frontFilePath)
        {
            EditorApplication.update -= UpdateSceneDelayed;
            return;
        }

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetImporter.assetPath).OfType<Sprite>().ToArray();

        if (sprites.Length != EnvironmentSpritemapGenerator.SelectedSpritesheetRegions.Count || sprites.Length != EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList.Count)
        {
            Debug.LogError("Mismatch between number of sprites and number of objects!");
            //EditorApplication.update -= UpdateSceneDelayed;
            return;
        }

        if (topFilePath == assetImporter.assetPath)
        {
            for (int i = 0; i < EnvironmentSpritemapGenerator.SelectedSpritesheetRegions.Count; i++)
            {
                Undo.RecordObject(EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList[i].TopSprite, "Assign spritesheet sprite to environment physics top sprite");
                EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList[i].TopSprite.sprite = sprites[i];
            }
            EditorApplication.update -= UpdateSceneDelayed;
            EditorPrefs.SetString("ExaltTopSpritesheetPath", "");
        }
        else if (frontFilePath == assetImporter.assetPath)
        {
            for (int i = 0; i < EnvironmentSpritemapGenerator.SelectedSpritesheetRegions.Count; i++)
            {
                Undo.RecordObject(EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList[i].FrontSprite, "Assign spritesheet sprite to environment physics front sprite");
                EnvironmentSpritemapGenerator.SelectedEnvtPhysicsList[i].FrontSprite.sprite = sprites[i];
            }
            EditorApplication.update -= UpdateSceneDelayed;
            EditorPrefs.SetString("ExaltFrontSpritesheetPath", "");
        }
    }
}
