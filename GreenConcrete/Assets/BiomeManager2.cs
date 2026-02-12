using System.Collections.Generic;
using UnityEngine;

public class VoronoiGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject tilePrefab;
    public int gridWidth = 50;
    public int gridHeight = 50;
    public float tileSize = 1f;

    [Header("Voronoi Centers")]
    public GameObject[] centers;

    // Stores tile data per grid coordinate
    private Dictionary<Vector2Int, TileData> tiles =
        new Dictionary<Vector2Int, TileData>();

    // Stores which tiles belong to each center
    private Dictionary<GameObject, List<Vector2Int>> centerTiles =
        new Dictionary<GameObject, List<Vector2Int>>();

    void Start()
    {
        InitializeCenters();
        GenerateGrid();
    }

    // --------------------------------------------
    // Initialize center ownership lists
    // --------------------------------------------
    void InitializeCenters()
    {
        foreach (GameObject center in centers)
        {
            centerTiles[center] = new List<Vector2Int>();
        }
    }

    // --------------------------------------------
    // Generate full grid
    // --------------------------------------------
    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                GenerateTile(x, 0, z);
            }
        }
    }

    // --------------------------------------------
    // Generate single tile
    // --------------------------------------------
    public void GenerateTile(int x, int y, int z)
    {
        Vector2Int gridPos = new Vector2Int(x, z);

        if (tiles.ContainsKey(gridPos))
            return;

        // Find closest center
        GameObject closestCenter = GetClosestCenter(x, z);

        // Instantiate tile
        GameObject tile = Instantiate(
            tilePrefab,
            new Vector3(x * tileSize, y * tileSize, z * tileSize),
            Quaternion.identity,
            transform
        );

        // Store tile data
        TileData data = new TileData
        {
            gridPosition = gridPos,
            owner = closestCenter,
            instance = tile
        };

        tiles.Add(gridPos, data);

        // Register ownership
        centerTiles[closestCenter].Add(gridPos);

        // OPTIONAL: color tile based on center
        ColorTile(tile, closestCenter);
    }

    // --------------------------------------------
    // Find closest center GameObject
    // --------------------------------------------
    GameObject GetClosestCenter(int x, int z)
    {
        float minDist = float.MaxValue;
        GameObject closest = null;

        Vector2 cellPos = new Vector2(x, z);

        foreach (GameObject center in centers)
        {
            Vector2 centerPos = WorldToGrid(center.transform.position);

            float dist = Vector2.Distance(cellPos, centerPos);

            if (dist < minDist)
            {
                minDist = dist;
                closest = center;
            }
        }

        return closest;
    }

    // --------------------------------------------
    // Convert world position to grid coordinate
    // --------------------------------------------
    Vector2 WorldToGrid(Vector3 worldPos)
    {
        return new Vector2(
            worldPos.x / tileSize,
            worldPos.z / tileSize
        );
    }

    // --------------------------------------------
    // Optional visual coloring
    // --------------------------------------------
    void ColorTile(GameObject tile, GameObject center)
    {
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer == null) return;

        int index = System.Array.IndexOf(centers, center);

        // Simple deterministic color
        Random.InitState(index * 999);
        renderer.material.color = Random.ColorHSV();
    }
}

// --------------------------------------------
// Tile data container
// --------------------------------------------
[System.Serializable]
public class TileData
{
    public Vector2Int gridPosition;
    public GameObject owner;
    public GameObject instance;
}