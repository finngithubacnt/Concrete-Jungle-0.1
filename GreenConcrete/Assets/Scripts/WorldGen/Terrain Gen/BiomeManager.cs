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
    public Vector3 c1;
    public Vector3 c2;
    public Vector3 c3;
    public Vector3 c4;

    public int rndfirst;
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
        Centers = tilePlacerAndSaverMono.Centers1;
        
        for (int i = 0; i < Centers.Length; i++)
        {
            
      

           
            corner1 = Centers[i].transform.position + new Vector3(-tileSize / 2, 0, -tileSize / 2);
            corner2 = Centers[i].transform.position + new Vector3(tileSize / 2, 0, -tileSize / 2);
            corner3 = Centers[i].transform.position + new Vector3(tileSize / 2, 0, tileSize / 2);
            corner4 = Centers[i].transform.position + new Vector3(-tileSize / 2, 0, tileSize / 2);
            
            Debug.Log("Drawing square around center: " + Centers[i].name);
            Center = Centers[i].transform.position;

            rndfirst = Random.Range(1, 4);
            int rndlast = Random.Range(1, 4);
            c1 = corner1; c2 = corner2; c3 = corner3; c4 = corner4;




            gizmoColor = Random.ColorHSV();
  
            
        }



    }

    public void DrawSquare(Vector3 Center)
    {
        Debug.DrawLine(corner1, corner2, gizmoColor, 90);
        Debug.DrawLine(corner2, corner3, gizmoColor, 90);
        Debug.DrawLine(corner3, corner4, gizmoColor, 90);
        Debug.DrawLine(corner4, corner1, gizmoColor, 90);
        //merge surrounding 4 lines onto this square


    }


}
