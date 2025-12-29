using UnityEngine;

public class InventoryItem
{
    public ItemDefinition definition;
    public int currentStackSize;
    public bool isRotated;
    public InventoryGrid containerGrid;

    public int Width => isRotated ? definition.height : definition.width;
    public int Height => isRotated ? definition.width : definition.height;

    public InventoryItem(ItemDefinition def, int stackSize = 1)
    {
        definition = def;
        currentStackSize = stackSize;
        isRotated = false;

        if (def.isContainer)
        {
            containerGrid = new InventoryGrid(def.containerWidth, def.containerHeight);
        }
    }

    public bool CanStack(InventoryItem other)
    {
        if (!definition.isStackable || !other.definition.isStackable)
            return false;

        if (definition != other.definition)
            return false;

        return currentStackSize < definition.maxStackSize;
    }

    public int AddToStack(int amount)
    {
        int spaceAvailable = definition.maxStackSize - currentStackSize;
        int amountToAdd = Mathf.Min(amount, spaceAvailable);
        currentStackSize += amountToAdd;
        return amount - amountToAdd;
    }

    public void Rotate()
    {
        if (definition.canRotate)
        {
            isRotated = !isRotated;
        }
    }

    public void Use(GameObject player)
    {
        IItemBehavior behavior = definition.GetBehavior();
        if (behavior != null && behavior.CanUse(player))
        {
            behavior.OnUse(player);
        }
    }

    public void Equip(GameObject player)
    {
        IItemBehavior behavior = definition.GetBehavior();
        if (behavior != null)
        {
            behavior.OnEquip(player);
        }
    }

    public void Unequip(GameObject player)
    {
        IItemBehavior behavior = definition.GetBehavior();
        if (behavior != null)
        {
            behavior.OnUnequip(player);
        }
    }
}
