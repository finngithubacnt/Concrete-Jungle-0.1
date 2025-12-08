using UnityEngine;
using UnityEditor;

public class QuickFixBiomes
{
    [MenuItem("Tools/Quick Fix: Update All Biome Settings")]
    public static void QuickFixAllBiomeSettings()
    {
        if (!EditorUtility.DisplayDialog(
            "Quick Fix Biomes",
            "This will:\n" +
            "1. Set all biome radiusInTiles to 15\n" +
            "2. Configure BiomeManager with optimal settings\n" +
            "3. Regenerate biome centers\n\n" +
            "This cannot be undone. Continue?",
            "Yes, Fix It",
            "Cancel"))
        {
            return;
        }

        int fixedCount = 0;

        string[] guids = AssetDatabase.FindAssets("t:BiomeDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BiomeDefinition biome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>(path);
            if (biome != null)
            {
                Undo.RecordObject(biome, "Quick Fix Biome");
                biome.radiusInTiles = 15;
                biome.radiusVariation = 0.3f;
                EditorUtility.SetDirty(biome);
                fixedCount++;
            }
        }

        BiomeManager bm = Object.FindFirstObjectByType<BiomeManager>();
        if (bm != null)
        {
            Undo.RecordObject(bm, "Quick Fix BiomeManager");
            
            bm.borderNoiseScale = 0.08f;
            bm.borderNoiseAmplitudeInTiles = 6f;
            bm.useMultiOctaveNoise = true;
            bm.borderNoiseOctaves = 3;
            bm.borderNoiseLacunarity = 2f;
            bm.borderNoisePersistence = 0.5f;
            bm.biomeBlendWidth = 8f;
            bm.useConnectedRegions = true;
            bm.usePerPixelBlending = true;
            bm.neighborLookups = 3;
            bm.alignTexturesAcrossTiles = true;
            
            bm.GenerateCenters();
            EditorUtility.SetDirty(bm);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = $"✓ Updated {fixedCount} biome definition(s)\n";
        if (bm != null)
        {
            message += "✓ Configured BiomeManager\n✓ Regenerated biome centers\n\n";
            message += "Enter Play mode to see the improved biome borders!";
            Selection.activeGameObject = bm.gameObject;
        }
        else
        {
            message += "⚠ BiomeManager not found in scene";
        }

        Debug.Log($"<color=green>Quick Fix Complete:</color> {message}");
        EditorUtility.DisplayDialog("Success!", message, "OK");
    }

    [MenuItem("Tools/Quick Fix: Reset Biome Centers Only")]
    public static void QuickResetBiomeCenters()
    {
        BiomeManager bm = Object.FindFirstObjectByType<BiomeManager>();
        if (bm != null)
        {
            Undo.RecordObject(bm, "Reset Biome Centers");
            bm.GenerateCenters();
            EditorUtility.SetDirty(bm);
            Selection.activeGameObject = bm.gameObject;
            Debug.Log("<color=green>Biome centers regenerated!</color>");
            EditorUtility.DisplayDialog("Success", "Biome centers have been regenerated.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "BiomeManager not found in scene.", "OK");
        }
    }
}
