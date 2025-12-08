using UnityEngine;
using UnityEditor;

public class CreateExampleItems : EditorWindow
{
    [MenuItem("Tools/Inventory System/Create Example Items")]
    static void CreateItems()
    {
        string path = "Assets/ItemDefinitions";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets", "ItemDefinitions");
        }

        CreateRifle(path);
        CreatePistol(path);
        CreateAmmo(path);
        CreateMedkit(path);
        CreateBackpack(path);
        CreateArmor(path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", 
            "Created 6 example items in /Assets/ItemDefinitions/\n\n" +
            "You can now use these items for testing!", 
            "OK");
    }

    static void CreateRifle(string path)
    {
        ItemDefinition rifle = ScriptableObject.CreateInstance<ItemDefinition>();
        rifle.itemName = "AK-47 Rifle";
        rifle.description = "Powerful assault rifle with good damage and range.";
        rifle.width = 4;
        rifle.height = 2;
        rifle.canRotate = true;
        rifle.isStackable = false;
        rifle.weight = 3.5f;
        rifle.value = 2500;
        rifle.category = ItemCategory.Weapon;

        AssetDatabase.CreateAsset(rifle, path + "/Rifle_AK47.asset");
        Debug.Log("Created Rifle item definition");
    }

    static void CreatePistol(string path)
    {
        ItemDefinition pistol = ScriptableObject.CreateInstance<ItemDefinition>();
        pistol.itemName = "9mm Pistol";
        pistol.description = "Reliable sidearm for close quarters combat.";
        pistol.width = 2;
        pistol.height = 2;
        pistol.canRotate = true;
        pistol.isStackable = false;
        pistol.weight = 0.8f;
        pistol.value = 500;
        pistol.category = ItemCategory.Weapon;

        AssetDatabase.CreateAsset(pistol, path + "/Pistol_9mm.asset");
        Debug.Log("Created Pistol item definition");
    }

    static void CreateAmmo(string path)
    {
        ItemDefinition ammo = ScriptableObject.CreateInstance<ItemDefinition>();
        ammo.itemName = "7.62x39 Ammo";
        ammo.description = "Standard rifle ammunition. Used for AK-pattern weapons.";
        ammo.width = 1;
        ammo.height = 1;
        ammo.canRotate = false;
        ammo.isStackable = true;
        ammo.maxStackSize = 60;
        ammo.weight = 0.01f;
        ammo.value = 5;
        ammo.category = ItemCategory.Ammo;

        AssetDatabase.CreateAsset(ammo, path + "/Ammo_762.asset");
        Debug.Log("Created Ammo item definition");
    }

    static void CreateMedkit(string path)
    {
        ItemDefinition medkit = ScriptableObject.CreateInstance<ItemDefinition>();
        medkit.itemName = "First Aid Kit";
        medkit.description = "Medical supplies for treating injuries. Restores health over time.";
        medkit.width = 2;
        medkit.height = 2;
        medkit.canRotate = false;
        medkit.isStackable = false;
        medkit.weight = 0.3f;
        medkit.value = 150;
        medkit.category = ItemCategory.Medical;

        AssetDatabase.CreateAsset(medkit, path + "/Medical_FirstAid.asset");
        Debug.Log("Created Medkit item definition");
    }

    static void CreateBackpack(string path)
    {
        ItemDefinition backpack = ScriptableObject.CreateInstance<ItemDefinition>();
        backpack.itemName = "Tactical Backpack";
        backpack.description = "Large military backpack with multiple compartments.";
        backpack.width = 4;
        backpack.height = 5;
        backpack.canRotate = false;
        backpack.isStackable = false;
        backpack.isContainer = true;
        backpack.containerWidth = 6;
        backpack.containerHeight = 8;
        backpack.weight = 1.2f;
        backpack.value = 800;
        backpack.category = ItemCategory.Backpack;

        AssetDatabase.CreateAsset(backpack, path + "/Backpack_Tactical.asset");
        Debug.Log("Created Backpack item definition");
    }

    static void CreateArmor(string path)
    {
        ItemDefinition armor = ScriptableObject.CreateInstance<ItemDefinition>();
        armor.itemName = "Plate Carrier";
        armor.description = "Heavy body armor providing excellent protection.";
        armor.width = 3;
        armor.height = 3;
        armor.canRotate = false;
        armor.isStackable = false;
        armor.weight = 5.5f;
        armor.value = 1500;
        armor.category = ItemCategory.Armor;

        AssetDatabase.CreateAsset(armor, path + "/Armor_PlateCarrier.asset");
        Debug.Log("Created Armor item definition");
    }
}
