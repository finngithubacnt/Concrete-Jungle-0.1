using UnityEngine;
using System.Collections.Generic;

public class DynamicPersistentTerrain : MonoBehaviour
{
    [Header("Player Tracking")]
    public Transform player;

    [Header("Terrain Settings")]
    public int heightmapResolution = 513;
    public float terrainSize = 512f;
    public float terrainHeight = 100f;
    public int renderRadius = 1; // 1 = 3x3, 2 = 5x5, etc.
    public Material terrainMaterial;

    [Header("Noise Settings")]
    public float noiseScale = 0.01f;
    public float noiseAmplitude = 1f;
    public int worldSeed = 1337;

    [Header("Edge Blending")]
    [Range(-50, 50)] public int blendWidth = 5;

    [Header("Biome")]
    public BiomeManager biomeManager; //inspector

    private Dictionary<Vector2Int, Terrain> activeTiles = new Dictionary<Vector2Int, Terrain>();
    private Dictionary<Vector2Int, float[,]> savedHeights = new Dictionary<Vector2Int, float[,]>();
    private Vector2Int currentCenter;

    void Start()
    {
        Random.InitState(worldSeed);
        UpdateTerrainGrid(GetBiomeManager());
    }

    void Update()
    {
        Vector2Int newCenter = GetPlayerGridPos();
        if (newCenter != currentCenter)
        {
            currentCenter = newCenter;
            UpdateTerrainGrid(GetBiomeManager());
        }
    }

    Vector2Int GetPlayerGridPos()
    {
        int x = Mathf.FloorToInt(player.position.x / terrainSize);
        int z = Mathf.FloorToInt(player.position.z / terrainSize);
        return new Vector2Int(x, z);
    }

    private BiomeManager GetBiomeManager()
    {
        return biomeManager;
    }

