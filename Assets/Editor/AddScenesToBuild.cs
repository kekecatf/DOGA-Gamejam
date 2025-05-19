#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class AddScenesToBuild : MonoBehaviour
{
    [MenuItem("Tools/Add All Scenes To Build Settings")]
    public static void AddAllScenesToBuildSettings()
    {
        // Sahneleri tutan liste
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        HashSet<string> existingScenePaths = new HashSet<string>();
        
        // Mevcut sahneleri kontrol et
        foreach (var scene in scenes)
        {
            existingScenePaths.Add(scene.path);
        }
        
        // Assets/Scenes klasöründeki tüm sahneleri bul
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        bool settingsChanged = false;
        
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            
            // Eğer sahne zaten Build Settings'de değilse ekle
            if (!existingScenePaths.Contains(scenePath))
            {
                Debug.Log($"Build Settings'e ekleniyor: {scenePath}");
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                settingsChanged = true;
            }
        }
        
        // Değişiklik varsa Build Settings'i güncelle
        if (settingsChanged)
        {
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Build Settings güncellendi. Tüm sahneler eklendi.");
        }
        else
        {
            Debug.Log("Tüm sahneler zaten Build Settings'de mevcut.");
        }
    }
    
    [MenuItem("Tools/Add MiniOyun To Build Settings")]
    public static void AddMiniOyunToBuildSettings()
    {
        // Sahneleri tutan liste
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        HashSet<string> existingScenePaths = new HashSet<string>();
        
        // Mevcut sahneleri kontrol et
        foreach (var scene in scenes)
        {
            existingScenePaths.Add(scene.path);
        }
        
        // MiniOyun sahnesinin yolu
        string miniOyunPath = "Assets/Scenes/MiniOyun.unity";
        
        // Eğer sahne varsa ve Build Settings'de değilse ekle
        if (File.Exists(miniOyunPath) && !existingScenePaths.Contains(miniOyunPath))
        {
            Debug.Log($"Build Settings'e ekleniyor: {miniOyunPath}");
            scenes.Add(new EditorBuildSettingsScene(miniOyunPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("MiniOyun sahnesi Build Settings'e eklendi.");
        }
        else if (existingScenePaths.Contains(miniOyunPath))
        {
            Debug.Log("MiniOyun sahnesi zaten Build Settings'de mevcut.");
        }
        else
        {
            Debug.LogError($"MiniOyun sahnesi bulunamadı: {miniOyunPath}");
        }
    }
}
#endif 