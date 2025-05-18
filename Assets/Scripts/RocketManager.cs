using UnityEngine;

// RocketManager - Merkezi roket yönetimi için sınıf
// Bu sınıf oyun başladığında otomatik olarak oluşturulur ve tüm roketlere erişim sağlar
public class RocketManager : MonoBehaviour
{
    private static RocketManager _instance;
    public static RocketManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // RocketManager bulunamadıysa oluştur
                GameObject managerObject = new GameObject("RocketManager");
                _instance = managerObject.AddComponent<RocketManager>();
                DontDestroyOnLoad(managerObject); // Sahne değişimlerinde korunur
            }
            return _instance;
        }
    }

    // Roket prefabları
    private GameObject _rocketPrefab;
    
    // RocketPrefab için property - yoksa yüklemeyi dener
    public GameObject RocketPrefab
    {
        get
        {
            if (_rocketPrefab == null)
            {
                LoadRocketPrefab();
            }
            return _rocketPrefab;
        }
    }
    
    private void Awake()
    {
        // Singleton yapısı - sadece bir tane olduğundan emin ol
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRocketPrefab();
            
            // RocketManagerBridge oluştur - Enemy sınıfı bunu kullanabilir
            CreateBridge();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // Roket prefabını yükler
    private void LoadRocketPrefab()
    {
        // Önce Resources klasöründen yüklemeyi dene
        _rocketPrefab = Resources.Load<GameObject>("Prefabs/RocketPrefab");
        
        if (_rocketPrefab == null)
        {
            _rocketPrefab = Resources.Load<GameObject>("RocketPrefab");
        }
        
        // Bulunamazsa, mevcut sahnedekilere bak
        if (_rocketPrefab == null)
        {
            GameObject[] prefabs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject prefab in prefabs)
            {
                if (prefab.name == "RocketPrefab")
                {
                    _rocketPrefab = prefab;
                    Debug.Log("RocketManager: RocketPrefab bulundu: " + prefab.name);
                    break;
                }
            }
        }
        
        // Hala bulunamadıysa, diğer prefablara bak
        if (_rocketPrefab == null)
        {
            GameObject[] allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject prefab in allPrefabs)
            {
                if (prefab.name.Contains("Rocket") && !prefab.name.Contains("Enemy"))
                {
                    _rocketPrefab = prefab;
                    Debug.LogWarning("RocketManager: Tam eşleşme bulunamadı, benzer isimli bir prefab kullanılıyor: " + prefab.name);
                    break;
                }
            }
        }
        
        if (_rocketPrefab != null)
        {
            Debug.Log("RocketManager: RocketPrefab yüklendi: " + _rocketPrefab.name);
            
            // RocketPrefab'ın RocketProjectile bileşeni var mı?
            ValidateRocketPrefab();
        }
        else
        {
            Debug.LogError("RocketManager: RocketPrefab yüklenemedi! Roketler doğru çalışmayabilir.");
            
            // Son çare olarak yeni bir prefab oluştur
            CreateEmergencyRocketPrefab();
        }
    }
    
    // RocketPrefab'ın geçerliliğini kontrol et
    private void ValidateRocketPrefab()
    {
        if (_rocketPrefab == null) return;
        
        // RocketProjectile bileşeni kontrolü
        RocketProjectile rocketComp = _rocketPrefab.GetComponent<RocketProjectile>();
        if (rocketComp == null)
        {
            Debug.LogWarning("RocketManager: RocketPrefab'da RocketProjectile bileşeni bulunamadı!");
            
            // Editor modunda prefab'a bileşen eklemek güvenli değil, bu yüzden sadece test amaçlı bir kopya oluştur
            GameObject testRocket = Instantiate(_rocketPrefab, new Vector3(-1000, -1000, -1000), Quaternion.identity);
            RocketProjectile.EnsureRocketComponentExists(testRocket);
            Debug.Log("RocketManager: Test roketine RocketProjectile bileşeni eklendi.");
            Destroy(testRocket);
        }
    }
    
    // Acil durum roket prefabı oluştur (hiçbir prefab bulunamazsa)
    private void CreateEmergencyRocketPrefab()
    {
        Debug.LogWarning("RocketManager: Acil durum roket prefabı oluşturuluyor...");
        
        // Temiz bir prefab oluştur
        GameObject emergencyRocket = new GameObject("EmergencyRocketPrefab");
        
        // Gerekli bileşenleri ekle
        SpriteRenderer sr = emergencyRocket.AddComponent<SpriteRenderer>();
        sr.color = Color.red;
        sr.sprite = Resources.FindObjectsOfTypeAll<Sprite>().Length > 0 ? 
                    Resources.FindObjectsOfTypeAll<Sprite>()[0] : null;
        
        // RocketProjectile bileşenini ekle
        RocketProjectile rp = emergencyRocket.AddComponent<RocketProjectile>();
        rp.speed = 8f;
        rp.turnSpeed = 3f;
        rp.lifetime = 5f;
        rp.damage = 30;
        
        // Rigidbody ekle
        Rigidbody2D rb = emergencyRocket.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // Collider ekle
        BoxCollider2D bc = emergencyRocket.AddComponent<BoxCollider2D>();
        bc.isTrigger = false;
        
        // Prefab olarak ayarla
        _rocketPrefab = emergencyRocket;
        
        // Temiz prefabı sahne dışına taşı
        emergencyRocket.transform.position = new Vector3(-1000, -1000, -1000);
        
        Debug.Log("RocketManager: Acil durum roket prefabı oluşturuldu.");
    }
    
    // Düşmanlara roket prefabını ata
    public void AssignRocketPrefabToEnemies()
    {
        if (_rocketPrefab == null) return;
        
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.enemyType == EnemyType.Rocket && enemy.rocketPrefab == null)
            {
                enemy.rocketPrefab = _rocketPrefab;
                Debug.Log("RocketManager: " + enemy.name + " düşmanına RocketPrefab atandı.");
            }
        }
    }
    
    // Update'te düzenli olarak kontrol et
    private void Update()
    {
        // Her 2 saniyede bir düşmanları kontrol et
        if (Time.frameCount % 120 == 0)
        {
            AssignRocketPrefabToEnemies();
        }
    }
    
    // RocketPrefab için statik yardımcı metot
    public static GameObject GetRocketPrefab()
    {
        return Instance.RocketPrefab;
    }
    
    // Düşmana roket prefabı atamak için statik yardımcı metot
    public static void AssignRocketPrefabToEnemy(Enemy enemy)
    {
        if (enemy != null && enemy.enemyType == EnemyType.Rocket && enemy.rocketPrefab == null)
        {
            enemy.rocketPrefab = Instance.RocketPrefab;
            Debug.Log("RocketManager: " + enemy.name + " düşmanına RocketPrefab atandı.");
        }
    }
    
    // Bridge oluştur
    private void CreateBridge()
    {
        // Bridge bileşeni ekle
        RocketManagerBridge bridge = gameObject.AddComponent<RocketManagerBridge>();
        
        // Rocket prefabını bridge'e ata
        bridge.rocketPrefab = _rocketPrefab;
        
        Debug.Log("RocketManager: RocketManagerBridge oluşturuldu ve prefab atandı.");
    }
} 