using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rocket : MonoBehaviour
{
    public float rotationSpeed = 200f;
    public float lifetime = 5f;
    public float explosionRadius = 2f;
    public GameObject explosionEffect;
    
    private Transform target;
    private bool targetLocked = false;
    private PlayerData playerData;
    private List<Collider2D> damagedEnemies = new List<Collider2D>(); // Hasar verilen düşmanları takip etmek için
    
    // Animator bileşeni referansı
    private Animator animator;
    
    void Start()
    {
        // Find PlayerData reference
        playerData = FindObjectOfType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("PlayerData bulunamadı! Roket düzgün çalışmayabilir.");
        }
        
        // Animator bileşenini al veya ekle
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            // Animator yoksa ekliyoruz
            animator = gameObject.AddComponent<Animator>();
            Debug.Log("Roket'e Animator bileşeni eklendi.");
        }
        else
        {
            // Animasyonu başlat
            animator.Play("RocketFly");
            Debug.Log("Roket animasyonu başlatıldı.");
        }
        
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
                transform.Translate(Vector2.right * playerData.anaGemiRoketSpeed * Time.deltaTime, Space.Self);
                return;
            }
        }
        
        // Hedefe doğru dön
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        
        // İleri doğru hareket et
        transform.Translate(Vector2.right * playerData.anaGemiRoketSpeed * Time.deltaTime, Space.Self);
        
        // Animator bileşeni varsa, animasyonu oynat
        if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("RocketFly"))
        {
            animator.Play("RocketFly");
        }
    }
    
    void FindClosestEnemy()
    {
        // Tüm düşmanları bul (Enemy ve Boss taglarını ara)
        GameObject[] enemiesWithEnemyTag = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] enemiesWithBossTag = GameObject.FindGameObjectsWithTag("Boss");
        
        // Her iki tag'e sahip düşmanları birleştir
        List<GameObject> allEnemies = new List<GameObject>();
        allEnemies.AddRange(enemiesWithEnemyTag);
        allEnemies.AddRange(enemiesWithBossTag);
        
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        
        // En yakın düşmanı bul
        foreach (GameObject enemy in allEnemies)
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
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
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
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
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
            enemy.TakeDamage(playerData.anaGemiRoketDamage);
            Debug.Log("Roket doğrudan düşmana çarptı: " + enemyCollider.name + ", " + playerData.anaGemiRoketDamage + " hasar verildi!");
            
            // Bu düşmanı hasarlı düşmanlar listesine ekle
            damagedEnemies.Add(enemyCollider);
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
        // Patlama alanındaki tüm düşmanları bul
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Eğer bu düşmana zaten doğrudan hasar verildiyse, tekrar hasar verme
            if (damagedEnemies.Contains(hitCollider))
            {
                continue;
            }
            
            // Düşmanlara hasar ver (Enemy ve Boss taglerini kontrol et)
            if (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("Boss"))
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Alan hasarını daha az ver (örneğin 70% kadar)
                    int areaDamage = Mathf.RoundToInt(playerData.anaGemiRoketDamage * 0.7f);
                    enemy.TakeDamage(areaDamage);
                    
                    Debug.Log("Roket patlamasıyla düşmana hasar verildi: " + hitCollider.name + ", " + areaDamage + " hasar!");
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
} 