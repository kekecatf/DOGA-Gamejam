using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Singleton yapısı
    public static GameManager Instance { get; private set; }
    
    // Oyun istatistikleri
    public int killedEnemyCount = 0;
    public float gameTime = 0f;
    public bool isGameOver = false;
    
    // Sahne adları
    public string gameSceneName = "GameScene";
    public string gameOverSceneName = "OyunSonu";
    
    // Scene değişimi kontrolü
    private bool isFirstLoad = true;
    
    private void Awake()
    {
        // Singleton yapısı
        if (Instance == null)
        {
            Instance = this;
            
            // Bu nesneyi sahne değişimlerinde korumak için
            DontDestroyOnLoad(gameObject);
            
            // PersistentObject tag'i ekle - yeniden başlatma için gerekli
            gameObject.tag = "PersistentObject";
            
            Debug.Log("GameManager oluşturuldu. Veriler sıfırlanıyor...");
            ResetGameStats();
            
            // PlayerData değerlerini güncelleme metodu
            UpdatePlayerDataValues();
        }
        else
        {
            Debug.Log("Mevcut bir GameManager zaten var. Bu kopya yok ediliyor.");
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Oyun başlangıç ayarları
        ResetGameStats();
        
        // Sahne değişim olayını dinle
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // PlayerData değerlerini güncelleme metodu
        UpdatePlayerDataValues();
    }
    
    private void OnDestroy()
    {
        // Olay dinleyiciyi kaldır
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // Sahne değiştiğinde çağrılır
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // GameScene sahnesine geçiş yaptıysa verileri sıfırla
        if (scene.name == gameSceneName && !isFirstLoad)
        {
            StartCoroutine(InitializeGameSceneDelayed());
        }
        
        if (isFirstLoad)
        {
            isFirstLoad = false;
        }
    }
    
    // Sahne yüklendikten kısa bir süre sonra oyunu başlatmayı sağlar
    private IEnumerator InitializeGameSceneDelayed()
    {
        // Sahnenin tam olarak yüklenmesi için kısa bir bekleme
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log("GameScene yüklendi. Oyun yeniden başlatılıyor...");
        
        // Oyun verilerini sıfırla
        ResetGameStats();
        
        // Oyun içi değişkenleri sıfırla
        Player.isDead = false;
        
        // Sahne objeleri ile etkileşim için hazır
        Debug.Log("Oyun sıfırlama tamamlandı.");
    }
    
    private void Update()
    {
        if (!isGameOver)
        {
            // Oyun devam ederken zamanı say
            gameTime += Time.deltaTime;
        }
    }
    
    // Düşman öldürüldüğünde çağrılacak
    public void EnemyKilled(EnemyType enemyType = EnemyType.Kamikaze)
    {
        killedEnemyCount++;
        Debug.Log($"Düşman öldürüldü! Toplam öldürülen düşman sayısı: {killedEnemyCount}");
    }
    
    // Zeplin öldüğünde çağrılacak
    public void GameOver()
    {
        isGameOver = true;
        Debug.Log("Oyun Bitti! Öldürülen düşman sayısı: " + killedEnemyCount);
        
        // Oyun sonu sahnesine geçiş
        SceneManager.LoadScene(gameOverSceneName);
    }
    
    // Oyun istatistiklerini sıfırla
    public void ResetGameStats()
    {
        killedEnemyCount = 0;
        gameTime = 0f;
        isGameOver = false;
        
        Debug.Log("Oyun istatistikleri sıfırlandı.");
    }
    
    // Oyunu yeniden başlat
    public void RestartGame()
    {
        Debug.Log("GameManager.RestartGame() çağrıldı.");
        
        // Kalıcı veriler güvenli bir şekilde temizleniyor
        StartCoroutine(SafeRestart());
    }
    
    // Güvenli bir şekilde yeniden başlatmak için 
    private IEnumerator SafeRestart()
    {
        // Oyun verilerini sıfırla
        ResetGameStats();
        
        // Beklemeden hemen sahneyi yükle (DontDestroyOnLoad nesneleri GameOverUI tarafından temizlenir)
        SceneManager.LoadScene(gameSceneName);
        
        yield return null;
    }
    
    // Ana menüye dön
    public void GoToMainMenu()
    {
        ResetGameStats();
        SceneManager.LoadScene("AnaMenu");
    }
    
    // PlayerData değerlerini güncelleme metodu
    public void UpdatePlayerDataValues()
    {
        PlayerData playerData = FindObjectOfType<PlayerData>();
        if (playerData != null)
        {
            // Not: Artık ApplyBalancedValues çağrılmıyor
            // Bu sayede Inspector'dan ayarlanan değerler korunuyor
            Debug.Log("GameManager: PlayerData bulundu, değerler korunuyor.");
        }
        else
        {
            Debug.LogError("PlayerData bulunamadı!");
            TryInitializePlayerData();
        }
    }
    
    // PlayerData'nın eksik olması durumunda yeniden oluşturma denemesi
    public void TryInitializePlayerData()
    {
        if (FindObjectOfType<PlayerData>() == null)
        {
            Debug.Log("GameManager: PlayerData yeniden oluşturuluyor...");
            GameObject playerDataObj = new GameObject("PlayerData");
            PlayerData newPlayerData = playerDataObj.AddComponent<PlayerData>();
            
            // Artık sabit bir değer atamıyoruz, varsayılan değerler korunacak
            
            DontDestroyOnLoad(playerDataObj);
            
            if (newPlayerData != null)
            {
                Debug.Log("GameManager: PlayerData başarıyla yeniden oluşturuldu.");
                Debug.Log("GameManager: PlayerData varsayılan değerlerle oluşturuldu.");
                UpdatePlayerDataValues();
            }
            else
            {
                Debug.LogError("GameManager: PlayerData oluşturma başarısız oldu!");
            }
        }
    }
} 