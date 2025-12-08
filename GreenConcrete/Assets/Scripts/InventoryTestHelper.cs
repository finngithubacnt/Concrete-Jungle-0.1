using UnityEngine;

public class InventoryTestHelper : MonoBehaviour
{
    [Header("Test Items")]
    public ItemDefinition testRifle;
    public ItemDefinition testAmmo;
    public ItemDefinition testMedkit;
    public ItemDefinition testBackpack;

    void Start()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            AddTestItems();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            AddTestItems();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ClearInventory();
        }
    }

    void AddTestItems()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("PlayerInventory not found!");
            return;
        }

        if (testRifle != null)
        {
            var rifle = new InventoryItem(testRifle);
            PlayerInventory.Instance.AddItem(rifle);
            Debug.Log("Added rifle to inventory");
        }

        if (testAmmo != null)
        {
            var ammo = new InventoryItem(testAmmo, 30);
            PlayerInventory.Instance.AddItem(ammo);
            Debug.Log("Added ammo to inventory");
        }

        if (testMedkit != null)
        {
            var medkit = new InventoryItem(testMedkit);
            PlayerInventory.Instance.AddItem(medkit);
            Debug.Log("Added medkit to inventory");
        }

        if (testBackpack != null)
        {
            var backpack = new InventoryItem(testBackpack);
            PlayerInventory.Instance.AddItem(backpack);
            Debug.Log("Added backpack to inventory");
        }
    }

    void ClearInventory()
    {
        if (PlayerInventory.Instance == null) return;

        var items = PlayerInventory.Instance.mainGrid.GetAllItems();
        foreach (var item in items.ToArray())
        {
            PlayerInventory.Instance.RemoveItem(item);
        }

        Debug.Log("Cleared inventory");
    }
}
