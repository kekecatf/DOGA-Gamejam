using UnityEngine;
using System;
using System.Collections;

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
        
        // Önce PlayerData oluştur (GameManager buna bağlı)
        PlayerData playerData = CheckOrCreateManager<PlayerData>("PlayerData");
        if (playerData == null)
        {
            Debug.LogError("GameInitializer: PlayerData oluşturulamadı! Diğer yöneticiler oluşturulmayacak.");
            return;
        }
        
        // PlayerData oluşturulduktan sonra kısa bir bekleme ekle
        // Bu, diğer bileşenlerin PlayerData'nın varlığını algılamasına yardımcı olur
        StartCoroutine(DelayedInitialization());
    }
    
    private System.Collections.IEnumerator DelayedInitialization()
    {
        // Kısa bir bekleme süresi (Tek bir frame bekle)
        yield return null;
        
        // GameManager oluştur - PlayerData nesnesinin varlığını kontrol et
        GameManager gameManager = CheckOrCreateManager<GameManager>("GameManager");
        if (gameManager == null)
        {
            Debug.LogError("GameInitializer: GameManager oluşturulamadı!");
            yield break;
        }
        
        // PlayerData'nın GameManager tarafından bulunduğundan emin ol
        if (PlayerData.Instance == null)
        {
            Debug.LogWarning("GameInitializer: PlayerData singleton bulunamadı, manuel olarak GameManager'a atanacak.");
            gameManager.GetComponentInChildren<GameManager>().TryInitializePlayerData();
        }
        
        // Wave Manager (EnemySpawner) oluştur
        EnemySpawner enemySpawner = CheckOrCreateManager<EnemySpawner>("EnemySpawner");
        
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
            
            // Eğer oluşturulan bileşen PlayerData ise, sadece bir onaylama mesajı göster
            // Artık sabit değerler atamıyoruz
            if (typeof(T) == typeof(PlayerData))
            {
                PlayerData playerData = manager as PlayerData;
                if (playerData != null)
                {
                    Debug.Log($"GameInitializer: PlayerData oluşturuldu, Inspector'dan değerleri ayarlayabilirsiniz.");
                }
            }
            
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