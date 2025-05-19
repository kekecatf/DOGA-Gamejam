using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    // Başlangıç değerleri - Sıfırlamalar için kullanılacak
    private const int DEFAULT_METAL_PARA = 500;
    private const int DEFAULT_ZEPLIN_SAGLIK = 1000;
    private const int DEFAULT_ANA_GEMI_SAGLIK = 100;
    
    // Mevcut değerler
    public int metalPara = 500;

    [Header("Zeplin Ayarları")]
    public int zeplinSaglik = 1000;
    public int zeplinSaglikLevel = 0;

    public int zeplinMinigunDamage = 10;
    public int zeplinMinigunLevel = 0;
    public float zeplinMinigunCooldown = 1.0f;
    public int zeplinMinigunCount = 1;

    public int zeplinRoketDamage = 20;
    public int zeplinRoketLevel = 0;
    public int zeplinRoketCount = 1;
    public float zeplinRoketDelay = 2.0f;

    [Header("Oyuncu Ayarları")]
    public int anaGemiSaglik = 100;
    public int anaGemiSaglikLevel = 0;

    public int anaGemiMinigunDamage = 5;
    public int anaGemiMinigunLevel = 0;
    public float anaGemiMinigunCooldown = 0.4f;
    public int anaGemiMinigunCount = 1;

    public int anaGemiRoketDamage = 20;
    public int anaGemiRoketLevel = 0;
    public int anaGemiRoketCount = 1;
    public float anaGemiRoketDelay = 2.0f;
    public float anaGemiRoketSpeed = 10.0f;

    // Düşman ayarları
    [Header("Düşman Ayarları")]
    public int enemyBaseHealth = 50;
    public int enemyBaseDamage = 10;
    public int enemyBaseScoreValue = 25;
    public float enemyDifficultyMultiplier = 1.0f;
    
    // Düşman tipleri için hasarlar
    [Header("Düşman Hasar Ayarları")]
    public int enemyKamikazeDamage = 10;  // Kamikaze düşmanların verdiği hasar
    public int enemyMinigunDamage = 3;    // Minigun düşmanların mermi başına verdiği hasar
    public int enemyRocketDamage = 20;    // Roket düşmanların roket başına verdiği hasar
    
    public float enemyKamikazeDamageMultiplier = 0.8f;  // Kamikaze hasar çarpanı
    public float enemyMinigunDamageMultiplier = 0.3f;   // Minigun hasar çarpanı
    public float enemyRocketDamageMultiplier = 1.5f;    // Roket hasar çarpanı
    
    [Header("Düşman Atış Hızı Ayarları")]
    public float enemyMinigunFireRate = 2.0f;     // Minigun düşmanlarının temel atış hızı (saniyede atış sayısı)
    public float enemyRocketFireRate = 1.0f;      // Roket düşmanlarının temel atış hızı (saniyede atış sayısı)
    public float enemyFireRateMultiplier = 1.0f;  // Tüm düşmanlar için atış hızı çarpanı

    // Singleton yapısı
    public static PlayerData Instance { get; private set; }

    // Oyun mekanikleri için kontrol değişkenleri
    [Header("Oyun Mekanikleri")]
    public bool isPlayerRespawned = false; // Oyuncu canlandı mı? (ilk ölümden sonra MiniGame başarılı olduysa)

    private void Awake()
    {
        // Singleton kontrol
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Identify as a persistent object for restart functionality
            gameObject.tag = "PersistentObject";
            
            // Hatalı PlayerPrefs değerlerini temizleme (bir defalık)
            ResetPlayerPrefs();
            
            // İlk başlangıçta verileri sıfırlama işlemi iptal edildi
            // Inspector'daki değerlerin korunması için ResetAllData çağrılmıyor
            
            // Kaydedilmiş değerleri yükle (varsa)
            LoadSavedValues();
            
            Debug.Log("PlayerData Singleton oluşturuldu. Inspector'daki mevcut değerler korunuyor.");
        }
        else
        {
            // Zaten bir PlayerData var, bu nesneyi yok et
            Debug.Log("Mevcut bir PlayerData zaten var. Bu kopya yok ediliyor.");
            Destroy(gameObject);
        }
    }
    
    // Hatalı PlayerPrefs değerlerini sıfırla
    private void ResetPlayerPrefs()
    {
        // PlayerPrefs'te mevcut bir değer varsa ve değer anormal ise
        if (PlayerPrefs.HasKey("zeplinSaglik"))
        {
            int savedHealth = PlayerPrefs.GetInt("zeplinSaglik");
            
            // Değer negatif veya çok düşükse
            if (savedHealth < 0 || savedHealth == -32) // Özellikle -32 değeri kontrol ediliyor
            {
                Debug.LogWarning($"Hatalı zeplinSaglik değeri ({savedHealth}) temizleniyor ve varsayılan değer ({DEFAULT_ZEPLIN_SAGLIK}) ayarlanıyor.");
                PlayerPrefs.DeleteKey("zeplinSaglik");
                PlayerPrefs.SetInt("zeplinSaglik", DEFAULT_ZEPLIN_SAGLIK);
                PlayerPrefs.Save();
            }
        }
    }
    
    // Kaydedilmiş değerleri PlayerPrefs'ten yükle
    private void LoadSavedValues()
    {
        // İlk kez mi çalışıyor kontrolü
        bool isFirstRun = !PlayerPrefs.HasKey("zeplinSaglik");
        
        // IMPORTANT: Force zeplinSaglik to always be 1000
        // This ensures consistent behavior regardless of PlayerPrefs
        zeplinSaglik = 1000;
        
        // Save this value to PlayerPrefs for consistency
        PlayerPrefs.SetInt("zeplinSaglik", 1000);
        PlayerPrefs.Save();
        
        Debug.Log("PlayerData: zeplinSaglik değeri her zaman 1000 olarak ayarlandı!");
        
        // NOTE: We're skipping the usual PlayerPrefs loading logic for zeplinSaglik
        
        // Diğer değerleri de ihtiyaca göre buraya ekleyebilirsiniz
    }
    
    // Değerleri PlayerPrefs'e kaydet
    public void SaveValues()
    {
        // Negatif değer kontrolü
        if (zeplinSaglik <= 0)
        {
            Debug.LogWarning($"Negatif zeplinSaglik değeri ({zeplinSaglik}) tespit edildi. Varsayılan değer ({DEFAULT_ZEPLIN_SAGLIK}) kaydediliyor.");
            zeplinSaglik = DEFAULT_ZEPLIN_SAGLIK;
        }
        
        // zeplinSaglik değerini kaydet
        PlayerPrefs.SetInt("zeplinSaglik", zeplinSaglik);
        
        // Diğer değerleri de ihtiyaca göre buraya ekleyebilirsiniz
        
        // Değişiklikleri kaydet
        PlayerPrefs.Save();
        Debug.Log($"PlayerData: Değerler kaydedildi. zeplinSaglik = {zeplinSaglik}");
    }

    // Tüm verileri sıfırla (yeniden başlatmada kullanılır)
    public void ResetAllData()
    {
        // Para ve seviye sıfırlama
        metalPara = DEFAULT_METAL_PARA;
        
        // Zeplin değerleri sıfırlama
        // NOT: zeplinSaglik değeri korunacak şekilde değiştirildi
        // zeplinSaglik = DEFAULT_ZEPLIN_SAGLIK; // Bu satır artık çalıştırılmayacak
        zeplinSaglikLevel = 0;
        zeplinMinigunDamage = 10;
        zeplinMinigunLevel = 0;
        zeplinMinigunCooldown = 1.0f;
        zeplinMinigunCount = 1;
        zeplinRoketDamage = 20;
        zeplinRoketLevel = 0;
        zeplinRoketCount = 1;
        zeplinRoketDelay = 2.0f;
        
        // Ana gemi değerleri sıfırlama
        anaGemiSaglik = DEFAULT_ANA_GEMI_SAGLIK;
        anaGemiSaglikLevel = 0;
        anaGemiMinigunDamage = 5;
        anaGemiMinigunLevel = 0;
        anaGemiMinigunCooldown = 0.4f;
        anaGemiMinigunCount = 1;
        anaGemiRoketDamage = 20;
        anaGemiRoketLevel = 0;
        anaGemiRoketCount = 1;
        anaGemiRoketDelay = 2.0f;
        anaGemiRoketSpeed = 10.0f;
        
        // Düşman zorluğunu başlangıç değerine sıfırla
        enemyDifficultyMultiplier = 1.0f;
        enemyFireRateMultiplier = 1.0f;
        
        // Düşman hasar değerleri - kamikaze ve diğer düşmanlar için hasar ayarları
        enemyKamikazeDamage = 10;
        enemyMinigunDamage = 3;
        enemyRocketDamage = 20;
        enemyKamikazeDamageMultiplier = 0.8f;
        enemyMinigunDamageMultiplier = 0.3f;
        enemyRocketDamageMultiplier = 1.5f;
        
        Debug.Log("PlayerData tüm değerler sıfırlandı (zeplinSaglik hariç)!");
    }

    // İsteğe bağlı olarak çağrılabilecek bir özellik değerlerini yeniden ayarlama fonksiyonu ekleyelim
    public void ApplyBalancedValues()
    {
        // NOT: Inspector'da ayarlanan değerlerin otomatik olarak sıfırlanması önlendi
        // Sadece manuel olarak ResetAllData() çağrıldığında değerler sıfırlanacak
        
        // ApplyBalancedValues artık değerleri değiştirmiyor,
        // konfigürasyon için sadece GameBalancer sınıfı kullanılmalı
        
        Debug.Log("PlayerData: Inspector'dan ayarlanan değerler korundu!");
    }
    
    // Tüm PlayerPrefs değerlerini silmeye yarayan metot
    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerData: Tüm PlayerPrefs değerleri silindi! Oyun yeniden başlatıldığında varsayılan değerler kullanılacak.");
        
        // Varsayılan değeri hemen ayarlayalım ve sağlık değerini düzeltelim
        zeplinSaglik = DEFAULT_ZEPLIN_SAGLIK;
        PlayerPrefs.SetInt("zeplinSaglik", DEFAULT_ZEPLIN_SAGLIK);
        PlayerPrefs.Save();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Düşman zorluğunu oyuncu seviyesine göre hesapla
        UpdateEnemyDifficulty();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Inspector'da değerler değiştiğinde çağrılır
    private void OnValidate()
    {
        // Inspector'da yapılan değişiklikleri kaydet
        if (Application.isPlaying)
        {
            Debug.Log("PlayerData: Inspector üzerinde değişiklik algılandı, değerler kaydediliyor...");
            SaveValues();
        }
    }
    
    // Nesne yok edildiğinde çağrılır (oyun kapanırken veya sahne değişiminde)
    private void OnDestroy()
    {
        // Değerleri kaydet
        SaveValues();
        Debug.Log("PlayerData: Nesne yok edilirken değerler kaydedildi.");
    }

    // Düşman zorluğunu oyuncu seviyesine göre güncelle
    public void UpdateEnemyDifficulty()
    {
        // Oyuncunun silah seviyelerine göre düşman zorluğunu ölçekle
        int playerLevel = Mathf.Max(anaGemiMinigunLevel, anaGemiRoketLevel);
        enemyDifficultyMultiplier = 1f + (playerLevel * 0.2f); // Her seviye için %20 artış
        
        // Atış hızı çarpanını da zorluğa göre ayarla
        enemyFireRateMultiplier = 1f + (playerLevel * 0.1f); // Her seviye için %10 artış
        
        Debug.Log("Düşman zorluğu güncellendi: " + enemyDifficultyMultiplier);
        Debug.Log("Düşman atış hızı çarpanı: " + enemyFireRateMultiplier);
    }
    
    // Düşman sağlığını hesapla
    public int CalculateEnemyHealth()
    {
        return Mathf.RoundToInt(enemyBaseHealth * enemyDifficultyMultiplier);
    }
    
    // Düşman hasarını hesapla
    public int CalculateEnemyDamage()
    {
        return Mathf.RoundToInt(enemyBaseDamage * enemyDifficultyMultiplier);
    }
    
    // Düşman tiplerine göre hasar hesaplama
    public int CalculateKamikazeDamage()
    {
        return Mathf.RoundToInt(enemyKamikazeDamage * enemyDifficultyMultiplier * enemyKamikazeDamageMultiplier);
    }
    
    public int CalculateMinigunDamage()
    {
        return Mathf.RoundToInt(enemyMinigunDamage * enemyDifficultyMultiplier * enemyMinigunDamageMultiplier);
    }
    
    public int CalculateRocketDamage()
    {
        return Mathf.RoundToInt(enemyRocketDamage * enemyDifficultyMultiplier * enemyRocketDamageMultiplier);
    }
    
    // Düşman atış hızlarını hesapla
    public float CalculateMinigunFireRate()
    {
        return enemyMinigunFireRate * enemyFireRateMultiplier;
    }
    
    public float CalculateRocketFireRate()
    {
        return enemyRocketFireRate * enemyFireRateMultiplier;
    }
    
    // Düşman ödül değerini hesapla
    public int CalculateEnemyScoreValue()
    {
        return Mathf.RoundToInt(enemyBaseScoreValue * enemyDifficultyMultiplier);
    }
}