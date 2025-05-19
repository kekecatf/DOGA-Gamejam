using UnityEngine;
using System.Collections;

public enum EnemyType
{
    Kamikaze,   // Kamikazeler hedefe doğru gider ve çarparak hasar verir
    Minigun     // Minigun düşmanları uzaktan ateş eder
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
    public float attackRange = 5f;    // Saldırı menzili (Minigun için)
    public float safeDistance = 4f;   // Güvenli mesafe (Minigun düşmanları için)
    
    [Header("Ateş Ayarları")]
    public GameObject bulletPrefab;   // Minigun mermisi
    public Transform firePoint;       // Ateş noktası
    public float fireRate = 2f;       // Ateş hızı (saniyede kaç kez)
    private float nextFireTime = 0f;  // Bir sonraki ateş zamanı
    public float initialStabilizationTime = 1.5f; // Düşman spawn olduktan sonra ateş etmeden önce geçecek süre
    private float spawnTime; // Düşmanın spawn olduğu zaman
    
    [Header("Mesafe Ayarları")]
    public float minigunAttackRange = 8f;  // Minigun ile ateş etme mesafesi
    public float minigunSafeDistance = 6f; // Minigun için güvenli mesafe
    public float minAttackDistance = 3f;   // Minimum saldırı mesafesi - çok yakınsa ateş etmez
    
    [Header("Hedef")]
    protected Vector2 targetPosition = Vector2.zero;  // Hedef pozisyonu
    protected Transform targetTransform; // Hedef transform değişkenini protected yapalım
    
    protected SpriteRenderer spriteRenderer;
    protected PlayerData playerData;
    
    protected int currentHealth = 100;
    protected int damage = 10;
    public int scoreValue = 25;
    public GameObject deathEffect;    // Ölüm efekti (isteğe bağlı)
    
    [Header("Hasar Ayarları")]
    public int minigunBulletDamage = 5;  // Minigun mermisinin verdiği hasar
    
    protected virtual void Start()
    {
        // Spawn zamanını kaydet
        spawnTime = Time.time;
        
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Rastgele başlangıç hızı varyasyonu (daha doğal görünüm için)
        moveSpeed = Random.Range(moveSpeed * 0.8f, moveSpeed * 1.2f);
        
        // Collider kontrolü ve ayarları
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // Collider yoksa ekle
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            
            // Eğer Kamikaze düşman ise trigger olarak ayarla (daha iyi çarpışma tespiti için)
            if (enemyType == EnemyType.Kamikaze)
            {
                boxCollider.isTrigger = true;
                Debug.Log("Kamikaze düşman için BoxCollider2D eklendi (isTrigger=true).");
            }
            else
            {
                // Diğer düşman tipleri için normal collider
                boxCollider.isTrigger = false;
                Debug.Log("Düşman için BoxCollider2D eklendi (isTrigger=false).");
            }
        }
        else
        {
            // Kamikaze düşmanlar için collider'ı trigger olarak ayarla
            if (enemyType == EnemyType.Kamikaze && !collider.isTrigger)
            {
                collider.isTrigger = true;
                Debug.Log("Kamikaze düşman collider'ı trigger olarak ayarlandı.");
            }
        }
        
        // FirePoint kontrolü
        if (firePoint == null)
        {
            // FirePoint yoksa oluştur
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(0.5f, 0, 0); // Sağa doğru 0.5 birim
            firePoint = firePointObj.transform;
        }
        
        // Death Effect kontrolü
        if (deathEffect == null)
        {
            // Death Effect prefabını bul
            GameObject deathEffectPrefab = Resources.Load<GameObject>("DeathEffectPrefab");
            if (deathEffectPrefab == null)
            {
                // Resources'ta yoksa, Project'te ara
                deathEffectPrefab = GameObject.Find("DeathEffectPrefab");
            }
            
            // Prefabı bulduysa ata
            if (deathEffectPrefab != null)
            {
                deathEffect = deathEffectPrefab;
                Debug.Log("Düşmana otomatik olarak death effect prefabı atandı");
            }
        }
        
