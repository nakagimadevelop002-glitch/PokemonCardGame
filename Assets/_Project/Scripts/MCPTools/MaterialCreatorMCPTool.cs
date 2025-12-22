using System.ComponentModel;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[McpServerToolType, Description("Create material assets")]
public class MaterialCreatorMCPTool
{
    [McpServerTool, Description("Create a material asset with specified shader")]
    public async ValueTask<string> CreateMaterial(
        [Description("Material name")] string materialName,
        [Description("Shader name (e.g., 'UI/Default', 'Particles/Standard Unlit')")] string shaderName = "UI/Default",
        [Description("Output path (default: Assets/_Project/Materials/)")] string outputPath = "Assets/_Project/Materials/")
    {
#if UNITY_EDITOR
        await UniTask.SwitchToMainThread();

        try
        {
            // Find shader
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                return $"ERROR: Shader '{shaderName}' not found";
            }

            // Create material
            Material material = new Material(shader);

            // Set blend mode for transparency
            if (shaderName.Contains("UI") || shaderName.Contains("Particle"))
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.renderQueue = 3000;
            }

            // Ensure directory exists
            if (!System.IO.Directory.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }

            // Save material
            string fullPath = $"{outputPath}{materialName}.mat";
            AssetDatabase.CreateAsset(material, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created material '{materialName}' at {fullPath}");
            return $"SUCCESS: Created material at {fullPath}";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create material: {e.Message}");
            return $"ERROR: {e.Message}";
        }
#else
        await UniTask.Yield();
        return "ERROR: Material creation only available in Unity Editor";
#endif
    }
}
