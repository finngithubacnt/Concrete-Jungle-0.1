using UnityEngine;

public class EquipmentBehavior : MonoBehaviour, IItemBehavior
{
    [Header("Equipment Properties")]
    public GameObject equipmentModel;
    public Transform attachPoint;
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localRotation = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    [Header("Stats")]
    public int armorBonus = 0;
    public int damageBonus = 0;
    public float speedModifier = 1f;

    private GameObject activeEquipmentModel;
    private bool isEquipped = false;

    public void OnUse(GameObject player)
    {
        if (isEquipped)
        {
            OnUnequip(player);
        }
        else
        {
            OnEquip(player);
        }
    }

    public void OnEquip(GameObject player)
    {
        if (equipmentModel != null)
        {
            Transform parent = attachPoint != null ? attachPoint : player.transform;
            activeEquipmentModel = Instantiate(equipmentModel, parent);
            activeEquipmentModel.transform.localPosition = localPosition;
            activeEquipmentModel.transform.localRotation = Quaternion.Euler(localRotation);
            activeEquipmentModel.transform.localScale = localScale;
            
            isEquipped = true;
            Debug.Log($"Equipment equipped! Armor: +{armorBonus}, Damage: +{damageBonus}");
        }
    }

    public void OnUnequip(GameObject player)
    {
        if (activeEquipmentModel != null)
        {
            Destroy(activeEquipmentModel);
            isEquipped = false;
            Debug.Log("Equipment unequipped!");
        }
    }

    public bool CanUse(GameObject player)
    {
        return true;
    }

    public void OnRightClick(InventoryItem item, GameObject player)
    {
        Debug.Log($"Right-clicked equipment: {item.definition.itemName}");
        OnUse(player);
    }
}
