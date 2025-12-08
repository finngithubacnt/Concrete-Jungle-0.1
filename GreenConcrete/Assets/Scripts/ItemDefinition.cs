using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Grid Size")]
    public int width = 1;
    public int height = 1;
    public bool canRotate = true;

    [Header("Stacking")]
    public bool isStackable = false;
    public int maxStackSize = 1;

    [Header("Container")]
    public bool isContainer = false;
    public int containerWidth = 0;
    public int containerHeight = 0;

    [Header("Properties")]
    public float weight = 0.1f;
    public int value = 100;
    public ItemCategory category = ItemCategory.Misc;
}

public enum ItemCategory
{
    Weapon,
    Ammo,
    Medical,
    Food,
    Attachment,
    Armor,
    Backpack,
    Container,
    Misc
}
