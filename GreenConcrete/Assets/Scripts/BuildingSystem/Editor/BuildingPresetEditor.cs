using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingPreset))]
public class BuildingPresetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BuildingPreset preset = (BuildingPreset)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Create Preview Prefab from Building Prefab"))
        {
            CreatePreviewPrefab(preset);
        }

        if (preset.requiredMaterials != null && preset.requiredMaterials.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Material Requirements Summary", EditorStyles.boldLabel);
            
            foreach (var mat in preset.requiredMaterials)
            {
                EditorGUILayout.LabelField($"  • {mat.materialType}: {mat.amount}");
            }
        }
    }

    void CreatePreviewPrefab(BuildingPreset preset)
    {
        if (preset.buildingPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Building Prefab is not assigned!", "OK");
            return;
        }

        string path = AssetDatabase.GetAssetPath(preset.buildingPrefab);
        string directory = System.IO.Path.GetDirectoryName(path);
        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
        string newPath = $"{directory}/{fileName}_Preview.prefab";

        GameObject instance = PrefabUtility.InstantiatePrefab(preset.buildingPrefab) as GameObject;
        
        if (instance != null)
        {
            MakeTransparent(instance);
            
            Collider[] colliders = instance.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            Rigidbody[] rigidbodies = instance.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                DestroyImmediate(rb);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, newPath);
            DestroyImmediate(instance);

            preset.previewPrefab = prefab;
            EditorUtility.SetDirty(preset);

            EditorUtility.DisplayDialog("Success", $"Preview prefab created at:\n{newPath}", "OK");
        }
    }

    void MakeTransparent(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.sharedMaterials;
            
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null)
                {
                    Material newMat = new Material(mats[i]);
                    
                    if (newMat.HasProperty("_Color"))
                    {
                        Color col = newMat.color;
                        col.a = 0.5f;
                        newMat.color = col;
                    }

                    newMat.SetFloat("_Surface", 1);
                    newMat.SetOverrideTag("RenderType", "Transparent");
                    newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    newMat.SetInt("_ZWrite", 0);
                    newMat.DisableKeyword("_ALPHATEST_ON");
                    newMat.EnableKeyword("_ALPHABLEND_ON");
                    newMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    newMat.renderQueue = 3000;

                    mats[i] = newMat;
                }
            }
            
            renderer.sharedMaterials = mats;
        }
    }
}
