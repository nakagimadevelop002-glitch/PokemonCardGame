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
    [McpServerTool, Description("Set any field value on a ScriptableObject (auto-parses primitives, string, enum, Color, Vector2/3/4, Sprite, etc)")]
    public async ValueTask<string> SetScriptableObjectFieldValue(
        [Description("Asset path (e.g., Assets/_Project/Data/Card.asset)")] string assetPath,
        [Description("Field name")] string fieldName,
        [Description("Value as string (will be parsed to appropriate type)")] string value)
    {
#if UNITY_EDITOR
        try
        {
            Debug.Log($"[SetScriptableObjectFieldValue] Starting: assetPath={assetPath}, fieldName={fieldName}, value={value}");
            await UniTask.SwitchToMainThread();

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null) return $"ERROR: Asset not found at {assetPath}";
            Debug.Log($"[SetScriptableObjectFieldValue] Asset loaded: {asset.GetType().Name}");

            var field = asset.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) return $"ERROR: Field '{fieldName}' not found";
            Debug.Log($"[SetScriptableObjectFieldValue] Field found: {fieldName} ({field.FieldType.Name})");

            // Parse value based on field type
            Debug.Log($"[SetScriptableObjectFieldValue] Calling ParseValue...");
            object parsedValue = ParseValue(field.FieldType, value);
            Debug.Log($"[SetScriptableObjectFieldValue] ParseValue returned: {(parsedValue == null ? "null" : parsedValue.ToString())}");

            if (parsedValue == null && !string.IsNullOrEmpty(value))
            {
                return $"ERROR: Failed to parse '{value}' as {field.FieldType.Name}";
            }

            Debug.Log($"[SetScriptableObjectFieldValue] Setting field value...");
            field.SetValue(asset, parsedValue);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SetScriptableObjectFieldValue] SUCCESS");
            return $"SUCCESS: Set {fieldName} = {parsedValue}";
        }
        catch (Exception e)
        {
            Debug.LogError($"[SetScriptableObjectFieldValue] Exception: {e.GetType().Name} - {e.Message}\n{e.StackTrace}");
            return $"ERROR: {e.Message}";
        }
#else
        await UniTask.Yield();
        return "ERROR: Editor only";
#endif
    }

    /// <summary>
    /// あらゆる型に対応する汎用パース処理
    /// </summary>
    private object ParseValue(Type targetType, string value)
    {
        try
        {
            if (string.IsNullOrEmpty(value))
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            // 1. UnityEngine.Object派生型（Sprite, Texture2D, Material, AudioClip, Prefab, ScriptableObject等全て）
            if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
            {
#if UNITY_EDITOR
                Debug.Log($"[ParseValue] Loading {targetType.Name} from path: {value}");

                // Special handling for Sprite (supports both Single and Multiple sprite modes)
                if (targetType == typeof(UnityEngine.Sprite))
                {
                    // Try LoadAllAssetsAtPath first (works for both Single and Multiple modes)
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(value).OfType<UnityEngine.Sprite>().ToArray();
                    if (sprites.Length > 0)
                    {
                        Debug.Log($"[ParseValue] Successfully loaded Sprite (found {sprites.Length} sprites, using first one)");
                        return sprites[0];
                    }
                    else
                    {
                        Debug.LogWarning($"[ParseValue] No Sprite found at path: {value}");
                        return null;
                    }
                }

                // AssetDatabase.LoadAssetAtPath<T>(path)を動的に生成して呼び出し
                var loadMethod = typeof(AssetDatabase)
                    .GetMethods()
                    .First(m => m.Name == "LoadAssetAtPath" && m.IsGenericMethod)
                    .MakeGenericMethod(targetType);

                Debug.Log($"[ParseValue] Invoking LoadAssetAtPath<{targetType.Name}>");
                var asset = loadMethod.Invoke(null, new object[] { value });

                if (asset == null)
                {
                    Debug.LogWarning($"[ParseValue] Asset not found at path: {value}");
                }
                else
                {
                    Debug.Log($"[ParseValue] Successfully loaded: {asset}");
                }
                return asset;
#else
                return null;
#endif
            }

            // 2. Enum型
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value, true);

            // 3. bool型（特殊処理）
            if (targetType == typeof(bool))
            {
                if (value.ToLower() == "true" || value == "1") return true;
                if (value.ToLower() == "false" || value == "0") return false;
                return bool.Parse(value);
            }

            // 4. プリミティブ型 + string
            if (targetType.IsPrimitive || targetType == typeof(string))
            {
                return Convert.ChangeType(value, targetType);
            }

            // 5. Unity特有型（Color, Vector2/3/4）
            if (targetType == typeof(Color))
            {
                Color color;
                if (ColorUtility.TryParseHtmlString(value, out color))
                    return color;
                return Color.white;
            }

            if (targetType == typeof(Vector2))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 2)
                    return new Vector2(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()));
            }

            if (targetType == typeof(Vector3))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 3)
                    return new Vector3(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()), float.Parse(parts[2].Trim()));
            }

            if (targetType == typeof(Vector4))
            {
                string[] parts = value.Split(',');
                if (parts.Length == 4)
                    return new Vector4(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()), float.Parse(parts[2].Trim()), float.Parse(parts[3].Trim()));
            }

            // 6. 複雑な型に対するフォールバック（JSON解釈）
            try
            {
                return JsonUtility.FromJson(value, targetType);
            }
            catch
            {
                // JSON解釈失敗時はnullを返す
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"ParseValue failed for type {targetType.Name}: {ex.Message}");
            return null;
        }
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
