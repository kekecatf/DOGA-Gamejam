using UnityEngine;

// RocketFixInitializer - Rocket düzeltmelerini oyun başladığında otomatik olarak ekler
public static class RocketFixInitializer
{
    // Unity başladığında otomatik çalışan metot
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        Debug.Log("RocketFixInitializer: Rocket düzeltmeleri başlatılıyor...");
        
        // "RocketFixManager" adında bir GameObject oluştur
        GameObject fixManager = new GameObject("RocketFixManager");
        Object.DontDestroyOnLoad(fixManager); // Sahne değişimlerinde korunur
        
        // RocketFixHelper sınıfını kullanabilmek için bileşenleri ekle
        EnemyRocketFixer fixer = fixManager.AddComponent<EnemyRocketFixer>();
        fixer.enablePatching = true;
        fixer.logDebugInfo = true;
        
        // Test edici ekle (varsayılan olarak devre dışı)
        RocketFixTester tester = fixManager.AddComponent<RocketFixTester>();
        tester.enabled = false;
        
        Debug.Log("RocketFixInitializer: Rocket düzeltmeleri uygulandı.");
        Debug.Log("RocketFixInitializer: 'RocketFixTester' bileşenini Inspector'dan aktif ederek test edebilirsiniz.");
    }
} 