using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Result describing what biome covers a tile and how strongly.
/// </summary>
public struct BiomeAssignment
{
    public BiomeDefinition biome;
    public float blend;     // 0..1 where 1 means strongly inside this biome, 0 near border
    public float weight;    // raw weight relative to other biomes (useful for multi-influence)
    public Vector2 centerPosition; // center in tile-space
}

[ExecuteAlways]
public class BiomeManager : MonoBehaviour
{
    public static BiomeManager Instance; // singleton

    [Header("World / Tile Settings")]
    [Tooltip("Tile size in Unity units (same used in generator).")]
    public float tileSize = 200f;

    [Tooltip("How far (in tiles) generated biome centers spread from origin (half-extents).")]
    public int worldExtentInTiles = 200; // covers [-extent, +extent] in tile coordinates

    [Header("Biome Centers")]
    [Tooltip("How many biome centers/anchors we'll place.")]
    public int centerCount = 20;

    [Tooltip("Seed for reproducible biome placement.")]
    public int worldSeed = 1337;

    [Tooltip("Perlin noise scale applied to the distance field (controls waviness of borders).")]
    public float borderNoiseScale = 0.08f;

    [Tooltip("How strong the noise distorts the border (in tile units).")]
    public float borderNoiseAmplitudeInTiles = 6f;

    [Header("Texture Blending (Per-Pixel)")]
    [Tooltip("Enable per-pixel biome sampling for organic boundaries. Disable for better performance (blocky tiles)")]
    public bool usePerPixelBlending = true;

    [Tooltip("How many nearest centers to consider when computing influences (2 is enough for blend).")]
    public int neighborLookups = 3;

    [Header("Biome Definitions (assign in inspector)")]
    public List<BiomeDefinition> biomeDefinitions = new List<BiomeDefinition>();

    // Internal center representation
    class BiomeCenter
    {
        public Vector2 pos; // tile-space coords
        public BiomeDefinition def;
        public float radius; // in tiles (derived from def + variation)
        public int id;
    }

    private List<BiomeCenter> centers = new List<BiomeCenter>();

    // public read-only accessor for debug / editor use
    public IReadOnlyList<BiomeDefinition> AvailableBiomes => biomeDefinitions;

    void OnEnable()
    {
        Instance = this;
        GenerateCenters();
    }

    [ContextMenu("Regenerate Biome Centers")]
    public void GenerateCenters()
    {
        centers.Clear();
        if (biomeDefinitions == null || biomeDefinitions.Count == 0)
        {
            Debug.LogWarning("BiomeManager: no biome definitions assigned.");
            return;
        }

        Random.InitState(worldSeed);
        for (int i = 0; i < centerCount; i++)
        {
            var def = biomeDefinitions[Random.Range(0, biomeDefinitions.Count)];
            float variation = 1f + Random.Range(-def.radiusVariation, def.radiusVariation);
            float radius = Mathf.Max(1f, def.radiusInTiles * variation);

            float px = Random.Range(-worldExtentInTiles, worldExtentInTiles);
            float py = Random.Range(-worldExtentInTiles, worldExtentInTiles);

            centers.Add(new BiomeCenter()
            {
                pos = new Vector2(px, py),
                def = def,
                radius = radius,
                id = i
            });
        }
    }

