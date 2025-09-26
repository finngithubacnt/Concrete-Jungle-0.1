using UnityEngine;
using System.Collections.Generic;

public class RoadBuildingSpawner : MonoBehaviour
{
    [Header("Building Floor Settings")]
    public List<GameObject> floorPrefabs;      // Middle floors
    public List<GameObject> groundFloorPrefabs; // Optional ground floors
    public List<GameObject> roofPrefabs;        // Optional roofs
    public int minFloors = 1;
    public int maxFloors = 5;
    public float floorHeight = 3f;

    [Header("Spawn Settings")]
    [Range(0f, 1f)] public float spawnChance = 0.8f; // Chance a building spawns at each spot
    public bool restrictToOneSide = false; // Only spawn buildings on half of the spots
    public bool randomRotation = false;    // Rotate buildings randomly

    [Header("Building Spawn Points")]
    public List<Transform> buildingSpots;

    void Start()
    {
        SpawnBuildingsAtSpots();
    }

    void SpawnBuildingsAtSpots()
    {
        if (buildingSpots == null || buildingSpots.Count == 0) return;

        // If restricted, randomly pick half the spots
        List<Transform> spotsToUse = new List<Transform>(buildingSpots);
        if (restrictToOneSide)
        {
            int halfCount = spotsToUse.Count / 2;
            spotsToUse = new List<Transform>(spotsToUse.GetRange(0, halfCount));
        }

        foreach (Transform spot in spotsToUse)
        {
            if (Random.value > spawnChance)
                continue; // Skip this spot

            int floors = Random.Range(minFloors, maxFloors + 1);
            Quaternion rotation = randomRotation ? Quaternion.Euler(0, Random.Range(0, 4) * 90, 0) : spot.rotation;

            for (int i = 0; i < floors; i++)
            {
                GameObject prefabToSpawn;

                if (i == 0 && groundFloorPrefabs.Count > 0) // ground floor
                {
                    prefabToSpawn = groundFloorPrefabs[Random.Range(0, groundFloorPrefabs.Count)];
                }
                else if (i == floors - 1 && roofPrefabs.Count > 0) // top floor
                {
                    prefabToSpawn = roofPrefabs[Random.Range(0, roofPrefabs.Count)];
                }
                else // middle floors
                {
                    prefabToSpawn = floorPrefabs[Random.Range(0, floorPrefabs.Count)];
                }

                Vector3 position = spot.position + Vector3.up * (i * floorHeight);
                Instantiate(prefabToSpawn, position, rotation, transform);
            }
        }
    }
}
