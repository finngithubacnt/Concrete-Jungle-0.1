using UnityEngine;

public class WeaponBehavior : MonoBehaviour, IItemBehavior
{
    public int damage = 25;
    public float range = 100f;
    public GameObject weaponModel;

    private GameObject activeWeaponModel;

    public void OnUse(GameObject player)
    {
        Debug.Log($"Fired weapon! Damage: {damage}");
    }

    public void OnEquip(GameObject player)
    {
        if (weaponModel != null)
        {
            activeWeaponModel = Instantiate(weaponModel, player.transform);
            Debug.Log("Weapon equipped and visible!");
        }
    }

    public void OnUnequip(GameObject player)
    {
        if (activeWeaponModel != null)
        {
            Destroy(activeWeaponModel);
            Debug.Log("Weapon unequipped and hidden!");
        }
    }

    public bool CanUse(GameObject player)
    {
        return true;
    }

    public void OnRightClick(InventoryItem item, GameObject player)
    {
        Debug.Log($"Right-clicked weapon: {item.definition.itemName}");
    }
}
