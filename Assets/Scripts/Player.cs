using UnityEngine;
using UnityEngine.UI; // UI bileşenleri için
using UnityEngine.EventSystems; // Joystick için gerekli

public class Player : MonoBehaviour
{
    // PlayerData referansı
    private PlayerData playerData;
    
    // Hareket hızı
    public float moveSpeed = 5f;
    
    // Rotasyon ayarları
    public float rotationSpeed = 5f; // Dönüş hızı
    
    // Joystick Referansı
    public Joystick joystick; // Dynamic Joystick referansı buraya sürüklenecek
    
    // UI Butonları
    [Header("UI Butonları")]
    public Button minigunButton; // Minigun ateşleme butonu
    public Button rocketButton; // Roket ateşleme butonu
    
    // Mermi ayarları
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float firePointXOffset = 1f; // FirePoint'in x ekseni ofset değeri
    private float nextFireTime = 0f;
    
    // Roket ayarları
    public GameObject rocketPrefab; // Roket prefabı
    public Transform rocketFirePoint; // Roket fırlatma noktası
    private float nextRocketTime = 0f; // Bir sonraki roket fırlatma zamanı
    
    // Bileşenler
    private SpriteRenderer spriteRenderer;
    private bool isFacingLeft = false;
    private float currentRotation = 0f;     // Mevcut gerçek rotasyon
    private Vector3 originalFirePointLocalPos;
    
    // Hareket kontrolü
    private Vector2 moveDirection = Vector2.zero;
    private float minJoystickMagnitude = 0.1f; // Minimum joystick hareketi için eşik değeri
    
    // Hasar ve efektler
    [Header("Hasar ve Efektler")]
    public GameObject damageEffect; // Hasar alınca çıkacak efekt (opsiyonel)
    public float invincibilityTime = 1.0f; // Hasar aldıktan sonra geçici dokunulmazlık süresi
    private float invincibilityTimer = 0f; // Dokunulmazlık sayacı
    private bool isInvincible = false; // Dokunulmazlık durumu
    
    // Sağlık UI
    [Header("Sağlık UI")]
    public Slider healthSlider; // Can çubuğu
    public Text healthText; // Can değeri metni (opsiyonel)
    private int maxHealth; // Maksimum sağlık değeri
    
    // Oyuncu durumu
    public static bool isDead = false; // Oyuncu ölü mü? (Zeplin kontrolü için)
    
    // Zeplin referansı
    private Zeplin zeplin;
    
    private void Start()
    {
        // Oyuncu başlangıçta canlı
        isDead = false;
        
        // Zeplin referansını bul
        zeplin = FindObjectOfType<Zeplin>();
        
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("PlayerData bulunamadı! Sahnede PlayerData objesi olduğundan emin olun.");
        }
        
        // Sprite Renderer bileşenini al
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Başlangıç rotasyonunu sıfırla
        transform.rotation = Quaternion.identity;
        
        // Fire point kontrolü
        if (firePoint == null)
        {
            // Eğer fire point atanmamışsa, oyuncunun merkezini kullan
            firePoint = transform;
            originalFirePointLocalPos = Vector3.zero;
        }
        else
        {
            // FirePoint'in orijinal pozisyonunu kaydet (sadece x ekseni)
            // Flip durumunda sadece x değeri değişecek, y değeri korunacak
            originalFirePointLocalPos = new Vector3(firePoint.localPosition.x, 0, 0);
            
            // FirePoint'in sprite'ını başlangıçta ayarla
            SpriteRenderer firePointSpriteRenderer = firePoint.GetComponent<SpriteRenderer>();
            if (firePointSpriteRenderer != null)
            {
                firePointSpriteRenderer.flipX = isFacingLeft;
            }
        }
        
