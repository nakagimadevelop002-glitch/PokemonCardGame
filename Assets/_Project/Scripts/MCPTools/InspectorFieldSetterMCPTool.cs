using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;
using UnityEngine;

[McpServerToolType, Description("Set Inspector field values on GameObjects with reflection")]
public class InspectorFieldSetterMCPTool
{
    [McpServerTool, Description("Set a public field on a component by GameObject name")]
    public async ValueTask<string> SetComponentField(
        [Description("Target GameObject name")] string objectName,
        [Description("Component type name (e.g., GameManager)")] string componentTypeName,
        [Description("Field name to set")] string fieldName,
        [Description("Value GameObject name (for Transform/GameObject references)")] string valueObjectName)
    {
        try
        {
            await UniTask.SwitchToMainThread();

            // Find target GameObject
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject == null)
            {
                return $"ERROR: GameObject '{objectName}' not found";
            }

            // Find component type
            var componentType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == componentTypeName && typeof(UnityEngine.Component).IsAssignableFrom(t));

            if (componentType == null)
            {
                return $"ERROR: Component type '{componentTypeName}' not found";
            }

            // Get component
            var component = targetObject.GetComponent(componentType);
            if (component == null)
            {
                return $"ERROR: Component '{componentTypeName}' not found on '{objectName}'";
            }

            // Find field
            FieldInfo field = componentType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                return $"ERROR: Field '{fieldName}' not found on '{componentTypeName}'";
            }

            // Find value GameObject
            GameObject valueObject = GameObject.Find(valueObjectName);
            if (valueObject == null)
            {
                return $"ERROR: Value GameObject '{valueObjectName}' not found";
            }

            // Set field based on type
            if (field.FieldType == typeof(Transform))
            {
                field.SetValue(component, valueObject.transform);
            }
            else if (field.FieldType == typeof(GameObject))
            {
                field.SetValue(component, valueObject);
            }
            else if (typeof(UnityEngine.Component).IsAssignableFrom(field.FieldType))
            {
                var valueComponent = valueObject.GetComponent(field.FieldType);
                if (valueComponent == null)
                {
                    return $"ERROR: Component '{field.FieldType.Name}' not found on '{valueObjectName}'";
                }
                field.SetValue(component, valueComponent);
            }
            else
            {
                return $"ERROR: Unsupported field type '{field.FieldType.Name}'";
            }

            Debug.Log($"Set {componentTypeName}.{fieldName} = {valueObjectName}");
            return $"SUCCESS: Set '{componentTypeName}.{fieldName}' to '{valueObjectName}' on '{objectName}'";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set field: {e.Message}");
            return $"ERROR: {e.Message}";
        }
    }

    [McpServerTool, Description("Set a ScriptableObject asset field on a component")]
    public async ValueTask<string> SetScriptableObjectField(
        [Description("Target GameObject name")] string objectName,
        [Description("Component type name (e.g., GameManager)")] string componentTypeName,
        [Description("Field name to set")] string fieldName,
        [Description("ScriptableObject asset path or name")] string assetPath)
    {
#if UNITY_EDITOR
        try
        {
            await UniTask.SwitchToMainThread();

            // Find target GameObject
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject == null)
            {
                return $"ERROR: GameObject '{objectName}' not found";
            }

            // Find component type
            var componentType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == componentTypeName && typeof(UnityEngine.Component).IsAssignableFrom(t));

            if (componentType == null)
            {
                return $"ERROR: Component type '{componentTypeName}' not found";
            }

            // Get component
            var component = targetObject.GetComponent(componentType);
            if (component == null)
            {
                return $"ERROR: Component '{componentTypeName}' not found on '{objectName}'";
            }

            // Find field
            FieldInfo field = componentType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                return $"ERROR: Field '{fieldName}' not found on '{componentTypeName}'";
            }

            // Load ScriptableObject asset using reflection
            var assetDatabaseType = typeof(UnityEditor.AssetDatabase);

            // Ensure path starts with "Assets/"
            if (!assetPath.StartsWith("Assets/") && assetPath.Contains("/"))
            {
                assetPath = "Assets/" + assetPath;
            }

            // Try to load by path first - get generic LoadAssetAtPath<T>(string path) method
            var loadAssetAtPathMethods = assetDatabaseType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "LoadAssetAtPath" &&
                           m.IsGenericMethod &&
                           m.GetParameters().Length == 1 &&
                           m.GetParameters()[0].ParameterType == typeof(string));

            if (!loadAssetAtPathMethods.Any())
            {
                return "ERROR: LoadAssetAtPath method not found";
            }

            var loadAssetAtPathMethod = loadAssetAtPathMethods.First().MakeGenericMethod(field.FieldType);
            var asset = loadAssetAtPathMethod.Invoke(null, new object[] { assetPath });

            // If not found, try to find by name
            if (asset == null)
            {
                var findAssetsMethod = assetDatabaseType.GetMethod("FindAssets",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string) },
                    null);
                var guidToAssetPathMethod = assetDatabaseType.GetMethod("GUIDToAssetPath",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string) },
                    null);

                string[] guids = (string[])findAssetsMethod.Invoke(null, new object[] { assetPath });
                if (guids.Length > 0)
                {
                    string path = (string)guidToAssetPathMethod.Invoke(null, new object[] { guids[0] });
                    asset = loadAssetAtPathMethod.Invoke(null, new object[] { path });
                }
            }

            if (asset == null)
            {
                return $"ERROR: ScriptableObject asset '{assetPath}' not found";
            }

            // Set field
            field.SetValue(component, asset);

            Debug.Log($"Set {componentTypeName}.{fieldName} = {assetPath}");
            return $"SUCCESS: Set '{componentTypeName}.{fieldName}' to '{assetPath}' on '{objectName}'";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set ScriptableObject field: {e.Message}");
            return $"ERROR: {e.Message}";
        }
#else
        await UniTask.Yield();
        return "ERROR: ScriptableObject field setting only available in Unity Editor";
#endif
    }

    [McpServerTool, Description("List all public fields on a component")]
    public async ValueTask<string> ListComponentFields(
        [Description("Target GameObject name")] string objectName,
        [Description("Component type name")] string componentTypeName)
    {
        try
        {
            await UniTask.SwitchToMainThread();

            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject == null)
            {
                return $"ERROR: GameObject '{objectName}' not found";
            }

            var componentType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == componentTypeName && typeof(UnityEngine.Component).IsAssignableFrom(t));

            if (componentType == null)
            {
                return $"ERROR: Component type '{componentTypeName}' not found";
            }

            var component = targetObject.GetComponent(componentType);
            if (component == null)
            {
                return $"ERROR: Component '{componentTypeName}' not found on '{objectName}'";
            }

            var fields = componentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            string result = $"Public fields on {componentTypeName}:\n";

            foreach (var field in fields)
            {
                var currentValue = field.GetValue(component);
                string valueStr = currentValue != null ? currentValue.ToString() : "null";
                result += $"- {field.Name} ({field.FieldType.Name}): {valueStr}\n";
            }

            return result.Trim();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to list fields: {e.Message}");
            return $"ERROR: {e.Message}";
        }
    }
}
