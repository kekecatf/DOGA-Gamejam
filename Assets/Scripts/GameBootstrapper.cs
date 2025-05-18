using UnityEngine;

// GameBootstrapper - Unity başladığında otomatik çalışan sınıf
// Bu sınıf, [RuntimeInitializeOnLoadMethod] attribute'u sayesinde
// MonoBehaviour olmadan ve herhangi bir GameObject'e eklenmeden otomatik çalıştırılır
public static class GameBootstrapper
{
    // Unity başladığında otomatik çalışan metot
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Debug.Log("GameBootstrapper: Oyun başlatılıyor...");
        GameInitializer.Initialize();
    }
} 