using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    // Add a flag to control debug messages
    [Header("Debug Settings")]
    public bool showDebugMessages = false;
    
    [System.Serializable]
    public class EnemySettings
    {
        public GameObject prefab;
        public float spawnWeight = 1f; // Spawn olma ağırlığı
        public float minSpawnInterval = 2f; // En az spawn aralığı
        public float maxSpawnInterval = 5f; // En fazla spawn aralığı
        public int maxCount = 10; // Ekranda aynı anda olabilecek maksimum sayı
        [HideInInspector] public float nextSpawnTime = 0f; // Bir sonraki spawn zamanı
        [HideInInspector] public int currentCount = 0; // Şu anki aktif sayısı
    }
    
    [Header("Düşman Prefabları")]
    public EnemySettings kamikazePrefab; // Kamikaze düşmanı
    public EnemySettings minigunPrefab; // Minigun düşmanı
    public GameObject bossPrefab; // Boss prefabı
    
    [Header("Spawn Ayarları")]
    public float playAreaWidth = 15f; // Oyun alanı genişliği
    public float playAreaHeight = 10f; // Oyun alanı yüksekliği
    public float minDistanceFromPlayer = 5f; // Oyuncuya minimum mesafe
    public float difficultyScaling = 0.1f; // Zamanla zorluğun artma oranı
    
    [Header("Spawn Bölgeleri")]
    public bool useCustomSpawnAreas = true; // Özel spawn bölgelerini kullan
    
    [System.Serializable]
    public class SpawnArea
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        public bool isActive = true;
    }
    
    public List<SpawnArea> spawnAreas = new List<SpawnArea>();
    
    [Header("Dalga Sistemi")]
    public bool useWaveSystem = true; // Dalga sistemini kullanmak için
    public int maxWaves = 5; // Maksimum dalga sayısı
    public float timeBetweenWaves = 10f; // Dalgalar arası bekleme süresi
    public int baseEnemiesPerWave = 5; // Temel dalga başına düşman sayısı
    public int enemyIncreasePerWave = 2; // Her dalgada eklenecek düşman sayısı
    public float waveBreakDuration = 3f; // Dalgalar arası mola süresi
    public float enemySpawnInterval = 0.5f; // Dalga içinde düşmanlar arası spawn aralığı
    private int currentWave = 0; // Mevcut dalga
    private float waveTimer = 0f; // Dalga zamanlayıcısı
    public bool isWaveBreak = false; // Dalga arası mı?
    private bool isAllWavesCompleted = false; // Tüm dalgalar tamamlandı mı?
    
    [Header("UI References")]
    public TextMeshProUGUI waveInfoText; // Dalga bilgisini göstermek için UI elemanı
    
    // Spawn etme zamanları
    private float gameStartTime;
    
    // Dalga için spawn edilecek toplam düşman sayısı
    private int totalEnemiesInWave = 0;
    
    // Dalga için şu ana kadar spawn edilen düşman sayısı
    private int spawnedEnemiesInWave = 0;
    
    // Şu anki dalgada öldürülen düşman sayısı
    public int killedEnemiesInWave = 0;
    
    private void Start()
    {
        // Initialize EnemySettings if they are null
        if (kamikazePrefab == null)
        {
            kamikazePrefab = new EnemySettings();
            kamikazePrefab.prefab = Resources.Load<GameObject>("Enemies/KamikazeEnemy");
            if (kamikazePrefab.prefab == null && showDebugMessages)
            {
                Debug.LogWarning("No KamikazeEnemy prefab in Resources folder. Game will continue with available prefabs.");
            }
        }
        
        if (minigunPrefab == null)
        {
            minigunPrefab = new EnemySettings();
            minigunPrefab.prefab = Resources.Load<GameObject>("Enemies/MinigunEnemy");
            if (minigunPrefab.prefab == null && showDebugMessages)
            {
                Debug.LogWarning("No MinigunEnemy prefab in Resources folder. Game will continue with available prefabs.");
            }
        }
        
        // Varsayılan spawn bölgelerini ekle (eğer henüz eklenmemişse)
        if (useCustomSpawnAreas && spawnAreas.Count == 0)
        {
            // Üst kenar bölgesi (y = 30 ~ 40)
            spawnAreas.Add(new SpawnArea { minX = -40, maxX = -30, minY = 30, maxY = 40, isActive = true });
            spawnAreas.Add(new SpawnArea { minX = 30, maxX = 40, minY = 30, maxY = 40, isActive = true });
            
            // Alt kenar bölgesi (y = -40 ~ -30)
            spawnAreas.Add(new SpawnArea { minX = -40, maxX = -30, minY = -40, maxY = -30, isActive = true });
            spawnAreas.Add(new SpawnArea { minX = 30, maxX = 40, minY = -40, maxY = -30, isActive = true });
            
            // Sol kenar bölgesi (x = -40 ~ -30)
            spawnAreas.Add(new SpawnArea { minX = -40, maxX = -30, minY = -30, maxY = 30, isActive = true });
            
            // Sağ kenar bölgesi (x = 30 ~ 40)
            spawnAreas.Add(new SpawnArea { minX = 30, maxX = 40, minY = -30, maxY = 30, isActive = true });
            
            Debug.Log("Varsayılan spawn bölgeleri oluşturuldu.");
        }
        
        // Oyun başlangıç zamanını kaydet
        gameStartTime = Time.time;
        
        // Her düşman tipi için başlangıç spawn zamanlarını ayarla
        InitializeEnemySettings();
        
        // Dalga sistemini kullanıyorsak ilk dalgayı başlat
        if (useWaveSystem)
        {
            StartNextWave();
            // UI otomatik güncellenecek
        }
    }
    
    private void InitializeEnemySettings()
    {
        // Add null checks before accessing prefabs
        if (kamikazePrefab != null)
        {
            kamikazePrefab.nextSpawnTime = Time.time + Random.Range(kamikazePrefab.minSpawnInterval, kamikazePrefab.maxSpawnInterval);
        }
        else if (showDebugMessages)
        {
            Debug.LogWarning("kamikazePrefab not assigned. Skipping initialization.");
        }
        
        if (minigunPrefab != null)
        {
            minigunPrefab.nextSpawnTime = Time.time + Random.Range(minigunPrefab.minSpawnInterval, minigunPrefab.maxSpawnInterval);
        }
        else if (showDebugMessages)
        {
            Debug.LogWarning("minigunPrefab not assigned. Skipping initialization.");
        }
    }
    
    private void Update()
    {
        // Tüm dalgalar tamamlandıysa düşman spawn etme
        if (isAllWavesCompleted) return;
        
        // Wave bilgisini sürekli güncelle
        UpdateWaveUI();
        
        // Dalga sistemini kullanıyorsak dalga mantığıyla spawn et
        if (useWaveSystem)
        {
            UpdateWaveSystem();
        }
        // Normal sürekli spawn sistemini kullan
        else
        {
            // Spawner aktifse düşmanları spawn et
            SpawnEnemies();
        }
        
        // Aktif düşman sayılarını güncelle
        UpdateEnemyCounts();
    }
    
    private void UpdateWaveUI()
    {
        if (waveInfoText != null)
        {
            if (isWaveBreak)
            {
                // Mola durumunda geri sayım göster
                waveInfoText.text = $"Dalga {currentWave}/{maxWaves} tamamlandı!\nYeni dalga için hazırlanılıyor...\nKalan süre: {waveTimer:F1} sn";
            }
            else if (isAllWavesCompleted)
            {
                waveInfoText.text = "Tüm dalgalar tamamlandı! Tebrikler!";
            }
            else
            {
                // Normal wave bilgisini göster - sadece dalga numarası ve aktif düşman sayısı
                int activeEnemies = GetTotalEnemyCount();
                
                waveInfoText.text = $"Dalga: {currentWave}/{maxWaves}\n" +
                                   $"Aktif Düşman: {activeEnemies}";
            }
        }
    }
    
    private void UpdateWaveSystem()
    {
        // Dalga arası molada isek ve mola süresi bittiyse
        if (isWaveBreak)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0)
            {
                // Mola süresi bitti, yeni dalgayı başlat
                isWaveBreak = false;
                StartNextWave();
                // UI otomatik güncellenecek
            }
            // Kalan süreyi UI'da göstermeyi artık UpdateWaveUI metodu yapıyor
        }
        // Dalga aktifse, tüm düşmanlar spawn edildiyse ve tüm düşmanlar öldürüldüyse
        else if (spawnedEnemiesInWave >= totalEnemiesInWave && GetTotalEnemyCount() == 0)
        {
            // Dalga tamamlandı, son dalga mıydı kontrol et
            if (currentWave >= maxWaves)
            {
                // Tüm dalgalar tamamlandı
                isAllWavesCompleted = true;
                Debug.Log("Tüm dalgalar tamamlandı! Oyun sonu.");
                
                // Burada oyun sonu işlemleri yapılabilir
                // Örneğin: ShowGameOverScreen(), GiveRewards() vb.
            }
            else
            {
                // Sonraki dalga için mola ver
                isWaveBreak = true;
                waveTimer = waveBreakDuration;
                Debug.Log($"Dalga {currentWave}/{maxWaves} tamamlandı! Yeni dalga için hazırlanılıyor...");
            }
        }
    }
    
    private void StartNextWave()
    {
        // Check if there are any valid enemy prefabs before starting a wave
        bool hasValidKamikaze = kamikazePrefab != null && kamikazePrefab.prefab != null;
        bool hasValidMinigun = minigunPrefab != null && minigunPrefab.prefab != null;
        
        if (!hasValidKamikaze && !hasValidMinigun)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning("No valid enemy prefabs available. Cannot start wave.");
            }
            return;
        }
        
        currentWave++;
        
        // Son wave'de boss spawn et (sabit 5. wave yerine)
        if (currentWave == maxWaves && bossPrefab != null)
        {
            SpawnBoss();
            totalEnemiesInWave = 1; // Sadece boss var
        }
        else
        {
            totalEnemiesInWave = CalculateEnemiesForWave(currentWave);
        }
        
        spawnedEnemiesInWave = 0;
        killedEnemiesInWave = 0;
        
        Debug.Log($"Dalga {currentWave}/{maxWaves} başlıyor! Düşman sayısı: {totalEnemiesInWave}");
        
        // Dalga düşmanlarını spawn et (boss wave'i değilse)
        if (currentWave != maxWaves)
        {
            StartCoroutine(SpawnWaveEnemies(totalEnemiesInWave));
        }
    }
    
    private int CalculateEnemiesForWave(int waveNumber)
    {
        // Temel düşman sayısı + (dalga numarası - 1) * her dalgada eklenecek düşman sayısı
        return baseEnemiesPerWave + (waveNumber - 1) * enemyIncreasePerWave;
    }
    
    private IEnumerator SpawnWaveEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Rastgele bir düşman tipi seç ve spawn et
            SpawnRandomEnemy();
            spawnedEnemiesInWave++;
            
            // Kısa bir bekleme süresi ile art arda spawn et
            yield return new WaitForSeconds(enemySpawnInterval);
        }
    }
    
    private void SpawnEnemies()
    {
        // Mevcut zamanı al
        float currentTime = Time.time;
        
        // Zorluğu zamanla arttır (uzun süren oyunlarda daha fazla düşman)
        float timeFactor = 1.0f + (currentTime - gameStartTime) * difficultyScaling / 100f;
        
        // Her düşman tipini kontrol et ve spawn et
        if (kamikazePrefab != null && kamikazePrefab.prefab != null)
        {
            if (currentTime >= kamikazePrefab.nextSpawnTime && kamikazePrefab.currentCount < kamikazePrefab.maxCount * timeFactor)
            {
                SpawnEnemy(kamikazePrefab);
                // Bir sonraki spawn zamanını ayarla
                kamikazePrefab.nextSpawnTime = currentTime + Random.Range(kamikazePrefab.minSpawnInterval, kamikazePrefab.maxSpawnInterval) / timeFactor;
            }
        }
        
        if (minigunPrefab != null && minigunPrefab.prefab != null)
        {
            if (currentTime >= minigunPrefab.nextSpawnTime && minigunPrefab.currentCount < minigunPrefab.maxCount * timeFactor)
            {
                SpawnEnemy(minigunPrefab);
                // Bir sonraki spawn zamanını ayarla
                minigunPrefab.nextSpawnTime = currentTime + Random.Range(minigunPrefab.minSpawnInterval, minigunPrefab.maxSpawnInterval) / timeFactor;
            }
        }
    }
    
    private void SpawnRandomEnemy()
    {
        // Ağırlıklı rastgele seçim için toplam ağırlık hesapla
        float totalWeight = 0;
        bool hasValidEnemies = false;
        
        // Check each enemy type to see if at least one is valid
        if (kamikazePrefab != null && kamikazePrefab.prefab != null && kamikazePrefab.currentCount < kamikazePrefab.maxCount)
        {
            totalWeight += kamikazePrefab.spawnWeight;
            hasValidEnemies = true;
        }
        
        if (minigunPrefab != null && minigunPrefab.prefab != null && minigunPrefab.currentCount < minigunPrefab.maxCount)
        {
            totalWeight += minigunPrefab.spawnWeight;
            hasValidEnemies = true;
        }
        
        // If no valid enemies, exit early
        if (!hasValidEnemies)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning("No valid enemy prefabs available to spawn.");
            }
            return;
        }
        
        // Rastgele bir değer seç
        float randomValue = Random.Range(0, totalWeight);
        float currentWeight = 0;
        
        // Enemies to check - only proceed if they're valid
        if (kamikazePrefab != null && kamikazePrefab.prefab != null && kamikazePrefab.currentCount < kamikazePrefab.maxCount)
        {
            currentWeight += kamikazePrefab.spawnWeight;
            if (randomValue <= currentWeight)
            {
                SpawnEnemy(kamikazePrefab);
                return;
            }
        }
        
        if (minigunPrefab != null && minigunPrefab.prefab != null && minigunPrefab.currentCount < minigunPrefab.maxCount)
        {
            SpawnEnemy(minigunPrefab);
        }
    }
    
    private void SpawnEnemy(EnemySettings enemySettings)
    {
        // Validate enemy settings and prefab
        if (enemySettings == null || enemySettings.prefab == null)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning("Attempted to spawn a null enemy prefab.");
            }
            return;
        }
        
        // Spawn pozisyonu al
        Vector2 spawnPos = GetRandomSpawnPosition();
        
        // Düşmanı oluştur
        GameObject enemy = Instantiate(enemySettings.prefab, spawnPos, Quaternion.identity);
        
        // EnemyTracker bileşeni ekle (takip için)
        EnemyTracker tracker = enemy.AddComponent<EnemyTracker>();
        tracker.spawner = this;
        
        // Düşman tipini belirle (tracker için)
        tracker.enemyType = DetermineEnemyType(enemySettings);
        
        // Aktif düşman sayısını artır
        enemySettings.currentCount++;
        
        if (showDebugMessages)
        {
            Debug.Log($"Düşman oluşturuldu: {tracker.enemyType}, Konum: {spawnPos}, Aktif Sayı: {enemySettings.currentCount}");
        }
    }
    
    private Vector2 GetRandomSpawnPosition()
    {
        if (useCustomSpawnAreas && spawnAreas.Count > 0)
        {
            // Rastgele bir aktif spawn bölgesi seç
            List<SpawnArea> activeAreas = spawnAreas.FindAll(area => area.isActive);
            
            if (activeAreas.Count == 0)
            {
                Debug.LogWarning("Aktif spawn bölgesi yok! Rastgele bir konum kullanılacak.");
                return GetRandomPositionAroundPlayArea();
            }
            
            SpawnArea selectedArea = activeAreas[Random.Range(0, activeAreas.Count)];
            
            // Seçilen bölge içinde rastgele bir konum belirle
            float x = Random.Range(selectedArea.minX, selectedArea.maxX);
            float y = Random.Range(selectedArea.minY, selectedArea.maxY);
            
            Vector2 spawnPosition = new Vector2(x, y);
            
            // Oyuncuya minimum mesafeyi kontrol et
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(spawnPosition, player.transform.position);
                
                // Eğer oyuncuya çok yakınsa yeni bir konum bul
                if (distanceToPlayer < minDistanceFromPlayer)
                {
                    return GetRandomSpawnPosition(); // Rekursif çağrı
                }
            }
            
            return spawnPosition;
        }
        else
        {
            // Eski yöntem (eski yöntemi koruyalım)
            return GetRandomPositionAroundPlayArea();
        }
    }
    
    // Eski spawn pozisyonu belirleme yöntemi
    private Vector2 GetRandomPositionAroundPlayArea()
    {
        // Oyun alanının dışında, ama çok uzak olmayan bir konum belirle
        float edgeOffset = 2f; // Ekranın kenarından ne kadar uzakta spawn edeceğimiz
        
        // Kenar seçimi: 0 = Üst, 1 = Sağ, 2 = Alt, 3 = Sol
        int edge = Random.Range(0, 4);
        
        float x = 0, y = 0;
        
        switch (edge)
        {
            case 0: // Üst kenar
                x = Random.Range(-playAreaWidth, playAreaWidth);
                y = playAreaHeight + edgeOffset;
                break;
            case 1: // Sağ kenar
                x = playAreaWidth + edgeOffset;
                y = Random.Range(-playAreaHeight, playAreaHeight);
                break;
            case 2: // Alt kenar
                x = Random.Range(-playAreaWidth, playAreaWidth);
                y = -playAreaHeight - edgeOffset;
                break;
            case 3: // Sol kenar
                x = -playAreaWidth - edgeOffset;
                y = Random.Range(-playAreaHeight, playAreaHeight);
                break;
        }
        
        Vector2 spawnPosition = new Vector2(x, y);
        
        // Oyuncuya minimum mesafeyi kontrol et
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(spawnPosition, player.transform.position);
            
            // Eğer oyuncuya çok yakınsa yeni bir konum bul
            if (distanceToPlayer < minDistanceFromPlayer)
            {
                return GetRandomSpawnPosition(); // Rekursif çağrı
            }
        }
        
        return spawnPosition;
    }
    
    private void UpdateEnemyCounts()
    {
        try
        {
            // Sahnedeki düşmanları say
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            // Her düşman tipinin sayısını sıfırla (null kontrolü ile)
            if (kamikazePrefab != null) kamikazePrefab.currentCount = 0;
            if (minigunPrefab != null) minigunPrefab.currentCount = 0;
            
            // Düşmanları tipine göre say
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) continue;
                
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    switch (enemyComponent.enemyType)
                    {
                        case EnemyType.Kamikaze:
                            if (kamikazePrefab != null) kamikazePrefab.currentCount++;
                            break;
                        case EnemyType.Minigun:
                            if (minigunPrefab != null) minigunPrefab.currentCount++;
                            break;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning($"Error in UpdateEnemyCounts: {e.Message}");
            }
        }
    }
    
    // Toplam düşman sayısını döndür
    private int GetTotalEnemyCount()
    {
        // Null kontrolleri ekle
        int kamikazeCount = (kamikazePrefab != null) ? kamikazePrefab.currentCount : 0;
        int minigunCount = (minigunPrefab != null) ? minigunPrefab.currentCount : 0;
        
        return kamikazeCount + minigunCount;
    }
    
    // EnemySettings'e göre düşman tipini belirle
    private string DetermineEnemyType(EnemySettings settings)
    {
        if (settings == null) return "Bilinmeyen";
        
        try
        {
            if (kamikazePrefab != null && settings == kamikazePrefab) return "Kamikaze";
            if (minigunPrefab != null && settings == minigunPrefab) return "Minigun";
        }
        catch (System.Exception e)
        {
            if (showDebugMessages)
            {
                Debug.LogWarning($"Error in DetermineEnemyType: {e.Message}");
            }
        }
        
        return "Bilinmeyen";
    }
    
    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss prefabı atanmamış!");
            return;
        }
        
        // Boss'u rastgele bir konumda spawn et (normal düşmanlarla aynı spawn mantığını kullan)
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        
        // EnemyTracker bileşeni ekle
        EnemyTracker tracker = boss.AddComponent<EnemyTracker>();
        tracker.spawner = this;
        tracker.enemyType = "Boss";
        
        Debug.Log($"Boss spawn edildi! Son dalga: {currentWave}/{maxWaves}");
    }
}

