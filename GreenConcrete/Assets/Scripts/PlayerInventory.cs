using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Main Inventory Grid")]
    public int inventoryWidth = 10;
    public int inventoryHeight = 6;

    [Header("Quick Slots")]
    public int quickSlotCount = 4;

    [Header("Equipment Slots")]
    public bool hasWeaponSlot = true;
    public bool hasArmorSlot = true;
    public bool hasBackpackSlot = true;

    public InventoryGrid mainGrid { get; private set; }
    private InventoryItem[] quickSlots;
    private InventoryItem weaponSlot;
    private InventoryItem armorSlot;
    private InventoryItem backpackSlot;

    public System.Action<InventoryItem> onItemAdded;
    public System.Action<InventoryItem> onItemRemoved;
    public System.Action onInventoryChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        mainGrid = new InventoryGrid(inventoryWidth, inventoryHeight);
        quickSlots = new InventoryItem[quickSlotCount];
    }

    public bool AddItem(InventoryItem item)
    {
        if (mainGrid.AddItem(item))
        {
            onItemAdded?.Invoke(item);
            onInventoryChanged?.Invoke();
            return true;
        }

        if (hasBackpackSlot && backpackSlot != null && backpackSlot.containerGrid != null)
        {
            if (backpackSlot.containerGrid.AddItem(item))
            {
                onItemAdded?.Invoke(item);
                onInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public bool RemoveItem(InventoryItem item)
    {
        if (mainGrid.RemoveItem(item))
        {
            onItemRemoved?.Invoke(item);
            onInventoryChanged?.Invoke();
            return true;
        }

        if (backpackSlot?.containerGrid != null && backpackSlot.containerGrid.RemoveItem(item))
        {
            onItemRemoved?.Invoke(item);
            onInventoryChanged?.Invoke();
            return true;
        }

        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (quickSlots[i] == item)
            {
                quickSlots[i] = null;
                onItemRemoved?.Invoke(item);
                onInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public bool EquipWeapon(InventoryItem item)
    {
        if (!hasWeaponSlot) return false;
        if (item.definition.category != ItemCategory.Weapon) return false;

        if (weaponSlot != null)
        {
            if (!AddItem(weaponSlot)) return false;
        }

        RemoveItem(item);
        weaponSlot = item;
        onInventoryChanged?.Invoke();
        return true;
    }

    public bool EquipArmor(InventoryItem item)
    {
        if (!hasArmorSlot) return false;
        if (item.definition.category != ItemCategory.Armor) return false;

        if (armorSlot != null)
        {
            if (!AddItem(armorSlot)) return false;
        }

        RemoveItem(item);
        armorSlot = item;
        onInventoryChanged?.Invoke();
        return true;
    }

    public bool EquipBackpack(InventoryItem item)
    {
        if (!hasBackpackSlot) return false;
        if (item.definition.category != ItemCategory.Backpack) return false;

        if (backpackSlot != null)
        {
            if (!AddItem(backpackSlot)) return false;
        }

        RemoveItem(item);
        backpackSlot = item;
        onInventoryChanged?.Invoke();
        return true;
    }

    public bool SetQuickSlot(int slotIndex, InventoryItem item)
    {
        if (slotIndex < 0 || slotIndex >= quickSlots.Length) return false;
        quickSlots[slotIndex] = item;
        onInventoryChanged?.Invoke();
        return true;
    }

    public InventoryItem GetQuickSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlots.Length) return null;
        return quickSlots[slotIndex];
    }

    public InventoryItem GetWeapon() => weaponSlot;
    public InventoryItem GetArmor() => armorSlot;
    public InventoryItem GetBackpack() => backpackSlot;

    public float GetTotalWeight()
    {
        float total = mainGrid.GetTotalWeight();
        
        if (weaponSlot != null)
            total += weaponSlot.definition.weight;
        if (armorSlot != null)
            total += armorSlot.definition.weight;
        if (backpackSlot != null)
            total += backpackSlot.definition.weight + (backpackSlot.containerGrid?.GetTotalWeight() ?? 0);

        return total;
    }
}
