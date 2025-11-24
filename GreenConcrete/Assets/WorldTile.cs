using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.XR;
public class WorldTile : MonoBehaviour
{
    public BiomeDefinition assignedBiome;
    public Vector2Int tileCoord;
    public Terrain terrain;
    public Material Material1;
    public Terrain terrain1;

    public void Start()
    {

        Initialize(Vector2Int coord, BiomeDefinition biome, Terrain terrainRef);
    }
    public void Initialize(Vector2Int coord, BiomeDefinition biome, Terrain terrainRef)
    {
        
        tileCoord = coord;
        assignedBiome = biome;
        terrain = terrainRef;

       ApplyBiomeEffects();
    }

    private void ApplyBiomeEffects()
    {
        if (assignedBiome == null || terrain == null)
        {
            Debug.LogWarning("Assigned biome or terrain is null. Cannot apply biome effects.");
            Debug.Log("Assigned Biome: " + (assignedBiome == null ? "null" : assignedBiome.biomeName));
            return;
        }

        // Example biome terrain layer override
        if (assignedBiome.biomeMaterial != null)
        {
            Debug.Log("Applying biome terrain layers.");
            terrain.terrainData.terrainLayers = new TerrainLayer[] { assignedBiome.biomeMaterial };
        }

        // Example: adjust terrain height multiplier or foliage density later
    }
}
