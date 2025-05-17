using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerDataUI : MonoBehaviour
{
    // PlayerData referansı
    private PlayerData playerData;
    
    // UI Paneli
    public GameObject uiPanel;
    
    // Para göstergeleri
    public Text metalParaText;
    
    // Zeplin geliştirmeleri için UI elemanları
    [Header("Zeplin Geliştirmeleri")]
    public Text zeplinSaglikText;
    public Text zeplinMinigunLevelText;
    public Text zeplinMinigunDamageText;
    public Text zeplinRoketLevelText;
    public Text zeplinRoketDamageText;
    
    // Ana Gemi geliştirmeleri için UI elemanları
    [Header("Ana Gemi Geliştirmeleri")]
    public Text anaGemiSaglikText;
    public Text anaGemiMinigunLevelText;
    public Text anaGemiMinigunDamageText;
    public Text anaGemiRoketLevelText;
    public Text anaGemiRoketDamageText;
    
    // Düşman ayarları için UI elemanları
    [Header("Düşman Ayarları")]
    public Text enemyHealthText;
    public Text enemyDamageText;
    public Text enemyScoreValueText;
    public Text enemyDifficultyText;
    
    // Yükseltme butonları
    [Header("Yükseltme Butonları")]
    public Button zeplinSaglikButton;
    public Button zeplinMinigunButton;
    public Button zeplinRoketButton;
    public Button anaGemiSaglikButton;
    public Button anaGemiMinigunButton;
    public Button anaGemiRoketButton;
    
    // Düşman ayarları butonları
    public Button increaseEnemyHealthButton;
    public Button decreaseEnemyHealthButton;
    public Button increaseEnemyDamageButton;
    public Button decreaseEnemyDamageButton;
    
    // Yükseltme maliyetleri
    private Dictionary<string, int> upgradeCosts = new Dictionary<string, int>()
    {
        {"zeplinSaglik", 100},
        {"zeplinMinigun", 150},
        {"zeplinRoket", 200},
        {"anaGemiSaglik", 100},
        {"anaGemiMinigun", 150},
        {"anaGemiRoket", 200}
    };
    
    void Start()
    {
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("PlayerData bulunamadı!");
            return;
        }
        
        // Butonlara listener'lar ekle
        SetupButtons();
        
        // UI'ı güncelle
        UpdateUI();
    }
    
    void Update()
    {
        // Her frame'de UI'ı güncelle (performans için daha az sıklıkta yapılabilir)
        UpdateUI();
        
        // Tab tuşu ile panel görünürlüğünü değiştir
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePanel();
        }
    }
    
    // UI Panelini aç/kapat
    public void TogglePanel()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(!uiPanel.activeSelf);
        }
    }
    
    // UI elemanlarını güncelle
    void UpdateUI()
    {
        if (playerData == null) return;
        
        // Para bilgisini güncelle
        if (metalParaText != null)
        {
            metalParaText.text = "Metal Para: " + playerData.metalPara;
        }
        
        // Zeplin bilgilerini güncelle
        if (zeplinSaglikText != null) zeplinSaglikText.text = "Sağlık: " + playerData.zeplinSaglik;
        if (zeplinMinigunLevelText != null) zeplinMinigunLevelText.text = "Minigun Seviye: " + playerData.zeplinMinigunLevel;
        if (zeplinMinigunDamageText != null) zeplinMinigunDamageText.text = "Hasar: " + playerData.zeplinMinigunDamage;
        if (zeplinRoketLevelText != null) zeplinRoketLevelText.text = "Roket Seviye: " + playerData.zeplinRoketLevel;
        if (zeplinRoketDamageText != null) zeplinRoketDamageText.text = "Hasar: " + playerData.zeplinRoketDamage;
        
        // Ana Gemi bilgilerini güncelle
        if (anaGemiSaglikText != null) anaGemiSaglikText.text = "Sağlık: " + playerData.anaGemiSaglik;
        if (anaGemiMinigunLevelText != null) anaGemiMinigunLevelText.text = "Minigun Seviye: " + playerData.anaGemiMinigunLevel;
        if (anaGemiMinigunDamageText != null) anaGemiMinigunDamageText.text = "Hasar: " + playerData.anaGemiMinigunDamage;
        if (anaGemiRoketLevelText != null) anaGemiRoketLevelText.text = "Roket Seviye: " + playerData.anaGemiRoketLevel;
        if (anaGemiRoketDamageText != null) anaGemiRoketDamageText.text = "Hasar: " + playerData.anaGemiRoketDamage;
        
        // Düşman bilgilerini güncelle
        if (enemyHealthText != null) enemyHealthText.text = "Düşman Sağlık: " + playerData.enemyBaseHealth;
        if (enemyDamageText != null) enemyDamageText.text = "Düşman Hasar: " + playerData.enemyBaseDamage;
        if (enemyScoreValueText != null) enemyScoreValueText.text = "Ödül Değeri: " + playerData.enemyBaseScoreValue;
        if (enemyDifficultyText != null) enemyDifficultyText.text = "Zorluk Çarpanı: " + playerData.enemyDifficultyMultiplier.ToString("F2");
        
        // Butonların interaktif durumunu güncelle (yeterli para varsa aktif)
        UpdateButtonInteractivity();
    }
    
    // Butonların interaktif durumunu güncelle
    void UpdateButtonInteractivity()
    {
        if (zeplinSaglikButton != null) 
            zeplinSaglikButton.interactable = playerData.metalPara >= upgradeCosts["zeplinSaglik"];
            
        if (zeplinMinigunButton != null) 
            zeplinMinigunButton.interactable = playerData.metalPara >= upgradeCosts["zeplinMinigun"];
            
        if (zeplinRoketButton != null) 
            zeplinRoketButton.interactable = playerData.metalPara >= upgradeCosts["zeplinRoket"];
            
        if (anaGemiSaglikButton != null) 
            anaGemiSaglikButton.interactable = playerData.metalPara >= upgradeCosts["anaGemiSaglik"];
            
        if (anaGemiMinigunButton != null) 
            anaGemiMinigunButton.interactable = playerData.metalPara >= upgradeCosts["anaGemiMinigun"];
            
        if (anaGemiRoketButton != null) 
            anaGemiRoketButton.interactable = playerData.metalPara >= upgradeCosts["anaGemiRoket"];
    }
    
    // Butonlara listener'lar ekle
    void SetupButtons()
    {
        if (zeplinSaglikButton != null)
            zeplinSaglikButton.onClick.AddListener(() => UpgradeZeplinSaglik());
            
        if (zeplinMinigunButton != null)
            zeplinMinigunButton.onClick.AddListener(() => UpgradeZeplinMinigun());
            
        if (zeplinRoketButton != null)
            zeplinRoketButton.onClick.AddListener(() => UpgradeZeplinRoket());
            
        if (anaGemiSaglikButton != null)
            anaGemiSaglikButton.onClick.AddListener(() => UpgradeAnaGemiSaglik());
            
        if (anaGemiMinigunButton != null)
            anaGemiMinigunButton.onClick.AddListener(() => UpgradeAnaGemiMinigun());
            
        if (anaGemiRoketButton != null)
            anaGemiRoketButton.onClick.AddListener(() => UpgradeAnaGemiRoket());
            
        // Düşman ayarları butonlarını ayarla
        if (increaseEnemyHealthButton != null)
            increaseEnemyHealthButton.onClick.AddListener(() => AdjustEnemyHealth(10));
            
        if (decreaseEnemyHealthButton != null)
            decreaseEnemyHealthButton.onClick.AddListener(() => AdjustEnemyHealth(-10));
            
        if (increaseEnemyDamageButton != null)
            increaseEnemyDamageButton.onClick.AddListener(() => AdjustEnemyDamage(5));
            
        if (decreaseEnemyDamageButton != null)
            decreaseEnemyDamageButton.onClick.AddListener(() => AdjustEnemyDamage(-5));
    }
    
    // Yükseltme metotları
    public void UpgradeZeplinSaglik()
    {
        if (playerData.metalPara >= upgradeCosts["zeplinSaglik"])
        {
            playerData.metalPara -= upgradeCosts["zeplinSaglik"];
            playerData.zeplinSaglikLevel++;
            playerData.zeplinSaglik += 20; // Her seviyede 20 sağlık artışı
            Debug.Log("Zeplin sağlığı yükseltildi! Yeni seviye: " + playerData.zeplinSaglikLevel);
            
            // Yükseltme maliyetini arttır
            upgradeCosts["zeplinSaglik"] += 50;
        }
    }
    
    public void UpgradeZeplinMinigun()
    {
        if (playerData.metalPara >= upgradeCosts["zeplinMinigun"])
        {
            playerData.metalPara -= upgradeCosts["zeplinMinigun"];
            playerData.zeplinMinigunLevel++;
            playerData.zeplinMinigunDamage += 5; // Her seviyede 5 hasar artışı
            Debug.Log("Zeplin minigun yükseltildi! Yeni seviye: " + playerData.zeplinMinigunLevel);
            
            // Yükseltme maliyetini arttır
            upgradeCosts["zeplinMinigun"] += 75;
            
            // Düşman zorluğunu güncelle
            playerData.UpdateEnemyDifficulty();
        }
    }
    
    public void UpgradeZeplinRoket()
    {
        if (playerData.metalPara >= upgradeCosts["zeplinRoket"])
        {
            playerData.metalPara -= upgradeCosts["zeplinRoket"];
            playerData.zeplinRoketLevel++;
            playerData.zeplinRoketDamage += 10; // Her seviyede 10 hasar artışı
            Debug.Log("Zeplin roket yükseltildi! Yeni seviye: " + playerData.zeplinRoketLevel);
            
            // Yükseltme maliyetini arttır
            upgradeCosts["zeplinRoket"] += 100;
            
            // Düşman zorluğunu güncelle
            playerData.UpdateEnemyDifficulty();
        }
    }
    
    public void UpgradeAnaGemiSaglik()
    {
        if (playerData.metalPara >= upgradeCosts["anaGemiSaglik"])
        {
            playerData.metalPara -= upgradeCosts["anaGemiSaglik"];
            playerData.anaGemiSaglikLevel++;
            playerData.anaGemiSaglik += 20; // Her seviyede 20 sağlık artışı
            Debug.Log("Ana gemi sağlığı yükseltildi! Yeni seviye: " + playerData.anaGemiSaglikLevel);
            
            // Yükseltme maliyetini arttır
            upgradeCosts["anaGemiSaglik"] += 50;
        }
    }
    
    public void UpgradeAnaGemiMinigun()
    {
        if (playerData.metalPara >= upgradeCosts["anaGemiMinigun"])
        {
            playerData.metalPara -= upgradeCosts["anaGemiMinigun"];
            playerData.anaGemiMinigunLevel++;
            playerData.anaGemiMinigunDamage += 5; // Her seviyede 5 hasar artışı
            Debug.Log("Ana gemi minigun yükseltildi! Yeni seviye: " + playerData.anaGemiMinigunLevel);
            
            // Yükseltme maliyetini arttır
            upgradeCosts["anaGemiMinigun"] += 75;
            
            // Düşman zorluğunu güncelle
            playerData.UpdateEnemyDifficulty();
        }
    }
    
    public void UpgradeAnaGemiRoket()
    {
        if (playerData.metalPara >= upgradeCosts["anaGemiRoket"])
        {
            playerData.metalPara -= upgradeCosts["anaGemiRoket"];
            playerData.anaGemiRoketLevel++;
            playerData.anaGemiRoketDamage += 10; // Her seviyede 10 hasar artışı
            playerData.anaGemiRoketSpeed += 2f; // Her seviyede 2 hız artışı
            
            Debug.Log("Ana gemi roket yükseltildi! Yeni seviye: " + playerData.anaGemiRoketLevel);
            
            // Yükseltme maliyetini arttır
            upgradeCosts["anaGemiRoket"] += 100;
            
            // Düşman zorluğunu güncelle
            playerData.UpdateEnemyDifficulty();
        }
    }
    
    // Düşman sağlığını ayarla
    public void AdjustEnemyHealth(int amount)
    {
        playerData.enemyBaseHealth += amount;
        
        // Minimum değer kontrolü
        if (playerData.enemyBaseHealth < 10)
            playerData.enemyBaseHealth = 10;
            
        Debug.Log("Düşman sağlığı ayarlandı: " + playerData.enemyBaseHealth);
    }
    
    // Düşman hasarını ayarla
    public void AdjustEnemyDamage(int amount)
    {
        playerData.enemyBaseDamage += amount;
        
        // Minimum değer kontrolü
        if (playerData.enemyBaseDamage < 5)
            playerData.enemyBaseDamage = 5;
            
        Debug.Log("Düşman hasarı ayarlandı: " + playerData.enemyBaseDamage);
    }
    
    // Test için para ekleme metodu
    public void AddMoney(int amount)
    {
        playerData.metalPara += amount;
        Debug.Log("Para eklendi! Yeni miktar: " + playerData.metalPara);
    }
} 