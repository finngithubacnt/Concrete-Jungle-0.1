using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class InventorySetupWizard : EditorWindow
{
    [MenuItem("Tools/Inventory System/Setup Inventory UI")]
    static void ShowWindow()
    {
        var window = GetWindow<InventorySetupWizard>("Inventory Setup");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Tarkov Inventory System Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This will create a complete inventory UI system in your scene:\n\n" +
            "• Inventory Canvas with proper setup\n" +
            "• Main grid (10x6)\n" +
            "• Backpack grid\n" +
            "• Tooltip system\n" +
            "• All necessary prefabs\n\n" +
            "Make sure you have TextMeshPro imported!",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Complete Inventory System", GUILayout.Height(40)))
        {
            CreateInventorySystem();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add PlayerInventory to Selected GameObject", GUILayout.Height(30)))
        {
            AddPlayerInventoryComponent();
        }
    }

    void CreateInventorySystem()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "Player GameObject not found in scene!", "OK");
            return;
        }

        if (player.GetComponent<PlayerInventory>() == null)
        {
            Undo.AddComponent<PlayerInventory>(player);
            Debug.Log("Added PlayerInventory component to Player");
        }

        CreatePrefabs();

        GameObject canvas = CreateInventoryCanvas();
        
        CreateInventoryUI(canvas);

        EditorUtility.DisplayDialog("Success", 
            "Inventory system created successfully!\n\n" +
            "Press Tab in Play mode to open the inventory.\n" +
            "Press F1 to add test items (attach InventoryTestHelper to Player).",
            "OK");
    }

    void CreatePrefabs()
    {
        string prefabPath = "Assets/Prefabs/Inventory";
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Inventory");
        }

        CreateGridCellPrefab(prefabPath);
        CreateInventoryItemUIPrefab(prefabPath);
    }

    void CreateGridCellPrefab(string path)
    {
        string prefabFullPath = path + "/GridCell.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabFullPath) != null)
        {
            Debug.Log("GridCell prefab already exists, skipping");
            return;
        }

        GameObject cell = new GameObject("GridCell");
        
        Image img = cell.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform rect = cell.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50, 50);

        PrefabUtility.SaveAsPrefabAsset(cell, prefabFullPath);
        DestroyImmediate(cell);
        
        Debug.Log("Created GridCell prefab");
    }

    void CreateInventoryItemUIPrefab(string path)
    {
        string prefabFullPath = path + "/InventoryItemUI.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabFullPath) != null)
        {
            Debug.Log("InventoryItemUI prefab already exists, skipping");
            return;
        }

        GameObject itemUI = new GameObject("InventoryItemUI");
        
        CanvasGroup canvasGroup = itemUI.AddComponent<CanvasGroup>();
        InventoryItemUI itemScript = itemUI.AddComponent<InventoryItemUI>();

        GameObject background = new GameObject("Background");
        background.transform.SetParent(itemUI.transform);
        Image bgImg = background.AddComponent<Image>();
        bgImg.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(itemUI.transform);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.preserveAspect = true;
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(2, 2);
        iconRect.offsetMax = new Vector2(-2, -2);

        GameObject stackText = new GameObject("StackText");
        stackText.transform.SetParent(itemUI.transform);
        TextMeshProUGUI tmpText = stackText.AddComponent<TextMeshProUGUI>();
        tmpText.text = "99";
        tmpText.fontSize = 14;
        tmpText.alignment = TextAlignmentOptions.BottomRight;
        tmpText.color = Color.white;
        tmpText.outlineColor = Color.black;
        tmpText.outlineWidth = 0.2f;
        tmpText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = stackText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(2, 2);
        textRect.offsetMax = new Vector2(-2, -2);

        itemScript.iconImage = iconImg;
        itemScript.stackText = tmpText;
        itemScript.backgroundImage = bgImg;

        RectTransform itemRect = itemUI.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(50, 50);

        PrefabUtility.SaveAsPrefabAsset(itemUI, prefabFullPath);
        DestroyImmediate(itemUI);
        
        Debug.Log("Created InventoryItemUI prefab");
    }

    GameObject CreateInventoryCanvas()
    {
        GameObject existingCanvas = GameObject.Find("InventoryCanvas");
        if (existingCanvas != null)
        {
            Debug.Log("InventoryCanvas already exists, using existing one");
            return existingCanvas;
        }

        GameObject canvasObj = new GameObject("InventoryCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject dragHandler = new GameObject("DragHandler");
        dragHandler.transform.SetParent(canvasObj.transform);
        dragHandler.AddComponent<InventoryDragHandler>();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Inventory Canvas");

        Debug.Log("Created InventoryCanvas");
        return canvasObj;
    }

    void CreateInventoryUI(GameObject canvas)
    {
        GameObject inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform);
        
        RectTransform panelRect = inventoryPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(1200, 800);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelBg = inventoryPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        CreateMainGrid(inventoryPanel);
        CreateBackpackGrid(inventoryPanel);
        CreateTooltip(canvas);

        InventoryUIManager uiManager = inventoryPanel.AddComponent<InventoryUIManager>();
        uiManager.inventoryPanel = inventoryPanel;

        InventoryGridUI mainGridUI = inventoryPanel.transform.Find("MainGrid")?.GetComponent<InventoryGridUI>();
        InventoryGridUI backpackGridUI = inventoryPanel.transform.Find("BackpackGrid")?.GetComponent<InventoryGridUI>();
        
        if (mainGridUI != null)
            uiManager.mainGridUI = mainGridUI;
        if (backpackGridUI != null)
            uiManager.backpackGridUI = backpackGridUI;

        inventoryPanel.SetActive(false);

        Debug.Log("Created Inventory UI panels");
    }

    void CreateMainGrid(GameObject parent)
    {
        GameObject mainGrid = new GameObject("MainGrid");
        mainGrid.transform.SetParent(parent.transform);

        RectTransform gridRect = mainGrid.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0, 0);
        gridRect.anchorMax = new Vector2(0, 1);
        gridRect.pivot = new Vector2(0, 1);
        gridRect.anchoredPosition = new Vector2(50, -50);
        gridRect.sizeDelta = new Vector2(519, 0);

        GameObject gridContainer = new GameObject("GridContainer");
        gridContainer.transform.SetParent(mainGrid.transform);
        
        RectTransform containerRect = gridContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = Vector2.zero;

        InventoryGridUI gridUI = mainGrid.AddComponent<InventoryGridUI>();
        gridUI.gridContainer = containerRect;
        gridUI.cellSize = 50;
        gridUI.cellSpacing = 2;

        string prefabPath = "Assets/Prefabs/Inventory";
        gridUI.cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/GridCell.prefab");
        gridUI.itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/InventoryItemUI.prefab");
    }

    void CreateBackpackGrid(GameObject parent)
    {
        GameObject backpackGrid = new GameObject("BackpackGrid");
        backpackGrid.transform.SetParent(parent.transform);

        RectTransform gridRect = backpackGrid.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(1, 0);
        gridRect.anchorMax = new Vector2(1, 1);
        gridRect.pivot = new Vector2(1, 1);
        gridRect.anchoredPosition = new Vector2(-50, -50);
        gridRect.sizeDelta = new Vector2(415, 0);

        GameObject gridContainer = new GameObject("GridContainer");
        gridContainer.transform.SetParent(backpackGrid.transform);
        
        RectTransform containerRect = gridContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = Vector2.zero;

        InventoryGridUI gridUI = backpackGrid.AddComponent<InventoryGridUI>();
        gridUI.gridContainer = containerRect;
        gridUI.cellSize = 50;
        gridUI.cellSpacing = 2;

        string prefabPath = "Assets/Prefabs/Inventory";
        gridUI.cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/GridCell.prefab");
        gridUI.itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/InventoryItemUI.prefab");

        backpackGrid.SetActive(false);
    }

    void CreateTooltip(GameObject canvas)
    {
        GameObject tooltip = new GameObject("TooltipPanel");
        tooltip.transform.SetParent(canvas.transform);

        RectTransform tooltipRect = tooltip.AddComponent<RectTransform>();
        tooltipRect.anchorMin = Vector2.zero;
        tooltipRect.anchorMax = Vector2.zero;
        tooltipRect.pivot = new Vector2(0, 1);
        tooltipRect.sizeDelta = new Vector2(300, 200);

        Image tooltipBg = tooltip.AddComponent<Image>();
        tooltipBg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

        GameObject nameText = new GameObject("NameText");
        nameText.transform.SetParent(tooltip.transform);
        TextMeshProUGUI nameTMP = nameText.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Item Name";
        nameTMP.fontSize = 18;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color = Color.white;
        
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.anchoredPosition = new Vector2(0, -10);
        nameRect.sizeDelta = new Vector2(-20, 30);

        GameObject descText = new GameObject("DescriptionText");
        descText.transform.SetParent(tooltip.transform);
        TextMeshProUGUI descTMP = descText.AddComponent<TextMeshProUGUI>();
        descTMP.text = "Item description goes here...";
        descTMP.fontSize = 14;
        descTMP.color = new Color(0.8f, 0.8f, 0.8f);
        
        RectTransform descRect = descText.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 1);
        descRect.anchorMax = new Vector2(1, 1);
        descRect.pivot = new Vector2(0.5f, 1);
        descRect.anchoredPosition = new Vector2(0, -45);
        descRect.sizeDelta = new Vector2(-20, 80);

        GameObject statsText = new GameObject("StatsText");
        statsText.transform.SetParent(tooltip.transform);
        TextMeshProUGUI statsTMP = statsText.AddComponent<TextMeshProUGUI>();
        statsTMP.text = "Stats";
        statsTMP.fontSize = 12;
        statsTMP.color = new Color(0.7f, 0.7f, 0.7f);
        
        RectTransform statsRect = statsText.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0);
        statsRect.anchorMax = new Vector2(1, 0);
        statsRect.pivot = new Vector2(0.5f, 0);
        statsRect.anchoredPosition = new Vector2(0, 10);
        statsRect.sizeDelta = new Vector2(-20, 60);

        InventoryTooltip tooltipScript = tooltip.AddComponent<InventoryTooltip>();
        tooltipScript.tooltipPanel = tooltip;
        tooltipScript.itemNameText = nameTMP;
        tooltipScript.descriptionText = descTMP;
        tooltipScript.statsText = statsTMP;

        tooltip.SetActive(false);
    }

    void AddPlayerInventoryComponent()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "No GameObject selected!", "OK");
            return;
        }

        if (selected.GetComponent<PlayerInventory>() != null)
        {
            EditorUtility.DisplayDialog("Info", "PlayerInventory already exists on this GameObject!", "OK");
            return;
        }

        Undo.AddComponent<PlayerInventory>(selected);
        Debug.Log("Added PlayerInventory to " + selected.name);
    }
}
