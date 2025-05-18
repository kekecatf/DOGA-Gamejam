using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    public int metalPara = 500;

    [Header("Zeplin Ayarları")]
    public int zeplinSaglik = 100;
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

    public int anaGemiMinigunDamage = 10;
    public int anaGemiMinigunLevel = 0;
    public float anaGemiMinigunCooldown = 1.0f;
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

   /* public Button zeplinMinigunButon; // Inspector'dan atayacaksın
    public Button anaGemiMinigunButon;
    public Button anaGemiRoketButon;
    public Button anaGemiSaglikButon;*/

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
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

    // Ana Gemi Minigun Geliştirme
    
}