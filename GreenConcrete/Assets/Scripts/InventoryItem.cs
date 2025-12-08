using UnityEngine;

public class InventoryItem
{
    public ItemDefinition definition;
    public int stackCount;
    public bool isRotated;
    public InventoryGrid containerGrid;

    public int Width => isRotated ? definition.height : definition.width;
    public int Height => isRotated ? definition.width : definition.height;

    public InventoryItem(ItemDefinition def, int count = 1)
    {
        definition = def;
        stackCount = Mathf.Clamp(count, 1, def.maxStackSize);
        isRotated = false;

        if (def.isContainer && def.containerWidth > 0 && def.containerHeight > 0)
        {
            containerGrid = new InventoryGrid(def.containerWidth, def.containerHeight);
        }
    }

    public bool CanStack(InventoryItem other)
    {
        if (!definition.isStackable || !other.definition.isStackable) return false;
        if (definition != other.definition) return false;
        if (stackCount >= definition.maxStackSize) return false;
        return true;
    }

    public int AddToStack(int amount)
    {
        int spaceAvailable = definition.maxStackSize - stackCount;
        int amountToAdd = Mathf.Min(amount, spaceAvailable);
        stackCount += amountToAdd;
        return amount - amountToAdd;
    }

    public void Rotate()
    {
        if (definition.canRotate)
        {
            isRotated = !isRotated;
        }
    }
}
