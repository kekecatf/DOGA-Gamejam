using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;  // Mermi ömrü - belirli süre sonra yok olur
    public bool isZeplinBullet = false; // Zeplin'den atılan mermi mi?
    public bool isEnemyBullet = false; // Düşman tarafından atılan mermi mi?
    public int damage = 0; // Özel hasar değeri (düşman mermileri için)
    
    private bool isMovingLeft = false;
    private PlayerData playerData;
    
    void Start()
    {
        // Tag'i ayarla - çarpışma kontrolü için önemli
        gameObject.tag = "Bullet";
        
        // Rigidbody ayarlarını kontrol et
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.Log("Bullet için Rigidbody2D eklendi.");
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
            // Collider yoksa ekle
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true; // Trigger olarak ayarla
            Debug.Log("Bullet için BoxCollider2D eklendi (isTrigger=true).");
        }
        else
        {
            // Emin olmak için collider'ın trigger olduğundan emin ol
            if (!collider.isTrigger)
            {
                Debug.Log("Bullet Collider2D isTrigger kapalıydı, açılıyor.");
                collider.isTrigger = true;
            }
        }
        
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("PlayerData bulunamadı! Mermi düzgün çalışmayabilir.");
        }
        
        // Belirli süre sonra mermiyi yok et
        Destroy(gameObject, lifetime);
        
        // Eğer damage değeri ayarlanmamışsa ve PlayerData varsa, varsayılan hasar değerini ata
        if (damage == 0 && playerData != null)
        {
            if (isEnemyBullet)
            {
                // Düşman mermisi için varsayılan hasar
                damage = 10;
            }
            else if (isZeplinBullet)
            {
                // Zeplin mermisi için hasar
                damage = playerData.zeplinMinigunDamage;
            }
            else
            {
                // Oyuncu mermisi için hasar
                damage = playerData.anaGemiMinigunDamage;
            }
        }
        else if (damage == 0)
        {
            // PlayerData yoksa varsayılan değer
            damage = 10;
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
    
    // Fiziksel çarpışma algılama için
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Mermi fiziksel çarpışma: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + "), Düşman Mermisi: " + isEnemyBullet);
        
        // Düşman mermisi ise farklı işlemler yap
        if (isEnemyBullet)
        {
            HandleEnemyBulletCollision(collision);
            return;
        }
        
        // Player ile çarpışmayı görmezden gel (kendi attığımız mermilerden etkilenmemeliyiz)
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet"))
        {
            return;
        }
        
        // Düşmanla çarpışma kontrolü
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Düşmana hasar ver (düşman script'i varsa)
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Düşmana (collision) " + damage + " hasar verildi!");
            }
            else
            {
                // Eğer düşman script'i yoksa direkt yok et
                Destroy(collision.gameObject);
                Debug.Log("Düşman (collision) yok edildi!");
            }
            
            // Mermiyi yok et
            Destroy(gameObject);
            
            Debug.Log("Mermi düşmana çarptı (collision): " + collision.gameObject.name);
        }
        // Diğer objelerle çarpışma
        else
        {
            // Mermiyi yok et
            Destroy(gameObject);
            
            Debug.Log("Mermi bir nesneye çarptı (collision): " + collision.gameObject.name);
        }
    }
    
    // Düşman mermisi fiziksel çarpışma kontrolü
    private void HandleEnemyBulletCollision(Collision2D collision)
    {
        Debug.Log("Düşman mermisi fiziksel çarpışma: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
        
        // Düşmanla çarpışmayı görmezden gel (düşman kendi mermileriyle hasar almamalı)
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Düşman mermisi kendisine veya başka bir mermiye çarptı (collision), yoksayılıyor.");
            return;
        }
        
        // Oyuncuyla çarpışma kontrolü
        if (collision.gameObject.CompareTag("Player"))
        {
            // Oyuncuya hasar ver
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Oyuncuya düşman mermisiyle (collision) " + damage + " hasar verildi!");
            }
            
            // Mermiyi yok et
            Destroy(gameObject);
        }
        // Zeplin ile çarpışma - tag veya component ile kontrol et
        else if (collision.gameObject.CompareTag("Zeplin") || collision.gameObject.GetComponent<Zeplin>() != null)
        {
            // Zeplin'e hasar ver
            Zeplin zeplin = collision.gameObject.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                zeplin.TakeDamage(damage);
                Debug.Log("Zeplin'e düşman mermisiyle (collision) " + damage + " hasar verildi! Mermi ID: " + GetInstanceID());
            }
            else
            {
                Debug.LogWarning("Zeplin etiketi var ama Zeplin bileşeni bulunamadı! (collision)");
            }
            
            // Mermiyi yok et
            Destroy(gameObject);
        }
        // Diğer objelerle çarpışma
        else
        {
            // Mermiyi yok et
            Destroy(gameObject);
            Debug.Log("Düşman mermisi bir nesneye çarptı (collision): " + collision.gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Düşman mermisi farklı davranıyor
        if (isEnemyBullet)
        {
            HandleEnemyBulletTrigger(other);
            return;
        }
        
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
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Düşmana " + damage + " hasar verildi!");
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
    
    // Düşman mermisi çarpışma kontrolü - Trigger için
    private void HandleEnemyBulletTrigger(Collider2D other)
    {
        Debug.Log("Düşman mermisi trigger çarpışma: " + other.gameObject.name + " (Tag: " + other.tag + ")");
        
        // Düşmanla çarpışmayı görmezden gel (düşman kendi mermileriyle hasar almamalı)
        if (other.CompareTag("Enemy") || other.CompareTag("Bullet"))
        {
            Debug.Log("Düşman mermisi kendisine veya başka bir mermiye çarptı, yoksayılıyor.");
            return;
        }
        
        // Oyuncuyla çarpışma kontrolü
        if (other.CompareTag("Player"))
        {
            // Oyuncuya hasar ver
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Oyuncuya düşman mermisiyle " + damage + " hasar verildi!");
            }
            
            // Mermiyi yok et
            Destroy(gameObject);
        }
        // Zeplin ile çarpışma - tag veya component ile kontrol et
        else if (other.CompareTag("Zeplin") || other.GetComponent<Zeplin>() != null)
        {
            // Zeplin'e hasar ver
            Zeplin zeplin = other.GetComponent<Zeplin>();
            if (zeplin != null)
            {
                zeplin.TakeDamage(damage);
                Debug.Log("Zeplin'e düşman mermisiyle " + damage + " hasar verildi! Mermi ID: " + GetInstanceID());
            }
            else
            {
                Debug.LogWarning("Zeplin etiketi var ama Zeplin bileşeni bulunamadı!");
            }
            
            // Mermiyi yok et
            Destroy(gameObject);
        }
        // Diğer objelerle çarpışma (duvarlar, engeller vb.)
        else if (!other.isTrigger) // Sadece fiziksel nesnelerle çarpışma durumunda
        {
            // Mermiyi yok et
            Destroy(gameObject);
            Debug.Log("Düşman mermisi bir nesneye çarptı: " + other.name);
        }
    }
} 