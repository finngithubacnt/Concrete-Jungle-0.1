using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BiomeRadiusFixer : EditorWindow
{
    private int newRadius = 15;

    [MenuItem("Tools/Fix Biome Radii")]
    public static void ShowWindow()
    {
        GetWindow<BiomeRadiusFixer>("Fix Biome Radii");
    }

    void OnGUI()
    {
        GUILayout.Label("Biome Radius Fixer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool will update the radius for all BiomeDefinition assets in your project.\n\n" +
            "Current issue: All biomes have radiusInTiles = 1, which is too small.\n" +
            "Recommended: 10-20 tiles for proper biome regions.",
            MessageType.Info
        );

        EditorGUILayout.Space();
        newRadius = EditorGUILayout.IntSlider("New Radius (tiles)", newRadius, 5, 30);
        EditorGUILayout.Space();

        if (GUILayout.Button("Fix All Biome Radii", GUILayout.Height(40)))
        {
            FixAllBiomeRadii();
        }

        EditorGUILayout.Space();
        
        if (GUILayout.Button("List All Biomes"))
        {
            ListAllBiomes();
        }
    }

    void FixAllBiomeRadii()
    {
        string[] guids = AssetDatabase.FindAssets("t:BiomeDefinition");
        List<BiomeDefinition> biomes = new List<BiomeDefinition>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BiomeDefinition biome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>(path);
            if (biome != null)
            {
                biomes.Add(biome);
            }
        }

        if (biomes.Count == 0)
        {
            EditorUtility.DisplayDialog("No Biomes Found", "No BiomeDefinition assets found in the project.", "OK");
            return;
        }

        foreach (BiomeDefinition biome in biomes)
        {
            Undo.RecordObject(biome, "Fix Biome Radius");
            biome.radiusInTiles = newRadius;
            EditorUtility.SetDirty(biome);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Updated {biomes.Count} biome(s) to radius = {newRadius} tiles");
        EditorUtility.DisplayDialog(
            "Success", 
            $"Updated {biomes.Count} biome(s) to radius = {newRadius} tiles.\n\n" +
            "Don't forget to regenerate biome centers in your BiomeManager!",
            "OK"
        );

        BiomeManager bm = FindFirstObjectByType<BiomeManager>();
        if (bm != null)
        {
            Selection.activeGameObject = bm.gameObject;
        }
    }

    void ListAllBiomes()
    {
        string[] guids = AssetDatabase.FindAssets("t:BiomeDefinition");
        
        Debug.Log("=== ALL BIOME DEFINITIONS ===");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BiomeDefinition biome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>(path);
            if (biome != null)
            {
                Debug.Log($"Biome: {biome.biomeName} | Radius: {biome.radiusInTiles} tiles | Path: {path}");
            }
        }
    }
}
