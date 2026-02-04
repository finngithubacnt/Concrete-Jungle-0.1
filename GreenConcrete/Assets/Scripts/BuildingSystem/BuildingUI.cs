using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BuildingUI : MonoBehaviour
{
    public static BuildingUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject buildMenuPanel;
    public Transform buildingButtonContainer;
    public GameObject buildingButtonPrefab;

    [Header("Building Presets")]
    public List<BuildingPreset> availableBuildings = new List<BuildingPreset>();

    [Header("Material Display")]
    public GameObject materialDisplayPanel;
    public TextMeshProUGUI logsText;
    public TextMeshProUGUI stonesText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI metalText;
    public TextMeshProUGUI clothText;

    private bool isMenuOpen = false;

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
        if (buildMenuPanel != null)
        {
            buildMenuPanel.SetActive(false);
        }

        PopulateBuildingButtons();
        UpdateMaterialDisplay();

        if (MaterialInventory.Instance != null)
        {
            MaterialInventory.Instance.onMaterialChanged.AddListener((type, amount) => UpdateMaterialDisplay());
        }
    }

    void Update()
    {
        if (BuildingManager.Instance != null && BuildingManager.Instance.isBuildModeActive)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                ToggleBuildMenu();
            }
        }
    }

    public void ShowBuildMenu()
    {
        if (buildMenuPanel != null)
        {
            buildMenuPanel.SetActive(true);
            isMenuOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void HideBuildMenu()
    {
        if (buildMenuPanel != null)
        {
            buildMenuPanel.SetActive(false);
            isMenuOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ToggleBuildMenu()
    {
        if (isMenuOpen)
        {
            HideBuildMenu();
        }
        else
        {
            ShowBuildMenu();
        }
    }

    void PopulateBuildingButtons()
    {
        if (buildingButtonContainer == null || buildingButtonPrefab == null)
            return;

        foreach (Transform child in buildingButtonContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (BuildingPreset preset in availableBuildings)
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                string requirementsText = GetRequirementsText(preset);
                buttonText.text = $"{preset.buildingName}\n{requirementsText}";
            }

            Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && preset.icon != null)
            {
                icon.sprite = preset.icon;
            }

            if (button != null)
            {
                BuildingPreset capturedPreset = preset;
                button.onClick.AddListener(() => OnBuildingSelected(capturedPreset));
            }
        }
    }

    string GetRequirementsText(BuildingPreset preset)
    {
        if (preset.requiredMaterials.Count == 0)
            return "No materials needed";

        string text = "";
        foreach (MaterialRequirement req in preset.requiredMaterials)
        {
            text += $"{req.materialType}: {req.amount} ";
        }
        return text.Trim();
    }

    void OnBuildingSelected(BuildingPreset preset)
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.SelectBuilding(preset);
        }
    }

    void UpdateMaterialDisplay()
    {
        if (MaterialInventory.Instance == null)
            return;

        if (logsText != null)
            logsText.text = $"Logs: {MaterialInventory.Instance.GetMaterialCount(MaterialType.Logs)}";

        if (stonesText != null)
            stonesText.text = $"Stones: {MaterialInventory.Instance.GetMaterialCount(MaterialType.Stones)}";

        if (woodText != null)
            woodText.text = $"Wood: {MaterialInventory.Instance.GetMaterialCount(MaterialType.Wood)}";

        if (metalText != null)
            metalText.text = $"Metal: {MaterialInventory.Instance.GetMaterialCount(MaterialType.Metal)}";

        if (clothText != null)
            clothText.text = $"Cloth: {MaterialInventory.Instance.GetMaterialCount(MaterialType.Cloth)}";
    }

    public void AddTestMaterials()
    {
        if (MaterialInventory.Instance != null)
        {
            MaterialInventory.Instance.AddMaterial(MaterialType.Logs, 50);
            MaterialInventory.Instance.AddMaterial(MaterialType.Stones, 50);
            MaterialInventory.Instance.AddMaterial(MaterialType.Wood, 50);
        }
    }
}
