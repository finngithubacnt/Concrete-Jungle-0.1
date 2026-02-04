using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class MaterialInventory : MonoBehaviour
{
    public static MaterialInventory Instance { get; private set; }

    [Header("Material Storage")]
    private Dictionary<MaterialType, int> materials = new Dictionary<MaterialType, int>();

    [Header("Events")]
    public UnityEvent<MaterialType, int> onMaterialChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeMaterials();
    }

    void InitializeMaterials()
    {
        foreach (MaterialType type in System.Enum.GetValues(typeof(MaterialType)))
        {
            materials[type] = 0;
        }
    }

    public void AddMaterial(MaterialType type, int amount)
    {
        if (materials.ContainsKey(type))
        {
            materials[type] += amount;
        }
        else
        {
            materials[type] = amount;
        }

        onMaterialChanged?.Invoke(type, materials[type]);
        Debug.Log($"Added {amount} {type}. Total: {materials[type]}");
    }

    public bool RemoveMaterial(MaterialType type, int amount)
    {
        if (!HasMaterial(type, amount))
        {
            Debug.LogWarning($"Not enough {type}. Need {amount}, have {GetMaterialCount(type)}");
            return false;
        }

        materials[type] -= amount;
        onMaterialChanged?.Invoke(type, materials[type]);
        return true;
    }

    public bool HasMaterial(MaterialType type, int amount)
    {
        return materials.ContainsKey(type) && materials[type] >= amount;
    }

    public int GetMaterialCount(MaterialType type)
    {
        return materials.ContainsKey(type) ? materials[type] : 0;
    }

    public bool HasMaterials(List<MaterialRequirement> requirements)
    {
        foreach (MaterialRequirement req in requirements)
        {
            if (!HasMaterial(req.materialType, req.amount))
            {
                return false;
            }
        }
        return true;
    }

    public bool ConsumeMaterials(List<MaterialRequirement> requirements)
    {
        if (!HasMaterials(requirements))
        {
            return false;
        }

        foreach (MaterialRequirement req in requirements)
        {
            RemoveMaterial(req.materialType, req.amount);
        }
        return true;
    }
}
