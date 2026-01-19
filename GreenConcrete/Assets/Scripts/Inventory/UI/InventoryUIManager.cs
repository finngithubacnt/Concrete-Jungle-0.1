using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject inventoryPanel;
    public InventoryGridUI mainGridUI;
    public InventoryGridUI backpackGridUI;

    [Header("Input Settings")]
    public KeyCode toggleKey = KeyCode.Tab;

    private bool isInventoryOpen = false;

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
    }

    void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.onInventoryChanged.AddListener(RefreshUI);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }

        if (isInventoryOpen)
        {
            RefreshUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OpenInventory()
    {
        if (!isInventoryOpen)
        {
            ToggleInventory();
        }
    }

    public void CloseInventory()
    {
        if (isInventoryOpen)
        {
            ToggleInventory();
        }
    }

    void RefreshUI()
    {
        if (!isInventoryOpen)
            return;

        if (mainGridUI != null && PlayerInventory.Instance != null)
        {
            if (PlayerInventory.Instance.mainGrid != null)
            {
                mainGridUI.RefreshGrid(PlayerInventory.Instance.mainGrid);
            }
            else
            {
                Debug.Log("PlayerInventory.mainGrid is null!");
            }
        }

        if (backpackGridUI != null && PlayerInventory.Instance != null && PlayerInventory.Instance.backpackSlot != null)
        {
            if (PlayerInventory.Instance.backpackSlot.containerGrid != null)
            {
                backpackGridUI.gameObject.SetActive(true);
                backpackGridUI.RefreshGrid(PlayerInventory.Instance.backpackSlot.containerGrid);
            }
            else
            {
                backpackGridUI.gameObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.onInventoryChanged.RemoveListener(RefreshUI);
        }
    }
}
