using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    
    [Header("Hedef")]
    private Vector2 targetPosition = Vector2.zero; // Hedef pozisyonu (objenin anlık konumundan alınır)
    
    private SpriteRenderer spriteRenderer;
    private PlayerData playerData;
    
    private int currentHealth;
    private int damage;
    public int scoreValue = 25;
    public GameObject deathEffect; // Ölüm efekti (isteğe bağlı)
    
    private void Start()
    {
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Rastgele başlangıç hızı varyasyonu (daha doğal görünüm için)
        moveSpeed = Random.Range(moveSpeed * 0.8f, moveSpeed * 1.2f);
        
        // Düşman değerlerini PlayerData'dan al
        InitializeEnemyStats();
    }
    
    // Düşman değerlerini PlayerData'dan alma
    private void InitializeEnemyStats()
    {
        if (playerData != null)
        {
            // PlayerData'dan düşman sağlığı, hasarı ve ödül değerini al
            currentHealth = playerData.CalculateEnemyHealth();
            damage = playerData.CalculateEnemyDamage();
            scoreValue = playerData.CalculateEnemyScoreValue();
            
            Debug.Log("Düşman değerleri PlayerData'dan alındı: Sağlık=" + currentHealth + 
                     ", Hasar=" + damage + ", Ödül=" + scoreValue);
        }
        else
        {
            // PlayerData bulunamazsa varsayılan değerleri kullan
            currentHealth = 50;
            damage = 10;
            scoreValue = 25;
            Debug.LogWarning("PlayerData bulunamadı! Varsayılan düşman değerleri kullanılıyor.");
        }
    }
    
    private void Update()
    {
        UpdateTargetPosition();
        MoveTowardsTarget();
        FlipSpriteBasedOnDirection();
    }
    
    // Hedef pozisyonunu güncelle
    private void UpdateTargetPosition()
    {
        // Player ölmediyse, Player'ı hedef al
        if (!Player.isDead)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                targetPosition = player.transform.position;
            }
        }
        // Player öldüyse, Zeplin'i hedef al
        else
        {
            Zeplin zeplin = FindObjectOfType<Zeplin>();
            if (zeplin != null)
            {
                targetPosition = zeplin.transform.position;
            }
        }
    }
    
    private void MoveTowardsTarget()
    {
        // Hedef yönünü hesapla
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Hedefe doğru hareket et
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        
        // Eğer hedefe ulaştıysa
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            // Hedefe ulaşıldığında (örneğin, oyuncuya zarar ver veya yok ol)
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
    
    private void FlipSpriteBasedOnDirection()
    {
        if (spriteRenderer != null)
        {
            // Hareket yönüne göre sprite'ı çevir
            // NOT: Düşman sprite'ınızın varsayılan yönüne göre bu değişebilir
            Vector2 direction = targetPosition - (Vector2)transform.position;
            spriteRenderer.flipY = direction.x < 0;
        }
    }
    
    private void OnReachedTarget()
    {
        // Player'a ulaştıysa
        if (!Player.isDead)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Player playerComponent = player.GetComponent<Player>();
                if (playerComponent != null)
                {
                    playerComponent.TakeDamage(damage);
                    Debug.Log("Düşman Player'a ulaştı ve " + damage + " hasar verdi!");
                }
            }
        }
        // Zeplin'e ulaştıysa
        else
        {
            Zeplin zeplin = FindObjectOfType<Zeplin>();
            if (zeplin != null)
            {
                zeplin.TakeDamage(damage);
                Debug.Log("Düşman Zeplin'e ulaştı ve " + damage + " hasar verdi!");
            }
        }
        
        // Düşmanı yok et
        Die();
    }
    
    // Düşmana hasar verme metodu (Bullet tarafından çağrılacak)
    public void TakeDamage(int damageAmount)
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
            Debug.Log("Düşman hasar aldı. Kalan sağlık: " + currentHealth);
        }
    }
    
    void Die()
    {
        // Ölüm animasyonu veya efekti (isteğe bağlı)
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Skor ekle (PlayerData üzerinden)
        if (playerData != null)
        {
            // Parasını arttır
            playerData.metalPara += scoreValue;
            Debug.Log("Düşman öldürüldü! Para kazanıldı: " + scoreValue);
        }
        else
        {
            // PlayerData referansını tekrar bulmayı dene
            playerData = FindObjectOfType<PlayerData>();
            if (playerData != null)
            {
                playerData.metalPara += scoreValue;
                Debug.Log("Düşman öldürüldü! Para kazanıldı: " + scoreValue);
            }
        }
        
        // Düşmanı yok et
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mermi veya oyuncuyla çarpışma
        if (collision.CompareTag("Player") || collision.CompareTag("Bullet"))
        {
            TakeDamage(damage);
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
} 