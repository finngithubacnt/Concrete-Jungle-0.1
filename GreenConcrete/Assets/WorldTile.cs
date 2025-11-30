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

        if (assignedBiome.terrainLayers != null && assignedBiome.terrainLayers.Length > 0)
        {
            Debug.Log($"Applying {assignedBiome.terrainLayers.Length} terrain layers from biome '{assignedBiome.biomeName}'");
            terrain.terrainData.terrainLayers = assignedBiome.terrainLayers;
            
            int alphamapWidth = terrain.terrainData.alphamapWidth;
            int alphamapHeight = terrain.terrainData.alphamapHeight;
            int layerCount = assignedBiome.terrainLayers.Length;
            
            float[,,] alphaMaps = new float[alphamapWidth, alphamapHeight, layerCount];
            
            for (int y = 0; y < alphamapHeight; y++)
            {
                for (int x = 0; x < alphamapWidth; x++)
                {
                    alphaMaps[x, y, 0] = 1.0f;
                }
            }
            
            terrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
        }
        else
        {
            Debug.LogWarning($"Biome '{assignedBiome.biomeName}' has no terrain layers assigned!");
        }
    }
}
