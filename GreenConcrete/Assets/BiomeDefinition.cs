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

    [Header("Vegetation / Props (placeholders)")]
    public GameObject[] vegetationPrefabs;
    [Range(0f, 5f)]
    public float vegetationDensity = 0.5f;

    [Header("Ambient / Environment (placeholders)")]
    public AudioClip ambientSound;
    public Color ambientTint = Color.white;
    public float ambientIntensity = 1f;
}
