using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    // Başlangıç değerleri - Sıfırlamalar için kullanılacak
    private const int DEFAULT_METAL_PARA = 500;
    private const int DEFAULT_ZEPLIN_SAGLIK = 100;
    private const int DEFAULT_ANA_GEMI_SAGLIK = 100;
    
    // Mevcut değerler
    public int metalPara = DEFAULT_METAL_PARA;

    [Header("Zeplin Ayarları")]
    public int zeplinSaglik = DEFAULT_ZEPLIN_SAGLIK;
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
    public int anaGemiSaglik = DEFAULT_ANA_GEMI_SAGLIK;
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
    public int enemyKamikazeDamage = 20;  // Kamikaze düşmanların verdiği hasar
    public int enemyMinigunDamage = 5;    // Minigun düşmanların mermi başına verdiği hasar
    public int enemyRocketDamage = 30;    // Roket düşmanların roket başına verdiği hasar
    
    public float enemyKamikazeDamageMultiplier = 2.0f;  // Kamikaze hasar çarpanı
    public float enemyMinigunDamageMultiplier = 0.5f;   // Minigun hasar çarpanı
    public float enemyRocketDamageMultiplier = 3.0f;    // Roket hasar çarpanı
    
    [Header("Düşman Atış Hızı Ayarları")]
    public float enemyMinigunFireRate = 2.0f;     // Minigun düşmanlarının temel atış hızı (saniyede atış sayısı)
    public float enemyRocketFireRate = 1.0f;      // Roket düşmanlarının temel atış hızı (saniyede atış sayısı)
    public float enemyFireRateMultiplier = 1.0f;  // Tüm düşmanlar için atış hızı çarpanı

    // Singleton yapısı
    public static PlayerData Instance { get; private set; }

    private void Awake()
    {
        // Singleton kontrol
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Identify as a persistent object for restart functionality
            gameObject.tag = "PersistentObject";
            
            // İlk başlangıçta verileri sıfırla
            ResetAllData();
        }
        else
        {
            // Zaten bir PlayerData var, bu nesneyi yok et
            Debug.Log("Mevcut bir PlayerData zaten var. Bu kopya yok ediliyor.");
            Destroy(gameObject);
        }
    }

    // Tüm verileri sıfırla (yeniden başlatmada kullanılır)
    public void ResetAllData()
    {
        // Para ve seviye sıfırlama
        metalPara = DEFAULT_METAL_PARA;
        
        // Zeplin değerleri sıfırlama
        zeplinSaglik = DEFAULT_ZEPLIN_SAGLIK;
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
        
        Debug.Log("PlayerData tüm değerler sıfırlandı!");
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