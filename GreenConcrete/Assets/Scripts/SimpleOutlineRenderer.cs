using UnityEngine;

[ExecuteInEditMode]
public class SimpleOutlineRenderer : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.white;
    [Range(0f, 0.1f)]
    public float outlineWidth = 0.01f;
    
    [Header("Material Setup")]
    public Material outlineMaterial;
    
    private Renderer targetRenderer;
    private Material[] originalMaterials;
    private Material[] modifiedMaterials;

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null && outlineMaterial != null)
        {
            SetupOutlineMaterials();
        }
    }

    void Update()
    {
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }
    }

    void SetupOutlineMaterials()
    {
        if (targetRenderer == null) return;
        
        originalMaterials = targetRenderer.sharedMaterials;
        modifiedMaterials = new Material[originalMaterials.Length + 1];
        
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            modifiedMaterials[i] = originalMaterials[i];
        }
        
        modifiedMaterials[modifiedMaterials.Length - 1] = outlineMaterial;
        targetRenderer.materials = modifiedMaterials;
    }

    void OnValidate()
    {
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }
    }

    void OnDisable()
    {
        if (targetRenderer != null && originalMaterials != null)
        {
            targetRenderer.materials = originalMaterials;
        }
    }
}
