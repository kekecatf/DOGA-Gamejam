using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 200f;
    public float lifetime = 5f;
    public int damage = 50;
    public float explosionRadius = 2f;
    public GameObject explosionEffect;
    
    private Transform target;
    private bool targetLocked = false;
    
    void Start()
    {
        // Belirli bir süre sonra roketi yok et (hedef bulamazsa)
        Destroy(gameObject, lifetime);
        
        // En yakın düşmanı hedef al
        FindClosestEnemy();
        
        // Rigidbody2D ve Collider kontrolü
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
        }
    }
    
    void Update()
    {
        // Eğer hedef yoksa veya yok edildiyse yeni hedef bul
        if (target == null)
        {
            FindClosestEnemy();
            
            // Hedef hala bulunamadıysa düz ilerle
            if (target == null)
            {
                transform.Translate(Vector2.right * speed * Time.deltaTime, Space.Self);
                return;
            }
        }
        
        // Hedefe doğru dön
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        
        // İleri doğru hareket et
        transform.Translate(Vector2.right * speed * Time.deltaTime, Space.Self);
    }
    
    void FindClosestEnemy()
    {
        // Tüm düşmanları bul
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        
        // En yakın düşmanı bul
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        
        // En yakın düşmanı hedef olarak ayarla
        if (closestEnemy != null)
        {
            target = closestEnemy.transform;
            targetLocked = true;
            Debug.Log("Roket hedef buldu: " + target.name);
        }
        else
        {
            targetLocked = false;
            Debug.Log("Roket hedef bulamadı!");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Roket trigger çarpışması algılandı: " + other.name + " tag: " + other.tag);
        
        // Oyuncu ile çarpışmayı görmezden gel
        if (other.CompareTag("Player") || other.CompareTag("Bullet") || other.CompareTag("Rocket"))
        {
            return;
        }
        
        // Düşman ile çarpışma kontrolü
        if (other.CompareTag("Enemy"))
        {
            HandleEnemyHit(other);
        }
        // Diğer nesnelerle çarpışma (duvarlar, engeller vb.)
        else if (!other.isTrigger) // Sadece fiziksel nesnelerle çarpışma durumunda
        {
            HandleObstacleHit(other);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Roket fiziksel çarpışma algılandı: " + collision.gameObject.name);
        
        // Oyuncu ile çarpışmayı görmezden gel
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Rocket"))
        {
            return;
        }
        
        // Düşman ile çarpışma kontrolü
        if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision.collider);
        }
        else
        {
            HandleObstacleHit(collision.collider);
        }
    }
    
    void HandleEnemyHit(Collider2D enemyCollider)
    {
        // Çarpışma noktasında patlama efekti oluştur
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // Düşmana doğrudan hasar ver
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Doğrudan çarpışan düşmana tam hasar ver
            enemy.TakeDamage(damage);
            Debug.Log("Roket doğrudan düşmana çarptı: " + enemyCollider.name + ", " + damage + " hasar verildi!");
        }
        else
        {
            // Eğer düşman script'i yoksa direkt yok et
            Destroy(enemyCollider.gameObject);
            Debug.Log("Düşman yok edildi!");
        }
        
        // Alan hasarı ver (diğer yakındaki düşmanlara)
        Explode();
        
        // Roketi yok et
        Debug.Log("Roket yok ediliyor...");
        Destroy(gameObject);
    }
    
    void HandleObstacleHit(Collider2D obstacleCollider)
    {
        // Çarpışma noktasında patlama efekti oluştur
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // Alan hasarı ver
        Explode();
        
        // Roketi yok et
        Debug.Log("Roket engele çarptı ve yok ediliyor...");
        Destroy(gameObject);
    }
    
    void Explode()
    {
        // Patlama alanındaki tüm collider'ları bul
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D hit in colliders)
        {
            // Düşmanlara hasar ver
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Uzaklığa göre hasar hesapla (merkezde tam hasar, kenarlarda daha az)
                    float distance = Vector2.Distance(transform.position, hit.transform.position);
                    float damagePercent = 1f - (distance / explosionRadius);
                    int calculatedDamage = Mathf.RoundToInt(damage * damagePercent);
                    
                    // Minimum hasar garantisi
                    calculatedDamage = Mathf.Max(calculatedDamage, damage / 4);
                    
                    // Hasarı uygula
                    enemy.TakeDamage(calculatedDamage);
                    Debug.Log("Roket patlaması " + hit.name + " düşmanına " + calculatedDamage + " hasar verdi!");
                }
            }
        }
    }
    
    // Patlama alanını görselleştir (sadece Unity Editor'da)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    
    // Roket hasarını ayarla
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
} 