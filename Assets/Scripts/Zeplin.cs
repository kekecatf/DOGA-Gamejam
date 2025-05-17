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
    public float invincibilityTime = 1.0f; // Hasar sonrası dokunulmazlık süresi
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
            
            // Test için klavye ile ateş etme (Space = minigun, R = roket)
            if (Input.GetKey(KeyCode.Space))
            {
                FireMinigun();
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                FireRocket();
            }
        }
        
        // Dokunulmazlık süresi kontrolü
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
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
        if (joystick == null) return;
        
        // Joystick girişini al
        float horizontalInput = joystick.Horizontal;
        float verticalInput = joystick.Vertical;
        
        // Deadzone uygula (çok küçük değerleri yoksay)
        horizontalInput = Mathf.Abs(horizontalInput) < 0.1f ? 0 : horizontalInput;
        verticalInput = Mathf.Abs(verticalInput) < 0.1f ? 0 : verticalInput;
        
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
        
        // Roket bileşenini alıp Zeplin'e ait olduğunu belirtebiliriz (opsiyonel)
        Rocket rocketComponent = rocket.GetComponent<Rocket>();
        if (rocketComponent != null)
        {
            // İleride zeplin roketlerine özel özellik eklemek için kullanılabilir
        }
        
        // Bekleme süresini ayarla
        nextRocketFireTime = Time.time + (playerData != null ? playerData.zeplinRoketDelay : 2.0f);
        
        Debug.Log("Zeplin roket fırlattı!");
    }
    
    // Zeplin'e hasar verme metodu
    public void TakeDamage(int damage)
    {
        // Dokunulmazlık kontrolü
        if (isInvincible)
        {
            Debug.Log("Zeplin dokunulmaz durumda, hasar yoksayıldı!");
            return;
        }
        
        if (playerData == null)
        {
            playerData = FindObjectOfType<PlayerData>();
            if (playerData == null)
            {
                Debug.LogError("PlayerData bulunamadı! Hasar uygulanamıyor.");
                return;
            }
        }
        
        // PlayerData'daki sağlık değerini azalt
        playerData.zeplinSaglik -= damage;
        
        // Hasar efekti göster (eğer varsa)
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // UI elemanlarını güncelle
        UpdateUI();
        
        Debug.Log("Zeplin hasar aldı! Kalan sağlık: " + playerData.zeplinSaglik + "/" + maxHealth);
        
        // Dokunulmazlık süresini başlat
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        
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
        
        // Oyun sonu mantığı (örneğin: oyunu kaybetme ekranı)
        // GameManager.Instance.GameOver();
        
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
    }
    
    // Çarpışma algılama - Fiziksel çarpışma için
    void OnCollisionEnter2D(Collision2D collision)
    {
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
    }
} 