    /// <summary>
    /// Get the primary biome assignment for a tile coordinate (tileCoord = tile indices, not world units).
    /// </summary>
    public BiomeAssignment GetBiomeForTile(Vector2Int tileCoord)
    {
        float tx = tileCoord.x;
        float ty = tileCoord.y;

        BiomeCenter nearest = null;
        BiomeCenter secondNearest = null;
        float nearestDist = float.MaxValue;
        float secondDist = float.MaxValue;

        foreach (var c in centers)
        {
            float dx = tx - c.pos.x;
            float dy = ty - c.pos.y;
            float d = Mathf.Sqrt(dx * dx + dy * dy);

            float nx = (tx + c.pos.x) * borderNoiseScale;
            float ny = (ty + c.pos.y) * borderNoiseScale;
            float noise = (Mathf.PerlinNoise(nx + worldSeed * 0.1f, ny + worldSeed * 0.1f) - 0.5f) * 2f;
            float dDistorted = d + noise * borderNoiseAmplitudeInTiles;

            if (dDistorted < nearestDist)
            {
                secondDist = nearestDist;
                secondNearest = nearest;
                nearestDist = dDistorted;
                nearest = c;
            }
            else if (dDistorted < secondDist)
            {
                secondDist = dDistorted;
                secondNearest = c;
            }
        }

        if (nearest == null)
        {
            return new BiomeAssignment { biome = biomeDefinitions[0], blend = 1f, weight = 1f, centerPosition = Vector2.zero };
        }

        if (secondNearest == null)
        {
            return new BiomeAssignment { biome = nearest.def, blend = 1f, weight = 1f, centerPosition = nearest.pos };
        }

        float gap = secondDist - nearestDist;
        float avgRadius = (nearest.radius + secondNearest.radius) * 0.5f;
        float blendRange = Mathf.Max(3f, avgRadius * 0.6f);
        float raw = Mathf.Clamp01(gap / blendRange);
        float smooth = Mathf.SmoothStep(0f, 1f, raw);

        float nearestWeight = smooth;

        return new BiomeAssignment
        {
            biome = nearest.def,
            blend = nearestWeight,
            weight = nearestWeight,
            centerPosition = nearest.pos
        };
    }

  
    public List<BiomeAssignment> GetBiomeInfluences(Vector2Int tileCoord, int maxInfluences = 3)
    {
        int count = Mathf.Min(maxInfluences, centers.Count);
        var list = new List<(BiomeCenter center, float dist)>(centers.Count);

        float tx = tileCoord.x;
        float ty = tileCoord.y;

        foreach (var c in centers)
        {
            float dx = tx - c.pos.x;
            float dy = ty - c.pos.y;
            float d = Mathf.Sqrt(dx * dx + dy * dy);

            float nx = (tx + c.pos.x) * borderNoiseScale;
            float ny = (ty + c.pos.y) * borderNoiseScale;
            float noise = (Mathf.PerlinNoise(nx + worldSeed * 0.1f, ny + worldSeed * 0.1f) - 0.5f) * 2f;
            float dDistorted = d + noise * borderNoiseAmplitudeInTiles;

            list.Add((c, dDistorted));
        }

        list.Sort((a, b) => a.dist.CompareTo(b.dist));
        float totalInv = 0f;
        var assignments = new List<BiomeAssignment>();

        // convert distances to weights (closer -> higher)
        for (int i = 0; i < count; i++)
        {
            float w = 1f / (1f + list[i].dist); // simple inverse-distance weighting
            totalInv += w;
            assignments.Add(new BiomeAssignment
            {
                biome = list[i].center.def,
                blend = 0f, // set later
                weight = w,
                centerPosition = list[i].center.pos
            });
        }

        // normalize to 0..1 weights and set blend = normalized weight
        for (int i = 0; i < assignments.Count; i++)
        {
            assignments[i] = new BiomeAssignment
            {
                biome = assignments[i].biome,
                blend = assignments[i].weight / totalInv,
                weight = assignments[i].weight / totalInv,
                centerPosition = assignments[i].centerPosition
            };
        }

        return assignments;
    }

    public List<BiomeAssignment> GetBiomeInfluencesAtWorldPos(Vector2 worldPos, int maxInfluences = 3)
    {
        int count = Mathf.Min(maxInfluences, centers.Count);
        var list = new List<(BiomeCenter center, float dist)>(centers.Count);

        float wx = worldPos.x / tileSize;
        float wy = worldPos.y / tileSize;

        foreach (var c in centers)
        {
            float dx = wx - c.pos.x;
            float dy = wy - c.pos.y;
            float d = Mathf.Sqrt(dx * dx + dy * dy);

            float noiseScale = borderNoiseScale * 10f;
            float nx = worldPos.x * noiseScale;
            float ny = worldPos.y * noiseScale;
            float noise = (Mathf.PerlinNoise(nx + worldSeed * 0.1f, ny + worldSeed * 0.1f) - 0.5f) * 2f;
            
            float noiseAmplitude = borderNoiseAmplitudeInTiles * 0.5f;
            float dDistorted = d + noise * noiseAmplitude;

            list.Add((c, dDistorted));
        }

        list.Sort((a, b) => a.dist.CompareTo(b.dist));
        float totalInv = 0f;
        var assignments = new List<BiomeAssignment>();

        for (int i = 0; i < count; i++)
        {
            float w = 1f / (1f + list[i].dist);
            totalInv += w;
            assignments.Add(new BiomeAssignment
            {
                biome = list[i].center.def,
                blend = 0f,
                weight = w,
                centerPosition = list[i].center.pos
            });
        }

        for (int i = 0; i < assignments.Count; i++)
        {
            assignments[i] = new BiomeAssignment
            {
                biome = assignments[i].biome,
                blend = assignments[i].weight / totalInv,
                weight = assignments[i].weight / totalInv,
                centerPosition = assignments[i].centerPosition
            };
        }

        return assignments;
    }

