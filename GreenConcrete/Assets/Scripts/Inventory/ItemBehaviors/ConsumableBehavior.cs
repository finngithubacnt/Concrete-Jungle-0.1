using UnityEngine;

public class ConsumableBehavior : MonoBehaviour, IItemBehavior
{
    [Header("Consumable Properties")]
    public int healthRestore = 50;
    public int staminaRestore = 0;
    public bool destroyOnUse = true;

    [Header("Effects")]
    public GameObject consumeEffectPrefab;
    public AudioClip consumeSound;

    public void OnUse(GameObject player)
    {
        ConsumeItem(player);
    }

    public void OnEquip(GameObject player)
    {
        Debug.Log("Consumable items cannot be equipped");
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
        Debug.Log($"Right-clicked consumable: {item.definition.itemName}");
        ConsumeItem(player);

        if (destroyOnUse && item.definition.isStackable)
        {
            item.currentStackSize--;
            
            if (item.currentStackSize <= 0)
            {
                InventoryGrid grid = PlayerInventory.Instance?.mainGrid;
                if (grid != null)
                {
                    grid.RemoveItem(item);
                    Debug.Log($"Consumed last {item.definition.itemName}, removed from inventory");
                }
            }
            else
            {
                Debug.Log($"Consumed {item.definition.itemName}, {item.currentStackSize} remaining");
            }

            PlayerInventory.Instance?.onInventoryChanged?.Invoke();
        }
        else if (destroyOnUse)
        {
            InventoryGrid grid = PlayerInventory.Instance?.mainGrid;
            if (grid != null)
            {
                grid.RemoveItem(item);
                Debug.Log($"Consumed {item.definition.itemName}, removed from inventory");
            }

            PlayerInventory.Instance?.onInventoryChanged?.Invoke();
        }
    }

    void ConsumeItem(GameObject player)
    {
        if (healthRestore > 0)
        {
            Debug.Log($"Restored {healthRestore} health");
        }

        if (staminaRestore > 0)
        {
            Debug.Log($"Restored {staminaRestore} stamina");
        }

        if (consumeEffectPrefab != null)
        {
            Instantiate(consumeEffectPrefab, player.transform.position, Quaternion.identity);
        }

        if (consumeSound != null)
        {
            AudioSource.PlayClipAtPoint(consumeSound, player.transform.position);
        }
    }
}
