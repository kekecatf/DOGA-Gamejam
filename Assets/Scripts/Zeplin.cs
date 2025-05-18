using UnityEngine;
using UnityEngine.UI; // UI elemanları için

public class Zeplin : MonoBehaviour
{
    // UI elemanları
    [Header("UI Elemanları")]
    public Slider healthSlider; // Opsiyonel sağlık çubuğu
    public Text healthText; // Opsiyonel sağlık metni
    
    // Efektler
    [Header("Efektler")]
    public GameObject damageEffect; // Hasar alınca çıkacak efekt (opsiyonel)
    public GameObject destroyEffect; // Yok olunca çıkacak efekt (opsiyonel)
    
    // Hareket ayarları
    [Header("Hareket Ayarları")]
    public float moveSpeed = 4f; // Zeplin hareket hızı
    public Joystick joystick; // Joystick referansı
    
    // Ateş etme ayarları
    [Header("Silah Ayarları")]
    public Transform firePoint; // Mermi çıkış noktası
    public GameObject bulletPrefab; // Mermi prefab'ı
    public GameObject rocketPrefab; // Roket prefab'ı
    private float nextMinigunFireTime = 0f; // Sonraki ateş zamanı
    private float nextRocketFireTime = 0f; // Sonraki roket zamanı
    
    // UI Butonları
    [Header("UI Butonları")]
    public Button minigunButton; // Minigun butonu
    public Button rocketButton; // Roket butonu
    
    // Yön kontrolü
    private SpriteRenderer spriteRenderer;
    private bool isFacingLeft = false;
    private Vector3 originalFirePointLocalPos;
    
    // PlayerData referansı
    private PlayerData playerData;
    private int maxHealth; // Maksimum sağlık değerini saklamak için
    
    // Hasar kontrolü
    [Header("Hasar Kontrolü")]
    public float invincibilityTime = 0.2f; // Hasar sonrası dokunulmazlık süresi - 0.2 saniye (çok kısa)
    public bool useInvincibility = false;  // Dokunulmazlık özelliğini kullan mı?
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;
    
    // Kontrol durumu
    private bool isControlActive = false; // Zeplin kontrol edilebilir mi?
    
    // Kontrol geçiş efektleri
    [Header("Kontrol Geçiş Efektleri")]
    public GameObject activationEffect; // Kontrol aktifleştirildiğinde gösterilecek efekt
    public float activationHighlightDuration = 1.5f; // Aktifleştirme vurgusu süresi
    private float activationHighlightTimer = 0f;
    private bool isActivationHighlightActive = false;
    
    void Start()
    {
        // Set the tag to "Zeplin" to ensure proper collision detection
        gameObject.tag = "Zeplin";
        
        // Collider bileşenini kontrol et
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError("Zeplin'de Collider2D bileşeni bulunamadı! Çarpışma algılaması çalışmayacak.");
        }
        else
        {
            // Trigger ayarını düzelt - çarpışma için is Trigger FALSE olmalı
            if (collider.isTrigger)
            {
                Debug.LogWarning("Zeplin Collider2D IsTrigger açık, hasarlar algılanmayabilir. IsTrigger kapatılıyor.");
                collider.isTrigger = false;
            }
            
            Debug.Log("Zeplin Collider2D IsTrigger: " + collider.isTrigger);
            
            // Zeplin'in Rigidbody2D bileşeni var mı kontrol et
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("Zeplin'de Rigidbody2D bileşeni bulunamadı. Ekleniyor...");
                // Otomatik olarak ekle
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            
            // Rigidbody ayarlarını düzelt
            rb.bodyType = RigidbodyType2D.Kinematic; // Dynamic yerine Kinematic yaparak fizik etkilerini devre dışı bırak
            rb.gravityScale = 0f; // Yerçekimini kapat
            rb.interpolation = RigidbodyInterpolation2D.None;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            
            Debug.Log("Zeplin'in Rigidbody2D ayarları düzeltildi: Kinematic, yerçekimsiz");
        }
        
