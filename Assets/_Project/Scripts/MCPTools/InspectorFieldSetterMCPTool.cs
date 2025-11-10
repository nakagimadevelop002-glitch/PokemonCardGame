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

    [McpServerTool, Description("Set any field value on a component (auto-parses primitives, string, enum, Color, Vector2/3/4)")]
    public async ValueTask<string> SetFieldValue(
        [Description("Target GameObject name")] string objectName,
        [Description("Component type name (e.g., ModalSystem)")] string componentTypeName,
        [Description("Field name to set")] string fieldName,
        [Description("Value as string (will be parsed to appropriate type)")] string value)
    {
#if UNITY_EDITOR
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

            FieldInfo field = componentType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                return $"ERROR: Field '{fieldName}' not found on '{componentTypeName}'";
            }

            // Parse value based on field type
            object parsedValue = ParseValue(field.FieldType, value);
            if (parsedValue == null && !string.IsNullOrEmpty(value))
            {
                return $"ERROR: Failed to parse '{value}' as {field.FieldType.Name}";
            }

            field.SetValue(component, parsedValue);

            // Mark scene dirty and save
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log($"Scene '{scene.name}' saved after setting {componentTypeName}.{fieldName} = {parsedValue}");

            return $"SUCCESS: Set {objectName}.{componentTypeName}.{fieldName} = {parsedValue}";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set field value: {e.Message}");
            return $"ERROR: {e.Message}";
        }
#else
        await UniTask.Yield();
        return "ERROR: Field value setting only available in Unity Editor";
#endif
    }

    /// <summary>
    /// Parse string value to appropriate type
    /// </summary>
    private object ParseValue(Type targetType, string value)
    {
        try
        {
            // Null or empty
            if (string.IsNullOrEmpty(value))
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            // Bool
            if (targetType == typeof(bool))
            {
                if (value.ToLower() == "true" || value == "1") return true;
                if (value.ToLower() == "false" || value == "0") return false;
                return bool.Parse(value);
            }

            // Int
            if (targetType == typeof(int))
                return int.Parse(value);

            // Float
            if (targetType == typeof(float))
                return float.Parse(value);

            // Double
            if (targetType == typeof(double))
                return double.Parse(value);

            // Long
            if (targetType == typeof(long))
                return long.Parse(value);

            // String
            if (targetType == typeof(string))
                return value;

            // Enum
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value, true);

            // Color (hex format: #RRGGBB or #RRGGBBAA)
            if (targetType == typeof(Color))
            {
                Color color;
                if (ColorUtility.TryParseHtmlString(value, out color))
                    return color;
                return Color.white;
            }

            // Vector2 (format: "x,y")
            if (targetType == typeof(Vector2))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 2)
                    return new Vector2(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()));
            }

            // Vector3 (format: "x,y,z")
            if (targetType == typeof(Vector3))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 3)
                    return new Vector3(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()), float.Parse(parts[2].Trim()));
            }

            // Vector4 (format: "x,y,z,w")
            if (targetType == typeof(Vector4))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 4)
                    return new Vector4(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()), float.Parse(parts[2].Trim()), float.Parse(parts[3].Trim()));
            }

            // Quaternion (format: "x,y,z,w")
            if (targetType == typeof(Quaternion))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 4)
                    return new Quaternion(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()), float.Parse(parts[2].Trim()), float.Parse(parts[3].Trim()));
            }

            // Rect (format: "x,y,width,height")
            if (targetType == typeof(Rect))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 4)
                    return new Rect(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()), float.Parse(parts[2].Trim()), float.Parse(parts[3].Trim()));
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    [McpServerTool, Description("Set GameObject active state (enable/disable)")]
    public async ValueTask<string> SetGameObjectActive(
        [Description("Target GameObject name")] string objectName,
        [Description("Active state (true = enabled, false = disabled)")] string active)
    {
#if UNITY_EDITOR
        try
        {
            await UniTask.SwitchToMainThread();

            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject == null)
            {
                return $"ERROR: GameObject '{objectName}' not found";
            }

            bool activeState = active.ToLower() == "true" || active == "1";
            targetObject.SetActive(activeState);

            // Mark scene dirty and save
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log($"Scene '{scene.name}' saved after setting {objectName}.SetActive({activeState})");

            return $"SUCCESS: Set {objectName}.SetActive({activeState})";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set active state: {e.Message}");
            return $"ERROR: {e.Message}";
        }
#else
        await UniTask.Yield();
        return "ERROR: SetActive only available in Unity Editor";
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
