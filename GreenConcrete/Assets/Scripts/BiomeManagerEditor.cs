using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BiomeManager))]
public class BiomeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BiomeManager biomeManager = (BiomeManager)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Natural Landscape", GUILayout.Height(30)))
        {
            ApplyNaturalLandscapePreset(biomeManager);
        }
        
        if (GUILayout.Button("Stylized", GUILayout.Height(30)))
        {
            ApplyStylizedPreset(biomeManager);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Sharp Borders", GUILayout.Height(30)))
        {
            ApplySharpBordersPreset(biomeManager);
        }
        
        if (GUILayout.Button("Performance", GUILayout.Height(30)))
        {
            ApplyPerformancePreset(biomeManager);
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Regenerate Biome Centers", GUILayout.Height(35)))
        {
            biomeManager.GenerateCenters();
            EditorUtility.SetDirty(biomeManager);
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Use presets for quick configuration. Regenerate centers after changing settings for immediate effect.",
            MessageType.Info
        );
    }

    private void ApplyNaturalLandscapePreset(BiomeManager bm)
    {
        Undo.RecordObject(bm, "Apply Natural Landscape Preset");
        
        bm.borderNoiseScale = 0.08f;
        bm.borderNoiseAmplitudeInTiles = 6f;
        bm.useMultiOctaveNoise = true;
        bm.borderNoiseOctaves = 3;
        bm.borderNoiseLacunarity = 2f;
        bm.borderNoisePersistence = 0.5f;
        bm.biomeBlendWidth = 8f;
        bm.useConnectedRegions = true;
        bm.usePerPixelBlending = true;
        bm.alignTexturesAcrossTiles = true;
        
        bm.GenerateCenters();
        EditorUtility.SetDirty(bm);
        
        Debug.Log("Applied Natural Landscape preset to BiomeManager");
    }

    private void ApplyStylizedPreset(BiomeManager bm)
    {
        Undo.RecordObject(bm, "Apply Stylized Preset");
        
        bm.borderNoiseScale = 0.12f;
        bm.borderNoiseAmplitudeInTiles = 8f;
        bm.useMultiOctaveNoise = true;
        bm.borderNoiseOctaves = 2;
        bm.borderNoiseLacunarity = 2.5f;
        bm.borderNoisePersistence = 0.4f;
        bm.biomeBlendWidth = 5f;
        bm.useConnectedRegions = true;
        bm.usePerPixelBlending = true;
        bm.alignTexturesAcrossTiles = true;
        
        bm.GenerateCenters();
        EditorUtility.SetDirty(bm);
        
        Debug.Log("Applied Stylized preset to BiomeManager");
    }

    private void ApplySharpBordersPreset(BiomeManager bm)
    {
        Undo.RecordObject(bm, "Apply Sharp Borders Preset");
        
        bm.borderNoiseScale = 0.15f;
        bm.borderNoiseAmplitudeInTiles = 3f;
        bm.useMultiOctaveNoise = false;
        bm.borderNoiseOctaves = 1;
        bm.borderNoiseLacunarity = 2f;
        bm.borderNoisePersistence = 0.5f;
        bm.biomeBlendWidth = 3f;
        bm.useConnectedRegions = true;
        bm.usePerPixelBlending = true;
        bm.alignTexturesAcrossTiles = true;
        
        bm.GenerateCenters();
        EditorUtility.SetDirty(bm);
        
        Debug.Log("Applied Sharp Borders preset to BiomeManager");
    }

    private void ApplyPerformancePreset(BiomeManager bm)
    {
        Undo.RecordObject(bm, "Apply Performance Preset");
        
        bm.borderNoiseScale = 0.1f;
        bm.borderNoiseAmplitudeInTiles = 5f;
        bm.useMultiOctaveNoise = false;
        bm.borderNoiseOctaves = 1;
        bm.borderNoiseLacunarity = 2f;
        bm.borderNoisePersistence = 0.5f;
        bm.biomeBlendWidth = 6f;
        bm.useConnectedRegions = true;
        bm.usePerPixelBlending = false;
        bm.neighborLookups = 2;
        bm.alignTexturesAcrossTiles = true;
        
        bm.GenerateCenters();
        EditorUtility.SetDirty(bm);
        
        Debug.Log("Applied Performance preset to BiomeManager");
    }
}
