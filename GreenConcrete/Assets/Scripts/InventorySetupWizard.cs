using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class InventorySetupWizard : EditorWindow
{
    private GameObject canvasObject;
    private int gridWidth = 10;
    private int gridHeight = 6;
    private int cellSize = 50;
    private int cellSpacing = 2;

    [MenuItem("Tools/Inventory/Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<InventorySetupWizard>("Inventory Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Tarkov Inventory Setup Wizard", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This wizard will help you set up the basic Tarkov-style inventory UI hierarchy.",
            MessageType.Info
        );
        EditorGUILayout.Space();

        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
        cellSize = EditorGUILayout.IntField("Cell Size", cellSize);
        cellSpacing = EditorGUILayout.IntField("Cell Spacing", cellSpacing);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Basic Inventory UI", GUILayout.Height(40)))
        {
            CreateInventoryUI();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Grid Cell Prefab", GUILayout.Height(30)))
        {
            CreateGridCellPrefab();
        }

        if (GUILayout.Button("Create Item UI Prefab", GUILayout.Height(30)))
        {
            CreateItemUIPrefab();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "After creating the UI, manually assign prefabs and configure references in the Inspector.",
            MessageType.Warning
        );
    }

    void CreateInventoryUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        GameObject dragHandler = new GameObject("InventoryDragHandler");
        dragHandler.transform.SetParent(canvas.transform);
        dragHandler.AddComponent<InventoryDragHandler>();

        GameObject inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform);
        RectTransform panelRect = inventoryPanel.AddComponent<RectTransform>();
        Image panelImage = inventoryPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(1200, 800);
        panelRect.anchoredPosition = Vector2.zero;

        InventoryUIManager uiManager = inventoryPanel.AddComponent<InventoryUIManager>();

        GameObject gridContainer = new GameObject("GridContainer");
        gridContainer.transform.SetParent(inventoryPanel.transform);
        RectTransform gridRect = gridContainer.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0, 1);
        gridRect.anchorMax = new Vector2(0, 1);
        gridRect.pivot = new Vector2(0, 1);
        gridRect.anchoredPosition = new Vector2(50, -50);

        InventoryGridUI gridUI = gridContainer.AddComponent<InventoryGridUI>();
        gridUI.gridContainer = gridRect;
        gridUI.cellSize = cellSize;
        gridUI.cellSpacing = cellSpacing;

        GameObject tooltipPanel = new GameObject("TooltipPanel");
        tooltipPanel.transform.SetParent(canvas.transform);
        RectTransform tooltipRect = tooltipPanel.AddComponent<RectTransform>();
        Image tooltipImage = tooltipPanel.AddComponent<Image>();
        tooltipImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        
        tooltipRect.anchorMin = new Vector2(0, 1);
        tooltipRect.anchorMax = new Vector2(0, 1);
        tooltipRect.pivot = new Vector2(0, 1);
        tooltipRect.sizeDelta = new Vector2(300, 200);

        InventoryTooltip tooltip = tooltipPanel.AddComponent<InventoryTooltip>();

        GameObject nameText = CreateTextObject("NameText", tooltipPanel.transform, 18, FontStyles.Bold);
        GameObject descText = CreateTextObject("DescriptionText", tooltipPanel.transform, 14, FontStyles.Normal);
        GameObject statsText = CreateTextObject("StatsText", tooltipPanel.transform, 12, FontStyles.Normal);

        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -10);
        nameRect.sizeDelta = new Vector2(-20, 30);

        RectTransform descRect = descText.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 1);
        descRect.anchorMax = new Vector2(1, 1);
        descRect.pivot = new Vector2(0.5f, 1);
        descRect.anchoredPosition = new Vector2(0, -50);
        descRect.sizeDelta = new Vector2(-20, 60);

        RectTransform statsRect = statsText.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 1);
        statsRect.anchorMax = new Vector2(1, 1);
        statsRect.pivot = new Vector2(0.5f, 1);
        statsRect.anchoredPosition = new Vector2(0, -120);
        statsRect.sizeDelta = new Vector2(-20, 70);

        tooltip.tooltipPanel = tooltipPanel;
        tooltip.itemNameText = nameText.GetComponent<TextMeshProUGUI>();
        tooltip.descriptionText = descText.GetComponent<TextMeshProUGUI>();
        tooltip.statsText = statsText.GetComponent<TextMeshProUGUI>();

        uiManager.inventoryPanel = inventoryPanel;
        uiManager.mainGridUI = gridUI;

        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        Debug.Log("Inventory UI hierarchy created! Now assign prefabs and configure PlayerInventory.");
        Selection.activeGameObject = inventoryPanel;
    }

    GameObject CreateTextObject(string name, Transform parent, int fontSize, FontStyles style)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        return textObj;
    }

    void CreateGridCellPrefab()
    {
        GameObject cell = new GameObject("GridCell");
        RectTransform rect = cell.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(cellSize, cellSize);
        
        Image image = cell.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        string path = "Assets/Prefabs/Inventory";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Inventory");
        }

        string prefabPath = path + "/GridCell.prefab";
        PrefabUtility.SaveAsPrefabAsset(cell, prefabPath);
        DestroyImmediate(cell);

        Debug.Log("Grid Cell prefab created at: " + prefabPath);
    }

    void CreateItemUIPrefab()
    {
        GameObject item = new GameObject("InventoryItemUI");
        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(cellSize, cellSize);
        item.AddComponent<CanvasGroup>();
        InventoryItemUI itemUI = item.AddComponent<InventoryItemUI>();

        GameObject background = new GameObject("Background");
        background.transform.SetParent(item.transform);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);

        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(item.transform);
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(5, 5);
        iconRect.offsetMax = new Vector2(-5, -5);
        Image iconImage = icon.AddComponent<Image>();
        iconImage.color = Color.white;

        GameObject stackText = new GameObject("StackText");
        stackText.transform.SetParent(item.transform);
        RectTransform stackRect = stackText.AddComponent<RectTransform>();
        stackRect.anchorMin = new Vector2(1, 0);
        stackRect.anchorMax = new Vector2(1, 0);
        stackRect.pivot = new Vector2(1, 0);
        stackRect.sizeDelta = new Vector2(30, 20);
        stackRect.anchoredPosition = new Vector2(-5, 5);
        
        TextMeshProUGUI stackTMP = stackText.AddComponent<TextMeshProUGUI>();
        stackTMP.fontSize = 14;
        stackTMP.fontStyle = FontStyles.Bold;
        stackTMP.color = Color.white;
        stackTMP.alignment = TextAlignmentOptions.BottomRight;
        stackTMP.enableAutoSizing = false;

        itemUI.iconImage = iconImage;
        itemUI.stackText = stackTMP;
        itemUI.backgroundImage = bgImage;

        string path = "Assets/Prefabs/Inventory";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Inventory");
        }

        string prefabPath = path + "/InventoryItemUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(item, prefabPath);
        DestroyImmediate(item);

        Debug.Log("Item UI prefab created at: " + prefabPath);
    }
}
