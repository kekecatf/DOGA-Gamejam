using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;  // Mermi ömrü - belirli süre sonra yok olur
    
    private bool isMovingLeft = false;
    private PlayerData playerData;
    
    void Start()
    {
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("PlayerData bulunamadı! Mermi düzgün çalışmayabilir.");
        }
        
        // Belirli süre sonra mermiyi yok et
        Destroy(gameObject, lifetime);
        
        // Emin olmak için collider'ın trigger olduğundan emin ol
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log("Bullet collider trigger yapıldı");
        }
        
        /* Çarpışma Algılama Kuralları:
         * 1. OnTriggerEnter2D için en az bir objenin Collider'ı trigger olmalıdır
         * 2. Her iki objenin de Collider bileşeni olmalıdır
         * 3. Hareketli olan obje (mermi) üzerinde Rigidbody2D olmalıdır
         *    (Rigidbody2D bileşenini Unity Editor üzerinden ekleyin)
         * 
         * NOT: En yaygın kurulum şudur:
         * - Mermi: IsTrigger = true ve Rigidbody2D (kinematic olabilir)
         * - Düşman: Normal Collider (IsTrigger = false) ve isteğe bağlı Rigidbody2D
         */
    }
    
    void Update()
    {
        // Hareket yönünü belirle (transform.right kullanarak rotasyona göre)
        Vector2 direction = transform.right;
        
        // Eğer sola bakıyorsa, yönü tersine çevir
        if (isMovingLeft)
        {
            direction = -direction;
        }
        
        // Hesaplanan yönde hareket et
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
    
    // Oyuncudan yön bilgisini al
    public void SetDirection(bool isLeft)
    {
        isMovingLeft = isLeft;
        
        // Sprite'ın yönünü ayarla (sol veya sağ)
        Vector3 scale = transform.localScale;
        scale.x = isMovingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    
    // Rotasyon bilgisini al (firePoint'in rotasyonu)
    public void SetRotation(float newRotation)
    {
        transform.rotation = Quaternion.Euler(0, 0, newRotation);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player ile çarpışmayı görmezden gel (kendi attığımız mermilerden etkilenmemeliyiz)
        if (other.CompareTag("Player") || other.CompareTag("Bullet"))
        {
            return;
        }
        
        // Düşmanla çarpışma kontrolü
        if (other.CompareTag("Enemy"))
        {
            // Düşmana hasar ver (düşman script'i varsa)
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && playerData != null)
            {
                int damage = playerData.anaGemiMinigunDamage;
                enemy.TakeDamage(damage);
                Debug.Log("Düşmana " + damage + " hasar verildi!");
            }
            else if (enemy != null)
            {
                // PlayerData bulunamazsa varsayılan değer kullan
                enemy.TakeDamage(10);
                Debug.Log("Düşmana varsayılan hasar verildi: 10");
            }
            else
            {
                // Eğer düşman script'i yoksa direkt yok et
                Destroy(other.gameObject);
                Debug.Log("Düşman yok edildi!");
            }
            
            // Mermiyi yok et
            Destroy(gameObject);
            
            Debug.Log("Mermi düşmana çarptı: " + other.name);
        }
        // Diğer objelerle çarpışma (duvarlar, engeller vb.)
        else if (!other.isTrigger) // Sadece fiziksel nesnelerle çarpışma durumunda
        {
            // Mermiyi yok et
            Destroy(gameObject);
            
            Debug.Log("Mermi bir nesneye çarptı: " + other.name);
        }
    }
} 