        // Roket fırlatma noktası kontrolü
        if (rocketFirePoint == null)
        {
            // Eğer roket fırlatma noktası atanmamışsa, normal ateş noktasını kullan
            rocketFirePoint = firePoint;
            Debug.LogWarning("RocketFirePoint atanmamış, normal FirePoint kullanılıyor!");
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
        
        // Minigun butonu kontrolü ve listener ekleme
        if (minigunButton != null)
        {
            // Butona tıklama olayı ekle
            minigunButton.onClick.AddListener(MobileFireButton);
        }
        else
        {
            Debug.LogWarning("MinigunButton atanmamış! Inspector'dan atayın.");
        }
        
        // Roket butonu kontrolü ve listener ekleme
        if (rocketButton != null)
        {
            // Butona tıklama olayı ekle
            rocketButton.onClick.AddListener(MobileRocketButton);
        }
        else
        {
            Debug.LogWarning("RocketButton atanmamış! Inspector'dan atayın.");
        }
        
        // Sağlık değerini başlat
        if (playerData != null)
        {
            maxHealth = playerData.anaGemiSaglik;
            UpdateHealthUI();
        }
        
        // Oyuncu bilgilerini logla
        if (playerData != null)
        {
            Debug.Log("Oyuncu Sağlık: " + playerData.anaGemiSaglik);
            Debug.Log("Mermi hasarı: " + playerData.anaGemiMinigunDamage + ", Ateş hızı: " + playerData.anaGemiMinigunCooldown);
            Debug.Log("Roket hasarı: " + playerData.anaGemiRoketDamage + ", Bekleme süresi: " + playerData.anaGemiRoketDelay);
        }
    }
    
    private void Update()
    {
        // Eğer oyuncu ölmüşse kontrolü devre dışı bırak
        if (isDead) return;
        
        // Hareket ve yönlendirme
        Movement();
        
        // Ateş etme (space tuşu veya ekstra buton)
        // Space tuşu sadece test için kullanılacak, asıl ateş etme butona bağlı
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + (playerData != null ? playerData.anaGemiMinigunCooldown : 0.3f);
        }
        
        // Roket fırlatma (R tuşu veya ekstra buton)
        if (Input.GetKeyDown(KeyCode.R))
        {
            FireRocket();
        }
        
