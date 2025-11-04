using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;
using UnityEngine;
using UnityEditor;

[McpServerToolType, Description("Attach MonoBehaviour scripts to GameObjects with flexible targeting")]
public class AttachScriptMCPTool
{
    [McpServerTool, Description("Attach a script to a specific GameObject by name")]
    public async ValueTask<string> AttachScriptToObject(
        [Description("Target GameObject name")] string objectName,
        [Description("Script class name (must be MonoBehaviour)")] string scriptClassName)
    {
        try
        {
            await UniTask.SwitchToMainThread();

            // Find the target GameObject
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject == null)
            {
                return $"ERROR: GameObject '{objectName}' not found in scene";
            }

            // Find the script type using reflection
            var scriptType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == scriptClassName && t.IsSubclassOf(typeof(MonoBehaviour)));

            if (scriptType == null)
            {
                return $"ERROR: Script class '{scriptClassName}' not found or not a MonoBehaviour";
            }

            // Check if component already exists
            if (targetObject.GetComponent(scriptType) != null)
            {
                return $"WARNING: '{scriptClassName}' already attached to '{objectName}'";
            }

            // Attach the component
            var component = targetObject.AddComponent(scriptType);

            Debug.Log($"Successfully attached {scriptClassName} to {objectName}");
            return $"SUCCESS: Attached '{scriptClassName}' to GameObject '{objectName}'";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to attach script: {e.Message}");
            throw;
        }
    }

    [McpServerTool, Description("Attach a script to all GameObjects with a specific tag")]
    public async ValueTask<string> AttachScriptToObjectsWithTag(
        [Description("Target tag name")] string tag,
        [Description("Script class name (must be MonoBehaviour)")] string scriptClassName)
    {
        try
        {
            await UniTask.SwitchToMainThread();

            // Find all GameObjects with the specified tag
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(tag);
            if (targetObjects.Length == 0)
            {
                return $"ERROR: No GameObjects found with tag '{tag}'";
            }

            // Find the script type
            var scriptType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == scriptClassName && t.IsSubclassOf(typeof(MonoBehaviour)));

            if (scriptType == null)
            {
                return $"ERROR: Script class '{scriptClassName}' not found or not a MonoBehaviour";
            }

            int attachedCount = 0;
            int skippedCount = 0;
            string resultLog = "";

            foreach (GameObject obj in targetObjects)
            {
                if (obj.GetComponent(scriptType) == null)
                {
                    obj.AddComponent(scriptType);
                    attachedCount++;
                    resultLog += $"✓ Attached to '{obj.name}'\n";
                }
                else
                {
                    skippedCount++;
                    resultLog += $"- Skipped '{obj.name}' (already has component)\n";
                }
            }

            Debug.Log($"Batch script attachment completed: {attachedCount} attached, {skippedCount} skipped");
            return $"SUCCESS: Attached '{scriptClassName}' to {attachedCount} GameObjects with tag '{tag}'\n" +
                   $"Skipped: {skippedCount} (already had component)\n\nDetails:\n{resultLog}";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to batch attach script: {e.Message}");
            throw;
        }
    }

    [McpServerTool, Description("List all available MonoBehaviour scripts in the project")]
    public async ValueTask<string> ListAvailableScripts()
    {
        try
        {
            await UniTask.SwitchToMainThread();

            var monoBehaviourScripts = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(MonoBehaviour)) && !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToArray();

            if (monoBehaviourScripts.Length == 0)
            {
                return "No MonoBehaviour scripts found in the project";
            }

            string scriptList = "Available MonoBehaviour scripts:\n";
            foreach (var script in monoBehaviourScripts)
            {
                scriptList += $"- {script.Name}\n";
            }

            Debug.Log($"Found {monoBehaviourScripts.Length} MonoBehaviour scripts");
            return scriptList.Trim();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to list scripts: {e.Message}");
            throw;
        }
    }

    [McpServerTool, Description("Remove a specific component from a GameObject")]
    public async ValueTask<string> RemoveScriptFromObject(
        [Description("Target GameObject name")] string objectName,
        [Description("Script class name to remove")] string scriptClassName)
    {
        try
        {
            await UniTask.SwitchToMainThread();

            // Find the target GameObject
            GameObject targetObject = GameObject.Find(objectName);
            if (targetObject == null)
            {
                return $"ERROR: GameObject '{objectName}' not found in scene";
            }

            // Find the script type
            var scriptType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == scriptClassName);

            if (scriptType == null)
            {
                return $"ERROR: Script class '{scriptClassName}' not found";
            }

            // Find and remove the component
            var component = targetObject.GetComponent(scriptType);
            if (component == null)
            {
                return $"WARNING: '{scriptClassName}' not found on GameObject '{objectName}'";
            }

            UnityEngine.Object.DestroyImmediate(component);

            Debug.Log($"Removed {scriptClassName} from {objectName}");
            return $"SUCCESS: Removed '{scriptClassName}' from GameObject '{objectName}'";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to remove script: {e.Message}");
            throw;
        }
    }

    [McpServerTool, Description("List all GameObjects in the current scene")]
    public async ValueTask<string> ListGameObjects()
    {
        try
        {
            await UniTask.SwitchToMainThread();

            // Find all GameObjects in the scene
            GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID);
            
            if (allObjects.Length == 0)
            {
                return "No GameObjects found in the current scene";
            }

            string objectList = $"GameObjects in scene ({allObjects.Length} total):\n";
            foreach (GameObject obj in allObjects.OrderBy(o => o.name))
            {
                var components = obj.GetComponents<MonoBehaviour>();
                string componentInfo = components.Length > 0 
                    ? $" [Scripts: {string.Join(", ", components.Select(c => c.GetType().Name))}]"
                    : " [No scripts]";
                
                objectList += $"- {obj.name}{componentInfo}\n";
            }

            return objectList.Trim();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to list GameObjects: {e.Message}");
            throw;
        }
    }
}