        // Düşman değerlerini PlayerData'dan al
        InitializeEnemyStats();
    }
    
    // Düşman değerlerini PlayerData'dan alma
    private void InitializeEnemyStats()
    {
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        
        // Player data null değilse, düşman istatistiklerini PlayerData'dan al
        if (playerData != null)
        {
            // Sağlık ve hasar değerlerini PlayerData'ya göre ayarla
            currentHealth = playerData.CalculateEnemyHealth();
            
            // Düşman tipine göre hasar değerini belirle
            switch (enemyType)
            {
                case EnemyType.Kamikaze:
                    currentHealth = playerData.CalculateEnemyHealth() / 2; // Daha az HP
                    damage = playerData.CalculateKamikazeDamage();
                    
                    // Kamikazenin hasarını sınırla
                    damage = Mathf.Min(damage, 20); // Maksimum 20 hasar
                    
                    moveSpeed *= 1.3f; // Daha hızlı
                    // Kamikaze için atış hızı yok
                    break;
                    
                case EnemyType.Minigun:
                    currentHealth = playerData.CalculateEnemyHealth();
                    damage = playerData.CalculateMinigunDamage();
                    minigunBulletDamage = damage;
                    moveSpeed *= 0.8f; // Daha yavaş
                    // Atış hızını PlayerData'dan al
                    fireRate = playerData.CalculateMinigunFireRate();
                    // Minigun saldırı mesafesini ayarla
                    attackRange = minigunAttackRange;
                    safeDistance = minigunSafeDistance;
                    break;
            }
            
            // Ödül değerini PlayerData'dan al
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
            }
            
            Debug.Log($"{enemyType} düşmanı oluşturuldu. Sağlık: {currentHealth}, Hasar: {damage}, " +
                      $"Ateş Hızı: {fireRate}, Hareket Hızı: {moveSpeed}, Skor Değeri: {scoreValue}");
        }
        else
        {
            // PlayerData bulunamadıysa varsayılan değerleri kullan
            currentHealth = 50;
            
            switch (enemyType)
            {
                case EnemyType.Kamikaze:
                    damage = 10; // Reduced from 20 to 10
                    moveSpeed *= 1.3f; // Daha hızlı
                    // Kamikaze için atış hızı yok
                    break;
                    
                case EnemyType.Minigun:
                    damage = 5;
                    minigunBulletDamage = damage;
                    moveSpeed *= 0.8f; // Daha yavaş
                    fireRate = 2f; // Varsayılan ateş hızı
                    // Minigun saldırı mesafesini ayarla
                    attackRange = minigunAttackRange;
                    safeDistance = minigunSafeDistance;
                    break;
            }
            
            Debug.LogWarning("PlayerData bulunamadı! Varsayılan düşman değerleri kullanılıyor.");
        }
    }
    
    protected virtual void Update()
    {
        // Check if enemy has fallen below y=-100
        if (transform.position.y < -100f)
        {
            Destroy(gameObject);
            return;
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
                RangedEnemyBehavior();
                break;
        }
        
        UpdateSpriteFlipping();
    }
    
    // Uzak mesafe düşman davranışı (Minigun için)
    private void RangedEnemyBehavior()
    {
        if (targetTransform == null) return;
        
        // Hedefe olan mesafeyi hesapla
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        
        // Hedefin Zeplin olup olmadığını kontrol et
        bool isTargetingZeplin = targetTransform.GetComponent<Zeplin>() != null;
        
        // Düşman tipi için uzaklık (Minigun düşmanları için)
        float optimalDistance = 8.0f;
        float minDistance = 6.0f;
        
        // Düşman tipi için ateş etme mesafesi
        float firingRange = minigunAttackRange;
        
        // Zeplin hedefleniyorsa ateş mesafesini artır
        if (isTargetingZeplin)
        {
            // Zeplin'i daha uzaktan vurmak için
            optimalDistance = 10.0f;
            // Ateş menzilini de artır
            firingRange *= 1.5f;
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
        
        // Ateş etme kontrolü - şartlar uygun mu kontrol et
        if (Time.time >= nextFireTime)
        {
            // Stabilizasyon süresi kontrolü - düşman yeni spawn olduysa ateş etme
            bool isStabilized = (Time.time - spawnTime) >= initialStabilizationTime;
            
            // Menzil kontrolü - düşman hedefe yeterince yakın mı?
            bool isInFiringRange = distanceToTarget <= firingRange;
            
            // Minimum mesafe kontrolü - düşman hedefe çok yakın değil mi?
            bool isNotTooClose = distanceToTarget >= minAttackDistance;
            
            // Ateş etme koşulları - hem stabilize olmuş hem de menzilde olmalı ve çok yakın olmamalı
            if (isStabilized && isInFiringRange && isNotTooClose)
            {
                // Zeplin'e karşı daha az sıklıkta ateş etsin - 0.7x daha yavaş
                float fireRateMultiplier = isTargetingZeplin ? 0.7f : 1.0f;
                
                // Ateş et
                FireMinigun();
                
                // Bir sonraki ateş zamanını ayarla
                nextFireTime = Time.time + (1f / (fireRate * fireRateMultiplier));
            }
            else
            {
                // Ateş etme koşulları sağlanmadı
                if (!isStabilized && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"{enemyType} düşmanı henüz stabilize olmadı, ateş edilmiyor. " +
                              $"Kalan süre: {initialStabilizationTime - (Time.time - spawnTime):F1} saniye");
                }
                
                if (!isInFiringRange && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"{enemyType} düşmanı menzil dışında, ateş edilmiyor. " +
                              $"Mesafe: {distanceToTarget:F1}, Gereken menzil: {firingRange:F1}");
                }
                
                if (!isNotTooClose && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"{enemyType} düşmanı hedefe çok yakın, ateş edilmiyor. " +
                              $"Mesafe: {distanceToTarget:F1}, Minimum mesafe: {minAttackDistance:F1}");
                }
            }
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
        
        // Zeplin'e karşı hasar ve hız ayarlaması
        if (isTargetingZeplin)
        {
            bulletDamage = Mathf.FloorToInt(bulletDamage * 0.75f); // Zeplin'e daha az hasar (önceki 3.0f yerine 0.75f)
            bulletComponent.speed *= 1.25f; // Hızı hafif arttır (önceki 1.5f yerine 1.25f)
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
    
    // Hedef pozisyonunu güncelle
    private void UpdateTargetPosition()
    {
        // Hedefi güncellemeden önce hedefin mevcut konumunu kaydet
        Vector2 previousTargetPosition = targetPosition;
        bool hadTarget = targetTransform != null;
        
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
                
                // Normal durumlarda oyuncuyu hedef al
                targetPosition = player.transform.position;
            }
        }
    }
    
    protected virtual void MoveTowardsTarget()
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
        else if (targetTransform.GetComponent<Zeplin>() != null)
        {
            Zeplin zeplin = targetTransform.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                // Zeplin'e daha az hasar verelim
                int adjustedDamage = Mathf.FloorToInt(damage * 0.5f); // Hasar %50'ye düşürüldü (0.7f yerine 0.5f)
                zeplin.TakeDamage(adjustedDamage);
                Debug.Log("Kamikaze düşman Zeplin'e çarptı ve " + adjustedDamage + " hasar verdi! (önceki: " + damage + ")");
            }
        }
        
        // Düşmanı yok et
        Die();
    }
    
    public virtual void TakeDamage(int damageAmount)
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
    
    protected virtual void Die()
    {
        // Add Rigidbody2D for falling physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure Rigidbody2D for falling
        rb.gravityScale = 1f;
        rb.mass = 1f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Add some random rotation and velocity for more natural falling
        float randomRotation = Random.Range(-180f, 180f);
        rb.angularVelocity = randomRotation;
        
        // Add some random horizontal velocity
        float randomHorizontalForce = Random.Range(-2f, 2f);
        rb.linearVelocity = new Vector2(randomHorizontalForce, rb.linearVelocity.y);
        
        // Disable enemy movement and collision
        GetComponent<Collider2D>().enabled = true; // Keep collider enabled for physics
        
        // Start death animation and effects
        StartCoroutine(PlayDeathAnimation());
    }
    
    IEnumerator PlayDeathAnimation()
    {
        // Ölüm sesi çal
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosionSound();
        }
        
        // Ölüm animasyonu veya efekti
        if (deathEffect != null)
        {
            // Efekti oluştur
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            
            // Efektin boyutunu düşmanla benzer yap (isteğe bağlı)
            effect.transform.localScale = transform.localScale;
            
            // Efektin Animator bileşenini kontrol et
            Animator animator = effect.GetComponent<Animator>();
            if (animator != null)
            {
                // Animator varsa, animasyon süresi kadar bekle
                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    // Animasyon süresini al
                    float animationDuration = clipInfo[0].clip.length;
                    Debug.Log("Ölüm animasyonu süresi: " + animationDuration);
                    
                    // Düşmanı gizle (sprite renderer'ı devre dışı bırak)
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = false;
                    }
                    
                    // Animasyon süresi kadar bekle
                    yield return new WaitForSeconds(animationDuration);
                }
                else
                {
                    // Animasyon bilgisi yoksa varsayılan süre kadar bekle
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                // DeathEffect script süresi kadar bekle
                DeathEffect deathEffectScript = effect.GetComponent<DeathEffect>();
                float waitTime = (deathEffectScript != null) ? deathEffectScript.lifetime : 0.5f;
                
                // Düşmanı gizle
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
                
                yield return new WaitForSeconds(waitTime);
            }
        }
        else
        {
            // Efekt yoksa kısa bir süre bekle
            yield return new WaitForSeconds(0.3f);
        }
        
        // GameManager'a öldürülen düşmanı bildir
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyKilled(enemyType);
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
        
        // Try to use ItemDropManager if available
        ItemDropManager dropManager = ItemDropManager.Instance;
        if (dropManager != null)
        {
            // Use the manager to try dropping an item
            dropManager.TryDropItem(transform.position);
        }
        else
        {
            // Fallback: Calculate whether to drop an item (20% chance)
            if (Random.Range(0f, 100f) <= 20f)
            {
                // Try to load the item prefab from Resources
                GameObject itemPrefab = Resources.Load<GameObject>("ItemPrefab");
                if (itemPrefab != null)
                {
                    // Spawn the item at the enemy's position
                    GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
                    Debug.Log("Item dropped at position: " + transform.position);
                }
                else
                {
                    Debug.LogWarning("ItemPrefab not found in Resources folder. Create a prefab named 'ItemPrefab' and place it in a Resources folder.");
                }
            }
        }
        
        // Düşmanı yok et
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enemy OnTriggerEnter2D: " + gameObject.name + " triggered with " + collision.gameObject.name + " (Tag: " + collision.tag + ")");
        
        // Sadece kamikaze düşmanlar için çarpışma tespiti
        if (enemyType == EnemyType.Kamikaze)
        {
            // Oyuncuyla çarpışma
            if (collision.CompareTag("Player"))
            {
                // Kamikaze sesi çal
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayKamikazeSound();
                }
                
                Player player = collision.GetComponent<Player>();
                if (player != null)
                {
                    // Oyuncuya hasar vermeden önce debug log
                    Debug.Log("Kamikaze düşman Player'a hasar vermeye çalışıyor. Hasar: " + damage);
                    
                    // Oyuncuya doğrudan hasar ver
                    player.TakeDamage(damage);
                    
                    // Başarılı log
                    Debug.Log("Kamikaze düşman Player'a çarptı ve " + damage + " hasar verdi! Player sağlık: " + 
                              (player.GetComponent<PlayerData>() != null ? player.GetComponent<PlayerData>().anaGemiSaglik.ToString() : "bilinmiyor"));
                    
                    // Düşmanı yok et
                    Die();
                }
                else
                {
                    Debug.LogError("Player tagine sahip objede Player bileşeni bulunamadı!");
                }
            }
            // Zeplinle çarpışma
            else if (collision.GetComponent<Zeplin>() != null)
            {
                // Kamikaze sesi çal
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayKamikazeSound();
                }
                
                Zeplin zeplin = collision.GetComponent<Zeplin>();
                if (zeplin != null)
                {
                    // Zeplin'e daha az hasar verelim
                    int adjustedDamage = Mathf.FloorToInt(damage * 0.5f); // Hasar %50'ye düşürüldü (0.7f yerine 0.5f)
                    zeplin.TakeDamage(adjustedDamage);
                    Debug.Log("Kamikaze düşman Zeplin'e çarptı ve " + adjustedDamage + " hasar verdi! (önceki: " + damage + ")");
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
        
        // Roketle çarpışma
        if (collision.CompareTag("Rocket"))
        {
            RocketProjectile rocket = collision.GetComponent<RocketProjectile>();
            if (rocket != null && !rocket.isEnemyRocket)
            {
                // Roket ile daha fazla hasar al
                Debug.Log($"Düşman roket ile vuruldu! Roket hasarı: {rocket.damage}");
                TakeDamage(rocket.damage);
                
                // Roketi patlat
                rocket.HandleHit(gameObject);
                
                // Roket log
                Debug.Log($"Roket düşmana çarptı ve {rocket.damage} hasar verdi!");
            }
        }
    }
    
    // Fiziksel çarpışma işleme
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Enemy OnCollisionEnter2D: " + gameObject.name + " collided with " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
        
        // Roketle çarpışma kontrolü
        if (collision.gameObject.CompareTag("Rocket"))
        {
            RocketProjectile rocket = collision.gameObject.GetComponent<RocketProjectile>();
            if (rocket != null && !rocket.isEnemyRocket)
            {
                // Roket ile daha fazla hasar al
                Debug.Log($"Düşman roket ile vuruldu (Collision)! Roket hasarı: {rocket.damage}");
                TakeDamage(rocket.damage);
                
                // Roketi patlat
                rocket.HandleHit(gameObject);
                
                // Roket log
                Debug.Log($"Roket düşmana çarptı (Collision) ve {rocket.damage} hasar verdi!");
                
                return; // Başka işlem yapma
            }
        }
        
        // Kamikaze düşmanlar dışındakiler için veya yedek çarpışma kontrolü olarak
        if (collision.gameObject.CompareTag("Player"))
        {
            // Kamikaze ise ses çal
            if (enemyType == EnemyType.Kamikaze && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayKamikazeSound();
            }
            
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                // Oyuncuya hasar vermeden önce debug log
                Debug.Log("Düşman Player'a hasar vermeye çalışıyor (Collision). Hasar: " + damage);
                
                // Oyuncuya hasar ver
                player.TakeDamage(damage);
                
                // Başarılı log
                Debug.Log("Düşman Player'a çarptı ve " + damage + " hasar verdi! (OnCollisionEnter2D)");
                
                // Kamikaze ise kendini imha et
                if (enemyType == EnemyType.Kamikaze)
                {
                    Die();
                }
            }
            else
            {
                Debug.LogError("Player tagine sahip objede Player bileşeni bulunamadı! (OnCollisionEnter2D)");
            }
        }
        
        // Zeplinle çarpışma
        if (collision.gameObject.CompareTag("Zeplin") || collision.gameObject.GetComponent<Zeplin>() != null)
        {
            // Kamikaze ise ses çal
            if (enemyType == EnemyType.Kamikaze && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayKamikazeSound();
            }
            
            Zeplin zeplin = collision.gameObject.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                // Zeplin'e daha az hasar verelim
                int adjustedDamage = damage;
                
                if (enemyType == EnemyType.Kamikaze)
                {
                    adjustedDamage = Mathf.FloorToInt(damage * 0.5f); // Kamikaze için hasar %50'ye düşürüldü
                }
                else
                {
                    adjustedDamage = Mathf.FloorToInt(damage * 0.7f); // Diğer düşmanlar için hasar %70'e düşürüldü
                }
                
                zeplin.TakeDamage(adjustedDamage);
                Debug.Log("Düşman Zeplin'e çarptı ve " + adjustedDamage + " hasar verdi! (önceki: " + damage + ") (OnCollisionEnter2D)");
                
                // Kamikaze ise kendini imha et
                if (enemyType == EnemyType.Kamikaze)
                {
                    Die();
                }
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

    protected virtual void Fire()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // Mermiyi oluştur
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            
            // Mermi bileşenini bul ve hasarı ayarla
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.damage = minigunBulletDamage;
            }
        }
    }
} 