using UnityEngine;
[CreateAssetMenu(fileName = "NewBiomeData", menuName = "Biome Data")]
public class BiomeDefenition : ScriptableObject
{
    public string[] BiomeName; 
    public int BiomeID = 0; public Color BiomeDebugColor;
    public void Awake()
    {
        for (int i = 0; i < BiomeName.Length; i++)
        {
            BiomeID = +1;
        }
    }
}
