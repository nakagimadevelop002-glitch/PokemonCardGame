using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;
using UnityEngine;
using UnityEditor;

[McpServerToolType, Description("Set field values on ScriptableObject assets")]
public class ScriptableObjectFieldSetterMCPTool
{
    [McpServerTool, Description("Set a string field on a ScriptableObject")]
    public async ValueTask<string> SetStringField(
        [Description("Asset path (e.g., Assets/_Project/Data/Card.asset)")] string assetPath,
        [Description("Field name")] string fieldName,
        [Description("String value")] string value)
    {
#if UNITY_EDITOR
        try
        {
            await UniTask.SwitchToMainThread();
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null) return $"ERROR: Asset not found at {assetPath}";

            var field = asset.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) return $"ERROR: Field '{fieldName}' not found";

            field.SetValue(asset, value);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return $"SUCCESS: Set {fieldName} = '{value}'";
        }
        catch (Exception e) { return $"ERROR: {e.Message}"; }
#else
        await UniTask.Yield();
        return "ERROR: Editor only";
#endif
    }

    [McpServerTool, Description("Set an int field on a ScriptableObject")]
    public async ValueTask<string> SetIntField(
        [Description("Asset path")] string assetPath,
        [Description("Field name")] string fieldName,
        [Description("Int value")] int value)
    {
#if UNITY_EDITOR
        try
        {
            await UniTask.SwitchToMainThread();
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null) return $"ERROR: Asset not found at {assetPath}";

            var field = asset.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) return $"ERROR: Field '{fieldName}' not found";

            field.SetValue(asset, value);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return $"SUCCESS: Set {fieldName} = {value}";
        }
        catch (Exception e) { return $"ERROR: {e.Message}"; }
#else
        await UniTask.Yield();
        return "ERROR: Editor only";
#endif
    }

    [McpServerTool, Description("Set a bool field on a ScriptableObject")]
    public async ValueTask<string> SetBoolField(
        [Description("Asset path")] string assetPath,
        [Description("Field name")] string fieldName,
        [Description("Bool value")] bool value)
    {
#if UNITY_EDITOR
        try
        {
            await UniTask.SwitchToMainThread();
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null) return $"ERROR: Asset not found at {assetPath}";

            var field = asset.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) return $"ERROR: Field '{fieldName}' not found";

            field.SetValue(asset, value);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return $"SUCCESS: Set {fieldName} = {value}";
        }
        catch (Exception e) { return $"ERROR: {e.Message}"; }
#else
        await UniTask.Yield();
        return "ERROR: Editor only";
#endif
    }

    [McpServerTool, Description("Set an enum field on a ScriptableObject by string name")]
    public async ValueTask<string> SetEnumField(
        [Description("Asset path")] string assetPath,
        [Description("Field name")] string fieldName,
        [Description("Enum value name (e.g., 'Basic', 'Stage1')")] string enumValueName)
    {
#if UNITY_EDITOR
        try
        {
            await UniTask.SwitchToMainThread();
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null) return $"ERROR: Asset not found at {assetPath}";

            var field = asset.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) return $"ERROR: Field '{fieldName}' not found";
            if (!field.FieldType.IsEnum) return $"ERROR: Field '{fieldName}' is not an enum";

            var enumValue = Enum.Parse(field.FieldType, enumValueName);
            field.SetValue(asset, enumValue);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return $"SUCCESS: Set {fieldName} = {enumValueName}";
        }
        catch (Exception e) { return $"ERROR: {e.Message}"; }
#else
        await UniTask.Yield();
        return "ERROR: Editor only";
#endif
    }

    [McpServerTool, Description("Get all field names and current values from a ScriptableObject")]
    public async ValueTask<string> GetFields(
        [Description("Asset path")] string assetPath)
    {
#if UNITY_EDITOR
        try
        {
            await UniTask.SwitchToMainThread();
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null) return $"ERROR: Asset not found at {assetPath}";

            var fields = asset.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            string result = $"Fields in {asset.GetType().Name}:\n";
            foreach (var field in fields)
            {
                var value = field.GetValue(asset);
                string valueStr = value != null ? value.ToString() : "null";
                result += $"- {field.Name} ({field.FieldType.Name}): {valueStr}\n";
            }
            return result.Trim();
        }
        catch (Exception e) { return $"ERROR: {e.Message}"; }
#else
        await UniTask.Yield();
        return "ERROR: Editor only";
#endif
    }
}
