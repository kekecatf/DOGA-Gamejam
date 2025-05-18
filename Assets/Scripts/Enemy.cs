using UnityEngine;using System.Collections;

public enum EnemyType
{
    Kamikaze,   // Kamikazeler hedefe doğru gider ve çarparak hasar verir
    Minigun,    // Minigun düşmanları uzaktan ateş eder
    Rocket      // Roket düşmanları füze fırlatır
}

// IDamageable interface to standardize damage processing
public interface IDamageable
{
    void TakeDamage(int damageAmount);
}

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Düşman Tipi")]
    public EnemyType enemyType = EnemyType.Kamikaze;
    
    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float attackRange = 5f;    // Saldırı menzili (Minigun ve Rocket için)
    public float safeDistance = 4f;   // Güvenli mesafe (Minigun ve Rocket düşmanları için)
    
    [Header("Ateş Ayarları")]
    public GameObject bulletPrefab;   // Minigun mermisi
    public GameObject rocketPrefab;   // Roket
    public Transform firePoint;       // Ateş noktası
    public float fireRate = 2f;       // Ateş hızı (saniyede kaç kez)
    private float nextFireTime = 0f;  // Bir sonraki ateş zamanı
    
    [Header("Mesafe Ayarları")]
    public float minigunAttackRange = 8f;  // Minigun ile ateş etme mesafesi
    public float rocketAttackRange = 10f;  // Roket ile ateş etme mesafesi
    public float minigunSafeDistance = 6f; // Minigun için güvenli mesafe
    public float rocketSafeDistance = 8f;  // Roket için güvenli mesafe
    
    [Header("Hedef")]
    private Vector2 targetPosition = Vector2.zero;  // Hedef pozisyonu
    private Transform targetTransform; // Hedef transform
    
    private SpriteRenderer spriteRenderer;
    private PlayerData playerData;
    
    private int currentHealth;
    private int damage;
    public int scoreValue = 25;
    public GameObject deathEffect;    // Ölüm efekti (isteğe bağlı)
    
    [Header("Hasar Ayarları")]
    public int minigunBulletDamage = 5;  // Minigun mermisinin verdiği hasar
    public int rocketDamage = 30;       // Roketin verdiği hasar
    
    private void Start()
    {
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Rastgele başlangıç hızı varyasyonu (daha doğal görünüm için)
        moveSpeed = Random.Range(moveSpeed * 0.8f, moveSpeed * 1.2f);
        
        // FirePoint kontrolü
        if (firePoint == null)
        {
            // FirePoint yoksa oluştur
            GameObject newFirePoint = new GameObject("FirePoint");
            newFirePoint.transform.parent = transform;
            newFirePoint.transform.localPosition = new Vector3(0.5f, 0, 0); // Düşmanın önünde
            firePoint = newFirePoint.transform;
        }
        
        // Roket düşmanı için roket prefab kontrolü
        if (enemyType == EnemyType.Rocket)
        {
            // Roket prefabı atanmamışsa, Resources klasöründen yükle veya EnemyRocketPrefab içinden bul
            if (rocketPrefab == null)
            {
                // Önce Resources'tan yüklemeyi dene
                rocketPrefab = Resources.Load<GameObject>("RocketPrefab");
                
                // Yine bulunamazsa Assets/Prefabs içinde ara
                if (rocketPrefab == null)
                {
                    GameObject[] allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (GameObject prefab in allPrefabs)
                    {
                        if (prefab.name.Contains("RocketPrefab"))
                        {
                            rocketPrefab = prefab;
                            Debug.Log("RocketPrefab bulundu: " + prefab.name);
                            break;
                        }
                    }
                }
                
                // Hala bulunamadıysa, uyarı ver
                if (rocketPrefab == null)
                {
                    Debug.LogError("Roket Düşmanı için RocketPrefab bulunamadı! Bu düşman roket ateşleyemeyecek.");
                }
                else
                {
                    Debug.Log("Roket Düşmanı için RocketPrefab otomatik olarak atandı: " + rocketPrefab.name);
                }
            }
            
            // Rocket prefab'taki RocketProjectile bileşenini kontrol et
            if (rocketPrefab != null)
            {
                RocketProjectile rocketComp = rocketPrefab.GetComponent<RocketProjectile>();
                if (rocketComp == null)
                {
                    Debug.LogWarning("Rocket prefab'ta RocketProjectile bileşeni bulunamadı! Bu düşmanın roketleri bileşen eksikliği yaşayabilir.");
                    
                    // Unity Editor'da çalışıyorsak prefab'ı düzenlemeyi dene
                    #if UNITY_EDITOR
                    try
                    {
                        // Prefab'a RocketProjectile bileşenini ekle
                        UnityEditor.PrefabUtility.InstantiateAttachedAsset(rocketPrefab);
                        Debug.Log("Rocket prefaba editor üzerinden bileşen ekleme denemesi yapıldı.");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Editor üzerinden prefab düzenleme başarısız: " + ex.Message);
                    }
                    #endif
                }
            }
        }
        
        // Düşman değerlerini PlayerData'dan al
        InitializeEnemyStats();
    }
    
    // Düşman değerlerini PlayerData'dan alma
    private void InitializeEnemyStats()
    {
        if (playerData != null)
        {
            // Düşman tipine göre stats ayarlamaları
            switch (enemyType)
            {
                case EnemyType.Kamikaze:
                    currentHealth = playerData.CalculateEnemyHealth() / 2; // Daha az HP
                    damage = playerData.CalculateKamikazeDamage(); // Düşman tipi için özel hasar
                    moveSpeed *= 1.3f; // Daha hızlı
                    // Kamikaze için saldırı mesafesi ve güvenli mesafe kullanılmaz
                    break;
                    
                case EnemyType.Minigun:
                    currentHealth = playerData.CalculateEnemyHealth();
                    damage = playerData.CalculateMinigunDamage(); // Düşman tipi için özel hasar
                    moveSpeed *= 0.8f; // Daha yavaş
                    // Minigun saldırı mesafesini ayarla
                    attackRange = minigunAttackRange;
                    safeDistance = minigunSafeDistance;
                    // Minigun mermi hasarını ayarla
                    minigunBulletDamage = damage;
                    break;
                    
                case EnemyType.Rocket:
                    currentHealth = playerData.CalculateEnemyHealth() * 2; // Daha çok HP
                    damage = playerData.CalculateRocketDamage(); // Düşman tipi için özel hasar
                    moveSpeed *= 0.6f; // En yavaş
                    fireRate = 1f; // Daha yavaş ateş
                    // Roket saldırı mesafesini ayarla
                    attackRange = rocketAttackRange;
                    safeDistance = rocketSafeDistance;
                    // Roket hasarını ayarla
                    rocketDamage = damage;
                    break;
            }
            
            scoreValue = playerData.CalculateEnemyScoreValue();
            
            // Düşman tipine göre ödül değerini ayarla
            switch (enemyType)
            {
                case EnemyType.Kamikaze:
                    scoreValue = (int)(scoreValue * 0.8f);
                    break;
                case EnemyType.Minigun:
                    scoreValue = (int)(scoreValue * 1.2f);
                    break;
                case EnemyType.Rocket:
                    scoreValue = (int)(scoreValue * 1.5f);
                    break;
            }
            
            Debug.Log($"{enemyType} düşman değerleri: Sağlık={currentHealth}, Hasar={damage}, Ödül={scoreValue}");
        }
        else
        {
            // PlayerData bulunamazsa varsayılan değerleri kullan
            currentHealth = 50;
            damage = 10;
            scoreValue = 25;
            Debug.LogWarning("PlayerData bulunamadı! Varsayılan düşman değerleri kullanılıyor.");
        }
    }
    
    private void Update()
    {
        // Roket düşmanı için rocketPrefab kontrolü
        if (enemyType == EnemyType.Rocket && rocketPrefab == null)
        {
            // Her framede kontrol etmeyelim
            if (Time.frameCount % 60 == 0) // Her saniye bir kontrol et
            {
                TryAssignRocketPrefab();
            }
        }

        UpdateTargetPosition();
        
        // Eğer UpdateTargetPosition içinde RangedEnemyBehavior çağrıldıysa
        // (oyuncu ölmüş ve düşman tipi kamikaze değilse)
        // burada tekrar çağırmamak için erken dönelim
        if (Player.isDead && enemyType != EnemyType.Kamikaze)
        {
            // Rotasyonu güncellemek için sadece bu kısmı çalıştır
            UpdateSpriteFlipping();
            return;
        }
        
        // Düşman tipine göre davranış
        switch (enemyType)
        {
            case EnemyType.Kamikaze:
                MoveTowardsTarget();
                break;
                
            case EnemyType.Minigun:
            case EnemyType.Rocket:
                RangedEnemyBehavior();
                break;
        }
        
        UpdateSpriteFlipping();
    }
    
    // RocketPrefab atamaya çalışma
    private void TryAssignRocketPrefab()
    {
        // RocketManagerBridge'i ara
        GameObject rocketManagerObj = GameObject.Find("RocketManager");
        if (rocketManagerObj != null)
        {
            // RocketManagerBridge bileşenini al
            var bridge = rocketManagerObj.GetComponent<RocketManagerBridge>();
            if (bridge != null)
            {
                // Bridge üzerinden prefabı al
                rocketPrefab = bridge.GetRocketPrefab();
                if (rocketPrefab != null)
                {
                    Debug.Log("Rocket düşmanı için RocketManagerBridge'den RocketPrefab alındı: " + rocketPrefab.name);
                    return;
                }
            }
        }
        
        // Eğer Bridge yoksa manuel olarak dene
        Debug.Log("RocketManagerBridge kullanılamadı, manuel arama yapılıyor...");
        
        // Resources veya Scene'den RocketPrefab'ı bulmaya çalış
        GameObject rocketPrefabObj = Resources.Load<GameObject>("Prefabs/RocketPrefab");
        
        if (rocketPrefabObj == null)
        {
            rocketPrefabObj = Resources.Load<GameObject>("RocketPrefab");
        }
        
        if (rocketPrefabObj == null)
        {
            // Scene'de aktif olan tüm GameObject'leri ara
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("RocketPrefab"))
                {
                    rocketPrefabObj = obj;
                    break;
                }
            }
        }
        
        if (rocketPrefabObj != null)
        {
            rocketPrefab = rocketPrefabObj;
            Debug.Log("Rocket düşmanı için manuel aramada RocketPrefab bulundu: " + rocketPrefabObj.name);
            
            // RocketPrefab'ın RocketProjectile bileşenini kontrol et
            ValidateRocketPrefab();
        }
        else
        {
            Debug.LogWarning("Rocket düşmanı için hiçbir şekilde RocketPrefab bulunamadı! Bu düşman roket ateşleyemeyecek.");
        }
    }
    
    // RocketPrefab'ın geçerliliğini kontrol et
    private void ValidateRocketPrefab()
    {
        if (rocketPrefab == null) return;
        
        // RocketProjectile bileşeni kontrolü
        RocketProjectile rocketComp = rocketPrefab.GetComponent<RocketProjectile>();
        if (rocketComp == null)
        {
            Debug.LogWarning("RocketPrefab'da RocketProjectile bileşeni bulunamadı! Runtime'da eklemeye çalışılacak.");
            
            // Runtime'da RocketProjectile Component'ini ekleyelim
            GameObject testRocket = Instantiate(rocketPrefab, new Vector3(-1000, -1000, -1000), Quaternion.identity);
            RocketProjectile.EnsureRocketComponentExists(testRocket);
            
            // Değişiklikler uygulandıktan sonra prefab'ı güncelleyemeyiz, ancak bir log ile bilgi verelim
            Debug.Log("Test roketine RocketProjectile bileşeni eklendi. Düşman ateş ettiğinde rocket bileşeni otomatik eklenecek.");
            
            // Test roketini yok et
            Destroy(testRocket);
        }
    }
    
    // Uzak mesafe düşman davranışı (Minigun ve Rocket için)
    private void RangedEnemyBehavior()
    {
        if (targetTransform == null) return;
        
        // Hedefe olan mesafeyi hesapla
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        
        // Hedefin Zeplin olup olmadığını kontrol et
        bool isTargetingZeplin = targetTransform.GetComponent<Zeplin>() != null;
        
        // Düşman tipine göre uzaklıklar (Minigun ve Rocket düşmanları farklı mesafelerden saldırır)
        float optimalDistance = enemyType == EnemyType.Minigun ? 8.0f : 12.0f;
        float minDistance = enemyType == EnemyType.Minigun ? 6.0f : 8.0f;
        
        // Zeplin hedefleniyorsa ateş mesafesini artır
        if (isTargetingZeplin)
        {
            // Zeplin'i daha uzaktan vurmak için
            optimalDistance = enemyType == EnemyType.Minigun ? 10.0f : 15.0f;
        }
        
        // Hedefe çok yakınsa kaç
        bool isTooClose = distanceToTarget < minDistance;
        
        // Hedefe bakacak şekilde dön - her durumda
        Vector2 lookDirection = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Debug çizgilerle göster
        Debug.DrawLine(transform.position, targetPosition, Color.red);
        Debug.DrawRay(transform.position, transform.right * optimalDistance, Color.yellow);
        
        // Hareket mantığı - çok yakınsa kaç, uygun mesafede değilse yaklaş
        if (isTooClose)
        {
            // Hedeften kaç - daha hızlı
            Vector2 direction = ((Vector2)transform.position - targetPosition).normalized;
            transform.position += (Vector3)(direction * moveSpeed * 1.2f * Time.deltaTime);
            
            // Loglama
            if (Time.frameCount % 120 == 0) // 2 saniyede bir
            {
                Debug.Log($"{enemyType} düşmanı {(isTargetingZeplin ? "Zeplin'e" : "Player'a")} çok yakın, uzaklaşıyor. Mesafe: {distanceToTarget}");
            }
        }
        else if (distanceToTarget > optimalDistance)
        {
            // Hedefe yaklaş - ama optimal mesafeye kadar
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * moveSpeed * 0.8f * Time.deltaTime);
            
            // Loglama
            if (Time.frameCount % 120 == 0) // 2 saniyede bir
            {
                Debug.Log($"{enemyType} düşmanı {(isTargetingZeplin ? "Zeplin'e" : "Player'a")} yaklaşıyor. Mesafe: {distanceToTarget}");
            }
        }
        
        // Ateş etme kontrolü - çok daha sık
        if (Time.time >= nextFireTime)
        {
            // Zeplin'e karşı daha sık ateş etsin - 4x daha hızlı
            float fireRateMultiplier = isTargetingZeplin ? 4.0f : 1.0f;
            
            // Ateş et
            if (enemyType == EnemyType.Minigun)
            {
                FireMinigun();
            }
            else // Rocket
            {
                FireRocket();
            }
            
            // Bir sonraki ateş zamanını ayarla
            nextFireTime = Time.time + (1f / (fireRate * fireRateMultiplier));
        }
    }
    
    // Minigun ateşleme
    private void FireMinigun()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Minigun düşmanı için bullet prefab tanımlanmamış!");
            return;
        }
        
        // Hedef Zeplin mi kontrolü
        bool isTargetingZeplin = targetTransform != null && targetTransform.GetComponent<Zeplin>() != null;
        
        // Mermi oluştur - küçük bir saçılma ile (daha gerçekçi)
        float randomSpread = Random.Range(-3f, 3f); // Derece cinsinden rastgele saçılma
        Quaternion spreadRotation = Quaternion.Euler(0, 0, randomSpread);
        Quaternion finalRotation = transform.rotation * spreadRotation;
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, finalRotation);
        
        // Mermi bileşenini al
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent == null)
        {
            Debug.LogError("Bullet prefabında Bullet script bileşeni bulunamadı!");
            Destroy(bullet);
            return;
        }
        
        // Merminin düşman tarafından atıldığını belirt
        bulletComponent.isEnemyBullet = true;
        
        // Hasar değerini ayarla
        int bulletDamage = minigunBulletDamage > 0 ? minigunBulletDamage : damage;
        
        // Zeplin'e karşı hasar ve hız artışı
        if (isTargetingZeplin)
        {
            bulletDamage = (int)(bulletDamage * 3.0f); // 3x fazla hasar
            bulletComponent.speed *= 1.5f; // 1.5x daha hızlı
        }
        
        bulletComponent.damage = bulletDamage;
        
        // Rigidbody ve Collider ayarlarını kontrol et
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        Collider2D collider = bullet.GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = bullet.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = false; // Fiziksel çarpışma için
        }
        
        // Ateş efekti veya sesi (opsiyonel)
        Debug.Log($"Minigun düşmanı {(isTargetingZeplin ? "Zeplin'e" : "Player'a")} ateş etti! Hasar: {bulletDamage}");
    }
    
    // Roket fırlatma
    private void FireRocket()
    {
        if (rocketPrefab == null)
        {
            Debug.LogWarning("Rocket düşmanı için rocket prefab tanımlanmamış! rocketPrefab atanmaya çalışılıyor...");
            TryAssignRocketPrefab();
            
            // Yine de bulunamadıysa geri dön
            if (rocketPrefab == null)
            {
                return;
            }
        }
        
        // Hedef Zeplin mi kontrolü
        bool isTargetingZeplin = targetTransform != null && targetTransform.GetComponent<Zeplin>() != null;
        
        Debug.Log($"Roket ateşleniyor... Prefab: {rocketPrefab.name}, Hedef: {(isTargetingZeplin ? "Zeplin" : "Player")}");
        
        // RocketFixHelper ile güvenli şekilde roket oluştur
        GameObject rocket = RocketFixHelper.CreateEnemyRocket(rocketPrefab, firePoint.position, transform.rotation);
        
        // Roket oluşturulamadıysa geri dön
        if (rocket == null)
        {
            Debug.LogError("Roket oluşturulamadı! FireRocket fonksiyonundan çıkılıyor.");
            return;
        }
        
        // Roket bileşenini al
        RocketProjectile rocketComponent = rocket.GetComponent<RocketProjectile>();
        if (rocketComponent != null)
        {
            // Hasar değerini belirle
            int actualDamage = rocketDamage > 0 ? rocketDamage : damage;
            
            // Zeplin'e karşı daha fazla hasar
            if (isTargetingZeplin)
            {
                actualDamage = (int)(actualDamage * 4.0f); // 4x fazla hasar
            }
            
            // Hasar değerini ayarla
            rocketComponent.damage = actualDamage;
            
            // Zeplin'e karşı roket hızını artır, dönüşünü daha agresif yap
            if (isTargetingZeplin)
            {
                rocketComponent.speed *= 1.8f; // 1.8x daha hızlı
                rocketComponent.turnSpeed *= 2.0f; // 2x daha hızlı dönüş
            }
            
            Debug.Log($"Rocket düşmanı {(isTargetingZeplin ? "Zeplin'e" : "Player'a")} başarıyla roket fırlattı! " +
                     $"Hasar: {actualDamage}, Hız: {rocketComponent.speed}");
        }
        else
        {
            Debug.LogError("RocketProjectile bileşeni bulunamadı! Bu beklenmeyen bir durum.");
        }
    }
    
    // Hedef pozisyonunu güncelle
    private void UpdateTargetPosition()
    {
        // Her zaman önce Zeplin'i hedef almaya çalış
        Zeplin zeplin = FindObjectOfType<Zeplin>();
        if (zeplin != null)
        {
            targetTransform = zeplin.transform;
            targetPosition = zeplin.transform.position;
            
            // Kamikaze düşmanları Zeplin'e doğru gitsin,
            // diğer düşmanlar uzaktan ateş etsin
            if (enemyType != EnemyType.Kamikaze)
            {
                // Zeplin'i hedef olarak al ama ona doğru hareket etme
                RangedEnemyBehavior();
                
                // Update içinde ikinci kez çağrılmasını önlemek için
                return;
            }
        }
        // Eğer Zeplin bulunamazsa, Player'ı hedef al
        else if (!Player.isDead)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                targetTransform = player.transform;
                targetPosition = player.transform.position;
            }
        }
    }
    
    private void MoveTowardsTarget()
    {
        if (targetTransform == null) return;
        
        // Hedef yönünü hesapla
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Hedefe doğru hareket et
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        
        // Eğer hedefe ulaştıysa
        if (Vector2.Distance(transform.position, targetPosition) < 0.5f)
        {
            // Hedefe ulaşıldığında (kamikaze davranışı)
            OnReachedTarget();
        }
        
        // Düşmanın rotasyonunu hareket yönüne göre ayarla
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void UpdateSpriteFlipping()
    {
        if (spriteRenderer != null)
        {
            // Rotasyon 90 veya -90 derece civarında ise Y ekseni üzerinde flip yap
            float normalizedRotation = transform.eulerAngles.z;
            if (normalizedRotation > 180)
                normalizedRotation -= 360;
                
            bool shouldFlipY = Mathf.Abs(Mathf.Abs(normalizedRotation) - 90) < 45;
            spriteRenderer.flipY = shouldFlipY;
        }
    }
    
    private void OnReachedTarget()
    {
        // Sadece kamikaze düşmanları için hedefe ulaşma davranışı
        if (enemyType != EnemyType.Kamikaze) return;
        
        // Player'a ulaştıysa
        if (!Player.isDead && targetTransform.CompareTag("Player"))
        {
            Player playerComponent = targetTransform.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.TakeDamage(damage);
                Debug.Log("Kamikaze düşman Player'a çarptı ve " + damage + " hasar verdi!");
            }
        }
        // Zeplin'e ulaştıysa
        else if (Player.isDead && targetTransform.GetComponent<Zeplin>() != null)
        {
            Zeplin zeplin = targetTransform.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                zeplin.TakeDamage(damage);
                Debug.Log("Kamikaze düşman Zeplin'e çarptı ve " + damage + " hasar verdi!");
            }
        }
        
        // Düşmanı yok et
        Die();
    }
    
    // Düşmana hasar verme metodu (Bullet tarafından çağrılacak)
    public void TakeDamage(int damageAmount)
    {
        // Sağlık değerini azalt
        currentHealth -= damageAmount;
        
        // Hasar efekti (isteğe bağlı)
        // Örnek: Flash efekti, kısa titreşim, vs.
        
        // Eğer sağlık sıfırın altına düştüyse yok et
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log($"{enemyType} düşman hasar aldı. Kalan sağlık: {currentHealth}");
        }
    }
    
    void Die()
    {
        // Ölüm animasyonu veya efekti (isteğe bağlı)
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Skor ekle (PlayerData üzerinden)
        if (playerData != null)
        {
            // Parasını arttır
            playerData.metalPara += scoreValue;
            Debug.Log($"{enemyType} düşman öldürüldü! Para kazanıldı: {scoreValue}");
        }
        else
        {
            // PlayerData referansını tekrar bulmayı dene
            playerData = FindObjectOfType<PlayerData>();
            if (playerData != null)
            {
                playerData.metalPara += scoreValue;
                Debug.Log($"{enemyType} düşman öldürüldü! Para kazanıldı: {scoreValue}");
            }
        }
        
        // Düşmanı yok et
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Sadece kamikaze düşmanlar için çarpışma tespiti
        if (enemyType == EnemyType.Kamikaze)
        {
            // Oyuncuyla çarpışma
            if (collision.CompareTag("Player"))
            {
                Player player = collision.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Debug.Log("Kamikaze düşman Player'a çarptı ve " + damage + " hasar verdi!");
                    Die();
                }
            }
            // Zeplinle çarpışma
            else if (collision.GetComponent<Zeplin>() != null)
            {
                Zeplin zeplin = collision.GetComponent<Zeplin>();
                if (zeplin != null)
                {
                    zeplin.TakeDamage(damage);
                    Debug.Log("Kamikaze düşman Zeplin'e çarptı ve " + damage + " hasar verdi!");
                    Die();
                }
            }
        }
        
        // Mermiyle çarpışma (tüm düşman tipleri için)
        if (collision.CompareTag("Bullet"))
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            if (bullet != null && !bullet.isEnemyBullet)
            {
                TakeDamage(bullet.damage);
                Destroy(bullet.gameObject); // Mermiyi yok et
            }
        }
    }
    
    // Düşmanın verdiği hasar değerini döndür
    public int GetDamageAmount()
    {
        return damage;
    }
    
    // Düşmanın mevcut sağlık değerini döndür
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // Düşmanın sağlık değerini ayarla
    public void SetHealth(int health)
    {
        currentHealth = health;
    }
} 