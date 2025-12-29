using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    public ItemDefinition testHealthPotion;
    public ItemDefinition testRifle;
    public ItemDefinition testBackpack;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            AddTestItems();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            UseFirstItem();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            EquipFirstWeapon();
        }
    }

    void AddTestItems()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("PlayerInventory not found!");
            return;
        }

        if (testHealthPotion != null)
        {
            InventoryItem potion = new InventoryItem(testHealthPotion, 3);
            if (PlayerInventory.Instance.AddItem(potion))
                Debug.Log("Added 3 health potions!");
        }

        if (testRifle != null)
        {
            InventoryItem rifle = new InventoryItem(testRifle);
            if (PlayerInventory.Instance.AddItem(rifle))
                Debug.Log("Added rifle!");
        }

        if (testBackpack != null)
        {
            InventoryItem backpack = new InventoryItem(testBackpack);
            if (PlayerInventory.Instance.AddItem(backpack))
                Debug.Log("Added backpack!");
        }
    }

    void UseFirstItem()
    {
        if (PlayerInventory.Instance == null) return;

        var items = PlayerInventory.Instance.mainGrid.GetAllItems();
        if (items.Count > 0)
        {
            PlayerInventory.Instance.UseItem(items[0]);
        }
    }

    void EquipFirstWeapon()
    {
        if (PlayerInventory.Instance == null) return;

        var items = PlayerInventory.Instance.mainGrid.GetAllItems();
        foreach (var item in items)
        {
            if (item.definition.width >= 2)
            {
                PlayerInventory.Instance.EquipWeapon(item);
                Debug.Log($"Equipped {item.definition.itemName}");
                break;
            }
        }
    }
}
