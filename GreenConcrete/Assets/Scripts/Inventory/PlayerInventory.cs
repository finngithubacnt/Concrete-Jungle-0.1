using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Grid Settings")]
    public int inventoryWidth = 10;
    public int inventoryHeight = 6;

    [Header("Quick Slots")]
    public int quickSlotCount = 4;

    [Header("Equipment")]
    public bool hasWeaponSlot = true;
    public bool hasArmorSlot = true;
    public bool hasBackpackSlot = true;

    public InventoryGrid mainGrid { get; private set; }
    private InventoryItem[] quickSlots;

    public InventoryItem weaponSlot { get; private set; }
    public InventoryItem armorSlot { get; private set; }
    public InventoryItem backpackSlot { get; private set; }

    public UnityEvent onInventoryChanged;
    public UnityEvent<InventoryItem> onItemAdded;
    public UnityEvent<InventoryItem> onItemRemoved;
    public UnityEvent<InventoryItem> onItemUsed;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
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

        if (hasBackpackSlot && backpackSlot != null && backpackSlot.containerGrid != null)
        {
            if (backpackSlot.containerGrid.RemoveItem(item))
            {
                onItemRemoved?.Invoke(item);
                onInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public void UseItem(InventoryItem item)
    {
        if (HasItem(item))
        {
            item.Use(gameObject);
            onItemUsed?.Invoke(item);
        }
    }

    public bool HasItem(InventoryItem item)
    {
        if (mainGrid.GetAllItems().Contains(item))
            return true;

        if (backpackSlot != null && backpackSlot.containerGrid != null)
            if (backpackSlot.containerGrid.GetAllItems().Contains(item))
                return true;

        if (weaponSlot == item || armorSlot == item || backpackSlot == item)
            return true;

        for (int i = 0; i < quickSlots.Length; i++)
            if (quickSlots[i] == item)
                return true;

        return false;
    }

    public bool HasItemOfType(ItemDefinition itemDef)
    {
        foreach (InventoryItem item in mainGrid.GetAllItems())
            if (item.definition == itemDef)
                return true;

        if (backpackSlot != null && backpackSlot.containerGrid != null)
            foreach (InventoryItem item in backpackSlot.containerGrid.GetAllItems())
                if (item.definition == itemDef)
                    return true;

        return false;
    }

    public bool EquipWeapon(InventoryItem item)
    {
        if (!hasWeaponSlot)
            return false;

        if (weaponSlot != null)
        {
            weaponSlot.Unequip(gameObject);
            AddItem(weaponSlot);
        }

        weaponSlot = item;
        RemoveItem(item);
        item.Equip(gameObject);
        onInventoryChanged?.Invoke();
        return true;
    }

    public bool EquipArmor(InventoryItem item)
    {
        if (!hasArmorSlot)
            return false;

        if (armorSlot != null)
        {
            armorSlot.Unequip(gameObject);
            AddItem(armorSlot);
        }

        armorSlot = item;
        RemoveItem(item);
        item.Equip(gameObject);
        onInventoryChanged?.Invoke();
        return true;
    }

    public bool EquipBackpack(InventoryItem item)
    {
        if (!hasBackpackSlot || !item.definition.isContainer)
            return false;

        if (backpackSlot != null)
        {
            backpackSlot.Unequip(gameObject);
            AddItem(backpackSlot);
        }

        backpackSlot = item;
        RemoveItem(item);
        item.Equip(gameObject);
        onInventoryChanged?.Invoke();
        return true;
    }

    public bool AddToQuickSlot(InventoryItem item, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlotCount)
            return false;

        quickSlots[slotIndex] = item;
        onInventoryChanged?.Invoke();
        return true;
    }

    public InventoryItem GetQuickSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlotCount)
            return null;

        return quickSlots[slotIndex];
    }

    public float GetTotalWeight()
    {
        float total = mainGrid.GetTotalWeight();

        if (weaponSlot != null)
            total += weaponSlot.definition.weight;

        if (armorSlot != null)
            total += armorSlot.definition.weight;

        if (backpackSlot != null)
        {
            total += backpackSlot.definition.weight;
            if (backpackSlot.containerGrid != null)
                total += backpackSlot.containerGrid.GetTotalWeight();
        }

        return total;
    }
}
