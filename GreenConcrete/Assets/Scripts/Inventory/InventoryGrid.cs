using System.Collections.Generic;
using UnityEngine;

public class InventoryGrid
{
    public int width { get; private set; }
    public int height { get; private set; }

    private InventoryItem[,] grid;
    private Dictionary<InventoryItem, Vector2Int> itemPositions;

    public InventoryGrid(int w, int h)
    {
        width = w;
        height = h;
        grid = new InventoryItem[width, height];
        itemPositions = new Dictionary<InventoryItem, Vector2Int>();
    }

    public bool CanPlaceItem(InventoryItem item, int x, int y)
    {
        if (x < 0 || y < 0)
            return false;

        if (x + item.Width > width || y + item.Height > height)
            return false;

        for (int ix = 0; ix < item.Width; ix++)
        {
            for (int iy = 0; iy < item.Height; iy++)
            {
                if (grid[x + ix, y + iy] != null && grid[x + ix, y + iy] != item)
                    return false;
            }
        }

        return true;
    }

    public bool PlaceItem(InventoryItem item, int x, int y)
    {
        if (!CanPlaceItem(item, x, y))
            return false;

        if (itemPositions.ContainsKey(item))
            RemoveItem(item);

        for (int ix = 0; ix < item.Width; ix++)
        {
            for (int iy = 0; iy < item.Height; iy++)
            {
                grid[x + ix, y + iy] = item;
            }
        }

        itemPositions[item] = new Vector2Int(x, y);
        return true;
    }

    public bool RemoveItem(InventoryItem item)
    {
        if (!itemPositions.ContainsKey(item))
            return false;

        Vector2Int pos = itemPositions[item];

        for (int ix = 0; ix < item.Width; ix++)
        {
            for (int iy = 0; iy < item.Height; iy++)
            {
                if (pos.x + ix < width && pos.y + iy < height)
                    grid[pos.x + ix, pos.y + iy] = null;
            }
        }

        itemPositions.Remove(item);
        return true;
    }

    public InventoryItem GetItemAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return null;
        return grid[x, y];
    }

    public Vector2Int? GetItemPosition(InventoryItem item)
    {
        if (itemPositions.ContainsKey(item))
            return itemPositions[item];
        return null;
    }

    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(itemPositions.Keys);
    }

    public Vector2Int? FindSpaceForItem(InventoryItem item, bool tryRotate = true)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (CanPlaceItem(item, x, y))
                    return new Vector2Int(x, y);
            }
        }

        if (tryRotate && item.definition.canRotate)
        {
            item.Rotate();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (CanPlaceItem(item, x, y))
                        return new Vector2Int(x, y);
                }
            }
            item.Rotate();
        }

        return null;
    }

    public bool AddItem(InventoryItem item)
    {
        if (item.definition.isStackable)
        {
            foreach (InventoryItem existingItem in GetAllItems())
            {
                if (existingItem.CanStack(item))
                {
                    int remaining = existingItem.AddToStack(item.currentStackSize);
                    if (remaining == 0)
                        return true;
                    item.currentStackSize = remaining;
                }
            }
        }

        Vector2Int? pos = FindSpaceForItem(item);
        if (pos.HasValue)
        {
            return PlaceItem(item, pos.Value.x, pos.Value.y);
        }

        return false;
    }

    public float GetTotalWeight()
    {
        float total = 0f;
        foreach (InventoryItem item in GetAllItems())
        {
            total += item.definition.weight * item.currentStackSize;
        }
        return total;
    }
}
