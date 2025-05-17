using UnityEngine;
using UnityEngine.UI; // UI elemanları için

public class Zeplin : MonoBehaviour
{
    // Zeplin özellikleri
    [Header("Zeplin Özellikleri")]
    public int maxHealth = 100;
    private int currentHealth;
    
    // UI elemanları
    [Header("UI Elemanları")]
    public Slider healthSlider; // Opsiyonel sağlık çubuğu
    public Text healthText; // Opsiyonel sağlık metni
    
    // Efektler
    [Header("Efektler")]
    public GameObject damageEffect; // Hasar alınca çıkacak efekt (opsiyonel)
    public GameObject destroyEffect; // Yok olunca çıkacak efekt (opsiyonel)
    
    // PlayerData referansı
    private PlayerData playerData;
    
    void Start()
    {
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        
        // PlayerData'dan başlangıç sağlık değerini al
        if (playerData != null)
        {
            maxHealth = playerData.zeplinSaglik;
            Debug.Log("Zeplin sağlığı PlayerData'dan alındı: " + maxHealth);
        }
        
        // Başlangıç sağlık değerini ayarla
        currentHealth = maxHealth;
        
        // UI elemanlarını güncelle
        UpdateUI();
    }
    
    // Zeplin'e hasar verme metodu
    public void TakeDamage(int damage)
    {
        // Sağlık değerini azalt
        currentHealth -= damage;
        
        // Hasar efekti göster (eğer varsa)
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // UI elemanlarını güncelle
        UpdateUI();
        
        Debug.Log("Zeplin hasar aldı! Kalan sağlık: " + currentHealth + "/" + maxHealth);
        
        // Eğer sağlık sıfırın altına düştüyse
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Zeplin'in yok olma metodu
    void Die()
    {
        // Yok olma efekti göster (eğer varsa)
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Zeplin yok oldu!");
        
        // Oyun sonu mantığı (örneğin: oyunu kaybetme ekranı)
        // GameManager.Instance.GameOver();
        
        // Zeplin'i yok et
        Destroy(gameObject);
    }
    
    // UI elemanlarını güncelleme metodu
    void UpdateUI()
    {
        // Sağlık çubuğunu güncelle (eğer varsa)
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        // Sağlık metnini güncelle (eğer varsa)
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
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
                damage = enemy.damageAmount;
            }
            
            // Zeplin'e hasar ver
            TakeDamage(damage);
            
            // Düşmanı yok et
            Destroy(other.gameObject);
            
            Debug.Log("Düşman Zeplin'e çarptı ve yok edildi!");
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
                damage = enemy.damageAmount;
            }
            
            // Zeplin'e hasar ver
            TakeDamage(damage);
            
            // Düşmanı yok et
            Destroy(collision.gameObject);
            
            Debug.Log("Düşman Zeplin'e çarptı ve yok edildi!");
        }
    }
    
    // PlayerData'ya sağlık değerini kaydet
    void OnDestroy()
    {
        if (playerData != null)
        {
            playerData.zeplinSaglik = currentHealth > 0 ? currentHealth : 0;
            Debug.Log("Zeplin sağlığı PlayerData'ya kaydedildi: " + playerData.zeplinSaglik);
        }
    }
} 