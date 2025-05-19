using UnityEngine;

// This script ensures the ZeplinHealthFixer is added to a GameObject at game startup
[DefaultExecutionOrder(-10000)] // Extremely high priority to run before other scripts
public class ZeplinHealthFixerInitializer : MonoBehaviour
{
    // This will run before any other script
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Debug.Log("ZeplinHealthFixerInitializer: Creating ZeplinHealthFixer");
        
        // Create a new GameObject and add the ZeplinHealthFixer component
        GameObject fixerObject = new GameObject("ZeplinHealthFixer");
        fixerObject.AddComponent<ZeplinHealthFixer>();
        
        // Don't destroy this object when loading new scenes
        DontDestroyOnLoad(fixerObject);
        
        Debug.Log("ZeplinHealthFixerInitializer: ZeplinHealthFixer created");
    }
} 