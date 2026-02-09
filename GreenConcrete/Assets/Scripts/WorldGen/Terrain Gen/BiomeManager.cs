using UnityEngine;
using System.Collections.Generic;
using UnityEngine.TerrainTools;
public class BiomeManager : MonoBehaviour
{
    public int BiomeID;
    public string BiomeName;
    public Color BiomeDebugColor;
    public int CenterCount;
    public int tileSize;
    public int GeneratedCenterCount;
    public int max;
    [Header("Cordinates")]
    public int x4;
    public int y4;

    [Header("Refrences")]
    
    BiomeDefenition biomeDefenitionClass;
    public Color gizmoColor = Color.red;
    public float gizmoRadius = 1f;

    TilePlacerAndSaver tilePlacerAndSaverMono;
    public GameObject TiPl;
    public GameObject[] Tiles1;
    public GameObject[] center1;
    

   

    void Update()
        {
        tilePlacerAndSaverMono = TiPl.GetComponent<TilePlacerAndSaver>();
        tileSize = tilePlacerAndSaverMono.TileSize;
        Tiles1 = tilePlacerAndSaverMono.Tiles;
        Tiles1 = tilePlacerAndSaverMono.Tiles;
        Vector3Int key = new Vector3Int(x4, y4);
        for (int i = 0; i < tilePlacerAndSaverMono.Tiles.Length; i++)
        {
            if (Tiles1[i] != null)
            {
               

                if (GeneratedCenterCount >= tilePlacerAndSaverMono.tilecountmax)
                {
                    return;
                }
                Terrain terrain = Tiles1[i].GetComponent<Terrain>();
                Vector3 tilePos = Tiles1[i].transform.position;
                int randomNumberZ = UnityEngine.Random.Range(0, tileSize);
                int randomNumberX = UnityEngine.Random.Range(0, tileSize);

                Vector3 randomPos = new Vector3(tilePos.x + randomNumberX, tilePos.y, tilePos.z + randomNumberZ);
                Instantiate(center1[BiomeID], randomPos, Quaternion.identity);
                GeneratedCenterCount += 1;
            }
        }

    }
  
}
