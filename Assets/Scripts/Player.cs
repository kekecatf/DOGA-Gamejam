using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI bileşenleri için
using UnityEngine.EventSystems; // Event sistemi için
using UnityEngine.SceneManagement; // Sahne yönetimi için
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    private Animator animator; // Animator bileşeni
    private bool isFacingLeft = true; // Artık varsayılan olarak sola bakıyor (flip X açık)
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

    // Sprite animasyonu için değişkenler
    [Header("Sprite Animasyonu")]
    public float animationSpeed = 0.1f;     // Her kare arasındaki zaman
    public Sprite[] animationFrames;        // El ile atanacak sprite kareleri
    private float animationTimer = 0f;
    private int currentFrameIndex = 0;
    private bool isAnimationPlaying = false;

    // Minigun buton durumu için değişken
    private bool isMinigunButtonPressed = false;

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

        // Collider kontrolü ve ayarları
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // Collider yoksa ekle
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = false; // Fiziksel çarpışma için trigger kapalı

            // Sprite boyutuna uygun collider
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Sprite boyutuna göre collider boyutu ayarla (biraz küçült)
                boxCollider.size = new Vector2(
                    spriteRenderer.sprite.bounds.size.x * 0.8f,
                    spriteRenderer.sprite.bounds.size.y * 0.8f
                );
            }

            Debug.Log("Player için BoxCollider2D eklendi (isTrigger=false).");
        }
        else
        {
            Debug.Log("Player'da mevcut bir Collider var. Tip: " + collider.GetType().Name + ", IsTrigger: " + collider.isTrigger);

            // Etiket kontrolü
            if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag != "Player")
            {
                gameObject.tag = "Player";
                Debug.Log("Player etiketi atandı.");
            }
        }

        // Rigidbody2D kontrolü
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Yerçekimini kapat
            rb.freezeRotation = true; // Rotasyonu dondur
            rb.linearDamping = 3f; // Hareket direnci ekle
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Debug.Log("Player için Rigidbody2D eklendi (gravityScale=0, freezeRotation=true).");
        }

        // Sprite Renderer bileşenini al
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Animator bileşenini kontrol et
        animator = GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Eğer düzgün animator controller varsa animasyonu başlat
            animator.enabled = true;
            Debug.Log("Animator kullanılarak animasyon başlatıldı.");
        }
        else
        {
            // Animator yoksa veya controller atanmamışsa manuel sprite animasyonu kullan
            SetupManualAnimation();
        }

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
                firePointSpriteRenderer.flipX = isFacingLeft; // Varsayılan olarak flip X açık
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
            // Butona tıklama olayını kaldır
            minigunButton.onClick.RemoveAllListeners();

            // Pointer olaylarını eklemek için Event Trigger ekle veya al
            EventTrigger eventTrigger = minigunButton.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = minigunButton.gameObject.AddComponent<EventTrigger>();
            }

            // PointerDown olayı ekle
            EventTrigger.Entry pointerDownEvent = new EventTrigger.Entry();
            pointerDownEvent.eventID = EventTriggerType.PointerDown;
            pointerDownEvent.callback.AddListener((data) => { OnMinigunButtonDown(); });
            eventTrigger.triggers.Add(pointerDownEvent);

            // PointerUp olayı ekle
            EventTrigger.Entry pointerUpEvent = new EventTrigger.Entry();
            pointerUpEvent.eventID = EventTriggerType.PointerUp;
            pointerUpEvent.callback.AddListener((data) => { OnMinigunButtonUp(); });
            eventTrigger.triggers.Add(pointerUpEvent);

            Debug.Log("Minigun butonu için sürekli ateş modu etkinleştirildi!");
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

        // Manuel sprite animasyonunu güncelle
        if (isAnimationPlaying && animationFrames != null && animationFrames.Length > 0)
        {
            UpdateSpriteAnimation();
        }

        // Hareket ve yönlendirme
        Movement();

        // Klavye tuşları ile ateş etme (Z tuşu ile minigun)
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + (playerData != null ? playerData.anaGemiMinigunCooldown / 2 : 0.15f);
            Debug.Log("Z tuşu ile minigun ateşlendi!");
        }

        // X tuşu ile roket fırlatma
        if (Input.GetKeyDown(KeyCode.X))
        {
            FireRocket();
            Debug.Log("X tuşu ile roket fırlatıldı!");
        }

        // Ateş etme (space tuşu veya ekstra buton)
        // Space tuşu sadece test için kullanılacak, asıl ateş etme butona bağlı
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + (playerData != null ? playerData.anaGemiMinigunCooldown / 2 : 0.15f);
        }

        // Eğer minigun butonu basılıysa sürekli ateş et
        if (isMinigunButtonPressed && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + (playerData != null ? playerData.anaGemiMinigunCooldown / 2 : 0.15f);
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

        // Rokete "Rocket" etiketini ata
        rocket.tag = "Rocket";

        // Ekran titreşimi uygula - sadece roket için bir kez titreşim
        if (ScreenShakeManager.Instance != null)
        {
            // Tek seferlik orta şiddetli titreşim - silah seviyesine göre şiddeti arttır
            float intensity = 0.1f;
            if (playerData != null && playerData.anaGemiRoketLevel > 1)
            {
                // Roket seviyesi arttıkça titreşim biraz daha artsın (0.02f artış)
                intensity += (playerData.anaGemiRoketLevel * 0.02f);
            }

            // Tek seferlik titreşim uygula (0.3 saniye)
            ScreenShakeManager.Instance.ShakeOnce(intensity, 0.3f);
        }

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

        // Klavye girişini al (her zaman kontrol et)
        // W,A,S,D tuşları için kontrol
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            verticalInput = 1;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            verticalInput = -1;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontalInput = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontalInput = 1;

        // Önceki hareket yönünü kaydet
        Vector2 previousMoveDirection = moveDirection;

        // Yeni hareket yönünü hesapla
        Vector2 targetDirection = new Vector2(horizontalInput, verticalInput).normalized;

        // Hareket yönü değişimini yumuşatma (lerp)
        // Eğer hareket girişi varsa kademeli olarak hedef yöne yaklaş
        if (targetDirection.sqrMagnitude > 0.01f)
        {
            // Daha yumuşak dönüş için kademeli geçiş faktörü
            float smoothFactor = rotationSpeed * Time.deltaTime;

            // Ani dönüşleri sınırlamak için maksimum dönüş açısı
            float maxTurnAngle = 120f * Time.deltaTime; // Saniyede maksimum 120 derece dönüş

            // Hedef açı hesapla
            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            // Mevcut açı hesapla
            float currentAngle = Mathf.Atan2(previousMoveDirection.y, previousMoveDirection.x) * Mathf.Rad2Deg;

            // Açısal farkı hesapla (-180 ile 180 arasında)
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

            // Açısal farkı sınırla
            float clampedAngleDiff = Mathf.Clamp(angleDiff, -maxTurnAngle, maxTurnAngle);

            // Yeni açı hesapla
            float newAngle = currentAngle + clampedAngleDiff;

            // Önceki yön sıfır ise (hareket yoksa) doğrudan hedef yöne dön
            if (previousMoveDirection.sqrMagnitude < 0.01f)
            {
                moveDirection = targetDirection;
            }
            else
            {
                // Yeni yönü hesapla
                moveDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)).normalized;

                // Ek olarak hedef yöne doğru lerp yapalım (daha yumuşak geçiş)
                moveDirection = Vector2.Lerp(moveDirection, targetDirection, smoothFactor * 0.5f);
            }
        }
        else
        {
            // Girdi yoksa yavaşça hareket yönünü azalt
            moveDirection = Vector2.Lerp(moveDirection, Vector2.zero, Time.deltaTime * 5f);
        }

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

            // FirePoint pozisyonunu güncelle - moveDirection.x ile isLeft mantığını tersine çevir
            // Varsayılan olarak flip X açık olduğundan moveDirection.x > 0 ise sağa bakıyoruz
            UpdateFirePointPosition(moveDirection.x > 0);
        }
    }

    private void UpdateFirePointPosition(bool isLeft)
    {
        // Yön değişimini kontrol et
        bool wasFlipped = isFacingLeft;
        isFacingLeft = isLeft;

        // Rotasyon 90 veya -90 derece civarında ise Y ekseni üzerinde flip kontrol et
        float normalizedRotation = transform.eulerAngles.z;
        if (normalizedRotation > 180)
            normalizedRotation -= 360;

        bool shouldFlipY = Mathf.Abs(Mathf.Abs(normalizedRotation) - 90) < 45;

        // Sprite yönünü ayarla
        if (spriteRenderer != null)
        {
            // Sadece Y ekseninde flip yap
            spriteRenderer.flipY = shouldFlipY;
        }

        // Fire point pozisyonunu rotasyona ve flip durumuna göre ayarla
        if (firePoint != null && firePoint != transform)
        {
            // Rotasyon durumuna göre fire point pozisyonunu ayarla
            Vector3 newPosition = firePoint.localPosition;

            // X pozisyonunu ayarla - TERSİNE ÇEVRİLDİ
            if (shouldFlipY)
            {
                // 90/-90 derece rotasyonda (dikey)
                if (normalizedRotation > 0) // 90 derece civarı (yukarı)
                {
                    newPosition.x = isFacingLeft ? -Mathf.Abs(originalFirePointLocalPos.x) : Mathf.Abs(originalFirePointLocalPos.x);
                }
                else // -90 derece civarı (aşağı)
                {
                    newPosition.x = isFacingLeft ? Mathf.Abs(originalFirePointLocalPos.x) : -Mathf.Abs(originalFirePointLocalPos.x);
                }
            }
            else
            {
                // Normal yatay pozisyon - TERSİNE ÇEVRİLDİ
                newPosition.x = isFacingLeft ? Mathf.Abs(originalFirePointLocalPos.x) : -Mathf.Abs(originalFirePointLocalPos.x);
            }

            firePoint.localPosition = newPosition;

            // Fire point sprite'ını Y ekseninde flip yap
            SpriteRenderer firePointSpriteRenderer = firePoint.GetComponent<SpriteRenderer>();
            if (firePointSpriteRenderer != null)
            {
                firePointSpriteRenderer.flipY = shouldFlipY;
            }
        }

        // Roket fire point için de aynı işlemleri yap
        if (rocketFirePoint != null && rocketFirePoint != transform && rocketFirePoint != firePoint)
        {
            Vector3 rocketPosition = rocketFirePoint.localPosition;
            float rocketOriginalX = rocketPosition.x >= 0 ? Mathf.Abs(rocketPosition.x) : -Mathf.Abs(rocketPosition.x);

            // X pozisyonunu ayarla - TERSİNE ÇEVRİLDİ
            if (shouldFlipY)
            {
                // 90/-90 derece rotasyonda (dikey)
                if (normalizedRotation > 0) // 90 derece civarı (yukarı)
                {
                    rocketPosition.x = isFacingLeft ? -Mathf.Abs(rocketOriginalX) : Mathf.Abs(rocketOriginalX);
                }
                else // -90 derece civarı (aşağı)
                {
                    rocketPosition.x = isFacingLeft ? Mathf.Abs(rocketOriginalX) : -Mathf.Abs(rocketOriginalX);
                }
            }
            else
            {
                // Normal yatay pozisyon - TERSİNE ÇEVRİLDİ
                rocketPosition.x = isFacingLeft ? Mathf.Abs(rocketOriginalX) : -Mathf.Abs(rocketOriginalX);
            }

            rocketFirePoint.localPosition = rocketPosition;

            // Roket fire point sprite'ını Y ekseninde flip yap
            SpriteRenderer rocketFirePointSpriteRenderer = rocketFirePoint.GetComponent<SpriteRenderer>();
            if (rocketFirePointSpriteRenderer != null)
            {
                rocketFirePointSpriteRenderer.flipY = shouldFlipY;
            }
        }
    }

    // Minigun butonu basıldığında çağrılan metot
    public void OnMinigunButtonDown()
    {
        isMinigunButtonPressed = true;

        // İlk ateşi hemen başlat (eğer hazırsa)
        if (Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + (playerData != null ? playerData.anaGemiMinigunCooldown / 2 : 0.15f);
            Debug.Log("Minigun butonu basıldı - sürekli ateş başladı!");
        }
    }

    // Minigun butonu bırakıldığında çağrılan metot
    public void OnMinigunButtonUp()
    {
        isMinigunButtonPressed = false;
        Debug.Log("Minigun butonu bırakıldı - ateş durduruldu!");
    }

    // Mobil ateş butonu için public metot (eski metot - geriye dönük uyumluluk için)
    public void MobileFireButton()
    {
        // Eğer oyuncu ölmüşse devre dışı bırak
        if (isDead) return;

        if (Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + (playerData != null ? playerData.anaGemiMinigunCooldown / 2 : 0.15f);

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

        // Mermi firePoint rotasyonu ile oluşturulur - böylece her zaman firePoint'in +X yönünde ilerler
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Mermi bileşenini al
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            // Sadece sprite'ın görünümünü ayarla, hareket etkilenmesin
            bulletComponent.SetDirection(isFacingLeft);

            // Debug log
            Debug.Log("Mermi oluşturuldu, her zaman firePoint +X yönünde ilerleyecek");
        }

        // Ekran titreşimini kaldırıldı - sadece roket için titreşim olacak
    }

    // Oyuncuya hasar verme metodu
    public void TakeDamage(int damage)
    {
        Debug.Log($">>> Player.TakeDamage çağrıldı - Hasar: {damage}, İnvincible: {isInvincible}, Invincibility Timer: {invincibilityTimer:F2}");

        // Eğer oyuncu zaten ölmüşse işlem yapma
        if (isDead)
        {
            Debug.Log("Oyuncu zaten ölü, hasar yoksayıldı!");
            return;
        }

        // Eğer dokunulmazlık süresi aktifse hasarı yoksay
        if (isInvincible)
        {
            Debug.Log("Oyuncu dokunulmaz durumda, hasar yoksayıldı! (" + damage + " hasar)");
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

        // Önceki sağlık değerini kaydet
        int previousHealth = playerData.anaGemiSaglik;

        // PlayerData'daki sağlık değerini azalt
        playerData.anaGemiSaglik -= damage;

        // Hasar uygulandığını doğrulama
        if (previousHealth == playerData.anaGemiSaglik)
        {
            Debug.LogError($"Hasar uygulanamadı! Önceki Sağlık: {previousHealth}, Şimdiki Sağlık: {playerData.anaGemiSaglik}, Hasar: {damage}");
        }
        else
        {
            Debug.Log($"Oyuncuya {damage} hasar uygulandı! Önceki Sağlık: {previousHealth}, Yeni Sağlık: {playerData.anaGemiSaglik}");
        }

        // Hasar efekti göster (eğer varsa)
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }

        // Sağlık UI'ını güncelle
        UpdateHealthUI();

        // Dokunulmazlık süresini başlat
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        Debug.Log($"Oyuncu için dokunulmazlık başlatıldı. Süre: {invincibilityTimer:F2}s");

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
        Debug.Log("Player.Die() çağrıldı. isDead = " + isDead);
        
        // PlayerData null kontrolü
        if (playerData == null)
        {
            Debug.LogError("Die(): playerData null! PlayerData'yı yeniden bulmaya çalışıyor...");
            playerData = FindObjectOfType<PlayerData>();
            if (playerData == null)
            {
                Debug.LogError("Die(): PlayerData bulunamadı! Yeni bir tane oluşturuluyor.");
                GameObject playerDataObj = new GameObject("PlayerData");
                playerData = playerDataObj.AddComponent<PlayerData>();
                DontDestroyOnLoad(playerDataObj);
                Debug.Log("Die(): Yeni PlayerData oluşturuldu.");
            }
        }
        
        // Oyuncunun daha önce canlanıp canlanmadığını kontrol et
        if (playerData != null && playerData.isPlayerRespawned)
        {
            // İkinci ölüm - Zeplin kontrolüne geç
            Debug.Log("Oyuncu ikinci kez öldü! Kontrol Zeplin'e geçiyor...");
            
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
        }
        else
        {
            // İlk ölüm - MiniGame sahnesine geç
            Debug.Log("Oyuncu ilk kez öldü! MiniOyun sahnesine geçilecek...");
            
            // playerData.isPlayerRespawned = false; // Değeri değiştirme, MiniGame sonrası true yapılacak
            
            // Verileri kaydet
            if (playerData != null)
            {
                playerData.SaveValues();
                Debug.Log("Oyuncu öldü! Veriler kaydedildi. isPlayerRespawned: " + playerData.isPlayerRespawned);
            }
            
            // Health Slider'ı bul ve devre dışı bırak/yok et
            if (healthSlider != null)
            {
                Destroy(healthSlider.gameObject);
                Debug.Log("Player Health Slider sahneden kaldırıldı!");
            }
            
            // Ölüm efekti 
            if (damageEffect != null)
            {
                Instantiate(damageEffect, transform.position, Quaternion.identity);
            }
            
            // Sprite'ı sönükleştir
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
            
            // Oyun nesnesini yok et (efektlerin oynatılabilmesi için kısa bir süre bekle)
            Destroy(gameObject, 0.5f);
            
            // 0.5 saniye sonra doğrudan sahne geçişi yap
            Debug.Log("MiniOyun sahnesine geçiliyor...");
            Invoke("LoadMiniGameScene", 0.5f);
        }
    }

    // Doğrudan MiniOyun sahnesini yükle - Invoke ile çağrılacak
    private void LoadMiniGameScene()
    {
        Debug.Log("MiniOyun sahnesine doğrudan geçiş yapılıyor...");
        SceneManager.LoadScene("MiniOyun");
    }

    // Çarpışma algılama - Trigger için
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player OnTriggerEnter2D: " + gameObject.name + " triggered with " + other.gameObject.name + " (Tag: " + other.tag + ")");

        // Eğer oyuncu dokunulmazsa veya ölüyse çarpışmaları yoksay
        if (isInvincible || isDead)
        {
            Debug.Log("Player dokunulmaz/ölü durumda, çarpışma yoksayıldı!");
            return;
        }

        // Düşman ile çarpışma kontrolü
        if (other.CompareTag("Enemy"))
        {
            // Düşmandan hasar miktarını al
            Enemy enemy = other.GetComponent<Enemy>();
            int damage = 10; // Varsayılan hasar

            if (enemy != null)
            {
                // Düşman tipine göre işlem yap
                if (enemy.enemyType == EnemyType.Kamikaze)
                {
                    damage = 15; // Kamikaze düşmanı için hasar değeri
                    Debug.Log("Kamikaze düşman oyuncuya çarptı ve yok edilecek!");
                    Destroy(other.gameObject);
                }
                else if (enemy.enemyType == EnemyType.Minigun)
                {
                    damage = 10; // Minigun düşmanı çarpışma hasarı
                    Debug.Log("Minigun düşman oyuncuya çarptı! Hasar uygulandı, ama düşman yok edilmedi.");
                    // Minigun düşmanı yok edilmiyor, sadece hasar veriyor
                }

                // Hasar uygulamadan önce log
                Debug.Log("Düşman Player'a çarpmak üzere (Trigger). Düşman tipi: " + enemy.enemyType + ", Hasar: " + damage);
            }

            // Oyuncuya hasar ver
            TakeDamage(damage);
        }

        // Düşman roketi ile çarpışma
        if (other.CompareTag("Rocket"))
        {
            RocketProjectile rocket = other.GetComponent<RocketProjectile>();
            if (rocket != null && rocket.isEnemyRocket)
            {
                TakeDamage(rocket.damage);
                Debug.Log("Oyuncu düşman roketiyle vuruldu! Hasar: " + rocket.damage);

                // Roketi patlat veya yok et
                Destroy(other.gameObject);
            }
        }

        // Düşman mermisi ile çarpışma
        if (other.tag == "Bullet")
        {
            // Mermi komponenti al
            Bullet bullet = other.GetComponent<Bullet>();
            
            // Eğer düşman mermisiyse hasar ver
            if (bullet != null && bullet.isEnemyBullet)
            {
                TakeDamage(bullet.damage);
                Debug.Log("Oyuncu düşman mermisiyle vuruldu! Hasar: " + bullet.damage);
                
                // Mermiyi yok et
                Destroy(other.gameObject);
            }
        }
    }

    // Çarpışma algılama - Fiziksel çarpışma için
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player OnCollisionEnter2D: " + gameObject.name + " collided with " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");

        // Eğer oyuncu dokunulmazsa veya ölüyse çarpışmaları yoksay
        if (isInvincible || isDead)
        {
            Debug.Log("Player dokunulmaz/ölü durumda, çarpışma yoksayıldı! (Collision)");
            return;
        }

        // Düşman ile çarpışma kontrolü
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Düşmandan hasar miktarını al
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            int damage = 10; // Varsayılan hasar

            if (enemy != null)
            {
                // Düşman tipine göre işlem yap
                if (enemy.enemyType == EnemyType.Kamikaze)
                {
                    damage = 15; // Kamikaze düşmanı için hasar değeri
                    Debug.Log("Kamikaze düşman oyuncuya çarptı ve yok edilecek! (Collision)");
                    Destroy(collision.gameObject);
                }
                else if (enemy.enemyType == EnemyType.Minigun)
                {
                    damage = 10; // Minigun düşmanı çarpışma hasarı
                    Debug.Log("Minigun düşman oyuncuya çarptı! Hasar uygulandı, ama düşman yok edilmedi. (Collision)");
                    // Minigun düşmanı yok edilmiyor, sadece hasar veriyor
                }

                // Hasar uygulamadan önce log
                Debug.Log("Düşman Player'a çarptı (Collision). Düşman tipi: " + enemy.enemyType + ", Hasar: " + damage);
            }

            // Oyuncuya hasar ver
            TakeDamage(damage);
        }

        // Düşman mermisi ile çarpışma
        if (collision.gameObject.tag == "Bullet")
        {
            // Mermi komponenti al
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            
            // Eğer düşman mermisiyse hasar ver
            if (bullet != null && bullet.isEnemyBullet)
            {
                TakeDamage(bullet.damage);
                Debug.Log("Oyuncu düşman mermisiyle vuruldu! (Collision) Hasar: " + bullet.damage);
                
                // Mermiyi yok et
                Destroy(collision.gameObject);
            }
        }
    }

    // Manuel sprite animasyonu ayarı
    private void SetupManualAnimation()
    {
        // Animator'ı devre dışı bırak (varsa)
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Sprite dizisini kontrol et
        if (animationFrames == null || animationFrames.Length == 0)
        {
            Debug.LogWarning("Animasyon kareleri (animationFrames) atanmamış! Inspector'dan 5 adet kareyi sırasıyla atayın.");
            return;
        }

        // Animasyonu başlat
        isAnimationPlaying = true;
        currentFrameIndex = 0;
        animationTimer = 0f;

        // İlk kareyi ayarla
        if (spriteRenderer != null && animationFrames.Length > 0)
        {
            spriteRenderer.sprite = animationFrames[0];
            Debug.Log("Manuel sprite animasyonu başlatıldı. Kare sayısı: " + animationFrames.Length);
        }
    }

    // Manuel sprite animasyonunu güncelle
    private void UpdateSpriteAnimation()
    {
        // Eğer sprite renderer veya animasyon kareleri yoksa dön
        if (spriteRenderer == null || animationFrames == null || animationFrames.Length == 0)
            return;

        // Zamanlayıcıyı güncelle
        animationTimer += Time.deltaTime;

        // Zaman geldiğinde bir sonraki kareye geç
        if (animationTimer >= animationSpeed)
        {
            // Sonraki kareye geç (döngüsel)
            currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;

            // Sprite'ı güncelle
            spriteRenderer.sprite = animationFrames[currentFrameIndex];

            // Zamanlayıcıyı sıfırla
            animationTimer = 0f;
        }
    }
}