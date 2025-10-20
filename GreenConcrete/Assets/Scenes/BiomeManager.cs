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
        // We'll compute distances to nearest centers, optionally noise-distort them,
        // then compute a smooth blend factor using the nearest and second-nearest centers.
        float tx = tileCoord.x;
        float ty = tileCoord.y;

        // find nearest centers (simple linear scan - centerCount is small)
        BiomeCenter nearest = null;
        BiomeCenter secondNearest = null;
        float nearestDist = float.MaxValue;
        float secondDist = float.MaxValue;

        foreach (var c in centers)
        {
            float dx = tx - c.pos.x;
            float dy = ty - c.pos.y;
            float d = Mathf.Sqrt(dx * dx + dy * dy);

            // Distortion by Perlin noise (in tile-space)
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

        // if there is no second, just return nearest with full blend
        if (nearest == null)
        {
            return new BiomeAssignment { biome = biomeDefinitions[0], blend = 1f, weight = 1f, centerPosition = Vector2.zero };
        }

        if (secondNearest == null)
        {
            return new BiomeAssignment { biome = nearest.def, blend = 1f, weight = 1f, centerPosition = nearest.pos };
        }

        // Compute a smooth blend value between nearest and secondNearest.
        // If nearestDist is far less than secondDist -> blend close to 1 (fully inside nearest).
        // If they're close -> blend near 0.5 (transition).
        float gap = secondDist - nearestDist; // positive if nearest closer
        // normalize gap against a sensible range (use average of their radii to scale)
        float avgRadius = (nearest.radius + secondNearest.radius) * 0.5f;
        float blendRange = Mathf.Max(3f, avgRadius * 0.6f); // tune this for softer/harder borders
        float raw = Mathf.Clamp01(gap / blendRange);

        // Use smoothstep for prettier falloff
        float smooth = Mathf.SmoothStep(0f, 1f, raw);

        // Weight can be used for multi-influence decisions; here nearest weight = smooth
        float nearestWeight = smooth;
        float secondWeight = 1f - smooth;

        return new BiomeAssignment
        {
            biome = nearest.def,
            blend = nearestWeight,
            weight = nearestWeight,
            centerPosition = nearest.pos
        };
    }

    /// <summary>
    /// Get top N influences (biome + weight) for a tile. Good for blending multiple biome effects.
    /// </summary>
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

    // Editor debug drawing of centers and their radii in tile-space
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        foreach (var c in centers)
        {
            Gizmos.color = c.def != null ? c.def.debugColor : Color.white;
            // convert tile-space center to world units (y=0)
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
}
