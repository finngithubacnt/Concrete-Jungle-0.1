using UnityEngine;

public interface IItemBehavior
{
    void OnUse(GameObject player);
    void OnEquip(GameObject player);
    void OnUnequip(GameObject player);
    bool CanUse(GameObject player);
}
