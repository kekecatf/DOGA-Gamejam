using UnityEngine;

public class RocketProjectile : MonoBehaviour
{
    public float speed = 5f;                  // Roket hızı
    public float turnSpeed = 3f;              // Dönüş hızı
    public float lifetime = 5f;               // Roketin ömrü (saniye)
    public float activationDelay = 0.5f;      // Hedef takibine başlamadan önceki gecikme
    public float explosionRadius = 2f;        // Patlama yarıçapı
    public int damage = 50;                   // Hasar miktarı
    public bool isEnemyRocket = false;        // Düşman roketi mi?
    
    public GameObject explosionEffect;        // Patlama efekti prefabı
    
    private Transform target;                 // Hedef (oyuncu, düşman veya zeplin)
    private bool isTracking = false;          // Hedef takip edilmeye başlandı mı
    private float activationTime;             // Takibe başlama zamanı
    private PlayerData playerData;            // Oyuncu veri referansı
    private float lastTargetSearchTime = 0f;  // Son hedef arama zamanı
    private float targetSearchInterval = 0.5f; // Hedef arama sıklığı
    
    private void Start()
    {
        Debug.Log($"[RocketProjectile] Start metodu başladı. GameObject: {gameObject.name}, isEnemyRocket: {isEnemyRocket}");
        
        // Set the appropriate tag
        gameObject.tag = "Rocket";
        
        // Rigidbody ayarlarını kontrol et
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
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
        
        // Collider kontrolü
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // Collider yoksa ekle (Rocket genelde daha uzun olduğu için BoxCollider kullan)
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = false; // Fiziksel çarpışma için trigger kapalı
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
            }
            else
            {
                // Oyuncu roketi hasarı
                damage = playerData.anaGemiRoketDamage;
            }
        }
        
        // İlk hedefi bul
        FindTarget();
        
        // Debug bilgisi
        Debug.Log($"[RocketProjectile] Rocket oluşturuldu. Düşman roketi: {isEnemyRocket}, Hasar: {damage}, " +
                 $"Rigidbody2D: {rb != null}, Collider: {collider != null}, isTrigger: {(collider != null ? collider.isTrigger : false)}");
    }
    
    private void Update()
    {
        // Roketi hareket ettir
        MoveRocket();
        
        // Takip başladı mı kontrolü
        if (!isTracking && Time.time >= activationTime)
        {
            isTracking = true;
        }
        
        // Oyuncu roketleri için periyodik olarak en yakın hedefi güncelle
        if (!isEnemyRocket && isTracking && Time.time >= lastTargetSearchTime + targetSearchInterval)
        {
            FindTarget();
            lastTargetSearchTime = Time.time;
        }
        
        // Düşman roketi için hedef öldüyse veya yok olduysa hedefi güncelle
        if (isEnemyRocket && (target == null || (Player.isDead && target.CompareTag("Player"))))
        {
            FindTarget();
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
        // İleri doğru hareketi her zaman sağla
        transform.Translate(Vector2.right * speed * Time.deltaTime, Space.Self);
        
        // Eğer takip aktifse ve hedef varsa
        if (isTracking && target != null)
        {
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
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Roket trigger çarpışma: " + other.gameObject.name + " (Tag: " + other.tag + "), Düşman Roketi: " + isEnemyRocket);
        
        // Kendimizi yok etmeyelim (oyuncu roketi oyuncuya, düşman roketi düşmana çarpmasın)
        if ((isEnemyRocket && other.CompareTag("Enemy")) || 
            (!isEnemyRocket && other.CompareTag("Player")))
        {
            return;
        }
        
        // Özellikle Zeplin ile çarpışma kontrolü
        if (isEnemyRocket && (other.CompareTag("Zeplin") || other.GetComponent<Zeplin>() != null))
        {
            // Zeplin'e roketi direkt çarptırınca patlasın ve hasar versin
            Zeplin zeplin = other.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                Debug.Log("Düşman roketi direkt Zeplin'e çarptı! (Trigger) Hasar: " + damage);
                zeplin.TakeDamage(damage);
            }
        }
        
        // Patlama oluştur
        Explode();
        
        // Roket objesini yok et
        Destroy(gameObject);
    }
    
    // Fiziksel çarpışmalar için
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Roket fiziksel çarpışma: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + "), Düşman Roketi: " + isEnemyRocket);
        
        // Kendimizi yok etmeyelim (oyuncu roketi oyuncuya, düşman roketi düşmana çarpmasın)
        if ((isEnemyRocket && collision.gameObject.CompareTag("Enemy")) || 
            (!isEnemyRocket && collision.gameObject.CompareTag("Player")))
        {
            return;
        }
        
        // Özellikle Zeplin ile çarpışma kontrolü
        if (isEnemyRocket && (collision.gameObject.CompareTag("Zeplin") || collision.gameObject.GetComponent<Zeplin>() != null))
        {
            // Zeplin'e roketi direkt çarptırınca patlasın ve hasar versin
            Zeplin zeplin = collision.gameObject.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                Debug.Log("Düşman roketi direkt Zeplin'e çarptı! (Collision) Hasar: " + damage);
                zeplin.TakeDamage(damage);
            }
        }
        
        // Patlama oluştur
        Explode();
        
        // Roket objesini yok et
        Destroy(gameObject);
    }
    
    private void Explode()
    {
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
            // Kendimize hasar vermeyelim
            if ((isEnemyRocket && hitCollider.CompareTag("Enemy")) || 
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
                else if (hitCollider.CompareTag("Zeplin") || hitCollider.GetComponent<Zeplin>() != null)
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
                if (hitCollider.CompareTag("Enemy"))
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
} 