using UnityEngine;
using System.Collections.Generic;
using UnityEngine.TerrainTools;
using UnityEngine.TerrainUtils;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Tilemaps;
public class TilePlacerAndSaver : MonoBehaviour
{
    [Header("References")]
    public GameObject Player;
    public GameObject TilePrefab;
    public GameObject start;
    [Header("Settings")]
    public int TileSize = 9;
    public int PlayerViewDistance = 9;
    Terrain terrain;
    Terrain GeneratedTerrain;
    Terrain topNeighbor1;
    Terrain bottomNeighbor1;
    Terrain leftNeighbor1;
    Terrain rightNeighbor1;

    [Header("Tile Position")]

    public int x;
    public int y;
    public int z;
    public int x1;
    public int y1;
    public int z1;
    public int x2;
    public int y2;
    public int z2;
    [Header("Tile List")]
    public GameObject[] Tiles;
    public GameObject closestTile;
    public int tilecountmax = 0;
    public GameObject CenterPrefab;
    public GameObject[] Centers1;
    HashSet<Vector2Int> generatedTiles = new HashSet<Vector2Int>();
    Dictionary<Vector2Int, TileData> tiles = new Dictionary<Vector2Int, TileData>();
    Dictionary<Vector2Int, CenterData> centers = new Dictionary<Vector2Int, CenterData>();
   ///public HashSet<Vector2Int> Centers = new HashSet<Vector2Int>();
    Dictionary<Vector2Int, TileData> data;
    Dictionary<Vector2Int, GameObject> visuals;



    public class TileData
    {
        public Vector2Int gridPos;

        public GameObject instance;
        public Terrain terrainp;
        public float height;
        public bool hasTree;
        public int biomeId;
    }
    public class CenterData
    {
        public Vector2Int gridPos;
        public GameObject instance;

    }
    void Start()
    {

        GameObject terrain = Instantiate(TilePrefab, new Vector3(x * 1 / TileSize, y * 1 / TileSize, z * 1 / TileSize), Quaternion.identity);
        terrain.name = $"Tile_{x}_{y}_{z}";
        terrain.transform.parent = this.transform;
        terrain.tag = "Tile";
    }

    void Update()
    {
        Terrain terrain = GetComponentInChildren<Terrain>();
        terrain.terrainData.size = new Vector3(TileSize, 600, TileSize);
        Tiles = GameObject.FindGameObjectsWithTag("Tile");
        Centers1 = GameObject.FindGameObjectsWithTag("Center");
        // Find the closest tile to the player
        for (int i = 0; i < Tiles.Length; i++)
        {
            float distance = Vector3.Distance(Player.transform.position, Tiles[i].transform.position);
            if (closestTile == null || distance < Vector3.Distance(Player.transform.position, closestTile.transform.position))
            {
                closestTile = Tiles[i];
            }
        }

        terrain = closestTile.GetComponent<Terrain>();
        x1 = (int)closestTile.transform.position.x / TileSize;
        y1 = (int)closestTile.transform.position.y / TileSize;
        z1 = (int)closestTile.transform.position.z / TileSize;
        Vector3Int cell = new Vector3Int(x, y, z);
        /*
        //top
        if (terrain.topNeighbor == null)
        {
            GenerateTile(x, y, z + 1);
            topNeighbor1 = terrain.topNeighbor;
            TilePrefab.name = $"Tile_{x}_{y}_{z}";
            TilePrefab.transform.parent = start.transform;
            TilePrefab.tag = "Tile";
        }

        //bottom
        if (terrain.bottomNeighbor == null)
        {
            GenerateTile(x, y, z - 1);
            bottomNeighbor1 = terrain.bottomNeighbor;
            TilePrefab.name = $"Tile_{x}_{y}_{z}";
            TilePrefab.transform.parent = start.transform;
            TilePrefab.tag = "Tile";
        }

        //left
        if (terrain.leftNeighbor == null)
        {
            GenerateTile(x - 1, y, z);
            leftNeighbor1 = terrain.leftNeighbor;
            TilePrefab.name = $"Tile_{x}_{y}_{z}";
            TilePrefab.transform.parent = start.transform;
            TilePrefab.tag = "Tile";
        }

        //right
        if (terrain.rightNeighbor == null)
        {
            GenerateTile(x + 1, y, z);
            rightNeighbor1 = terrain.rightNeighbor;
            TilePrefab.name = $"Tile_{x}_{y}_{z}";
            TilePrefab.transform.parent = start.transform;
            TilePrefab.tag = "Tile";
        }
        */

        // Generate tiles in a grid within the player's view distance
        for (int x = -PlayerViewDistance; x <= PlayerViewDistance; x++)
        {
            for (int z = -PlayerViewDistance; z <= PlayerViewDistance; z++)
            {
                int tileX = x1 + x;
                int tileZ = z1 + z;
                GenerateTile(tileX, 0, tileZ);
            }
        }

    }
    // Generates a tile at the specified coordinates if it hasn't been generated already
    public void GenerateTile(int x, int y, int z)
    {
        Vector2Int key = new Vector2Int(x, z);
        Vector2Int key1 = new Vector2Int(x, z);
        if (tiles.ContainsKey(key))
            return;

        GameObject tile = Instantiate(
            TilePrefab,
            new Vector3(x * TileSize, y * TileSize, z * TileSize),
            Quaternion.identity
             

        );

       int x2 = Random.Range(0, TileSize);
       int y2 = Random.Range(0, TileSize);
       int z2 = Random.Range(0, TileSize);

        GameObject Centers = Instantiate(
            CenterPrefab,
            new Vector3(x * TileSize + x2, y * TileSize, z * TileSize + z2),
            Quaternion.identity


        );
        CenterData data1 = new CenterData
        {
            gridPos = key1,
            instance = Centers,


        };
        Centers.name = $"Center_{x}_{y}_{z}";
        TileData data = new TileData
        {
            gridPos = key,
            instance = tile,
            height = y,
            biomeId = 1,//need to set this based on the biome generator
            terrainp = tile.GetComponent<Terrain>()

        };
        centers.Add(key1, data1);
        tiles.Add(key, data);
        TilePrefab.name = $"Tile_{x}_{y}_{z}";
       


    }
}
