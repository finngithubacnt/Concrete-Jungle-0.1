using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public InventoryGrid inventoryGrid;
    public int cellSize = 50;
    public int cellSpacing = 2;

    [Header("References")]
    public RectTransform gridContainer;
    public GameObject cellPrefab;
    public GameObject itemPrefab;

    [Header("Visual")]
    public Color normalCellColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color highlightCellColor = new Color(0.3f, 0.5f, 0.3f, 0.8f);
    public Color invalidCellColor = new Color(0.5f, 0.2f, 0.2f, 0.8f);

    private Image[,] cellImages;
    private Dictionary<InventoryItem, InventoryItemUI> itemUIs;

    void Start()
    {
        if (inventoryGrid == null)
        {
            inventoryGrid = new InventoryGrid(10, 6);
        }

        itemUIs = new Dictionary<InventoryItem, InventoryItemUI>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
        if (gridContainer == null) return;

        cellImages = new Image[inventoryGrid.width, inventoryGrid.height];

        float totalWidth = inventoryGrid.width * (cellSize + cellSpacing) - cellSpacing;
        float totalHeight = inventoryGrid.height * (cellSize + cellSpacing) - cellSpacing;
        
        gridContainer.sizeDelta = new Vector2(totalWidth, totalHeight);

        for (int y = 0; y < inventoryGrid.height; y++)
        {
            for (int x = 0; x < inventoryGrid.width; x++)
            {
                GameObject cell = Instantiate(cellPrefab, gridContainer);
                RectTransform cellRect = cell.GetComponent<RectTransform>();
                
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                cellRect.anchoredPosition = new Vector2(
                    x * (cellSize + cellSpacing),
                    -y * (cellSize + cellSpacing)
                );

                Image cellImage = cell.GetComponent<Image>();
                if (cellImage != null)
                {
                    cellImage.color = normalCellColor;
                    cellImages[x, y] = cellImage;
                }
            }
        }
    }

    public void RefreshDisplay()
    {
        foreach (var itemUI in itemUIs.Values)
        {
            if (itemUI != null)
            {
                Destroy(itemUI.gameObject);
            }
        }
        itemUIs.Clear();

        var items = inventoryGrid.GetAllItems();
        foreach (var item in items)
        {
            CreateItemUI(item);
        }

        ResetCellColors();
    }

    void CreateItemUI(InventoryItem item)
    {
        Vector2Int? pos = inventoryGrid.GetItemPosition(item);
        if (!pos.HasValue) return;

        GameObject itemObj = Instantiate(itemPrefab, gridContainer);
        InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
        
        if (itemUI != null)
        {
            itemUI.Initialize(item, this);
            itemUI.SetGridPosition(pos.Value.x, pos.Value.y);
            itemUIs[item] = itemUI;
        }
    }

    public void HighlightCells(int x, int y, int width, int height, bool isValid)
    {
        ResetCellColors();

        Color highlightColor = isValid ? highlightCellColor : invalidCellColor;

        for (int ix = 0; ix < width; ix++)
        {
            for (int iy = 0; iy < height; iy++)
            {
                int cellX = x + ix;
                int cellY = y + iy;
                
                if (cellX >= 0 && cellX < inventoryGrid.width && 
                    cellY >= 0 && cellY < inventoryGrid.height)
                {
                    cellImages[cellX, cellY].color = highlightColor;
                }
            }
        }
    }

    public void ResetCellColors()
    {
        for (int x = 0; x < inventoryGrid.width; x++)
        {
            for (int y = 0; y < inventoryGrid.height; y++)
            {
                cellImages[x, y].color = normalCellColor;
            }
        }
    }

    public Vector2Int ScreenToGridPosition(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridContainer, 
            screenPos, 
            null, 
            out Vector2 localPoint
        );

        int x = Mathf.FloorToInt(localPoint.x / (cellSize + cellSpacing));
        int y = Mathf.FloorToInt(-localPoint.y / (cellSize + cellSpacing));

        return new Vector2Int(x, y);
    }

    public Vector2 GridToWorldPosition(int x, int y)
    {
        Vector2 localPos = new Vector2(
            x * (cellSize + cellSpacing),
            -y * (cellSize + cellSpacing)
        );

        Vector3 worldPos = gridContainer.TransformPoint(localPos);
        return worldPos;
    }

    public int GetCellSize() => cellSize;
    public int GetCellSpacing() => cellSpacing;

    public void RemoveItemUI(InventoryItem item)
    {
        if (itemUIs.ContainsKey(item))
        {
            if (itemUIs[item] != null)
            {
                Destroy(itemUIs[item].gameObject);
            }
            itemUIs.Remove(item);
        }
    }
}
