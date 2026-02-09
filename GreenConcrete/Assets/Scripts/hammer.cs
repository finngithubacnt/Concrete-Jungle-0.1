using UnityEngine;

public class hammer : MonoBehaviour
{
    

    public void OnUse(GameObject player)
    {
        Debug.Log($"gg");
    }

    public void OnEquip(GameObject player)
    {
    }

    public void OnUnequip(GameObject player)
    {
    }

    public bool CanUse(GameObject player)
    {
        return true;
    }

    public void OnRightClick(InventoryItem item, GameObject player)
    {
        Debug.Log($"Right-clicked hammer - ");
        OnUse(player);

        if (item.definition.isStackable)
        {
            item.currentStackSize--;

            if (item.currentStackSize <= 0)
            {
                InventoryGrid grid = PlayerInventory.Instance?.mainGrid;
                if (grid != null)
                {
                    grid.RemoveItem(item);
                    Debug.Log("Consumed last potion, removed from inventory");
                }
            }
            else
            {
                Debug.Log($"Consumed potion, {item.currentStackSize} remaining");
            }

            PlayerInventory.Instance?.onInventoryChanged?.Invoke();
        }
    }
}

