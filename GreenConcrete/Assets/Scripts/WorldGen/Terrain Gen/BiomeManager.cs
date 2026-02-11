using UnityEngine;
using System.Collections.Generic;
using UnityEngine.TerrainTools;
public class BiomeManager : MonoBehaviour
{

    [Header("Cordinates")]
    public int x4;
    public int y4;
    public Vector3 corner1;
    public Vector3 corner2;
    public Vector3 corner3;
    public Vector3 corner4;
    public Vector3 Center;
    [Header("Refrences")]

    BiomeDefenition biomeDefenitionClass;
    public Color gizmoColor = Color.red;
    public float gizmoRadius = 1f;
    [Header("Transfer")]
    TilePlacerAndSaver tilePlacerAndSaverMono;
    public int tileSize;

    public GameObject[] Centers;

    public void Start()
    {
        tilePlacerAndSaverMono = GetComponent<TilePlacerAndSaver>();
        Debug.Log("BiomeManager started, tilePlacerAndSaverMono reference set: " + (tilePlacerAndSaverMono != null));
    }

    public void Update()
    {
       // Centers = tilePlacerAndSaverMono.tiles[new(x4,y4)];
       tileSize = tilePlacerAndSaverMono.TileSize;
        Centers = tilePlacerAndSaverMono.Centers;
        
        for (int i = 0; i < Centers.Length; i++)
        {
            //draw a square around each center
            corner1 = Centers[i].transform.position + new Vector3(-tileSize / 2, 0, -tileSize / 2);
            corner2 = Centers[i].transform.position + new Vector3(tileSize / 2, 0, -tileSize / 2);
            corner3 = Centers[i].transform.position + new Vector3(tileSize / 2, 0, tileSize / 2);
            corner4 = Centers[i].transform.position + new Vector3(-tileSize / 2, 0, tileSize / 2);
            Debug.Log("Drawing square around center: " + Centers[i].name);
            Center = Centers[i].transform.position;
            DrawSquare(Center);

        }
    }

    public void DrawSquare(Vector3 Center)
    {
        Debug.DrawLine(corner1, corner2, gizmoColor, 90);
        Debug.DrawLine(corner2, corner3, gizmoColor, 90);
        Debug.DrawLine(corner3, corner4, gizmoColor, 90);
        Debug.DrawLine(corner4, corner1, gizmoColor, 90);
    }

   
}