    void UpdateTerrainGrid(BiomeManager biomeManager)
    {
        Vector2Int playerGrid = GetPlayerGridPos();
        for (int gx = -renderRadius; gx <= renderRadius; gx++)
        {
            for (int gz = -renderRadius; gz <= renderRadius; gz++)
            {
                Vector2Int tilePos = new Vector2Int(playerGrid.x + gx, playerGrid.y + gz);

                if (!activeTiles.ContainsKey(tilePos))
                {
                    Terrain newTile = CreateOrLoadTile(tilePos);
                    
                    if (biomeManager != null)
                    {
                        biomeManager.ApplyBiomeEffects(newTile, tilePos, 3);
                    }
                    
                    activeTiles.Add(tilePos, newTile);
                }
            }
        }
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in activeTiles)
        {
            if (Mathf.Abs(kvp.Key.x - playerGrid.x) > renderRadius + 1 ||
                Mathf.Abs(kvp.Key.y - playerGrid.y) > renderRadius + 1)
            {
                SaveTileData(kvp.Key, kvp.Value.terrainData);
                Destroy(kvp.Value.gameObject);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
        activeTiles.Remove(key);
    }

    Terrain CreateOrLoadTile(Vector2Int gridPos)
    {
        TerrainData data = new TerrainData();
        data.heightmapResolution = heightmapResolution;
        data.size = new Vector3(terrainSize, terrainHeight, terrainSize);

        float[,] heights;

        if (savedHeights.TryGetValue(gridPos, out heights))
        {
            // Use a copy so later modifications to the TerrainData don't accidentally mutate our saved buffer
            float[,] copy = new float[heightmapResolution, heightmapResolution];
            System.Array.Copy(heights, copy, heights.Length);
            data.SetHeights(0, 0, copy);
        }
        else
        {
            heights = GenerateTerrainHeights(gridPos);
            // Create a copy for TerrainData to avoid sharing the same reference that we store in savedHeights
            float[,] copyForTerrain = new float[heightmapResolution, heightmapResolution];
            System.Array.Copy(heights, copyForTerrain, heights.Length);
            data.SetHeights(0, 0, copyForTerrain);

            // store our canonical heights for persistence (keep separate from the TerrainData copy)
            savedHeights[gridPos] = heights;
        }

        GameObject terrainObj = Terrain.CreateTerrainGameObject(data);
        terrainObj.transform.position = new Vector3(gridPos.x * terrainSize, 0, gridPos.y * terrainSize);
        Terrain terrain = terrainObj.GetComponent<Terrain>();
        if (terrainMaterial) terrain.materialTemplate = terrainMaterial;

        return terrain;
    }

    float[,] GenerateTerrainHeights(Vector2Int gridPos)
    {
        float[,] heights = new float[heightmapResolution, heightmapResolution];

        for (int y = 0; y < heightmapResolution; y++)
        {
            for (int x = 0; x < heightmapResolution; x++)
            {
                float worldX = (x + gridPos.x * (heightmapResolution - 1)) * noiseScale;
                float worldY = (y + gridPos.y * (heightmapResolution - 1)) * noiseScale;
                heights[y, x] = Mathf.PerlinNoise(worldX + worldSeed, worldY + worldSeed) * noiseAmplitude;
            }
        }

        BlendEdges(gridPos, heights);
        return heights;
    }

    void BlendEdges(Vector2Int gridPos, float[,] heights)
    {
        int res = heightmapResolution;
        // enforce sensible, non-negative blend width and prevent out-of-range indexing
        int bw = Mathf.Clamp(blendWidth, 1, Mathf.Max(1, res / 2));

        // helper to fetch neighbor heights (prefer active tile data, otherwise saved data)
        float[,] GetNeighborHeights(Vector2Int pos, out Terrain neighborTerrain, out bool neighborActive)
        {
            neighborTerrain = null;
            neighborActive = false;
            if (activeTiles.TryGetValue(pos, out neighborTerrain) && neighborTerrain != null)
            {
                // GetHeights returns a copy so safe to mutate and write back later
                neighborActive = true;
                return neighborTerrain.terrainData.GetHeights(0, 0, res, res);
            }

            if (savedHeights.TryGetValue(pos, out var saved))
            {
                return saved;
            }

            neighborTerrain = null;
            neighborActive = false;
            return null;
        }
        var dirs = new (Vector2Int offset, string edge)[]
        {
            (Vector2Int.up, "north"),
            (Vector2Int.down, "south"),
            (Vector2Int.left, "west"),
            (Vector2Int.right, "east")
        };

        foreach (var dir in dirs)
        {
            Vector2Int nPos = gridPos + dir.offset;
            var neighborHeights = GetNeighborHeights(nPos, out Terrain neighborTerrain, out bool neighborActive);
            if (neighborHeights == null)
                continue;

           
            bool neighborModified = false;

            for (int i = 0; i < res; i++)
            {
                for (int b = 0; b < bw; b++)
                {
                    float alpha = (b + 1f) / (bw + 1f); // smooth ramp 
                    switch (dir.edge)
                    {
                        case "north"://z+
                        {
                        int nRow = b;
                        int myRow = res - 1 - b;
                        float nVal = neighborHeights[nRow, i];
                        float myVal = heights[myRow, i];
                        float blended = Mathf.Lerp(nVal, myVal, alpha);
                        heights[myRow, i] = blended;
                        if (neighborActive)
                         {
                             neighborHeights[nRow, i] = blended;
                             neighborModified = true;
                         }
                        }
                            
                        break;

                        case "south"://z-
                        {
                         int nRow = res - 1 - b;
                         int myRow = b;
                         float nVal = neighborHeights[nRow, i];
                         float myVal = heights[myRow, i];
                         float blended = Mathf.Lerp(nVal, myVal, alpha);
                         heights[myRow, i] = blended;
                         if (neighborActive)
                            {
                              neighborHeights[nRow, i] = blended;
                              neighborModified = true;
                            }
                        }
                        break;

                        case "west"://x-
                           
                            {
                                int nCol = res - 1 - b;
                                int myCol = b;
                                float nVal = neighborHeights[i, nCol];
                                float myVal = heights[i, myCol];
                                float blended = Mathf.Lerp(nVal, myVal, alpha);
                                heights[i, myCol] = blended;
                                if (neighborActive)
                                {
                                    neighborHeights[i, nCol] = blended;
                                    neighborModified = true;
                               }
                      }
                        break;

                        case "east"://x+
                            {
                                int nCol = b;
                                int myCol = res - 1 - b;
                                float nVal = neighborHeights[i, nCol];
                                float myVal = heights[i, myCol];
                                float blended = Mathf.Lerp(nVal, myVal, alpha);
                                heights[i, myCol] = blended;
                                if (neighborActive)
                                {
                                    neighborHeights[i, nCol] = blended;
                                    neighborModified = true;
                                }
                              }
                        break;
                    }
             }
            }
            if (neighborActive && neighborModified && neighborTerrain != null)
            {
                neighborTerrain.terrainData.SetHeights(0, 0, neighborHeights);
                float[,] neighborCopy = new float[res, res];
                System.Array.Copy(neighborHeights, neighborCopy, neighborHeights.Length);
                savedHeights[nPos] = neighborCopy;
            }
        }
    }
    void SaveTileData(Vector2Int gridPos, TerrainData data)
    {
        int res = heightmapResolution;
        float[,] heights = data.GetHeights(0, 0, res, res);
        float[,] copy = new float[res, res];
        System.Array.Copy(heights, copy, heights.Length);
        savedHeights[gridPos] = copy;
    }
}