using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public int cellSize = 50;
    public int cellSpacing = 2;

    [Header("Prefab References")]
    public GameObject cellPrefab;
    public GameObject itemPrefab;

    [Header("Colors")]
    public Color normalCellColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color highlightCellColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
    public Color invalidCellColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);

    private InventoryGrid currentGrid;
    private RectTransform rectTransform;
    private GameObject[,] cellObjects;
    private Dictionary<InventoryItem, InventoryItemUI> itemUIObjects;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        itemUIObjects = new Dictionary<InventoryItem, InventoryItemUI>();
    }

    public void InitializeGrid(InventoryGrid grid)
    {
        currentGrid = grid;
        CreateGridCells();
    }

    void CreateGridCells()
    {
        if (currentGrid == null || cellPrefab == null)
            return;

        ClearCells();

        cellObjects = new GameObject[currentGrid.width, currentGrid.height];

        rectTransform.sizeDelta = new Vector2(
            currentGrid.width * (cellSize + cellSpacing) - cellSpacing,
            currentGrid.height * (cellSize + cellSpacing) - cellSpacing
        );

        for (int x = 0; x < currentGrid.width; x++)
        {
            for (int y = 0; y < currentGrid.height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                RectTransform cellRect = cell.GetComponent<RectTransform>();

                cellRect.anchorMin = new Vector2(0, 1);
                cellRect.anchorMax = new Vector2(0, 1);
                cellRect.pivot = new Vector2(0, 1);

                cellRect.anchoredPosition = new Vector2(
                    x * (cellSize + cellSpacing),
                    -y * (cellSize + cellSpacing)
                );

                cellRect.sizeDelta = new Vector2(cellSize, cellSize);

                Image cellImage = cell.GetComponent<Image>();
                if (cellImage != null)
                {
                    cellImage.color = normalCellColor;
                }

                cellObjects[x, y] = cell;
            }
        }
    }

    void ClearCells()
    {
        if (cellObjects != null)
        {
            for (int x = 0; x < cellObjects.GetLength(0); x++)
            {
                for (int y = 0; y < cellObjects.GetLength(1); y++)
                {
                    if (cellObjects[x, y] != null)
                    {
                        Destroy(cellObjects[x, y]);
                    }
                }
            }
        }
    }

    public void RefreshGrid(InventoryGrid grid)
    {
        if (currentGrid != grid)
        {
            InitializeGrid(grid);
        }

        ClearItems();
        CreateItemVisuals();
    }

    void ClearItems()
    {
        foreach (var kvp in itemUIObjects)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        itemUIObjects.Clear();
    }

    void CreateItemVisuals()
    {
        if (currentGrid == null || itemPrefab == null)
            return;

        List<InventoryItem> items = currentGrid.GetAllItems();
        foreach (InventoryItem item in items)
        {
            Vector2Int? pos = currentGrid.GetItemPosition(item);
            if (pos.HasValue)
            {
                CreateItemUI(item, pos.Value);
            }
        }
    }

    void CreateItemUI(InventoryItem item, Vector2Int gridPos)
    {
        GameObject itemObj = Instantiate(itemPrefab, transform);
        RectTransform itemRect = itemObj.GetComponent<RectTransform>();

        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(0, 1);
        itemRect.pivot = new Vector2(0, 1);

        itemRect.anchoredPosition = new Vector2(
            gridPos.x * (cellSize + cellSpacing),
            -gridPos.y * (cellSize + cellSpacing)
        );

        itemRect.sizeDelta = new Vector2(
            item.Width * cellSize + (item.Width - 1) * cellSpacing,
            item.Height * cellSize + (item.Height - 1) * cellSpacing
        );

        InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
        if (itemUI != null)
        {
            itemUI.SetItem(item, this);
            itemUIObjects[item] = itemUI;
        }
    }

    public void HighlightCells(int x, int y, int width, int height, bool isValid)
    {
        ResetCellColors();

        if (cellObjects == null)
            return;

        Color highlightColor = isValid ? highlightCellColor : invalidCellColor;

        for (int ix = 0; ix < width; ix++)
        {
            for (int iy = 0; iy < height; iy++)
            {
                int cellX = x + ix;
                int cellY = y + iy;

                if (cellX >= 0 && cellX < currentGrid.width && cellY >= 0 && cellY < currentGrid.height)
                {
                    Image cellImage = cellObjects[cellX, cellY].GetComponent<Image>();
                    if (cellImage != null)
                    {
                        cellImage.color = highlightColor;
                    }
                }
            }
        }
    }

    public void ResetCellColors()
    {
        if (cellObjects == null)
            return;

        for (int x = 0; x < cellObjects.GetLength(0); x++)
        {
            for (int y = 0; y < cellObjects.GetLength(1); y++)
            {
                if (cellObjects[x, y] != null)
                {
                    Image cellImage = cellObjects[x, y].GetComponent<Image>();
                    if (cellImage != null)
                    {
                        cellImage.color = normalCellColor;
                    }
                }
            }
        }
    }

    public Vector2Int? GetGridPositionFromScreenPoint(Vector2 screenPoint)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screenPoint, null, out Vector2 localPoint))
        {
            return null;
        }

        int x = Mathf.FloorToInt(localPoint.x / (cellSize + cellSpacing));
        int y = Mathf.FloorToInt(-localPoint.y / (cellSize + cellSpacing));

        if (x >= 0 && x < currentGrid.width && y >= 0 && y < currentGrid.height)
        {
            return new Vector2Int(x, y);
        }

        return null;
    }

    public InventoryGrid GetGrid()
    {
        return currentGrid;
    }
}
