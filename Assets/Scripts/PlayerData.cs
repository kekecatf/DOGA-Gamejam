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
        
        Debug.Log("Düşman zorluğu güncellendi: " + enemyDifficultyMultiplier);
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
    
    // Düşman ödül değerini hesapla
    public int CalculateEnemyScoreValue()
    {
        return Mathf.RoundToInt(enemyBaseScoreValue * enemyDifficultyMultiplier);
    }

    // Ana Gemi Minigun Geliştirme
    
}