using UnityEngine;
using System;

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
        
        // GameManager oluştur
        CheckOrCreateManager<GameManager>("GameManager");
        
        // Wave Manager (EnemySpawner) oluştur
        CheckOrCreateManager<EnemySpawner>("EnemySpawner");
        
        Debug.Log("GameInitializer: Tüm oyun yöneticileri başlatıldı.");
    }
    
    // Generic manager oluşturma yardımcı fonksiyonu
    private T CheckOrCreateManager<T>(string managerName) where T : MonoBehaviour
    {
        T manager = FindObjectOfType<T>();
        
        if (manager == null)
        {
            GameObject managerObj = new GameObject(managerName);
            manager = managerObj.AddComponent<T>();
            Debug.Log($"GameInitializer: {managerName} oluşturuldu.");
        }
        
        return manager;
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