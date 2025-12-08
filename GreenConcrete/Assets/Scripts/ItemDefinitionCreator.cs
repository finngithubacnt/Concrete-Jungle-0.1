using UnityEngine;
using UnityEditor;

public class ItemDefinitionCreator : EditorWindow
{
    [MenuItem("Tools/Inventory/Create Example Items")]
    public static void ShowWindow()
    {
        GetWindow<ItemDefinitionCreator>("Item Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("Quick Item Definition Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Click buttons below to create example ItemDefinition assets in /Assets/ItemDefinitions/",
            MessageType.Info
        );
        EditorGUILayout.Space();

        if (GUILayout.Button("Create Rifle (4x2)", GUILayout.Height(35)))
        {
            CreateRifle();
        }

        if (GUILayout.Button("Create Pistol (2x1)", GUILayout.Height(35)))
        {
            CreatePistol();
        }

        if (GUILayout.Button("Create Ammo (1x1, Stackable)", GUILayout.Height(35)))
        {
            CreateAmmo();
        }

        if (GUILayout.Button("Create Medkit (2x2)", GUILayout.Height(35)))
        {
            CreateMedkit();
        }

        if (GUILayout.Button("Create Backpack (4x5, Container)", GUILayout.Height(35)))
        {
            CreateBackpack();
        }

        if (GUILayout.Button("Create Food (1x1, Stackable)", GUILayout.Height(35)))
        {
            CreateFood();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Create All Example Items", GUILayout.Height(50)))
        {
            CreateRifle();
            CreatePistol();
            CreateAmmo();
            CreateMedkit();
            CreateBackpack();
            CreateFood();
            Debug.Log("Created all example items!");
        }
    }

    void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/ItemDefinitions"))
        {
            AssetDatabase.CreateFolder("Assets", "ItemDefinitions");
        }
    }

    void CreateRifle()
    {
        EnsureFolder();
        ItemDefinition item = CreateInstance<ItemDefinition>();
        item.itemName = "AK-47 Rifle";
        item.description = "7.62x39mm assault rifle. Reliable and powerful.";
        item.width = 4;
        item.height = 2;
        item.canRotate = true;
        item.isStackable = false;
        item.weight = 3.5f;
        item.value = 2500;
        item.category = ItemCategory.Weapon;

        AssetDatabase.CreateAsset(item, "Assets/ItemDefinitions/Rifle_AK47.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Rifle item definition");
    }

    void CreatePistol()
    {
        EnsureFolder();
        ItemDefinition item = CreateInstance<ItemDefinition>();
        item.itemName = "Glock 17";
        item.description = "9mm semi-automatic pistol. Compact and reliable.";
        item.width = 2;
        item.height = 1;
        item.canRotate = true;
        item.isStackable = false;
        item.weight = 0.6f;
        item.value = 800;
        item.category = ItemCategory.Weapon;

        AssetDatabase.CreateAsset(item, "Assets/ItemDefinitions/Pistol_Glock.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Pistol item definition");
    }

    void CreateAmmo()
    {
        EnsureFolder();
        ItemDefinition item = CreateInstance<ItemDefinition>();
        item.itemName = "7.62x39 Ammo";
        item.description = "Rifle ammunition. Standard military rounds.";
        item.width = 1;
        item.height = 1;
        item.canRotate = false;
        item.isStackable = true;
        item.maxStackSize = 60;
        item.weight = 0.01f;
        item.value = 2;
        item.category = ItemCategory.Ammo;

        AssetDatabase.CreateAsset(item, "Assets/ItemDefinitions/Ammo_762x39.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Ammo item definition");
    }

    void CreateMedkit()
    {
        EnsureFolder();
        ItemDefinition item = CreateInstance<ItemDefinition>();
        item.itemName = "First Aid Kit";
        item.description = "Medical supplies for treating injuries. Restores 75 HP.";
        item.width = 2;
        item.height = 2;
        item.canRotate = false;
        item.isStackable = false;
        item.weight = 0.3f;
        item.value = 150;
        item.category = ItemCategory.Medical;

        AssetDatabase.CreateAsset(item, "Assets/ItemDefinitions/Medical_FirstAidKit.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Medkit item definition");
    }

    void CreateBackpack()
    {
        EnsureFolder();
        ItemDefinition item = CreateInstance<ItemDefinition>();
        item.itemName = "Tactical Backpack";
        item.description = "Large military backpack with multiple compartments.";
        item.width = 4;
        item.height = 5;
        item.canRotate = false;
        item.isStackable = false;
        item.isContainer = true;
        item.containerWidth = 6;
        item.containerHeight = 8;
        item.weight = 1.2f;
        item.value = 500;
        item.category = ItemCategory.Backpack;

        AssetDatabase.CreateAsset(item, "Assets/ItemDefinitions/Backpack_Tactical.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Backpack item definition");
    }

    void CreateFood()
    {
        EnsureFolder();
        ItemDefinition item = CreateInstance<ItemDefinition>();
        item.itemName = "Energy Drink";
        item.description = "Restores stamina and provides temporary speed boost.";
        item.width = 1;
        item.height = 1;
        item.canRotate = false;
        item.isStackable = true;
        item.maxStackSize = 5;
        item.weight = 0.2f;
        item.value = 25;
        item.category = ItemCategory.Food;

        AssetDatabase.CreateAsset(item, "Assets/ItemDefinitions/Food_EnergyDrink.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Food item definition");
    }
}
