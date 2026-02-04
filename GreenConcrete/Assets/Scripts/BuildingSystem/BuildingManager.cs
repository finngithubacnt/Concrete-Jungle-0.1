using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    [Header("Build Mode")]
    public bool isBuildModeActive = false;

    [Header("Building Settings")]
    public BuildingPreset currentPreset;
    public LayerMask placementLayer;
    public float maxPlacementDistance = 10f;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    [Header("References")]
    public Camera playerCamera;
    public Transform buildingParent;

    private GameObject previewObject;
    private bool canPlace = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float currentRotation = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (buildingParent == null)
        {
            GameObject parent = new GameObject("Buildings");
            buildingParent = parent.transform;
        }
    }

    void Update()
    {
        if (isBuildModeActive && currentPreset != null)
        {
            UpdateBuildingPreview();
            HandleBuildingInput();
        }
    }

    public void EnableBuildMode()
    {
        isBuildModeActive = true;
        BuildingUI.Instance?.ShowBuildMenu();
        Debug.Log("Build mode enabled. Press B to open building menu.");
    }

    public void DisableBuildMode()
    {
        isBuildModeActive = false;
        ClearPreview();
        BuildingUI.Instance?.HideBuildMenu();
        Debug.Log("Build mode disabled.");
    }

    public void SelectBuilding(BuildingPreset preset)
    {
        currentPreset = preset;
        currentRotation = 0f;
        ClearPreview();

        if (preset.previewPrefab != null)
        {
            previewObject = Instantiate(preset.previewPrefab);
            SetupPreviewObject(previewObject);
        }

        BuildingUI.Instance?.HideBuildMenu();
        Debug.Log($"Selected building: {preset.buildingName}");
    }

    void SetupPreviewObject(GameObject preview)
    {
        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Rigidbody[] rigidbodies = preview.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            Destroy(rb);
        }
    }

    void UpdateBuildingPreview()
    {
        if (previewObject == null)
            return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxPlacementDistance, placementLayer))
        {
            targetPosition = hit.point;

            if (currentPreset.snapToGrid)
            {
                targetPosition.x = Mathf.Round(targetPosition.x / currentPreset.gridSize) * currentPreset.gridSize;
                targetPosition.z = Mathf.Round(targetPosition.z / currentPreset.gridSize) * currentPreset.gridSize;
            }

            targetPosition.y += currentPreset.placementHeight;
            targetRotation = Quaternion.Euler(0, currentRotation, 0);

            previewObject.transform.position = targetPosition;
            previewObject.transform.rotation = targetRotation;
            previewObject.SetActive(true);

            canPlace = CheckPlacementValid();
            UpdatePreviewMaterial(canPlace);
        }
        else
        {
            previewObject.SetActive(false);
            canPlace = false;
        }
    }

    bool CheckPlacementValid()
    {
        if (MaterialInventory.Instance == null)
            return false;

        return MaterialInventory.Instance.HasMaterials(currentPreset.requiredMaterials);
    }

    void UpdatePreviewMaterial(bool valid)
    {
        if (previewObject == null)
            return;

        Material targetMaterial = valid ? validPlacementMaterial : invalidPlacementMaterial;
        
        if (targetMaterial != null)
        {
            Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = targetMaterial;
                }
                renderer.materials = mats;
            }
        }
    }

    void HandleBuildingInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateBuilding();
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceBuilding();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B))
        {
            CancelCurrentBuilding();
        }
    }

    void RotateBuilding()
    {
        currentRotation += currentPreset.rotationStep;
        if (currentRotation >= 360f)
        {
            currentRotation = 0f;
        }
    }

    void TryPlaceBuilding()
    {
        if (!canPlace)
        {
            Debug.LogWarning("Cannot place building here. Check materials or placement validity.");
            return;
        }

        if (MaterialInventory.Instance.ConsumeMaterials(currentPreset.requiredMaterials))
        {
            GameObject building = Instantiate(currentPreset.buildingPrefab, targetPosition, targetRotation, buildingParent);
            Debug.Log($"Placed {currentPreset.buildingName}!");
        }
    }

    void CancelCurrentBuilding()
    {
        currentPreset = null;
        ClearPreview();
        BuildingUI.Instance?.ShowBuildMenu();
    }

    void ClearPreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }
}
