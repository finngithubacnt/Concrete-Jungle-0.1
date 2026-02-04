using UnityEngine;

public class BuildingToolBehavior : MonoBehaviour, IItemBehavior
{
    [Header("Building Tool Properties")]
    public GameObject buildingModePrefab;
    public float buildRange = 5f;
    public bool requiresResources = true;

    [Header("Tool Info")]
    public string toolName = "Building Tool";
    public string description = "Right-click to enter building mode";

    private bool isBuildModeActive = false;
    private GameObject buildModeInstance;

    public void OnUse(GameObject player)
    {
        ToggleBuildMode(player);
    }

    public void OnEquip(GameObject player)
    {
        Debug.Log($"{toolName} equipped and ready!");
    }

    public void OnUnequip(GameObject player)
    {
        if (isBuildModeActive)
        {
            ExitBuildMode();
        }
    }

    public bool CanUse(GameObject player)
    {
        return true;
    }

    public void OnRightClick(InventoryItem item, GameObject player)
    {
        Debug.Log($"Right-clicked {toolName}");
        ToggleBuildMode(player);
    }

    void ToggleBuildMode(GameObject player)
    {
        if (isBuildModeActive)
        {
            ExitBuildMode();
        }
        else
        {
            EnterBuildMode(player);
        }
    }

    void EnterBuildMode(GameObject player)
    {
        isBuildModeActive = true;
        
        if (buildingModePrefab != null)
        {
            buildModeInstance = Instantiate(buildingModePrefab, player.transform);
            Debug.Log($"Entered building mode with {toolName}");
        }
        else
        {
            Debug.Log($"Building mode activated (range: {buildRange}m)");
        }

        Debug.Log("Press right-click again to exit building mode");
    }

    void ExitBuildMode()
    {
        isBuildModeActive = false;
        
        if (buildModeInstance != null)
        {
            Destroy(buildModeInstance);
            buildModeInstance = null;
        }
        
        Debug.Log("Exited building mode");
    }
}