        // Başlangıçta kontrol devre dışı
        isControlActive = false;
        
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("PlayerData bulunamadı! Zeplin düzgün çalışmayabilir.");
            return;
        }
        
        // Maksimum sağlık değerini kaydet
        maxHealth = playerData.zeplinSaglik;
        
        // UI elemanlarını güncelle
        UpdateUI();
        
        // SpriteRenderer bileşenini al
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer bileşeni bulunamadı!");
        }
        
        // Fire point kontrolü
        if (firePoint == null)
        {
            // Eğer fire point atanmamışsa, zeplinin merkezini kullan
            firePoint = transform;
            originalFirePointLocalPos = Vector3.zero;
        }
        else
        {
            // FirePoint'in orijinal pozisyonunu kaydet
            originalFirePointLocalPos = new Vector3(firePoint.localPosition.x, 0, 0);
        }
        
        // Joystick kontrolü
        if (joystick == null)
        {
            // Sahnedeki joystick'i otomatik bul
            joystick = FindObjectOfType<Joystick>();
            if (joystick == null)
            {
                Debug.LogWarning("Joystick bulunamadı! Inspector'dan atayın veya Dynamic Joystick'in sahnede olduğundan emin olun.");
            }
        }
        
        // Minigun butonu kontrolü
        if (minigunButton != null)
        {
            // Butona tıklama olayı ekle
            minigunButton.onClick.AddListener(FireMinigun);
        }
        else
        {
            Debug.LogWarning("MinigunButton atanmamış! Inspector'dan atayın.");
        }
        
        // Roket butonu kontrolü
        if (rocketButton != null)
        {
            // Butona tıklama olayı ekle
            rocketButton.onClick.AddListener(FireRocket);
        }
        else
        {
            Debug.LogWarning("RocketButton atanmamış! Inspector'dan atayın.");
        }
    }
    
    void Update()
    {
        // Kontrol aktif değilse ve oyuncu ölmemişse hiçbir şey yapma
        if (!isControlActive && !Player.isDead) return;
        
        // Oyuncu öldüyse ve kontrol aktif değilse, kontrolü aktifleştir
        if (Player.isDead && !isControlActive)
        {
            ActivateZeplinControl();
        }
        
        // Hareket
        if (isControlActive)
        {
            Movement();
            
            // Test için klavye ile ateş etme (Space veya Z = minigun, R veya X = roket)
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z))
            {
                FireMinigun();
            }
            
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.X))
            {
                FireRocket();
            }
        }
        
        // Dokunulmazlık süresi kontrolü (sadece kullanılıyorsa)
        if (useInvincibility && isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // Periyodik olarak dokunulmazlık süresini logla
            if (Time.frameCount % 60 == 0) // Her 60 karede bir (yaklaşık 1 saniyede bir)
            {
                Debug.Log($"Zeplin dokunulmazlık süresi: {invincibilityTimer:F1} saniye kaldı");
            }
            
            // Yanıp sönme efekti
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = Time.time % 0.2f < 0.1f;
            }
            
            // Dokunulmazlık süresi bittiyse
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
                Debug.Log("Zeplin dokunulmazlık süresi bitti! Artık hasara açık.");
            }
        }
        
        // Aktifleştirme vurgusu kontrolü
        if (isActivationHighlightActive)
        {
            activationHighlightTimer -= Time.deltaTime;
            
            // Vurgu efekti (parlama veya renk değişimi)
            if (spriteRenderer != null)
            {
                // Zamanla azalan parlama efekti
                float pulseIntensity = Mathf.PingPong(Time.time * 4, 1.0f);
                spriteRenderer.color = Color.Lerp(Color.white, Color.yellow, pulseIntensity * 0.5f);
            }
            
            // Vurgu süresi bittiyse
            if (activationHighlightTimer <= 0)
            {
                isActivationHighlightActive = false;
                // Normal renge dön
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
            }
        }
    }
    
    // Zeplin kontrolünü aktifleştir (Player öldüğünde çağrılır)
    public void ActivateZeplinControl()
    {
        if (isControlActive) return; // Zaten aktifse bir şey yapma
        
        isControlActive = true;
        Debug.Log("Zeplin kontrolü aktifleştirildi!");
        
        // Aktifleştirme efekti göster
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }
        
        // Aktifleştirme vurgusunu başlat
        isActivationHighlightActive = true;
        activationHighlightTimer = activationHighlightDuration;
        
        // Burada ekstra UI gösterimi veya oyuncuya bildirim eklenebilir
        // Örneğin: "Zeplin kontrolü aktif! Düşmanları yok etmeye devam et!"
    }
    
    void Movement()
    {
        // Joystick girişini al (eğer varsa)
        float horizontalInput = 0f;
        float verticalInput = 0f;
        
        if (joystick != null)
        {
            horizontalInput = joystick.Horizontal;
            verticalInput = joystick.Vertical;
            
            // Deadzone uygula (çok küçük değerleri yoksay)
            if (Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
            {
                horizontalInput = 0;
                verticalInput = 0;
            }
        }
        
        // Klavye girişi - W,A,S,D veya ok tuşları
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            verticalInput = 1;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            verticalInput = -1;
            
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontalInput = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontalInput = 1;
        
        // Hareket vektörü oluştur
        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0).normalized;
        
        // Pozisyonu güncelle
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Sprite yönünü ayarla
        if (horizontalInput != 0 && spriteRenderer != null)
        {
            bool wasFlipped = isFacingLeft;
            isFacingLeft = horizontalInput < 0;
            spriteRenderer.flipX = isFacingLeft;
            
            // Yön değiştiyse ateş noktasını güncelle
            if (wasFlipped != isFacingLeft && firePoint != null && firePoint != transform)
            {
                UpdateFirePointPosition();
            }
        }
    }
    
    // FirePoint pozisyonunu güncelleme
    private void UpdateFirePointPosition()
    {
        if (firePoint == null || firePoint == transform) return;
        
        // FirePoint'in x değerini yöne göre ayarla, y değerini koru
        Vector3 newPosition = firePoint.localPosition;
        
        // Sadece x değerini yöne göre değiştir
        if (isFacingLeft)
        {
            newPosition.x = -Mathf.Abs(originalFirePointLocalPos.x);
        }
        else
        {
            newPosition.x = Mathf.Abs(originalFirePointLocalPos.x);
        }
        
        // y değeri korundu, sadece x değeri değişti
        firePoint.localPosition = newPosition;
        
        // FirePoint'in sprite'ını da flip et (eğer SpriteRenderer bileşeni varsa)
        SpriteRenderer firePointSpriteRenderer = firePoint.GetComponent<SpriteRenderer>();
        if (firePointSpriteRenderer != null)
        {
            firePointSpriteRenderer.flipX = isFacingLeft;
        }
    }
    
    // Minigun ateşleme metodu
    public void FireMinigun()
    {
        // Kontrol aktif değilse ateş etme
        if (!isControlActive) return;
        
        // Ateş bekleme süresini kontrol et
        if (Time.time < nextMinigunFireTime)
        {
            return;
        }
        
        // Mermi prefabı kontrolü
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Mermi prefabı atanmamış!");
            return;
        }
        
        // Ateş noktası kontrolü
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // Mermi oluştur
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Mermi bileşenini al ve yön ayarla
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.SetDirection(isFacingLeft);
            // Zeplin mermisini belirt (farklı davranış için kullanılabilir)
            bulletComponent.isZeplinBullet = true;
        }
        
        // Sonraki ateş zamanını ayarla
        nextMinigunFireTime = Time.time + (playerData != null ? playerData.zeplinMinigunCooldown : 0.5f);
        
        Debug.Log("Zeplin minigun ateşledi!");
    }
    
    // Roket ateşleme metodu
    public void FireRocket()
    {
        // Kontrol aktif değilse ateş etme
        if (!isControlActive) return;
        
        // Bekleme süresini kontrol et
        if (Time.time < nextRocketFireTime)
        {
            float remainingTime = nextRocketFireTime - Time.time;
            Debug.Log("Roket hazır değil! " + remainingTime.ToString("F1") + " saniye kaldı.");
            return;
        }
        
        // Roket prefabı kontrolü
        if (rocketPrefab == null)
        {
            Debug.LogError("Roket prefabı atanmamış!");
            return;
        }
        
        // Ateş noktası kontrolü
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // Roketi oluştur
        GameObject rocket = Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
        
        // Rokete "Rocket" etiketini ata
        rocket.tag = "Rocket";
        
        // Roket bileşenini alıp Zeplin'e ait olduğunu belirtebiliriz (opsiyonel)
        Rocket rocketComponent = rocket.GetComponent<Rocket>();
        if (rocketComponent != null)
        {
            // İleride zeplin roketlerine özel özellik eklemek için kullanılabilir
        }
        
        // RocketProjectile bileşeni kontrol et
        RocketProjectile rocketProjectile = rocket.GetComponent<RocketProjectile>();
        if (rocketProjectile != null)
        {
            // Zeplin roketi olduğunu belirt (düşman roketi değil)
            rocketProjectile.isEnemyRocket = false;
            // Hasar değerini PlayerData'dan al
            if (playerData != null)
            {
                rocketProjectile.damage = playerData.zeplinRoketDamage;
            }
        }
        
        // Bekleme süresini ayarla
        nextRocketFireTime = Time.time + (playerData != null ? playerData.zeplinRoketDelay : 2.0f);
        
        Debug.Log("Zeplin roket fırlattı!");
    }
    
    // Zeplin'e hasar verme metodu
    public void TakeDamage(int damage)
    {
        Debug.Log($"!!! Zeplin TakeDamage çağrıldı - Hasar: {damage}, İnvincible: {isInvincible}, Invincibility Timer: {invincibilityTimer:F2}");
        
        // Dokunulmazlık kontrolü (sadece kullanılıyorsa)
        if (useInvincibility && isInvincible)
        {
            Debug.Log("Zeplin dokunulmaz durumda, hasar yoksayıldı! (" + damage + " hasar)");
            return;
        }
        
        // PlayerData kontrolü
        if (playerData == null)
        {
            playerData = FindObjectOfType<PlayerData>();
            if (playerData == null)
            {
                Debug.LogError("PlayerData bulunamadı! Hasar uygulanamıyor.");
                return;
            }
        }
        
        // Önceki sağlık durumunu kaydet
        int previousHealth = playerData.zeplinSaglik;
        
        // PlayerData'daki sağlık değerini azalt
        playerData.zeplinSaglik -= damage;
        
        // Hasar uygulandığını doğrulama
        if (previousHealth == playerData.zeplinSaglik)
        {
            Debug.LogError($"Hasar uygulanamadı! Önceki Sağlık: {previousHealth}, Şimdiki Sağlık: {playerData.zeplinSaglik}, Hasar: {damage}");
        }
        else
        {
            Debug.Log($"Zeplin'e {damage} hasar uygulandı! Önceki Sağlık: {previousHealth}, Yeni Sağlık: {playerData.zeplinSaglik}");
        }
        
        // Hasar efekti göster (eğer varsa)
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // UI elemanlarını güncelle
        UpdateUI();
        
        // Dokunulmazlık süresini başlat (eğer kullanılıyorsa)
        if (useInvincibility)
        {
            isInvincible = true;
            invincibilityTimer = invincibilityTime;
            Debug.Log($"Zeplin için dokunulmazlık başlatıldı. Süre: {invincibilityTimer:F2}s");
        }
        
        Debug.Log($"Zeplin hasar aldı: {damage} hasar! Kalan sağlık: {playerData.zeplinSaglik}/{maxHealth}");
        
        // Eğer sağlık sıfırın altına düştüyse
        if (playerData.zeplinSaglik <= 0)
        {
            Die();
        }
    }
    
    // Zeplin'in yok olma metodu
    void Die()
    {
        // Yok olma efekti göster (eğer varsa)
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Zeplin yok oldu!");
        
        // Oyun sonu mantığı - GameManager'ı çağır
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError("GameManager bulunamadı! OyunSonu sahnesine geçiş yapılamıyor.");
            // Doğrudan sahne yükleme (alternatif)
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("OyunSonu");
            }
            catch (System.Exception e)
            {
                Debug.LogError("OyunSonu sahnesine geçiş başarısız! Hata: " + e.Message);
            }
        }
        
        // Zeplin'i yok et
        Destroy(gameObject);
    }
    
    // UI elemanlarını güncelleme metodu
    void UpdateUI()
    {
        if (playerData == null) return;
        
        // Sağlık çubuğunu güncelle (eğer varsa)
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = playerData.zeplinSaglik;
        }
        
        // Sağlık metnini güncelle (eğer varsa)
        if (healthText != null)
        {
            healthText.text = playerData.zeplinSaglik + " / " + maxHealth;
        }
    }
    
    // Çarpışma algılama - Trigger için
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Zeplin ile çarpışma algılandı: " + other.gameObject.name + " (Tag: " + other.tag + ")");
        
        // Düşman ile çarpışma kontrolü
        if (other.CompareTag("Enemy"))
        {
            // Düşmandan hasar miktarını al
            Enemy enemy = other.GetComponent<Enemy>();
            int damage = 10; // Varsayılan hasar
            
            if (enemy != null)
            {
                damage = enemy.GetDamageAmount();
            }
            
            // Zeplin'e hasar ver
            TakeDamage(damage);
            
            // Düşmanı yok et
            Destroy(other.gameObject);
            
            Debug.Log("Düşman Zeplin'e çarptı ve yok edildi!");
        }
        // Düşman mermisiyle çarpışma kontrolü
        else if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null && bullet.isEnemyBullet)
            {
                // Düşman mermisinden hasar al
                TakeDamage(bullet.damage);
                
                // Mermiyi yok et
                Destroy(other.gameObject);
                
                Debug.Log("Zeplin düşman mermisiyle vuruldu! Hasar: " + bullet.damage);
            }
        }
        // Roket ile çarpışma kontrolü
        else if (other.CompareTag("Rocket"))
        {
            RocketProjectile rocket = other.GetComponent<RocketProjectile>();
            if (rocket != null && rocket.isEnemyRocket)
            {
                // Düşman roketinden hasar al
                TakeDamage(rocket.damage);
                
                // Roketi yok et
                Destroy(other.gameObject);
                
                Debug.Log("Zeplin düşman roketiyle vuruldu! Hasar: " + rocket.damage);
            }
        }
    }
    
    // Çarpışma algılama - Fiziksel çarpışma için
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Zeplin ile fiziksel çarpışma algılandı: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
        
        // Düşman ile çarpışma kontrolü
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Düşmandan hasar miktarını al
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            int damage = 10; // Varsayılan hasar
            
            if (enemy != null)
            {
                damage = enemy.GetDamageAmount();
            }
            
            // Zeplin'e hasar ver
            TakeDamage(damage);
            
            // Düşmanı yok et
            Destroy(collision.gameObject);
            
            Debug.Log("Düşman Zeplin'e çarptı ve yok edildi!");
        }
        // Düşman mermisiyle çarpışma kontrolü
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null && bullet.isEnemyBullet)
            {
                // Düşman mermisinden hasar al
                TakeDamage(bullet.damage);
                
                // Mermiyi yok et
                Destroy(collision.gameObject);
                
                Debug.Log("Zeplin düşman mermisiyle vuruldu! (Collision) Hasar: " + bullet.damage);
            }
        }
        // Roket ile çarpışma kontrolü
        else if (collision.gameObject.CompareTag("Rocket"))
        {
            RocketProjectile rocket = collision.gameObject.GetComponent<RocketProjectile>();
            if (rocket != null && rocket.isEnemyRocket)
            {
                // Düşman roketinden hasar al
                TakeDamage(rocket.damage);
                
                // Roketi yok et
                Destroy(collision.gameObject);
                
                Debug.Log("Zeplin düşman roketiyle vuruldu! (Collision) Hasar: " + rocket.damage);
            }
        }
    }
} 