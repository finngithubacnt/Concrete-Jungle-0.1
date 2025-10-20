using UnityEngine;

[CreateAssetMenu(menuName = "Biomes/BiomeDefinition")]
public class BiomeDefinition : ScriptableObject
{
    public string biomeName = "Biome";
    public Color debugColor = Color.white;

    [Tooltip("Typical radius of this biome in tiles (approx).")]
    public int radiusInTiles = 15;

    [Tooltip("Random radius variation multiplier (0 = fixed).")]
    public float radiusVariation = 0.2f;

    [Tooltip("Optional priority used if you want to bias selection.")]
    public int priority = 0;

    // Future: add methods / references for painting, spawning, effects, etc.
}