// Düşman takip bileşeni - düşman yok edildiğinde sayacı azaltmak için
public class EnemyTracker : MonoBehaviour
{
    public EnemySpawner spawner;
    public string enemyType;
    
    private void OnDestroy()
    {
        try
        {
            // Spawner hala varsa sayacı azalt
            if (spawner != null)
            {
                // Düşman tipine göre sayacı azalt
                if (!string.IsNullOrEmpty(enemyType))
                {
                    switch (enemyType)
                    {
                        case "Kamikaze":
                            if (spawner.kamikazePrefab != null) 
                                spawner.kamikazePrefab.currentCount--;
                            break;
                        case "Minigun":
                            if (spawner.minigunPrefab != null)
                                spawner.minigunPrefab.currentCount--;
                            break;
                    }
                    
                    // Dalgadaki öldürülen düşman sayısını arttır (eğer oyun objesinin yok edilme sebebi ölüm ise)
                    // Not: Scene destroy olduğunda da OnDestroy çağrılacağı için, safAgeChecker ve Application.isPlaying kontrolü ekledim
                    if (gameObject != null && gameObject.scene.isLoaded && Application.isPlaying && !spawner.isWaveBreak)
                    {
                        spawner.killedEnemiesInWave++;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (spawner != null && spawner.showDebugMessages)
            {
                Debug.LogWarning($"Error in EnemyTracker.OnDestroy: {e.Message}");
            }
        }
    }
} 