using UnityEngine;

public class InventoryDragHandler : MonoBehaviour
{
    public static InventoryDragHandler Instance { get; private set; }

    private InventoryItem draggedItem;
    private InventoryGridUI sourceGrid;
    private InventoryItemUI draggedItemUI;
    private bool isDragging = false;

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

    void Update()
    {
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
    }

    public void StartDrag(InventoryItem item, InventoryGridUI grid, InventoryItemUI itemUI)
    {
        draggedItem = item;
        sourceGrid = grid;
        draggedItemUI = itemUI;
        isDragging = true;

        InventoryGrid gridData = grid.GetGrid();
        if (gridData != null)
        {
            gridData.RemoveItem(item);
        }
    }

    public void UpdateDrag(Vector2 screenPosition)
    {
        if (!isDragging || draggedItem == null || sourceGrid == null)
            return;

        Vector2Int? gridPos = sourceGrid.GetGridPositionFromScreenPoint(screenPosition);

        if (gridPos.HasValue)
        {
            InventoryGrid gridData = sourceGrid.GetGrid();
            bool canPlace = gridData.CanPlaceItem(draggedItem, gridPos.Value.x, gridPos.Value.y);
            sourceGrid.HighlightCells(gridPos.Value.x, gridPos.Value.y, draggedItem.Width, draggedItem.Height, canPlace);
        }
        else
        {
            sourceGrid.ResetCellColors();
        }
    }

    public void EndDrag(Vector2 screenPosition)
    {
        if (!isDragging || draggedItem == null || sourceGrid == null)
            return;

        sourceGrid.ResetCellColors();

        Vector2Int? gridPos = sourceGrid.GetGridPositionFromScreenPoint(screenPosition);
        InventoryGrid gridData = sourceGrid.GetGrid();

        bool placed = false;

        if (gridPos.HasValue && gridData != null)
        {
            if (gridData.PlaceItem(draggedItem, gridPos.Value.x, gridPos.Value.y))
            {
                placed = true;
            }
        }

        if (!placed && gridData != null)
        {
            Vector2Int? originalPos = gridData.FindSpaceForItem(draggedItem, false);
            if (originalPos.HasValue)
            {
                gridData.PlaceItem(draggedItem, originalPos.Value.x, originalPos.Value.y);
            }
            else
            {
                PlayerInventory.Instance?.AddItem(draggedItem);
            }

            if (draggedItemUI != null)
            {
                draggedItemUI.ReturnToOriginalPosition();
            }
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.onInventoryChanged?.Invoke();
        }

        isDragging = false;
        draggedItem = null;
        sourceGrid = null;
        draggedItemUI = null;
    }

    void RotateItem()
    {
        if (draggedItem != null && draggedItem.definition.canRotate)
        {
            draggedItem.Rotate();

            if (draggedItemUI != null)
            {
                RectTransform rect = draggedItemUI.GetComponent<RectTransform>();
                if (rect != null)
                {
                    int cellSize = 50;
                    int cellSpacing = 2;

                    rect.sizeDelta = new Vector2(
                        draggedItem.Width * cellSize + (draggedItem.Width - 1) * cellSpacing,
                        draggedItem.Height * cellSize + (draggedItem.Height - 1) * cellSpacing
                    );
                }
            }

            Debug.Log($"Rotated item: {draggedItem.Width}x{draggedItem.Height}");
        }
    }

    public bool IsDragging()
    {
        return isDragging;
    }
}