        // Test için: T tuşuna basılınca para ekle
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddMoney(50);
        }
        
        // Test için: Y tuşuna basılınca silahı yükselt
        if (Input.GetKeyDown(KeyCode.Y))
        {
            UpgradeMinigun();
        }
        
        // Dokunulmazlık süresini güncelle
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // Yanıp sönme efekti için sprite'ı aç/kapa
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = Time.time % 0.2f < 0.1f;
            }
            
            // Dokunulmazlık süresi bittiyse
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                // Sprite'ı tekrar görünür yap
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
                Debug.Log("Dokunulmazlık süresi bitti!");
            }
        }
    }
    
    // Sağlık UI'ını güncelle
    private void UpdateHealthUI()
    {
        if (playerData == null) return;
        
        // Sağlık çubuğunu güncelle
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = playerData.anaGemiSaglik;
        }
        
        // Sağlık metnini güncelle (eğer varsa)
        if (healthText != null)
        {
            healthText.text = playerData.anaGemiSaglik + " / " + maxHealth;
        }
    }
    
    // PlayerData'daki para değerini arttır
    public void AddMoney(int amount)
    {
        if (playerData != null)
        {
            playerData.metalPara += amount;
            Debug.Log("Yeni para miktarı: " + playerData.metalPara);
        }
    }
    
    // Silah yükseltme örneği
    public void UpgradeMinigun()
    {
        if (playerData != null && playerData.metalPara >= 100)
        {
            playerData.metalPara -= 100;
            playerData.anaGemiMinigunLevel++;
            playerData.anaGemiMinigunDamage += 5;
            
            Debug.Log("Silah yükseltildi! Yeni seviye: " + playerData.anaGemiMinigunLevel + 
                     ", Yeni hasar: " + playerData.anaGemiMinigunDamage);
        }
        else if (playerData != null)
        {
            Debug.Log("Yeterli para yok! Gerekli: 100, Mevcut: " + playerData.metalPara);
        }
    }
    
    // Roket yükseltme örneği
    public void UpgradeRocket()
    {
        if (playerData != null && playerData.metalPara >= 150)
        {
            playerData.metalPara -= 150;
            playerData.anaGemiRoketLevel++;
            playerData.anaGemiRoketDamage += 10;
            playerData.anaGemiRoketSpeed += 2f;
            
            Debug.Log("Roket yükseltildi! Yeni seviye: " + playerData.anaGemiRoketLevel + 
                     ", Yeni hasar: " + playerData.anaGemiRoketDamage +
                     ", Yeni hız: " + playerData.anaGemiRoketSpeed);
        }
        else if (playerData != null)
        {
            Debug.Log("Yeterli para yok! Gerekli: 150, Mevcut: " + playerData.metalPara);
        }
    }
    
    // Mobil roket butonu için public metot
    public void MobileRocketButton()
    {
        // Eğer oyuncu ölmüşse devre dışı bırak
        if (isDead) return;
        
        FireRocket();
    }
    
    // Roket fırlatma metodu - UI butonundan çağrılabilir
    public void FireRocket()
    {
        // Bekleme süresini kontrol et
        if (Time.time < nextRocketTime)
        {
            float remainingTime = nextRocketTime - Time.time;
            Debug.Log("Roket hazır değil! " + remainingTime.ToString("F1") + " saniye kaldı.");
            return;
        }
        
        // Roket prefabı kontrolü
        if (rocketPrefab == null)
        {
            Debug.LogError("Roket prefabı atanmamış!");
            return;
        }
        
        // Roket fırlatma noktasını kontrol et
        if (rocketFirePoint == null)
        {
            rocketFirePoint = firePoint; // Varsayılan olarak normal ateş noktasını kullan
        }
        
        // Roketi oluştur
        GameObject rocket = Instantiate(rocketPrefab, rocketFirePoint.position, rocketFirePoint.rotation);
        
        // Log that a rocket was fired
        Debug.Log("Roket fırlatıldı! (Hasar PlayerData'dan otomatik alınıyor)");
        
        // Bekleme süresini ayarla
        nextRocketTime = Time.time + (playerData != null ? playerData.anaGemiRoketDelay : 3f);
    }
    
    private void Movement()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;
        
        // Joystick girişini al (varsa)
        if (joystick != null)
        {
            horizontalInput = joystick.Horizontal;
            verticalInput = joystick.Vertical;
            
            // Deadzone uygula (çok küçük değerleri yoksay)
            if (Mathf.Abs(horizontalInput) < minJoystickMagnitude && Mathf.Abs(verticalInput) < minJoystickMagnitude)
            {
                horizontalInput = 0;
                verticalInput = 0;
            }
        }
        else
        {
            // Joystick yoksa klavye girişini al (test için)
            
            // WASD tuşları için kontrol
            if (Input.GetKey(KeyCode.W))
                verticalInput = 1;
            else if (Input.GetKey(KeyCode.S))
                verticalInput = -1;
                
            if (Input.GetKey(KeyCode.A))
                horizontalInput = -1;
            else if (Input.GetKey(KeyCode.D))
                horizontalInput = 1;
        }
        
        // Hareket yönünü kaydet
        moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        
        // Pozisyonu güncelle
        transform.position += new Vector3(moveDirection.x, moveDirection.y, 0) * moveSpeed * Time.deltaTime;
        
        // Eğer hareket varsa rotasyonu güncelle
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            // Hareket yönüne göre rotasyon hesapla
            float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            
            // Yumuşak rotasyon için Lerp kullan
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Mevcut rotasyonu kaydet
            currentRotation = transform.eulerAngles.z;
            
            // FirePoint pozisyonunu güncelle
            UpdateFirePointPosition(moveDirection.x < 0);
        }
    }
    
    private void UpdateFirePointPosition(bool isLeft)
    {
        // Yön değişimini kontrol et
        bool wasFlipped = isFacingLeft;
        isFacingLeft = isLeft;
        
        // Sadece yön değiştiyse firePoint'i güncelle
        if (wasFlipped != isFacingLeft && firePoint != null && firePoint != transform)
        {
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
        
        // Sprite yönünü ayarla
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isFacingLeft;
        }
    }
    
    // Mobil ateş butonu için public metot
    public void MobileFireButton()
    {
        // Eğer oyuncu ölmüşse devre dışı bırak
        if (isDead) return;
        
        if (Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + (playerData != null ? playerData.anaGemiMinigunCooldown : 0.3f);
            
            // Buton basıldığında görsel geri bildirim (opsiyonel)
            Debug.Log("Minigun butonu ile ateş edildi!");
        }
        else
        {
            // Henüz ateş edilemiyorsa, kalan süreyi göster (opsiyonel)
            float remainingTime = nextFireTime - Time.time;
            Debug.Log("Minigun hazır değil! " + remainingTime.ToString("F1") + " saniye kaldı.");
        }
    }
    
    // Mermi için mevcut rotasyonu döndüren public metot
    public float GetCurrentRotation()
    {
        return currentRotation;
    }
    
    private void FireBullet()
    {
        // Mermi prefabı kontrolü
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Mermi prefabı atanmamış!");
            return;
        }
        
        // Mermi her zaman aynı rotasyonla oluşturulur - yön değişimi SetDirection ile yapılır
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
        
        // Mermi bileşenini al
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            // Yön bilgisini ayarla (sağa veya sola hareket için)
            bulletComponent.SetDirection(isFacingLeft);
        }
    }
    
    // Oyuncuya hasar verme metodu
    public void TakeDamage(int damage)
    {
        // Eğer oyuncu zaten ölmüşse işlem yapma
        if (isDead) return;
        
        // Eğer dokunulmazlık süresi aktifse hasarı yoksay
        if (isInvincible)
        {
            Debug.Log("Oyuncu dokunulmaz durumda, hasar yoksayıldı!");
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
        
        // PlayerData'daki sağlık değerini azalt
        playerData.anaGemiSaglik -= damage;
        
        // Hasar efekti göster (eğer varsa)
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // Sağlık UI'ını güncelle
        UpdateHealthUI();
        
        Debug.Log("Oyuncu hasar aldı! Kalan sağlık: " + playerData.anaGemiSaglik + "/" + maxHealth);
        
        // Dokunulmazlık süresini başlat
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        
        // Eğer sağlık sıfırın altına düştüyse
        if (playerData.anaGemiSaglik <= 0)
        {
            Die();
        }
    }
    
    // Oyuncunun ölme metodu
    void Die()
    {
        // Ölüm durumunu ayarla
        isDead = true;
        
        Debug.Log("Oyuncu öldü! Kontrol Zeplin'e geçiyor...");
        
        // Zeplin'e bilgi ver
        if (zeplin != null)
        {
            zeplin.ActivateZeplinControl();
        }
        else
        {
            Debug.LogWarning("Zeplin referansı bulunamadı! Kontrol otomatik geçemeyecek.");
            // Zeplin referansını son bir kez daha bulmayı dene
            zeplin = FindObjectOfType<Zeplin>();
            if (zeplin != null)
            {
                zeplin.ActivateZeplinControl();
            }
        }
        
        // Health Slider'ı bul ve devre dışı bırak/yok et
        if (healthSlider != null)
        {
            // Slider'ın parent elementini bul (UI panel/canvas)
            Transform sliderParent = healthSlider.transform.parent;
            
            // Eğer doğrudan slider'ı yok etmek isteniyorsa:
            Destroy(healthSlider.gameObject);
            Debug.Log("Player Health Slider sahneden kaldırıldı!");
            
            // Alternatif olarak, parent objesini gizleme:
            // if (sliderParent != null)
            // {
            //     sliderParent.gameObject.SetActive(false);
            //     Debug.Log("Player Health Slider paneli gizlendi!");
            // }
        }
        
        // Oyun nesnesini belirli bir süre sonra yok et (efektlerin oynatılabilmesi için)
        // Bu süreyi ölüm animasyonunun süresine göre ayarlayabilirsiniz
        Destroy(gameObject, 1f);
        
        // Ölüm efekti eklemek isterseniz
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // Görsel geri bildirim (sprite'ı sönükleştir)
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0.5f; // Yarı saydam yap
            spriteRenderer.color = color;
        }
        
        // Collider'ı devre dışı bırak (çarpışmaları engellemek için)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Burada oyun sonu mantığı eklenebilir, ancak şimdilik sadece 
        // kontrolü Zeplin'e devrediyoruz, oyun devam edecek
        
        // Oyuncu bildirimini göster
        Debug.LogWarning("Oyuncu öldü! Zeplin kontrolüne geçiliyor!");
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
            
            // Oyuncuya hasar ver
            TakeDamage(damage);
            
            // Düşmanı yok et
            Destroy(other.gameObject);
            
            Debug.Log("Düşman oyuncuya çarptı ve yok edildi!");
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
            
            // Oyuncuya hasar ver
            TakeDamage(damage);
            
            // Düşmanı yok et
            Destroy(collision.gameObject);
            
            Debug.Log("Düşman oyuncuya çarptı ve yok edildi!");
        }
    }
} 