using UnityEngine;
using UnityEngine.UI; // UI bileşenleri için
using UnityEngine.EventSystems; // Joystick için gerekli

public class Player : MonoBehaviour
{
    // Hareket hızı
    public float moveSpeed = 5f;
    
    // Yönlendirme ayarları
    public float upRotation = 25f;
    public float downRotation = -25f;
    public float rotationSpeed = 1.5f; // Daha da yavaş rotasyon değişimi
    
    // Joystick Referansı
    public Joystick joystick; // Dynamic Joystick referansı buraya sürüklenecek
    
    // Mermi ayarları
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.3f;
    public float firePointXOffset = 1f; // FirePoint'in x ekseni ofset değeri
    private float nextFireTime = 0f;
    
    // Bileşenler
    private SpriteRenderer spriteRenderer;
    private bool isFacingLeft = false;
    private float currentRotation = 0f;     // Mevcut gerçek rotasyon
    private float targetRotation = 0f;      // Hedef rotasyon
    private float absoluteRotation = 0f;    // Mutlak rotasyon (flip bağımsız)
    private Vector3 originalFirePointLocalPos;
    
    // Dikey hareket kontrolü
    private float verticalInput = 0f;
    private float horizontalInput = 0f;
    
    private void Start()
    {
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
    }
    
    private void Update()
    {
        // Hareket ve yönlendirme
        Movement();
        RotateSmooth();
        
        // Ateş etme (space veya ekstra buton)
        HandleShooting();
    }
    
    private void Movement()
    {
        // Joystick girişini al (varsa)
        if (joystick != null)
        {
            horizontalInput = joystick.Horizontal;
            verticalInput = joystick.Vertical;
            
            // Deadzone uygula (çok küçük değerleri yoksay)
            horizontalInput = Mathf.Abs(horizontalInput) < 0.1f ? 0 : horizontalInput;
            verticalInput = Mathf.Abs(verticalInput) < 0.1f ? 0 : verticalInput;
        }
        else
        {
            // Joystick yoksa klavye girişini al (test için)
            horizontalInput = 0;
            verticalInput = 0;
            
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
            
        // Hareket vektörü oluştur
        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0).normalized;
        
        // Pozisyonu güncelle
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Sprite yönünü ayarla (sağ veya sol) ve FirePoint pozisyonunu güncelle
        if (horizontalInput != 0 && spriteRenderer != null)
        {
            bool wasFlipped = isFacingLeft;
            isFacingLeft = horizontalInput < 0;
            spriteRenderer.flipX = isFacingLeft;
            
            // Eğer yön değiştiyse, FirePoint pozisyonunu güncelle
            if (wasFlipped != isFacingLeft && firePoint != null && firePoint != transform)
            {
                // Pozisyon güncellemesi (sadece x ekseninde yapılacak)
                UpdateFirePointPosition();
                
                // ÖNEMLİ: Flip esnasında mevcut rotasyonu koru, hedefi güncelle
                // Eğer flip değiştiyse, mevcut rotasyonu aynen koru ama işaretini değiştir
                if (absoluteRotation != 0)
                {
                    // İşareti flip durumuna göre değiştir, ama değeri koru
                    targetRotation = isFacingLeft ? -absoluteRotation : absoluteRotation;
                    
                    // Mevcut rotasyonu direk olarak ayarla (Lerp kullanma)
                    currentRotation = targetRotation;
                    
                    // Fiziksel rotasyonu hemen güncelle
                    transform.rotation = Quaternion.Euler(0, 0, currentRotation);
                    
                    Debug.Log("Flip durumu değişti! Rotasyon korundu: " + currentRotation);
                }
            }
            // FirePoint pozisyonu değişmese bile, sprite'ı flip et
            else if (firePoint != null && firePoint != transform)
            {
                // FirePoint'in sprite'ını flip et (eğer SpriteRenderer bileşeni varsa)
                SpriteRenderer firePointSpriteRenderer = firePoint.GetComponent<SpriteRenderer>();
                if (firePointSpriteRenderer != null)
                {
                    firePointSpriteRenderer.flipX = isFacingLeft;
                }
            }
        }
        
        // Hedef rotasyonu belirle (yukarı ve aşağı hareket)
        UpdateTargetRotation();
    }
    
    private void UpdateTargetRotation()
    {
        // Eğer dikey hareket varsa mutlak rotasyonu güncelle
        if (verticalInput > 0)
        {
            // Yukarı hareket
            absoluteRotation = upRotation; // Pozitif değer (yukarı rotasyon)
        }
        else if (verticalInput < 0)
        {
            // Aşağı hareket
            absoluteRotation = downRotation; // Negatif değer (aşağı rotasyon)
        }
        else if (verticalInput == 0)
        {
            // Dikey hareket yoksa sıfıra dön
            absoluteRotation = 0f;
        }
        
        // Mutlak rotasyonu yöne göre hedef rotasyona çevir
        targetRotation = isFacingLeft ? -absoluteRotation : absoluteRotation;
    }
    
    private void UpdateFirePointPosition()
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
        
        // FirePoint rotasyonunu güncelle - y ekseni her zaman yukarı bakacak şekilde
        UpdateFirePointRotation();
    }
    
    private void UpdateFirePointRotation()
    {
        // FirePoint rotasyonu her durumda aynı olmalı - flip edilmiş haliyle bile
        // Rotasyonu her zaman player rotasyonu ile aynı tut, yön değişimi için sprite flip kullan
        firePoint.rotation = Quaternion.Euler(0, 0, currentRotation);
    }
    
    private void RotateSmooth()
    {
        // Mevcut rotasyonu hedef rotasyona çok yavaş ve kademeli olarak yaklaştır
        currentRotation = Mathf.Lerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Rotasyonu uygula
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        
        // FirePoint rotasyonunu da güncelle - y ekseni her zaman yukarı bakacak şekilde
        if (firePoint != null && firePoint != transform)
        {
            UpdateFirePointRotation();
        }
    }
    
    private void HandleShooting()
    {
        // Space tuşuna basılınca veya mobil ateş butonuna basılınca 
        // (mobil ateş butonu için public bir metot oluşturun)
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    // Mobil ateş butonu için public metot
    public void MobileFireButton()
    {
        if (Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + fireRate;
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
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Mermi bileşenini al
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            // Yön bilgisini ayarla (sağa veya sola hareket için)
            bulletComponent.SetDirection(isFacingLeft);
        }
    }
} 