using UnityEngine;
using System.Collections.Generic;

public class terrainBasics : MonoBehaviour
{
    [Header("Player Tracking")]
    public Transform player;

    [Header("Terrain Settings")]
    public int heightmapResolution = 513;
    public float terrainSize = 512f;
    public float terrainHeight = 100f;
    public int renderRadius = 1; // 1 = 3x3 grid
    public Material terrainMaterial;

    [Header("Noise Settings")]
    public float noiseScale = 0.01f;
    public float noiseAmplitude = 1f;
    public int worldSeed = 1337;

    [Header("Edge Blending")]
    [Range(-50, 50)] public int blendWidth = 5; // smooth transition along shared edges

    private Dictionary<Vector2Int, Terrain> activeTiles = new Dictionary<Vector2Int, Terrain>();
    private Dictionary<Vector2Int, float[,]> savedHeights = new Dictionary<Vector2Int, float[,]>();
    private Vector2Int currentCenter;

    void Start()
    {
        Random.InitState(worldSeed);
        UpdateTerrainGrid();
    }

    void Update()
    {
        Vector2Int newCenter = GetPlayerGridPos();
        if (newCenter != currentCenter)
        {
            currentCenter = newCenter;
            UpdateTerrainGrid();
        }
    }

    Vector2Int GetPlayerGridPos()
    {
        int x = Mathf.FloorToInt(player.position.x / terrainSize);
        int z = Mathf.FloorToInt(player.position.z / terrainSize);
        return new Vector2Int(x, z);
    }

    void UpdateTerrainGrid()
    {
        Vector2Int playerGrid = GetPlayerGridPos();

        // Generate nearby tiles
        for (int gx = -renderRadius; gx <= renderRadius; gx++)
        {
            for (int gz = -renderRadius; gz <= renderRadius; gz++)
            {
                Vector2Int tilePos = new Vector2Int(playerGrid.x + gx, playerGrid.y + gz);

                if (!activeTiles.ContainsKey(tilePos))
                {
                    Terrain newTile = CreateOrLoadTile(tilePos);
                    activeTiles.Add(tilePos, newTile);
                }
            }
        }

        // Unload distant tiles
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in activeTiles)
        {
            if (Mathf.Abs(kvp.Key.x - playerGrid.x) > renderRadius + 1 ||
                Mathf.Abs(kvp.Key.y - playerGrid.y) > renderRadius + 1)
            {
                // Save tile data before destroying
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

        // Load saved heights if returning to old area
        if (savedHeights.TryGetValue(gridPos, out heights))
        {
            data.SetHeights(0, 0, heights);
        }
        else
        {
            // Generate new heights
            heights = GenerateTerrainHeights(gridPos);
            data.SetHeights(0, 0, heights);
            savedHeights[gridPos] = heights;
        }

        // Create terrain object
        Vector3 pos = new Vector3(gridPos.x * terrainSize, 0, gridPos.y * terrainSize);
        GameObject terrainObj = Terrain.CreateTerrainGameObject(data);
        terrainObj.transform.position = pos;
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
                float noiseValue = Mathf.PerlinNoise(worldX + worldSeed, worldY + worldSeed) * noiseAmplitude;
                heights[y, x] = noiseValue;
            }
        }

        // Blend edges with neighboring saved or active tiles
        BlendEdges(gridPos, heights);

        return heights;
    }

    void BlendEdges(Vector2Int gridPos, float[,] heights)
    {
        int res = heightmapResolution;
        // ensure blend width is within sensible bounds (also prevents out-of-range indexing)
        int bw = Mathf.Clamp(blendWidth, 1, Mathf.Max(1, res / 2));

        // directions: neighbor offset and edge name
        foreach (var dir in new (Vector2Int offset, string edge)[]
        {
            (Vector2Int.up, "north"),
            (Vector2Int.down, "south"),
            (Vector2Int.left, "west"),
            (Vector2Int.right, "east")
        })
        {
            Vector2Int neighborPos = gridPos + dir.offset;

            // Try get neighbor from active tiles first (so we can update it immediately),
            // otherwise try savedHeights.
            float[,] neighborHeights = null;
            Terrain neighborTerrain = null;
            bool neighborActive = false;

            if (activeTiles.TryGetValue(neighborPos, out neighborTerrain))
            {
                neighborHeights = neighborTerrain.terrainData.GetHeights(0, 0, res, res);
                neighborActive = true;
            }
            else if (!savedHeights.TryGetValue(neighborPos, out neighborHeights))
            {
                // no neighbor data to blend with
                continue;
            }

            // Perform blending across the specified width.
            // Use alpha = (b + 1) / (bw + 1) so blends smoothly from neighbor -> current tile.
            for (int i = 0; i < res; i++)
            {
                for (int b = 0; b < bw; b++)
                {
                    float alpha = (b + 1f) / (bw + 1f);
                    switch (dir.edge)
                    {
                        case "north":
                            // neighbor is north of current tile:
                            // neighbor's south rows are neighborHeights[0 + b, i]
                            // current's north rows are heights[res - 1 - b, i]
                            {
                                float nVal = neighborHeights[b, i];
                                float myVal = heights[res - 1 - b, i];
                                float blended = Mathf.Lerp(nVal, myVal, alpha);
                                heights[res - 1 - b, i] = blended;
                                if (neighborActive) neighborHeights[b, i] = blended;
                            }
                            break;

                        case "south":
                            // neighbor is south:
                            // neighbor's north rows = neighborHeights[res - 1 - b, i]
                            // current's south rows = heights[b, i]
                            {
                                float nVal = neighborHeights[res - 1 - b, i];
                                float myVal = heights[b, i];
                                float blended = Mathf.Lerp(nVal, myVal, alpha);
                                heights[b, i] = blended;
                                if (neighborActive) neighborHeights[res - 1 - b, i] = blended;
                            }
                            break;

                        case "west":
                            // neighbor is left (west):
                            // neighbor's east columns = neighborHeights[i, res - 1 - b]
                            // current's west columns = heights[i, b]
                            {
                                float nVal = neighborHeights[i, res - 1 - b];
                                float myVal = heights[i, b];
                                float blended = Mathf.Lerp(nVal, myVal, alpha);
                                heights[i, b] = blended;
                                if (neighborActive) neighborHeights[i, res - 1 - b] = blended;
                            }
                            break;

                        case "east":
                            // neighbor is right (east):
                            // neighbor's west columns = neighborHeights[i, b]
                            // current's east columns = heights[i, res - 1 - b]
                            {
                                float nVal = neighborHeights[i, b];
                                float myVal = heights[i, res - 1 - b];
                                float blended = Mathf.Lerp(nVal, myVal, alpha);
                                heights[i, res - 1 - b] = blended;
                                if (neighborActive) neighborHeights[i, b] = blended;
                            }
                            break;
                    }
                }
            }

            // If neighbor is active we updated neighborHeights in memory — write back to the TerrainData
            // and update savedHeights so future loads are consistent.
            if (neighborActive && neighborTerrain != null)
            {
                neighborTerrain.terrainData.SetHeights(0, 0, neighborHeights);
                savedHeights[neighborPos] = neighborHeights;
            }
        }
    }

    void SaveTileData(Vector2Int gridPos, TerrainData data)
    {
        float[,] heights = data.GetHeights(0, 0, heightmapResolution, heightmapResolution);
        savedHeights[gridPos] = heights;
    }
}