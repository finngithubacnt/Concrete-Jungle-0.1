using UnityEngine;

public class MaterialTester : MonoBehaviour
{
    [Header("Test Materials")]
    public int testLogsAmount = 100;
    public int testStonesAmount = 100;
    public int testWoodAmount = 50;
    public int testMetalAmount = 25;
    public int testClothAmount = 25;

    [Header("Hotkeys")]
    public KeyCode addMaterialsKey = KeyCode.M;

    void Update()
    {
        if (Input.GetKeyDown(addMaterialsKey))
        {
            AddTestMaterials();
        }
    }

    public void AddTestMaterials()
    {
        if (MaterialInventory.Instance == null)
        {
            Debug.LogWarning("MaterialInventory instance not found!");
            return;
        }

        MaterialInventory.Instance.AddMaterial(MaterialType.Logs, testLogsAmount);
        MaterialInventory.Instance.AddMaterial(MaterialType.Stones, testStonesAmount);
        MaterialInventory.Instance.AddMaterial(MaterialType.Wood, testWoodAmount);
        MaterialInventory.Instance.AddMaterial(MaterialType.Metal, testMetalAmount);
        MaterialInventory.Instance.AddMaterial(MaterialType.Cloth, testClothAmount);

        Debug.Log($"Added test materials: {testLogsAmount} Logs, {testStonesAmount} Stones, {testWoodAmount} Wood, {testMetalAmount} Metal, {testClothAmount} Cloth");
    }

    void Start()
    {
        Debug.Log($"Press {addMaterialsKey} to add test materials for building.");
    }
}
