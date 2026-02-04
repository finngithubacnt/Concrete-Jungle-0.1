using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Building Preset", menuName = "Building System/Building Preset")]
public class BuildingPreset : ScriptableObject
{
    [Header("Building Info")]
    public string buildingName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Prefab")]
    public GameObject buildingPrefab;
    public GameObject previewPrefab;

    [Header("Placement Settings")]
    public float rotationStep = 90f;
    public float placementHeight = 0f;
    public bool snapToGrid = true;
    public float gridSize = 1f;

    [Header("Material Requirements")]
    public List<MaterialRequirement> requiredMaterials = new List<MaterialRequirement>();

    [Header("Building Category")]
    public BuildingCategory category = BuildingCategory.Foundation;
}

[System.Serializable]
public class MaterialRequirement
{
    public MaterialType materialType;
    public int amount;
}

public enum MaterialType
{
    Logs,
    Stones,
    Wood,
    Metal,
    Cloth
}

public enum BuildingCategory
{
    Foundation,
    Wall,
    Door,
    Roof,
    Floor,
    Stairs
}
