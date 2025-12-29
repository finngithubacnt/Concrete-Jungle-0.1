using UnityEngine;

public class HealthPotionBehavior : MonoBehaviour, IItemBehavior
{
    public float healAmount = 50f;

    public void OnUse(GameObject player)
    {
        Debug.Log($"Healed {healAmount} HP!");
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
}
