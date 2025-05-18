using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
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
    public EnemySettings rocketPrefab; // Roket düşmanı
    
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
    public bool useWaveSystem = false; // Dalga sistemini kullanmak için
    public float timeBetweenWaves = 10f; // Dalgalar arası bekleme süresi
    public int enemiesPerWave = 5; // Dalga başına düşman sayısı
    public float waveBreakDuration = 3f; // Dalgalar arası mola süresi
    private int currentWave = 0; // Mevcut dalga
    private float waveTimer = 0f; // Dalga zamanlayıcısı
    private bool isWaveBreak = false; // Dalga arası mı?
    
    // Spawn etme zamanları
    private float gameStartTime;
    
    private void Start()
    {
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
        }
    }
    
    private void InitializeEnemySettings()
    {
        // Her düşman tipinin ilk spawn zamanını ayarla
        kamikazePrefab.nextSpawnTime = Time.time + Random.Range(kamikazePrefab.minSpawnInterval, kamikazePrefab.maxSpawnInterval);
        minigunPrefab.nextSpawnTime = Time.time + Random.Range(minigunPrefab.minSpawnInterval, minigunPrefab.maxSpawnInterval);
        rocketPrefab.nextSpawnTime = Time.time + Random.Range(rocketPrefab.minSpawnInterval, rocketPrefab.maxSpawnInterval);
    }
    
    private void Update()
    {
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
            }
        }
        // Dalga aktifse ve düşman kalmadıysa
        else if (GetTotalEnemyCount() == 0)
        {
            // Dalga tamamlandı, mola ver
            isWaveBreak = true;
            waveTimer = waveBreakDuration;
            Debug.Log($"Dalga {currentWave} tamamlandı! Yeni dalga için hazırlanılıyor...");
        }
    }
    
    private void StartNextWave()
    {
        currentWave++;
        int enemiesToSpawn = enemiesPerWave + (currentWave - 1) * 2; // Her dalgada düşman sayısını arttır
        
        Debug.Log($"Dalga {currentWave} başlıyor! Düşman sayısı: {enemiesToSpawn}");
        
        // Dalga düşmanlarını spawn et
        StartCoroutine(SpawnWaveEnemies(enemiesToSpawn));
    }
    
    private IEnumerator SpawnWaveEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Rastgele bir düşman tipi seç ve spawn et
            SpawnRandomEnemy();
            
            // Kısa bir bekleme süresi ile art arda spawn et
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void SpawnEnemies()
    {
        // Mevcut zamanı al
        float currentTime = Time.time;
        
        // Zorluğu zamanla arttır (uzun süren oyunlarda daha fazla düşman)
        float timeFactor = 1.0f + (currentTime - gameStartTime) * difficultyScaling / 100f;
        
        // Her düşman tipini kontrol et ve spawn et
        if (currentTime >= kamikazePrefab.nextSpawnTime && kamikazePrefab.currentCount < kamikazePrefab.maxCount * timeFactor)
        {
            SpawnEnemy(kamikazePrefab);
            // Bir sonraki spawn zamanını ayarla
            kamikazePrefab.nextSpawnTime = currentTime + Random.Range(kamikazePrefab.minSpawnInterval, kamikazePrefab.maxSpawnInterval) / timeFactor;
        }
        
        if (currentTime >= minigunPrefab.nextSpawnTime && minigunPrefab.currentCount < minigunPrefab.maxCount * timeFactor)
        {
            SpawnEnemy(minigunPrefab);
            // Bir sonraki spawn zamanını ayarla
            minigunPrefab.nextSpawnTime = currentTime + Random.Range(minigunPrefab.minSpawnInterval, minigunPrefab.maxSpawnInterval) / timeFactor;
        }
        
        if (currentTime >= rocketPrefab.nextSpawnTime && rocketPrefab.currentCount < rocketPrefab.maxCount * timeFactor)
        {
            SpawnEnemy(rocketPrefab);
            // Bir sonraki spawn zamanını ayarla
            rocketPrefab.nextSpawnTime = currentTime + Random.Range(rocketPrefab.minSpawnInterval, rocketPrefab.maxSpawnInterval) / timeFactor;
        }
    }
    
    private void SpawnEnemy(EnemySettings enemySettings)
    {
        // Prefab kontrolü
        if (enemySettings.prefab == null) return;
        
        // Roket düşmanı için özel kontrol
        string enemyType = DetermineEnemyType(enemySettings);
        if (enemyType == "Roket")
        {
            // EnemyRocketPrefab içindeki rocketPrefab değişkenini kontrol et
            Enemy rocketEnemy = enemySettings.prefab.GetComponent<Enemy>();
            if (rocketEnemy != null)
            {
                Debug.Log("Rocket Enemy prefabı spawn edilecek. Enemy bileşeni var, FireRocket metodu kullanılacak.");
                
                // RocketPrefab'ın atanıp atanmadığını kontrol et
                if (rocketEnemy.rocketPrefab == null)
                {
                    // RocketPrefab'ı bulmaya çalış
                    GameObject rocketPrefab = null;
                    
                    // Önce Resources klasöründen yüklemeyi dene
                    rocketPrefab = Resources.Load<GameObject>("Prefabs/RocketPrefab");
                    
                    // Bulunamazsa scene'de arama yap
                    if (rocketPrefab == null)
                    {
                        GameObject[] allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
                        foreach (GameObject prefab in allPrefabs)
                        {
                            if (prefab.name == "RocketPrefab")
                            {
                                rocketPrefab = prefab;
                                break;
                            }
                        }
                    }
                    
                    // RocketPrefab'ı atama
                    if (rocketPrefab != null)
                    {
                        // RocketPrefab'ı düşmana ata
                        rocketEnemy.rocketPrefab = rocketPrefab;
                        Debug.Log("Rocket Enemy'ye RocketPrefab atandı: " + rocketPrefab.name);
                    }
                    else
                    {
                        Debug.LogError("RocketPrefab bulunamadı! Roket düşmanı roket fırlatamayacak.");
                    }
                }
                else
                {
                    Debug.Log("Roket düşmanının rocketPrefab'ı zaten atanmış: " + rocketEnemy.rocketPrefab.name);
                }
            }
            else
            {
                Debug.LogWarning("Roket düşmanında Enemy bileşeni bulunamadı!");
            }
        }
        
        // Spawn pozisyonu belirle
        Vector2 spawnPosition = GetRandomSpawnPosition();
        
        // Düşmanı spawn et
        GameObject enemy = Instantiate(enemySettings.prefab, spawnPosition, Quaternion.identity);
        
        // Spawn edilen düşmanı bu spawner'a bağla (isteğe bağlı)
        enemy.transform.parent = transform;
        
        // Aktif düşman sayısını arttır
        enemySettings.currentCount++;
        
        // Düşman yok edildiğinde sayacı azaltmak için bir bileşen ekle
        EnemyTracker tracker = enemy.AddComponent<EnemyTracker>();
        tracker.spawner = this;
        tracker.enemyType = enemyType;
        
        // Roket düşmanı için son bir kontrol daha
        if (enemyType == "Roket")
        {
            Enemy rocketEnemyInstance = enemy.GetComponent<Enemy>();
            if (rocketEnemyInstance != null && rocketEnemyInstance.rocketPrefab == null)
            {
                // Prefab'da rocketPrefab atanmış olsa bile instance'a kopyalanmamış olabilir
                // Tekrar bulmaya çalış
                GameObject rocketPrefab = Resources.Load<GameObject>("Prefabs/RocketPrefab");
                if (rocketPrefab == null)
                {
                    // Prefabs klasöründen direkt yüklemeyi dene
                    rocketPrefab = Resources.Load<GameObject>("RocketPrefab");
                }
                
                if (rocketPrefab != null)
                {
                    rocketEnemyInstance.rocketPrefab = rocketPrefab;
                    Debug.Log("Spawn edilen Roket düşmanına RocketPrefab atandı: " + rocketPrefab.name);
                }
                else
                {
                    Debug.LogError("RocketPrefab bulunamadı! Spawn edilen roket düşmanı roket fırlatamayacak.");
                }
            }
        }
        
        Debug.Log($"{enemyType} düşmanı spawn edildi. Konum: {spawnPosition}");
    }
    
    private void SpawnRandomEnemy()
    {
        // Prefabları ağırlıklarına göre değerlendir
        float totalWeight = kamikazePrefab.spawnWeight + minigunPrefab.spawnWeight + rocketPrefab.spawnWeight;
        float randomValue = Random.Range(0, totalWeight);
        
        // Ağırlıklara göre düşman tipini seç
        if (randomValue < kamikazePrefab.spawnWeight)
        {
            SpawnEnemy(kamikazePrefab);
        }
        else if (randomValue < kamikazePrefab.spawnWeight + minigunPrefab.spawnWeight)
        {
            SpawnEnemy(minigunPrefab);
        }
        else
        {
            SpawnEnemy(rocketPrefab);
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
        // Sahnedeki düşmanları say
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        // Her düşman tipinin sayısını sıfırla
        kamikazePrefab.currentCount = 0;
        minigunPrefab.currentCount = 0;
        rocketPrefab.currentCount = 0;
        
        // Düşmanları tipine göre say
        foreach (GameObject enemy in enemies)
        {
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                switch (enemyComponent.enemyType)
                {
                    case EnemyType.Kamikaze:
                        kamikazePrefab.currentCount++;
                        break;
                    case EnemyType.Minigun:
                        minigunPrefab.currentCount++;
                        break;
                    case EnemyType.Rocket:
                        rocketPrefab.currentCount++;
                        break;
                }
            }
        }
    }
    
    // Toplam düşman sayısını döndür
    private int GetTotalEnemyCount()
    {
        return kamikazePrefab.currentCount + minigunPrefab.currentCount + rocketPrefab.currentCount;
    }
    
    // EnemySettings'e göre düşman tipini belirle
    private string DetermineEnemyType(EnemySettings settings)
    {
        if (settings == kamikazePrefab) return "Kamikaze";
        if (settings == minigunPrefab) return "Minigun";
        if (settings == rocketPrefab) return "Roket";
        return "Bilinmeyen";
    }
}

// Düşman takip bileşeni - düşman yok edildiğinde sayacı azaltmak için
public class EnemyTracker : MonoBehaviour
{
    public EnemySpawner spawner;
    public string enemyType;
    
    private void OnDestroy()
    {
        // Spawner hala varsa sayacı azalt
        if (spawner != null)
        {
            switch (enemyType)
            {
                case "Kamikaze":
                    spawner.kamikazePrefab.currentCount--;
                    break;
                case "Minigun":
                    spawner.minigunPrefab.currentCount--;
                    break;
                case "Roket":
                    spawner.rocketPrefab.currentCount--;
                    break;
            }
        }
    }
} 