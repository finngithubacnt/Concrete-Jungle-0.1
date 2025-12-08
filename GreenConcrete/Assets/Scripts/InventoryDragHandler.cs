using UnityEngine;

public class InventoryDragHandler : MonoBehaviour
{
    public static InventoryDragHandler Instance { get; private set; }

    private InventoryItem currentDraggedItem;
    private InventoryItemUI currentDraggedItemUI;
    private InventoryGridUI sourceGrid;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartDrag(InventoryItem item, InventoryItemUI itemUI, InventoryGridUI grid)
    {
        currentDraggedItem = item;
        currentDraggedItemUI = itemUI;
        sourceGrid = grid;

        sourceGrid.inventoryGrid.RemoveItem(item);
    }

    public void EndDrag()
    {
        currentDraggedItem = null;
        currentDraggedItemUI = null;
        sourceGrid = null;
    }

    public bool IsDragging() => currentDraggedItem != null;
    public InventoryItem GetDraggedItem() => currentDraggedItem;
}
