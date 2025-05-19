using UnityEngine;

public class RocketProjectile : MonoBehaviour
{
    public float speed = 8f;                  // Roket hızı
    public float turnSpeed = 3f;              // Dönüş hızı
    public float lifetime = 5f;               // Roketin ömrü (saniye) - artırıldı
    public float activationDelay = 0.5f;      // Hedef takibine başlamadan önceki gecikme
    public float explosionRadius = 2f;        // Patlama yarıçapı
    public int damage = 15;                   // Hasar miktarı
    public bool isEnemyRocket = false;        // Düşman roketi mi?
    public Vector3 initialDirection = Vector3.right; // Başlangıç ateş yönü
    public float maxDistance = 50f;           // Maksimum menzil
    public float minTargetSearchCooldown = 0.2f; // Hedef arama arasındaki minimum süre
    public Transform sourceTransform = null;   // Roketi ateşleyen obje (self-collision önlemek için)
    
    public GameObject explosionEffect;        // Patlama efekti prefabı
    
    private Transform target;                 // Hedef (oyuncu, düşman veya zeplin)
    private bool isTracking = false;          // Hedef takip edilmeye başlandı mı
    private float activationTime;             // Takibe başlama zamanı
    private PlayerData playerData;            // Oyuncu veri referansı
    private float lastTargetSearchTime = 0f;  // Son hedef arama zamanı
    private float targetSearchInterval = 0.5f; // Hedef arama sıklığı
    private Vector3 startPosition;            // Başlangıç pozisyonu
    private bool isExploding = false;         // Patlama gerçekleşiyor mu?
    private Rigidbody2D rb;                   // Rigidbody referansı
    private bool isInitialized = false;       // Başlatılma durumu
    
    private void Start()
    {
        // Başlangıç pozisyonunu kaydet
        startPosition = transform.position;
        
        Debug.Log($"[RocketProjectile] Start metodu başladı. GameObject: {gameObject.name}, isEnemyRocket: {isEnemyRocket}");
        
        // Set the appropriate tag
        gameObject.tag = "Rocket";
        
        // Rigidbody ayarlarını kontrol et
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.Log("[RocketProjectile] Rocket için Rigidbody2D eklendi.");
        }
        
        // Rigidbody ayarlarını düzelt
        rb.gravityScale = 0f; // Yerçekimini kapat
        rb.bodyType = RigidbodyType2D.Kinematic; // Kinematic kullan
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // Asla uyku moduna geçmesin
        
        // Collider kontrolü
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // Collider yoksa ekle (Rocket genelde daha uzun olduğu için BoxCollider kullan)
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = false; // Fiziksel çarpışma için trigger kapalı
            
