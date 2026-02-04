using UnityEngine;

public class BuildModeController : MonoBehaviour
{
    [Header("Build Mode Toggle")]
    public KeyCode buildModeKey = KeyCode.F1;

    [Header("Starting State")]
    public bool startInBuildMode = false;

    void Start()
    {
        if (startInBuildMode && BuildingManager.Instance != null)
        {
            BuildingManager.Instance.EnableBuildMode();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(buildModeKey))
        {
            ToggleBuildMode();
        }
    }

    void ToggleBuildMode()
    {
        if (BuildingManager.Instance == null)
        {
            Debug.LogWarning("BuildingManager instance not found!");
            return;
        }

        if (BuildingManager.Instance.isBuildModeActive)
        {
            BuildingManager.Instance.DisableBuildMode();
        }
        else
        {
            BuildingManager.Instance.EnableBuildMode();
        }
    }

    public void EnableBuildMode()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.EnableBuildMode();
        }
    }

    public void DisableBuildMode()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.DisableBuildMode();
        }
    }
}
