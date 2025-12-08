using UnityEngine;

[CreateAssetMenu(menuName = "Biomes/Biome Definition")]
public class BiomeDefinition : ScriptableObject
{
    [Header("Identification")]
    public string biomeName = "New Biome";
    public Color debugColor = Color.white;

    [Header("Size Settings")]
    [Tooltip("Typical radius of this biome in tiles (approx).")]
    public int radiusInTiles = 15;

    [Tooltip("Random radius variation multiplier (0 = fixed).")]
    public float radiusVariation = 0.2f;

    [Header("Texture / Terrain Layers")]
    [Tooltip("Terrain layers used by this biome. You can add 1..N layers per biome.")]
    public TerrainLayer[] terrainLayers;

    [Header("Height Modification")]
    [Tooltip("Enable height modification for this biome.")]
    public bool modifyHeight = false;

    [Tooltip("Overall height multiplier (1.0 = no change, >1 = hills, <1 = flatten).")]
    [Range(0f, 3f)]
    public float heightMultiplier = 1f;

    [Tooltip("Base height offset applied to the entire terrain (-1 = valleys, 0 = normal, 1 = elevated).")]
    [Range(-1f, 1f)]
    public float baseHeightOffset = 0f;

    [Tooltip("Additional noise scale for this biome's terrain features.")]
    [Range(0f, 0.1f)]
    public float biomeNoiseScale = 0.01f;

    [Tooltip("Amplitude of biome-specific noise (creates hills/valleys).")]
    [Range(0f, 2f)]
    public float biomeNoiseAmplitude = 0f;

    [Tooltip("Optional height curve to shape the terrain (X=0..1 input height, Y=0..1 output height).")]
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Tile Prefab")]
    [Tooltip("Optional prefab to spawn on top of this tile (e.g., village, structure, special feature).")]
    public GameObject tilePrefab;

    [Tooltip("Prefab position offset from tile center.")]
    public Vector3 prefabPositionOffset = Vector3.zero;

    [Tooltip("Random rotation for the spawned prefab.")]
    public bool randomRotation = false;

    [Header("Vegetation / Props")]
    public GameObject[] vegetationPrefabs;
    [Range(0f, 5f)]
    public float vegetationDensity = 0.5f;

    [Header("Ambient / Environment")]
    public AudioClip ambientSound;
    public Color ambientTint = Color.white;
    public float ambientIntensity = 1f;
}
