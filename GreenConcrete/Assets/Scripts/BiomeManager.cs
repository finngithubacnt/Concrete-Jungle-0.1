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

    [Header("Advanced Border Control")]
    [Tooltip("Use multi-octave noise for more natural, fractal borders")]
    public bool useMultiOctaveNoise = true;

    [Tooltip("Number of noise octaves for border distortion (higher = more detail)")]
    [Range(1, 6)]
    public int borderNoiseOctaves = 3;

    [Tooltip("Lacunarity - frequency multiplier for each octave")]
    [Range(1.5f, 3f)]
    public float borderNoiseLacunarity = 2f;

    [Tooltip("Persistence - amplitude multiplier for each octave")]
    [Range(0.3f, 0.7f)]
    public float borderNoisePersistence = 0.5f;

    [Tooltip("Blend width between biomes in tiles (higher = smoother transitions)")]
    [Range(1f, 20f)]
    public float biomeBlendWidth = 8f;

    [Tooltip("Use Voronoi-style distribution for more connected biome regions")]
    public bool useConnectedRegions = true;

    [Header("Texture Blending (Per-Pixel)")]
    [Tooltip("Enable per-pixel biome sampling for organic boundaries. Disable for better performance (blocky tiles)")]
    public bool usePerPixelBlending = true;

    [Tooltip("How many nearest centers to consider when computing influences (2 is enough for blend).")]
    public int neighborLookups = 3;

    [Header("Texture Alignment")]
    [Tooltip("Align terrain textures across tile boundaries to prevent mismatched patterns")]
    public bool alignTexturesAcrossTiles = true;

    [Header("Debug Visualization")]
    [Tooltip("Show biome centers and radii in scene view")]
    public bool showDebugGizmos = true;

    [Tooltip("Draw biome border lines in scene view")]
    public bool showBorderDebug = false;

    [Tooltip("Debug grid resolution for border visualization")]
    [Range(10, 100)]
    public int debugGridSize = 30;

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

    private float GetMultiOctaveNoise(float x, float y, float scale, int octaves, float lacunarity, float persistence)
    {
        if (!useMultiOctaveNoise || octaves <= 1)
        {
            return Mathf.PerlinNoise(x * scale, y * scale);
        }

        float total = 0f;
        float frequency = scale;
        float amplitude = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            total += (Mathf.PerlinNoise(x * frequency, y * frequency) - 0.5f) * 2f * amplitude;
            maxValue += amplitude;
            
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
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

        if (useConnectedRegions)
        {
            GenerateConnectedRegions();
        }
        else
        {
            GenerateRandomCenters();
        }
    }

    private void GenerateRandomCenters()
    {
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

    private void GenerateConnectedRegions()
    {
        int minDistance = Mathf.Max(3, (int)(worldExtentInTiles * 2f / Mathf.Sqrt(centerCount)));

        for (int i = 0; i < centerCount; i++)
        {
            Vector2 newPos;
            int attempts = 0;
            const int maxAttempts = 30;

            do
            {
                float px = Random.Range(-worldExtentInTiles, worldExtentInTiles);
                float py = Random.Range(-worldExtentInTiles, worldExtentInTiles);
                newPos = new Vector2(px, py);
                attempts++;
            }
            while (attempts < maxAttempts && !IsValidCenterPosition(newPos, minDistance));

            var def = biomeDefinitions[Random.Range(0, biomeDefinitions.Count)];
            float variation = 1f + Random.Range(-def.radiusVariation, def.radiusVariation);
            float radius = Mathf.Max(1f, def.radiusInTiles * variation);

            centers.Add(new BiomeCenter()
            {
                pos = newPos,
                def = def,
                radius = radius,
                id = i
            });
        }
    }

    private bool IsValidCenterPosition(Vector2 pos, float minDistance)
    {
        foreach (var c in centers)
        {
            float dist = Vector2.Distance(pos, c.pos);
            if (dist < minDistance)
            {
                return false;
            }
        }
        return true;
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

            float noiseValue = GetMultiOctaveNoise(
                tx + worldSeed * 0.1f,
                ty + worldSeed * 0.1f,
                borderNoiseScale,
                borderNoiseOctaves,
                borderNoiseLacunarity,
                borderNoisePersistence
            );

            float dDistorted = d + noiseValue * borderNoiseAmplitudeInTiles;

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
        float blendRange = biomeBlendWidth;
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

            float noiseValue = GetMultiOctaveNoise(
                tx + worldSeed * 0.1f,
                ty + worldSeed * 0.1f,
                borderNoiseScale,
                borderNoiseOctaves,
                borderNoiseLacunarity,
                borderNoisePersistence
            );

            float dDistorted = d + noiseValue * borderNoiseAmplitudeInTiles;

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

            float noiseValue = GetMultiOctaveNoise(
                wx + worldSeed * 0.1f,
                wy + worldSeed * 0.1f,
                borderNoiseScale * 10f,
                borderNoiseOctaves,
                borderNoiseLacunarity,
                borderNoisePersistence
            );
            
            float noiseAmplitude = borderNoiseAmplitudeInTiles * 0.5f;
            float dDistorted = d + noiseValue * noiseAmplitude;

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
        if (!showDebugGizmos) return;

        Gizmos.color = Color.white;
        foreach (var c in centers)
        {
            Gizmos.color = c.def != null ? c.def.debugColor : Color.white;
            Vector3 worldPos = TileToWorld(c.pos);
            Gizmos.DrawSphere(worldPos + Vector3.up * 2f, Mathf.Max(tileSize * 0.2f, 2f));
            Gizmos.DrawWireSphere(worldPos, c.radius * tileSize);
        }

        if (showBorderDebug && centers.Count > 0)
        {
            DrawBorderDebug();
        }
    }

    private void DrawBorderDebug()
    {
        int gridRes = debugGridSize;
        float step = (worldExtentInTiles * 2f) / gridRes;

        for (int gx = 0; gx < gridRes; gx++)
        {
            for (int gz = 0; gz < gridRes; gz++)
            {
                float tx = -worldExtentInTiles + gx * step;
                float tz = -worldExtentInTiles + gz * step;

                Vector2Int tileCoord = new Vector2Int(Mathf.FloorToInt(tx), Mathf.FloorToInt(tz));
                var assignment = GetBiomeForTile(tileCoord);

                if (assignment.biome != null)
                {
                    Color biomeColor = assignment.biome.debugColor;
                    biomeColor.a = assignment.blend * 0.7f;
                    Gizmos.color = biomeColor;

                    Vector3 worldPos = new Vector3(tx * tileSize, 0.5f, tz * tileSize);
                    Gizmos.DrawCube(worldPos, Vector3.one * tileSize * step * 0.9f);

                    if (assignment.blend < 0.8f)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireCube(worldPos, Vector3.one * tileSize * step);
                    }
                }
            }
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

    private TerrainLayer CreateAlignedTerrainLayer(TerrainLayer sourceLayer, Vector2Int tileCoord)
    {
        TerrainLayer alignedLayer = new TerrainLayer();
        
        alignedLayer.diffuseTexture = sourceLayer.diffuseTexture;
        alignedLayer.normalMapTexture = sourceLayer.normalMapTexture;
        alignedLayer.maskMapTexture = sourceLayer.maskMapTexture;
        alignedLayer.tileSize = sourceLayer.tileSize;
        alignedLayer.specular = sourceLayer.specular;
        alignedLayer.metallic = sourceLayer.metallic;
        alignedLayer.smoothness = sourceLayer.smoothness;
        alignedLayer.normalScale = sourceLayer.normalScale;
        alignedLayer.diffuseRemapMin = sourceLayer.diffuseRemapMin;
        alignedLayer.diffuseRemapMax = sourceLayer.diffuseRemapMax;
        alignedLayer.maskMapRemapMin = sourceLayer.maskMapRemapMin;
        alignedLayer.maskMapRemapMax = sourceLayer.maskMapRemapMax;
        alignedLayer.smoothnessSource = sourceLayer.smoothnessSource;
        
        float offsetX = (tileCoord.x * tileSize) / sourceLayer.tileSize.x;
        float offsetY = (tileCoord.y * tileSize) / sourceLayer.tileSize.y;
        
        offsetX = offsetX - Mathf.Floor(offsetX);
        offsetY = offsetY - Mathf.Floor(offsetY);
        
        alignedLayer.tileOffset = new Vector2(
            sourceLayer.tileOffset.x - offsetX,
            sourceLayer.tileOffset.y - offsetY
        );
        
        return alignedLayer;
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
                    TerrainLayer layerToUse = alignTexturesAcrossTiles 
                        ? CreateAlignedTerrainLayer(l, tileCoord) 
                        : l;
                    mergedLayers.Add(layerToUse);
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
