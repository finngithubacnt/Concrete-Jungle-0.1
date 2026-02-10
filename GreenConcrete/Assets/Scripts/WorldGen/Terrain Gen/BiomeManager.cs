using UnityEngine;
using System.Collections.Generic;
using UnityEngine.TerrainTools;
public class BiomeManager : MonoBehaviour
{

    [Header("Cordinates")]
    public int x4;
    public int y4;

    [Header("Refrences")]

    BiomeDefenition biomeDefenitionClass;
    public Color gizmoColor = Color.red;
    public float gizmoRadius = 1f;
    [Header("Transfer")]
    TilePlacerAndSaver tilePlacerAndSaverMono;


    public GameObject[] Centers;
    public void Start()
    {
        tilePlacerAndSaverMono = GetComponent<TilePlacerAndSaver>();
    }
    public void Update()
    {
       // Centers = tilePlacerAndSaverMono.tiles[new(x4,y4)];

        for (int i = 0; i < Centers.Length; i++)
        {
            

        }
    }
}