            // Sprite varsa box collideri sprite boyutuna ayarla
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                boxCollider.size = spriteRenderer.sprite.bounds.size;
                boxCollider.offset = Vector2.zero;
            }
            
            Debug.Log("[RocketProjectile] Rocket için BoxCollider2D eklendi (isTrigger=false).");
        }
        else
        {
            // Fiziksel çarpışma için trigger'ı kapat
            if (collider.isTrigger)
            {
                Debug.Log("[RocketProjectile] Rocket Collider2D isTrigger açıktı, kapatılıyor (fiziksel çarpışma için).");
                collider.isTrigger = false;
            }
        }
        
        // Zeplin roketi ise hızını ve yaşam süresini artır
        if (!isEnemyRocket)
        {
            speed = 8f;        // Hız artırıldı
            turnSpeed = 5f;    // Dönüş hızı artırıldı
            lifetime = 10f;    // Yaşam süresi uzatıldı
            maxDistance = 70f; // Menzil artırıldı
        }
        
        // Belirli bir süre sonra roketi yok et
        Destroy(gameObject, lifetime);
        
        // Takip etmeye başlama zamanını ayarla
        activationTime = Time.time + activationDelay;
        
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        
        // Hasar değerlerini ayarla
        if (playerData != null && damage == 50) // Default değer değişmemişse
        {
            if (isEnemyRocket)
            {
                // Düşman roketi hasarı
                damage = playerData.CalculateRocketDamage();
                
                // Oyunun başında roket hasarını azalt (oyuncu deneyimi için)
                float gameTime = Time.timeSinceLevelLoad;
                if (gameTime < 15.0f)
                {
                    float damageReduction = Mathf.Lerp(0.3f, 1.0f, gameTime / 15.0f);
                    damage = Mathf.FloorToInt(damage * damageReduction);
                    Debug.Log($"[RocketProjectile] Oyunun başı için roket hasarı azaltıldı: {damage} (Azaltma: {damageReduction:F2})");
                }
            }
            else
            {
                // Oyuncu roketi hasarı
                damage = playerData.anaGemiRoketDamage;
            }
        }
        
        // İlk hedefi bul
        FindTarget();
        
        // İlk hızını ayarla ve harekete başla
        rb.linearVelocity = transform.right * speed;
        
        // Başlatma tamamlandı
        isInitialized = true;
        
        // Debug bilgisi
        Debug.Log($"[RocketProjectile] Rocket oluşturuldu. Düşman roketi: {isEnemyRocket}, Hasar: {damage}, " +
                 $"Rigidbody2D: {rb != null}, Velocity: {rb.linearVelocity.magnitude:F2}");
    }
    
    private void Update()
    {
        // Eğer patlama modundaysa güncellemeleri durdur
        if (isExploding) return;
        
        // Başlatma tamamlanmadıysa bekle
        if (!isInitialized) return;
        
        // Menzil kontrolü - maksimum mesafeyi aştıysa yok et
        if (Vector3.Distance(transform.position, startPosition) > maxDistance)
        {
            Debug.Log($"Roket maksimum menzili aştı: {Vector3.Distance(transform.position, startPosition):F2} > {maxDistance}");
            Explode();
            return;
        }
        
        // Takip başladı mı kontrolü
        if (!isTracking && Time.time >= activationTime)
        {
            isTracking = true;
        }
        
        // Hedef arama güncellemesi
        UpdateTargetSearch();
        
        // Roketi hareket ettir
        MoveRocket();
    }
    
    private void UpdateTargetSearch()
    {
        // Hedef aramalarını zaman aralıklarıyla sınırla
        if (Time.time < lastTargetSearchTime + minTargetSearchCooldown) return;
        
        // Hedef kayboldu veya hedefleme zamanı geldi
        bool shouldSearchTarget = false;
        
        // Oyuncu roketleri daha sık hedef arar
        if (!isEnemyRocket && isTracking && Time.time >= lastTargetSearchTime + targetSearchInterval)
        {
            shouldSearchTarget = true;
        }
        
        // Düşman roketi için hedef kayboldu veya uygun değil
        if (isEnemyRocket && (target == null || (Player.isDead && target.CompareTag("Player"))))
        {
            shouldSearchTarget = true;
        }
        
        // Gerekiyorsa hedefi güncelle
        if (shouldSearchTarget)
        {
            FindTarget();
            lastTargetSearchTime = Time.time;
        }
    }
    
    private void FindTarget()
    {
        if (isEnemyRocket)
        {
            // Düşman roketi her zaman Zeplin'i öncelikli hedef alır
            Zeplin zeplin = FindObjectOfType<Zeplin>();
            if (zeplin != null)
            {
                target = zeplin.transform;
                return;
            }
            
            // Eğer Zeplin bulunamazsa, Player'ı hedef al (varsa)
            if (!Player.isDead)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }
        else
        {
            // Oyuncu roketi en yakın düşmanı hedef alır
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            float closestDistance = Mathf.Infinity;
            Transform closestEnemy = null;
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) continue;
                
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
            
            // En yakın düşmanı hedef al
            target = closestEnemy;
            
            // Eğer düşman bulunamadıysa, roket düz gitsin (hedefsiz)
            if (target == null)
            {
                isTracking = false;
            }
        }
    }
    
    private void MoveRocket()
    {
        // İleri doğru hareketi her zaman sağla (aktivasyon öncesi initialDirection'a göre)
        if (Time.time < activationTime)
        {
            // Aktivasyon öncesinde düz hareket
            if (rb != null && rb.linearVelocity.sqrMagnitude < 0.1f)
            {
                rb.linearVelocity = initialDirection * speed;
            }
            return;
        }
        
        // Rigidbody hareketini her zaman kontrol et
        if (rb != null && rb.linearVelocity.sqrMagnitude < 0.1f)
        {
            // Hız düşükse yeniden ayarla
            rb.linearVelocity = transform.right * speed;
        }
        
        // Eğer takip aktifse ve hedef varsa
        if (isTracking && target != null)
        {
            // Hedefin hala var olduğunu kontrol et
            if (!target.gameObject.activeInHierarchy)
            {
                // Hedef artık yok, yeniden ara
                target = null;
                FindTarget();
                return;
            }
            
            // Dönüş hızını düşman/oyuncu roketine göre ayarla
            float actualTurnSpeed = turnSpeed;
            if (isEnemyRocket)
            {
                // Düşman roketi daha az hızlı dönebilir (Oyuncu kaçış şansı olsun)
                actualTurnSpeed *= 0.8f;
            }
            else
            {
                // Oyuncu roketi daha hızlı dönebilir
                actualTurnSpeed *= 1.2f;
            }
            
            // Hedefe doğru yönelme (dönme)
            Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
            direction.Normalize();
            
            // Dönüş açısını hesapla
            float rotateAmount = Vector3.Cross(direction, transform.right).z;
            
            // Yumuşak dönüş uygula
            transform.Rotate(0, 0, -rotateAmount * actualTurnSpeed * Time.deltaTime * 100);
            
            // Rigidbody hızını transform.right yönüne ayarla
            rb.linearVelocity = transform.right * speed;
        }
        else
        {
            // Hedef yoksa düz git
            rb.linearVelocity = transform.right * speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Düşman roketlerinin, düşmana çarpmasını önle
        if (isEnemyRocket && (other.CompareTag("Enemy") || other.CompareTag("Boss")))
        {
            return;
        }
        
        // Dost roketler oyuncuya zarar vermez
        if (!isEnemyRocket && other.CompareTag("Player"))
        {
            return;
        }
        
        // Düşman roketler
        if (isEnemyRocket)
        {
            // Oyuncuya çarpma
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Düşman roketi Player'a direkt çarptı! (Trigger) Hasar: " + damage);
                
                // Patlama efekti
                Explode();
                
                return;
            }
            
            // Zeplin'e çarpma
            Zeplin zeplin = other.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                // Yeni metodu kullan
                OnHitZeplin(zeplin);
                return;
            }
        }
        
        // Dost roketler (oyuncu roketleri)
        else
        {
            // Düşmana çarpma
            if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log("Oyuncu roketi düşmana direkt çarptı! (Trigger) Hasar: " + damage);
                    
                    // Patlama efekti
                    Explode();
                    
                    return;
                }
            }
        }
        
        // Diğer engellere çarpma
        if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
        {
            Debug.Log("Roket zemine veya engele çarptı!");
            Explode();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Düşman roketlerinin, düşmana çarpmasını önle
        if (isEnemyRocket && (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss")))
        {
            return;
        }
        
        // Dost roketler oyuncuya zarar vermez
        if (!isEnemyRocket && collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        
        // Düşman roketler
        if (isEnemyRocket)
        {
            // Oyuncuya çarpma
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Düşman roketi direkt Player'a çarptı! (Collision) Hasar: " + damage);
                
                // Patlama efekti
                Explode();
                
                return;
            }
            
            // Zeplin'e çarpma
            Zeplin zeplin = collision.gameObject.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                // Yeni metodu kullan
                OnHitZeplin(zeplin);
                return;
            }
        }
        
        // Dost roketler (oyuncu roketleri)
        else
        {
            // Düşmana çarpma
            if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log("Oyuncu roketi düşmana direkt çarptı! (Collision) Hasar: " + damage);
                    
                    // Patlama efekti
                    Explode();
                    
                    return;
                }
            }
        }
        
        // Diğer engellere çarpma
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Roket zemine veya engele çarptı!");
            Explode();
        }
    }
    
    private void Explode()
    {
        // Eğer zaten patlama sürecindeyse tekrar patlamayı önle
        if (isExploding) return;
        
        // Patlama sürecini başlat
        isExploding = true;
        
        // Hareketi durdur
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Sprite gösterimini kapat (patlamadan sonra görünmesin)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Collider'ı devre dışı bırak
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Patlama efekti
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // Patlama yarıçapındaki tüm objeleri bul
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        // Patlama alanındaki her objeye hasar ver
        foreach (Collider2D hitCollider in hitObjects)
        {
            // Null kontrol
            if (hitCollider == null) continue;
            
            // Kendimize hasar vermeyelim
            if ((isEnemyRocket && (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("Boss"))) || 
                (!isEnemyRocket && hitCollider.CompareTag("Player")))
            {
                continue;
            }
            
            // Düşman roketi ise oyuncuya/zepline hasar ver
            if (isEnemyRocket)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    Player player = hitCollider.GetComponent<Player>();
                    if (player != null)
                    {
                        player.TakeDamage(damage);
                        Debug.Log("Oyuncuya düşman raketiyle " + damage + " hasar verildi!");
                    }
                }
                else if (hitCollider.GetComponent<Zeplin>() != null)
                {
                    Zeplin zeplin = hitCollider.GetComponent<Zeplin>();
                    if (zeplin != null)
                    {
                        zeplin.TakeDamage(damage);
                        Debug.Log("Zeplin'e düşman raketiyle " + damage + " hasar verildi!");
                    }
                }
            }
            // Oyuncu roketi ise düşmanlara hasar ver
            else 
            {
                if (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("Boss"))
                {
                    Enemy enemy = hitCollider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                        Debug.Log("Düşmana roketle " + damage + " hasar verildi!");
                    }
                }
            }
        }
        
        // Roket objesini yok et (kısa bir gecikme ile patlama efektinin görünmesine izin ver)
        Destroy(gameObject, 0.1f);
    }
    
    // Patlama yarıçapını görselleştir (editor için)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    // RocketProjectile'ın bağımsız bir şekilde eklenmesi için statik yardımcı metot
    public static RocketProjectile EnsureRocketComponentExists(GameObject rocketObject)
    {
        if (rocketObject == null)
        {
            Debug.LogError("RocketProjectile.EnsureRocketComponentExists: Roket objesi null!");
            return null;
        }
        
        // RocketProjectile bileşenini kontrol et
        RocketProjectile rocketComponent = rocketObject.GetComponent<RocketProjectile>();
        if (rocketComponent == null)
        {
            Debug.Log("RocketProjectile bileşeni bulunamadı, otomatik ekleniyor: " + rocketObject.name);
            
            // Bileşeni ekle
            rocketComponent = rocketObject.AddComponent<RocketProjectile>();
            
            // Varsayılan değerleri ayarla
            rocketComponent.speed = 8f;
            rocketComponent.turnSpeed = 3f;
            rocketComponent.lifetime = 5f;
            rocketComponent.activationDelay = 0.5f;
            rocketComponent.explosionRadius = 3f;
            rocketComponent.damage = 30;
            
            // Rigidbody kontrolü
            Rigidbody2D rb = rocketObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = rocketObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            }
            
            // Collider kontrolü
            Collider2D collider = rocketObject.GetComponent<Collider2D>();
            if (collider == null)
            {
                BoxCollider2D boxCollider = rocketObject.AddComponent<BoxCollider2D>();
                boxCollider.isTrigger = false;
            }
            
            Debug.Log("RocketProjectile bileşeni başarıyla eklendi ve ayarlandı: " + rocketObject.name);
        }
        
        return rocketComponent;
    }

    public void OnHitZeplin(Zeplin zeplin)
    {
        if (zeplin != null)
        {
            // Zeplin için hasarı azalt
            int adjustedDamage = Mathf.FloorToInt(damage * 0.6f); // Hasarı %60'a düşür
            zeplin.TakeDamage(adjustedDamage);
            Debug.Log("Düşman roketi Zeplin'e " + adjustedDamage + " hasar verdi! (Orijinal hasar: " + damage + ")");
        }
        
        // Patlama efekti
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            // 1.5 saniye sonra patlama efektini yok et
            Destroy(explosion, 1.5f);
        }
        
        // Roketi yok et
        Destroy(gameObject);
    }

    public void HandleHit(GameObject targetObject)
    {
        if (targetObject == null) return;
        
        // Düşman roketi
        if (isEnemyRocket)
        {
            // Oyuncuya çarpma
            Player player = targetObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Oyuncuya düşman raketiyle " + damage + " hasar verildi!");
                return;
            }
            
            // Zeplin'e çarpma
            Zeplin zeplin = targetObject.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                // Yeni metodu kullan
                OnHitZeplin(zeplin);
                return;
            }
        }
        // Oyuncu roketi
        else
        {
            // Düşmana çarpma
            Enemy enemy = targetObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Düşmana roketle " + damage + " hasar verildi!");
                return;
            }
        }
    }
} 