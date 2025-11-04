using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using UnityNaturalMCP.Editor;

[CreateAssetMenu(fileName = "AllMCPToolsBuilder",
 menuName = "UnityNaturalMCP/All MCP Tools Builder")]
public class AllMCPToolsBuilder : McpBuilderScriptableObject
{
    public override void Build(IMcpServerBuilder builder)
    {
        Debug.Log("AllMCPToolsBuilder.Build() called - Starting tool registration");
        // Register all MCP tools here
        builder.WithTools<CubeCreatorMCPTool>();
        builder.WithTools<SeaweedCreatorMCPTool>();
        builder.WithTools<SeaweedCarpetMCPTool>();
        builder.WithTools<RefreshMCPToolsMCPTool>();
        builder.WithTools<CreateSceneMCPTool>();
        builder.WithTools<AttachScriptMCPTool>();
        builder.WithTools<PackageManagerMCPTool>();
        builder.WithTools<PackageInstallerMCPTool>();
        Debug.Log("Registering CustomShapeMCPTool...");
        builder.WithTools<CustomShapeMCPTool>();
        Debug.Log("Registering SimpleTestMCPTool...");
        builder.WithTools<SimpleTestMCPTool>();
        Debug.Log("Registering GameObjectManagerMCPTool...");
        builder.WithTools<GameObjectManagerMCPTool>();
        Debug.Log("Registering PrefabManagerMCPTool...");
        builder.WithTools<PrefabManagerMCPTool>();
        Debug.Log("Registering OrganicPatchMCPTool...");
        builder.WithTools<OrganicPatchMCPTool>();
        Debug.Log("Registering UICreatorMCPTool...");
        builder.WithTools<UICreatorMCPTool>();
        Debug.Log("Registering ComponentAttachMCPTool...");
        builder.WithTools<ComponentAttachMCPTool>();
        Debug.Log("Registering ComprehensiveUICreatorMCPTool...");
        builder.WithTools<ComprehensiveUICreatorMCPTool>();
        Debug.Log("Registering InspectorFieldSetterMCPTool...");
        builder.WithTools<InspectorFieldSetterMCPTool>();
        Debug.Log("Registering ScriptableObjectMCPTool...");
        builder.WithTools<ScriptableObjectMCPTool>();
        Debug.Log("Registering ScriptableObjectFieldSetterMCPTool...");
        builder.WithTools<ScriptableObjectFieldSetterMCPTool>();
        Debug.Log("Registering PlayModeControlMCPTool...");
        builder.WithTools<PlayModeControlMCPTool>();
        Debug.Log("Registering HierarchyManagerMCPTool...");
        builder.WithTools<HierarchyManagerMCPTool>();
        Debug.Log("Registering ScreenshotCaptureMCPTool...");
        builder.WithTools<ScreenshotCaptureMCPTool>();

        Debug.Log("AllMCPToolsBuilder.Build() completed - All tools registered");
        // Add new MCP tools here in the future:
        // builder.WithTools<YourNewMCPTool>();
    }
}