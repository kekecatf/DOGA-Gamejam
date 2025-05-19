using UnityEngine;

// Oyun dengesi için geçici yardımcı sınıf
// Bu bileşeni herhangi bir Game Object'e ekleyebilirsiniz
public class GameBalancer : MonoBehaviour
{
    [Header("Tuş Kontrolleri")]
    public KeyCode balanceKey = KeyCode.B;          // Denge değerlerini uygulamak için tuş
    public KeyCode showValuesKey = KeyCode.I;       // Mevcut değerleri görmek için tuş
    
    [Header("Düşman Hasar Ayarları")]
    public int kamikazeDamage = 10;
    public int minigunDamage = 3;
    public int rocketDamage = 20;
    public float kamikazeDamageMultiplier = 0.8f;
    public float minigunDamageMultiplier = 0.3f;
    public float rocketDamageMultiplier = 1.5f;
    
    [Header("Otomatik Uygulama")]
    public bool applyOnStart = true;              // Başlangıçta otomatik uygula
    
    private PlayerData playerData;
    
    // Start is called before the first frame update
    void Start()
    {
        ForceZeplinHealth1000();
        
        // Find references
        playerData = FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("GameBalancer: PlayerData bulunamadı!");
            return;
        }
        
        Debug.Log("GameBalancer başlatıldı. Oyun dengesi değerleri uygulandı.");
        ApplyInitialValues();
    }
    
    void Update()
    {
        // B tuşuna basıldığında denge değerlerini uygula
        if (Input.GetKeyDown(balanceKey))
        {
            ApplyBalancedValues();
        }
        
        // I tuşuna basıldığında mevcut değerleri göster
        if (Input.GetKeyDown(showValuesKey))
        {
            ShowCurrentValues();
        }
    }
    
    // Denge değerlerini uygula
    public void ApplyBalancedValues()
    {
        if (playerData != null)
        {
            // Inspector'da ayarlanan değerleri PlayerData'ya uygula
            playerData.enemyKamikazeDamage = kamikazeDamage;
            playerData.enemyMinigunDamage = minigunDamage;
            playerData.enemyRocketDamage = rocketDamage;
            playerData.enemyKamikazeDamageMultiplier = kamikazeDamageMultiplier;
            playerData.enemyMinigunDamageMultiplier = minigunDamageMultiplier;
            playerData.enemyRocketDamageMultiplier = rocketDamageMultiplier;
            
            Debug.Log("<color=green>Denge değerleri başarıyla uygulandı!</color>");
            ShowCurrentValues();
        }
    }
    
    // Mevcut değerleri konsola yazdır
    public void ShowCurrentValues()
    {
        if (playerData != null)
        {
            Debug.Log("<color=yellow>===== MEVCUT DÜŞMAN DEĞERLER =====</color>");
            Debug.Log($"Kamikaze Hasarı: {playerData.enemyKamikazeDamage} (Çarpan: {playerData.enemyKamikazeDamageMultiplier})");
            Debug.Log($"Minigun Hasarı: {playerData.enemyMinigunDamage} (Çarpan: {playerData.enemyMinigunDamageMultiplier})");
            Debug.Log($"Roket Hasarı: {playerData.enemyRocketDamage} (Çarpan: {playerData.enemyRocketDamageMultiplier})");
            Debug.Log($"Zorluk Çarpanı: {playerData.enemyDifficultyMultiplier}");
            
            // Hesaplanmış değerleri göster
            Debug.Log("<color=orange>===== HESAPLANMIŞ DEĞERLER =====</color>");
            Debug.Log($"Hesaplanmış Kamikaze Hasarı: {playerData.CalculateKamikazeDamage()}");
            Debug.Log($"Hesaplanmış Minigun Hasarı: {playerData.CalculateMinigunDamage()}");
            Debug.Log($"Hesaplanmış Roket Hasarı: {playerData.CalculateRocketDamage()}");
        }
    }
    
    // Tüm PlayerData değerlerini Inspector'dan oku ve uygula
    public void ApplyInspectorValues()
    {
        if (playerData != null)
        {
            // Değerler PlayerData'dan alınıp GameBalancer'a uygulanıyor
            kamikazeDamage = playerData.enemyKamikazeDamage;
            minigunDamage = playerData.enemyMinigunDamage;
            rocketDamage = playerData.enemyRocketDamage;
            kamikazeDamageMultiplier = playerData.enemyKamikazeDamageMultiplier;
            minigunDamageMultiplier = playerData.enemyMinigunDamageMultiplier;
            rocketDamageMultiplier = playerData.enemyRocketDamageMultiplier;
            
            Debug.Log("<color=cyan>PlayerData değerleri GameBalancer'a aktarıldı!</color>");
        }
    }
    
    // PlayerData'dan değerleri yükleyen yeni bir metot
    public void LoadValuesFromPlayerData()
    {
        if (playerData != null)
        {
            // İlk başlangıçta, eğer değerler düzenlenmemişse PlayerData'dan al
            if (kamikazeDamage == 10 && minigunDamage == 3 && rocketDamage == 20)
            {
                kamikazeDamage = playerData.enemyKamikazeDamage;
                minigunDamage = playerData.enemyMinigunDamage;
                rocketDamage = playerData.enemyRocketDamage;
                kamikazeDamageMultiplier = playerData.enemyKamikazeDamageMultiplier;
                minigunDamageMultiplier = playerData.enemyMinigunDamageMultiplier;
                rocketDamageMultiplier = playerData.enemyRocketDamageMultiplier;
                
                Debug.Log("<color=cyan>PlayerData değerleri GameBalancer'a yüklendi!</color>");
            }
        }
    }
    
    // Force zeplin health to 1000
    private void ForceZeplinHealth1000()
    {
        // Clear any previous zeplin health value in PlayerPrefs
        if (PlayerPrefs.HasKey("zeplinSaglik"))
        {
            PlayerPrefs.DeleteKey("zeplinSaglik");
        }
        
        // Set to 1000
        PlayerPrefs.SetInt("zeplinSaglik", 1000);
        PlayerPrefs.Save();
        
        // Also try to find and update PlayerData directly
        PlayerData pd = FindObjectOfType<PlayerData>();
        if (pd != null)
        {
            pd.zeplinSaglik = 1000;
            Debug.Log("GameBalancer: Zeplin sağlık değeri 1000'e zorlandı!");
        }
    }
    
    // PlayerData'ya ilk değerleri uygulayan yeni bir metot
    private void ApplyInitialValues()
    {
        if (playerData != null)
        {
            // İlk başlangıçta, eğer değerler düzenlenmemişse PlayerData'dan al
            if (kamikazeDamage == 10 && minigunDamage == 3 && rocketDamage == 20)
            {
                kamikazeDamage = playerData.enemyKamikazeDamage;
                minigunDamage = playerData.enemyMinigunDamage;
                rocketDamage = playerData.enemyRocketDamage;
                kamikazeDamageMultiplier = playerData.enemyKamikazeDamageMultiplier;
                minigunDamageMultiplier = playerData.enemyMinigunDamageMultiplier;
                rocketDamageMultiplier = playerData.enemyRocketDamageMultiplier;
                
                Debug.Log("<color=cyan>PlayerData değerleri GameBalancer'a yüklendi!</color>");
            }
            
            // PlayerData'ya ilk değerleri uygula
            playerData.enemyKamikazeDamage = kamikazeDamage;
            playerData.enemyMinigunDamage = minigunDamage;
            playerData.enemyRocketDamage = rocketDamage;
            playerData.enemyKamikazeDamageMultiplier = kamikazeDamageMultiplier;
            playerData.enemyMinigunDamageMultiplier = minigunDamageMultiplier;
            playerData.enemyRocketDamageMultiplier = rocketDamageMultiplier;
            
            Debug.Log("<color=cyan>PlayerData değerleri GameBalancer'a uygulandı!</color>");
        }
    }
} 