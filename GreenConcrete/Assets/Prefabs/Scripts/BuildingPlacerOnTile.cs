using UnityEngine;
using System.Collections.Generic;

public class BuildingPlacerOnTile : MonoBehaviour
{
    [Header("Building Settings")]
    public List<GameObject> floorPrefabs;
    public int minFloors = 1;
    public int maxFloors = 5;
    public float floorHeight = 3f;

    [Header("Placement Settings")]
    public float buildingOffsetFromEdge = 1f;
    public float buildingCheckSize = 3f;
    public LayerMask roadLayer;

    private void Start()
    {
        TryPlaceBuildings();
    }

    void TryPlaceBuildings()
    {
        Vector3 tileCenter = transform.position;
        float halfTile = 5f; // Half of 10f tile size

        // Check in all four directions
        TryPlaceBuilding(Vector3.forward, tileCenter + new Vector3(0, 0, halfTile - buildingOffsetFromEdge), 0f);   // North
        TryPlaceBuilding(Vector3.back, tileCenter + new Vector3(0, 0, -halfTile + buildingOffsetFromEdge), 180f); // South
        TryPlaceBuilding(Vector3.left, tileCenter + new Vector3(-halfTile + buildingOffsetFromEdge, 0, 0), -90f);  // West
        TryPlaceBuilding(Vector3.right, tileCenter + new Vector3(halfTile - buildingOffsetFromEdge, 0, 0), 90f);   // East
    }

    void TryPlaceBuilding(Vector3 direction, Vector3 spawnPosition, float yRotation)
    {
        float checkDistance = 2f;

        // Check if there's a road in the direction
        if (Physics.Raycast(transform.position + Vector3.up * 1f, direction, out RaycastHit hit, checkDistance, roadLayer))
        {
            // Make sure spawn area is clear
            Collider[] colliders = Physics.OverlapBox(spawnPosition, Vector3.one * buildingCheckSize * 0.5f, Quaternion.identity, roadLayer);
            if (colliders.Length == 0)
            {
                // Random number of floors
                int floorCount = Random.Range(minFloors, maxFloors + 1);
                Quaternion rotation = Quaternion.Euler(0, yRotation, 0);

                for (int i = 0; i < floorCount; i++)
                {
                    GameObject randomFloor = floorPrefabs[Random.Range(0, floorPrefabs.Count)];
                    Vector3 floorPosition = spawnPosition + new Vector3(0, i * floorHeight, 0);
                    Instantiate(randomFloor, floorPosition, rotation, transform);
                }
            }
        }
    }
}