    // Editor debug drawing of centers and their radii in tile-space
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        foreach (var c in centers)
        {
            Gizmos.color = c.def != null ? c.def.debugColor : Color.white;
            Vector3 worldPos = TileToWorld(c.pos);
            Gizmos.DrawSphere(worldPos + Vector3.up * 2f, Mathf.Max(tileSize * 0.2f, 2f));
            Gizmos.DrawWireSphere(worldPos, c.radius * tileSize);
        }
    }

    /// <summary>
    /// Convert tile-space coordinates to world-space centered position
    /// (tile coordinate -> world origin position of that tile).
    /// </summary>
    public Vector3 TileToWorld(Vector2 tileCoords)
    {
        return new Vector3(tileCoords.x * tileSize, 0f, tileCoords.y * tileSize);
    }

    /// <summary>
    /// Helper: tile index to world position (uses tile center if you prefer center).
    /// </summary>
    public Vector3 TileIndexToWorld(Vector2Int tileIndex)
    {
        return new Vector3(tileIndex.x * tileSize, 0f, tileIndex.y * tileSize);
    }
    public void ApplyBiomeEffects(Terrain terrain, Vector2Int tileCoord, int influenceCount = 3)
    {
        if (terrain == null) return;

        var influences = GetBiomeInfluences(tileCoord, influenceCount);
        var profs = new List<(BiomeDefinition def, BiomeAssignment assign)>();
        foreach (var inf in influences)
        {
            if (inf.biome != null && inf.biome.terrainLayers != null && inf.biome.terrainLayers.Length > 0)
                profs.Add((inf.biome, inf));
        }

        if (profs.Count == 0) return;

        var mergedLayers = new List<TerrainLayer>();
        var layerToIndex = new Dictionary<TerrainLayer, int>();

        foreach (var pair in profs)
        {
            var def = pair.def;
            foreach (var l in def.terrainLayers)
            {
                if (l == null) continue;
                if (!layerToIndex.ContainsKey(l))
                {
                    layerToIndex[l] = mergedLayers.Count;
                    mergedLayers.Add(l);
                }
            }
        }

        if (mergedLayers.Count == 0) return;

        terrain.terrainData.terrainLayers = mergedLayers.ToArray();

        int amapW = terrain.terrainData.alphamapWidth;
        int amapH = terrain.terrainData.alphamapHeight;
        int layerCount = mergedLayers.Count;
        float[,,] alphaMaps = new float[amapH, amapW, layerCount];

        if (usePerPixelBlending)
        {
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            for (int y = 0; y < amapH; y++)
            {
                for (int x = 0; x < amapW; x++)
                {
                    float normX = (float)x / (amapW - 1);
                    float normY = (float)y / (amapH - 1);
                    
                    float worldX = terrainPos.x + normX * terrainSize.x;
                    float worldZ = terrainPos.z + normY * terrainSize.z;
                    
                    var pixelInfluences = GetBiomeInfluencesAtWorldPos(new Vector2(worldX, worldZ), influenceCount);
                    
                    foreach (var inf in pixelInfluences)
                    {
                        if (inf.biome == null || inf.biome.terrainLayers == null) continue;
                        
                        float infWeight = inf.blend;
                        int defLayerCount = inf.biome.terrainLayers.Length;
                        float share = infWeight / defLayerCount;
                        
                        for (int li = 0; li < defLayerCount; li++)
                        {
                            var layer = inf.biome.terrainLayers[li];
                            if (layer == null) continue;
                            if (!layerToIndex.ContainsKey(layer)) continue;
                            int mergedIdx = layerToIndex[layer];
                            alphaMaps[y, x, mergedIdx] += share;
                        }
                    }
                }
            }
        }
        else
        {
            foreach (var pair in profs)
            {
                var def = pair.def;
                var assign = pair.assign;
                float infWeight = assign.blend;
                int defLayerCount = def.terrainLayers.Length;

                for (int y = 0; y < amapH; y++)
                {
                    for (int x = 0; x < amapW; x++)
                    {
                        float share = infWeight / defLayerCount;
                        for (int li = 0; li < defLayerCount; li++)
                        {
                            var layer = def.terrainLayers[li];
                            if (layer == null) continue;
                            int mergedIdx = layerToIndex[layer];
                            alphaMaps[y, x, mergedIdx] += share;
                        }
                    }
                }
            }
        }

        for (int y = 0; y < amapH; y++)
        {
            for (int x = 0; x < amapW; x++)
            {
                float sum = 0f;
                for (int l = 0; l < layerCount; l++) sum += alphaMaps[y, x, l];
                if (sum <= 0f)
                {
                    alphaMaps[y, x, 0] = 1f;
                    continue;
                }
                for (int l = 0; l < layerCount; l++) alphaMaps[y, x, l] /= sum;
            }
        }

        float[,,] converted = new float[amapW, amapH, layerCount];
        for (int y = 0; y < amapH; y++)
            for (int x = 0; x < amapW; x++)
                for (int l = 0; l < layerCount; l++)
                    converted[x, y, l] = alphaMaps[y, x, l];

        terrain.terrainData.SetAlphamaps(0, 0, converted);
    }

    public void ApplyBiomeHeightModification(Terrain terrain, Vector2Int tileCoord, int seed, int influenceCount = 3)
    {
        if (terrain == null) return;

        var influences = GetBiomeInfluences(tileCoord, influenceCount);
        
        bool anyHeightModification = false;
        foreach (var inf in influences)
        {
            if (inf.biome != null && inf.biome.modifyHeight)
            {
                anyHeightModification = true;
                break;
            }
        }

        if (!anyHeightModification) return;

        int heightmapWidth = terrain.terrainData.heightmapResolution;
        int heightmapHeight = terrain.terrainData.heightmapResolution;
        float[,] heights = terrain.terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        for (int y = 0; y < heightmapHeight; y++)
        {
            for (int x = 0; x < heightmapWidth; x++)
            {
                float originalHeight = heights[y, x];
                
                float normX = (float)x / (heightmapWidth - 1);
                float normY = (float)y / (heightmapHeight - 1);
                
                float worldX = terrainPos.x + normX * terrainSize.x;
                float worldZ = terrainPos.z + normY * terrainSize.z;
                
                var pixelInfluences = usePerPixelBlending 
                    ? GetBiomeInfluencesAtWorldPos(new Vector2(worldX, worldZ), influenceCount)
                    : influences;

                float modifiedHeight = originalHeight;
                float totalWeight = 0f;
                float accumulatedHeight = 0f;
                float totalModifyWeight = 0f;

                foreach (var inf in pixelInfluences)
                {
                    if (inf.biome == null) continue;

                    if (inf.biome.modifyHeight)
                    {
                        float biomeHeight = originalHeight;

                        if (inf.biome.biomeNoiseAmplitude > 0f)
                        {
                            float worldNoiseX = worldX * inf.biome.biomeNoiseScale;
                            float worldNoiseY = worldZ * inf.biome.biomeNoiseScale;
                            float noise = Mathf.PerlinNoise(worldNoiseX + seed, worldNoiseY + seed);
                            biomeHeight += noise * inf.biome.biomeNoiseAmplitude;
                        }

                        biomeHeight = biomeHeight * inf.biome.heightMultiplier + inf.biome.baseHeightOffset;

                        if (inf.biome.heightCurve != null && inf.biome.heightCurve.keys.Length > 0)
                        {
                            biomeHeight = inf.biome.heightCurve.Evaluate(Mathf.Clamp01(biomeHeight));
                        }

                        accumulatedHeight += biomeHeight * inf.blend;
                        totalModifyWeight += inf.blend;
                    }
                    
                    totalWeight += inf.blend;
                }

                if (totalModifyWeight > 0f && totalWeight > 0f)
                {
                    float blendedModifiedHeight = accumulatedHeight / totalModifyWeight;
                    float modifyInfluence = totalModifyWeight / totalWeight;
                    modifiedHeight = Mathf.Lerp(originalHeight, blendedModifiedHeight, modifyInfluence);
                }

                heights[y, x] = Mathf.Clamp01(modifiedHeight);
            }
        }

        terrain.terrainData.SetHeights(0, 0, heights);
    }

    public GameObject SpawnTilePrefab(Terrain terrain, Vector2Int tileCoord)
    {
        if (terrain == null) return null;

        var assignment = GetBiomeForTile(tileCoord);
        if (assignment.biome == null || assignment.biome.tilePrefab == null) return null;

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 centerPos = terrainPos + new Vector3(terrainSize.x * 0.5f, 0f, terrainSize.z * 0.5f);

        float normalizedX = 0.5f;
        float normalizedZ = 0.5f;
        float sampledHeight = terrain.terrainData.GetInterpolatedHeight(normalizedX, normalizedZ);
        
        Vector3 spawnPos = new Vector3(
            centerPos.x + assignment.biome.prefabPositionOffset.x,
            terrainPos.y + sampledHeight + assignment.biome.prefabPositionOffset.y,
            centerPos.z + assignment.biome.prefabPositionOffset.z
        );

        Quaternion rotation = Quaternion.identity;
        if (assignment.biome.randomRotation)
        {
            rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        GameObject spawnedPrefab = Instantiate(assignment.biome.tilePrefab, spawnPos, rotation);
        spawnedPrefab.name = $"{assignment.biome.tilePrefab.name}_{tileCoord.x}_{tileCoord.y}";
        spawnedPrefab.transform.SetParent(terrain.transform);

        return spawnedPrefab;
    }
}
