using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    
    [Header("Hedef")]
    public Vector2 targetPosition = Vector2.zero; // Varsayılan olarak (0,0) hedefi
    
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Rastgele başlangıç hızı varyasyonu (daha doğal görünüm için)
        moveSpeed = Random.Range(moveSpeed * 0.8f, moveSpeed * 1.2f);
    }
    
    private void Update()
    {
        MoveTowardsTarget();
        FlipSpriteBasedOnDirection();
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
        // Hedefe ulaşınca oyuncuya zarar verebilir veya oyun mekanikleri tetiklenebilir
        // Şimdilik sadece düşmanı yok edelim
        Destroy(gameObject);
    }
    
    // Düşman vurulduğunda çağrılacak fonksiyon
    public void TakeDamage()
    {
        // Burada düşmana ateş edildiğinde yapılacak işlemleri ekleyebilirsiniz
        // Örneğin: Efekt oluştur, puan ekle, yok ol, vb.
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mermi veya oyuncuyla çarpışma
        if (collision.CompareTag("Player") || collision.CompareTag("Bullet"))
        {
            TakeDamage();
        }
    }
} 