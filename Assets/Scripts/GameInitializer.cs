using UnityEngine;

// GameInitializer - Oyun başlatıcı script
// Bu script oyun başladığında gerekli yöneticileri ve bileşenleri oluşturur
public class GameInitializer : MonoBehaviour
{
    private static GameInitializer _instance;
    
    // Singleton oluşturma
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Tüm yöneticileri başlat
    private void InitializeManagers()
    {
        Debug.Log("GameInitializer: Oyun yöneticileri başlatılıyor...");
        
        // RocketManager'ı başlat
        InitializeRocketManager();
        
        // Diğer yöneticiler buraya eklenebilir
    }
    
    // RocketManager başlatma
    private void InitializeRocketManager()
    {
        GameObject rocketManagerObj = new GameObject("RocketManager");
        RocketManager rocketManager = rocketManagerObj.AddComponent<RocketManager>();
        Debug.Log("GameInitializer: RocketManager başlatıldı.");
        
        // Manuel olarak roket atama işlemini başlat
        rocketManager.AssignRocketPrefabToEnemies();
    }
    
    // GameInitializer'ı başlatmak için statik metot
    // Bu, herhangi bir yerden çağrılabilir
    public static void Initialize()
    {
        if (_instance == null)
        {
            GameObject initializerObj = new GameObject("GameInitializer");
            initializerObj.AddComponent<GameInitializer>();
            Debug.Log("GameInitializer: Oyun başlatıcı oluşturuldu.");
        }
    }
} 