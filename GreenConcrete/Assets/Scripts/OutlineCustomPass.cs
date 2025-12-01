using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class OutlineCustomPass : CustomPass
{
    [Header("Outline Settings")]
    public LayerMask outlineLayer = 0;
    public Color outlineColor = Color.white;
    [Range(0f, 20f)]
    public float outlineThickness = 1f;
    
    [Header("Advanced Settings")]
    [Range(0f, 1f)]
    public float depthSensitivity = 0.1f;
    [Range(0f, 1f)]
    public float normalsSensitivity = 0.5f;
    public bool useColorSensitivity = false;
    [Range(0f, 1f)]
    public float colorSensitivity = 0.1f;

    private Material outlineMaterial;
    private RTHandle outlineBuffer;
    
    private const string shaderName = "Hidden/OutlinePass";

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        CreateOutlineMaterial();
    }

    void CreateOutlineMaterial()
    {
        if (outlineMaterial == null)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"Outline shader '{shaderName}' not found! Make sure OutlinePassShader.shader exists.");
                return;
            }
            outlineMaterial = CoreUtils.CreateEngineMaterial(shader);
        }
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (outlineMaterial == null)
        {
            CreateOutlineMaterial();
            if (outlineMaterial == null) return;
        }

        if (outlineLayer == 0) return;

        CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ctx.cameraDepthBuffer);
        
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineThickness", outlineThickness);
        outlineMaterial.SetFloat("_DepthSensitivity", depthSensitivity);
        outlineMaterial.SetFloat("_NormalsSensitivity", normalsSensitivity);
        outlineMaterial.SetFloat("_ColorSensitivity", useColorSensitivity ? colorSensitivity : 0f);

        CustomPassUtils.DrawRenderers(ctx, outlineLayer);
        
        CoreUtils.DrawFullScreen(ctx.cmd, outlineMaterial, ctx.cameraColorBuffer);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(outlineMaterial);
        outlineBuffer?.Release();
    }
}
