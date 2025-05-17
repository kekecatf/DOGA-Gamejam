using UnityEngine;
using UnityEngine.UI; // UI elemanları için

public class Zeplin : MonoBehaviour
{
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
        if (playerData == null)
        {
            Debug.LogError("PlayerData bulunamadı! Zeplin düzgün çalışmayabilir.");
            return;
        }
        
        // UI elemanlarını güncelle
        UpdateUI();
    }
    
    // Zeplin'e hasar verme metodu
    public void TakeDamage(int damage)
    {
        if (playerData == null)
        {
            playerData = FindObjectOfType<PlayerData>();
            if (playerData == null)
            {
                Debug.LogError("PlayerData bulunamadı! Hasar uygulanamıyor.");
                return;
            }
        }
        
        // PlayerData'daki sağlık değerini azalt
        playerData.zeplinSaglik -= damage;
        
        // Hasar efekti göster (eğer varsa)
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // UI elemanlarını güncelle
        UpdateUI();
        
        Debug.Log("Zeplin hasar aldı! Kalan sağlık: " + playerData.zeplinSaglik + "/" + playerData.zeplinSaglik);
        
        // Eğer sağlık sıfırın altına düştüyse
        if (playerData.zeplinSaglik <= 0)
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
        if (playerData == null) return;
        
        // Sağlık çubuğunu güncelle (eğer varsa)
        if (healthSlider != null)
        {
            healthSlider.maxValue = playerData.zeplinSaglik;
            healthSlider.value = playerData.zeplinSaglik;
        }
        
        // Sağlık metnini güncelle (eğer varsa)
        if (healthText != null)
        {
            healthText.text = playerData.zeplinSaglik + " / " + playerData.zeplinSaglik;
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
                damage = enemy.GetDamageAmount();
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
                damage = enemy.GetDamageAmount();
            }
            
            // Zeplin'e hasar ver
            TakeDamage(damage);
            
            // Düşmanı yok et
            Destroy(collision.gameObject);
            
            Debug.Log("Düşman Zeplin'e çarptı ve yok edildi!");
        }
    }
} 