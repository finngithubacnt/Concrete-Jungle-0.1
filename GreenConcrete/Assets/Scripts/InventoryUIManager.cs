using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject inventoryPanel;
    public InventoryGridUI mainGridUI;
    public InventoryGridUI backpackGridUI;

    [Header("Input")]
    public KeyCode toggleInventoryKey = KeyCode.Tab;

    private bool isInventoryOpen;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (PlayerInventory.Instance != null)
        {
            mainGridUI.inventoryGrid = PlayerInventory.Instance.mainGrid;
            PlayerInventory.Instance.onInventoryChanged += OnInventoryChanged;
        }

        CloseInventory();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleInventoryKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        isInventoryOpen = true;
        inventoryPanel.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshInventoryDisplay();
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        inventoryPanel.SetActive(false);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnInventoryChanged()
    {
        if (isInventoryOpen)
        {
            RefreshInventoryDisplay();
        }
    }

    void RefreshInventoryDisplay()
    {
        mainGridUI.RefreshDisplay();

        if (PlayerInventory.Instance != null && PlayerInventory.Instance.GetBackpack() != null)
        {
            var backpack = PlayerInventory.Instance.GetBackpack();
            if (backpack.containerGrid != null && backpackGridUI != null)
            {
                backpackGridUI.inventoryGrid = backpack.containerGrid;
                backpackGridUI.gameObject.SetActive(true);
                backpackGridUI.RefreshDisplay();
            }
        }
        else if (backpackGridUI != null)
        {
            backpackGridUI.gameObject.SetActive(false);
        }
    }

    public bool IsInventoryOpen() => isInventoryOpen;

    void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.onInventoryChanged -= OnInventoryChanged;
        }
    }
}
