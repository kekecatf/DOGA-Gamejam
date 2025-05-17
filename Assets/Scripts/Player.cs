using UnityEngine;

public class Player : MonoBehaviour
{
    // Hareket hızı
    public float moveSpeed = 5f;
    
    // Yönlendirme ayarları
    public float upRotation = 25f;
    public float downRotation = -25f;
    public float rotationSpeed = 1.5f; // Daha da yavaş rotasyon değişimi
    
    // Mermi ayarları
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.3f;
    public float firePointXOffset = 1f; // FirePoint'in x ekseni ofset değeri
    private float nextFireTime = 0f;
    
    // Bileşenler
    private SpriteRenderer spriteRenderer;
    private bool isFacingLeft = false;
    private float currentRotation = 0f;
    private float targetRotation = 0f;
    private Vector3 originalFirePointLocalPos;
    
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
            // FirePoint'in orijinal pozisyonunu kaydet
            originalFirePointLocalPos = firePoint.localPosition;
        }
        
        // Debug mesajı
        Debug.Log("Player başlatıldı. Rotasyon kontrolü hazır.");
    }
    
    private void Update()
    {
        // Hareket ve yönlendirme
        Movement();
        RotateSmooth();
        
        // Ateş etme
        HandleShooting();
    }
    
    private void Movement()
    {
        // Yatay ve dikey girdi al
        float horizontal = 0;
        float vertical = 0;
        
        // WASD tuşları için kontrol
        if (Input.GetKey(KeyCode.W))
            vertical = 1;
        if (Input.GetKey(KeyCode.S))
            vertical = -1;
        if (Input.GetKey(KeyCode.A))
            horizontal = -1;
        if (Input.GetKey(KeyCode.D))
            horizontal = 1;
            
        // Hareket vektörü oluştur
        Vector3 direction = new Vector3(horizontal, vertical, 0).normalized;
        
        // Pozisyonu güncelle
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Sprite yönünü ayarla (sağ veya sol) ve FirePoint pozisyonunu güncelle
        if (horizontal != 0 && spriteRenderer != null)
        {
            bool wasFlipped = isFacingLeft;
            isFacingLeft = horizontal < 0;
            spriteRenderer.flipX = isFacingLeft;
            
            // Eğer yön değiştiyse, FirePoint pozisyonunu güncelle
            if (wasFlipped != isFacingLeft && firePoint != null && firePoint != transform)
            {
                UpdateFirePointPosition();
            }
        }
        
        // Hedef rotasyonu belirle (yukarı ve aşağı hareket)
        if (vertical > 0)
        {
            // Yukarı hareket - yönlü rotasyon
            targetRotation = isFacingLeft ? -upRotation : upRotation;
        }
        else if (vertical < 0)
        {
            // Aşağı hareket - yönlü rotasyon
            targetRotation = isFacingLeft ? -downRotation : downRotation;
        }
        else
        {
            // Hareketsiz - normal dönüş
            targetRotation = 0f;
        }
    }
    
    private void UpdateFirePointPosition()
    {
        // FirePoint'in x değerini yöne göre ayarla
        Vector3 newPosition = originalFirePointLocalPos;
        
        // Sola bakıyorsa x değerini tersine çevir
        if (isFacingLeft)
        {
            newPosition.x = -Mathf.Abs(originalFirePointLocalPos.x);
        }
        else
        {
            newPosition.x = Mathf.Abs(originalFirePointLocalPos.x);
        }
        
        // Yeni pozisyonu uygula
        firePoint.localPosition = newPosition;
        
        Debug.Log("FirePoint pozisyonu güncellendi: " + firePoint.localPosition);
    }
    
    private void RotateSmooth()
    {
        // Mevcut rotasyonu hedef rotasyona çok yavaş ve kademeli olarak yaklaştır
        currentRotation = Mathf.Lerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Rotasyonu uygula
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }
    
    private void HandleShooting()
    {
        // Space tuşuna basılınca ve ateş hızı sınırını geçtiyse
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    private void FireBullet()
    {
        // Mermi prefabı kontrolü
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Mermi prefabı atanmamış!");
            return;
        }
        
        // Ateş edilecek rotasyon hesaplama - sprite yönüne göre
        float bulletRotation = CalculateBulletRotation();
        
        // Mermi oluştur (sadece pozisyon - rotasyon sonra ayarlanacak)
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        // Mermi bileşenini al
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            // Geminin yönünü ve rotasyon değerini ayarla
            bulletComponent.SetDirection(isFacingLeft);
            bulletComponent.SetRotation(bulletRotation);
            
            Debug.Log("Player flip: " + isFacingLeft + ", Mermi rotasyonu: " + bulletRotation);
        }
        
        // Not: SpriteRenderer.flipX yerine transform.localScale.x kullanıyoruz artık
        // Bu işlem Bullet.cs içinde yapılıyor
    }
    
    private float CalculateBulletRotation()
    {
        // Mermi rotasyonunu player yönüne göre hesapla
        float rotation = 0f;
        
        if (isFacingLeft)
        {
            // Sol yöne bakıyorsa, 180 derece rotasyon - player rotasyonu (ters çevir)
            rotation = 180f - currentRotation; // - ile çeviriyoruz, yukarı/aşağı doğru yönelime dikkat
        }
        else
        {
            // Sağ yöne bakıyorsa, direkt player rotasyonu
            rotation = currentRotation;
        }
        
        return rotation;
    }
} 