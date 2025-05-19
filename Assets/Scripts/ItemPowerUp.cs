using UnityEngine;

public class ItemPowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        Health,         // Sağlık artırır
        Damage,         // Hasar artırır
        Speed,          // Hız artırır
        Shield,         // Geçici kalkan verir
        MetalPara       // Metal para verir
    }
    
    [Header("PowerUp Ayarları")]
    public PowerUpType powerUpType = PowerUpType.MetalPara;
    public int healthBonus = 20;            // Sağlık artış miktarı
    public int damageBonus = 5;             // Hasar artış miktarı
    public float speedBonus = 1.0f;         // Hız artış faktörü 
    public float shieldDuration = 5.0f;     // Kalkan süresi
    public int metalParaBonus = 25;         // Metal para miktarı
    
    [Header("Görsel Efektler")]
    public float floatSpeed = 1f;           // Yüzme hızı
    public float floatAmount = 0.2f;        // Yüzme miktarı
    public float rotationSpeed = 30f;       // Dönme hızı
    public GameObject collectEffect;         // Toplanma efekti
    
    private Vector3 startPosition;
    private float timeOffset;
    private SpriteRenderer spriteRenderer;
    private PlayerData playerData;
    
    void Start()
    {
        // Başlangıç pozisyonunu kaydet
        startPosition = transform.position;
        
        // Animasyon için random başlangıç zamanı
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        
        // SpriteRenderer bileşenini al
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Eğer SpriteRenderer yoksa ekle
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Collider kontrolü
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // CircleCollider2D ekle
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
            circleCollider.radius = 0.5f;
        }
        else
        {
            // Var olan collider'ı trigger yap
            collider.isTrigger = true;
        }
        
        // PlayerData referansını bul
        playerData = FindObjectOfType<PlayerData>();
        
        // 15 saniye sonra kendiğinden yok olma
        Destroy(gameObject, 15f);
    }
    
    void Update()
    {
        // Yukarı aşağı yüzme animasyonu
        float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Yavaşça dönme
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Yanıp sönme efekti
        if (spriteRenderer != null)
        {
            // Son 3 saniye yanıp sön (Destroy çağrılmadan önce)
            float timeRemaining = 15f - (Time.time - (timeOffset + startPosition.y)); // tahminî kalan süre
            if (timeRemaining < 3f)
            {
                float alpha = Mathf.PingPong(Time.time * 5f, 1f);
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Oyuncu veya Zeplin mi kontrol et
        if (other.CompareTag("Player") || other.CompareTag("Zeplin"))
        {
            ApplyPowerUp(other.gameObject);
            
            // Toplama efekti göster
            ShowCollectEffect();
            
            // PowerUp'ı yok et
            Destroy(gameObject);
        }
    }
    
    private void ApplyPowerUp(GameObject collector)
    {
        // PlayerData bulunamadıysa tekrar bul
        if (playerData == null)
        {
            playerData = FindObjectOfType<PlayerData>();
            if (playerData == null)
            {
                Debug.LogError("PlayerData bulunamadı! Power-up etki edemeyecek.");
                return;
            }
        }
        
        // PowerUp türüne göre efekti uygula
        switch (powerUpType)
        {
            case PowerUpType.Health:
                if (collector.CompareTag("Player"))
                {
                    // Ana gemi sağlığını artır
                    playerData.anaGemiSaglik = Mathf.Min(
                        playerData.anaGemiSaglik + healthBonus, 
                        100 + (playerData.anaGemiSaglikLevel * 10)
                    );
                    Debug.Log($"Sağlık PowerUp! +{healthBonus} can. Yeni sağlık: {playerData.anaGemiSaglik}");
                }
                else if (collector.CompareTag("Zeplin"))
                {
                    // Zeplin sağlığını artır
                    playerData.zeplinSaglik = Mathf.Min(
                        playerData.zeplinSaglik + healthBonus * 5, 
                        1000 + (playerData.zeplinSaglikLevel * 100)
                    );
                    Debug.Log($"Sağlık PowerUp! +{healthBonus * 5} zeplin sağlığı. Yeni sağlık: {playerData.zeplinSaglik}");
                }
                break;
                
            case PowerUpType.Damage:
                // Geçici hasar artışı
                if (collector.CompareTag("Player"))
                {
                    playerData.anaGemiMinigunDamage += damageBonus;
                    playerData.anaGemiRoketDamage += damageBonus * 2;
                    Debug.Log($"Hasar PowerUp! Minigun +{damageBonus}, Roket +{damageBonus * 2} hasar arttı!");
                }
                else if (collector.CompareTag("Zeplin"))
                {
                    playerData.zeplinMinigunDamage += damageBonus;
                    playerData.zeplinRoketDamage += damageBonus * 2;
                    Debug.Log($"Hasar PowerUp! Zeplin Minigun +{damageBonus}, Zeplin Roket +{damageBonus * 2} hasar arttı!");
                }
                break;
                
            case PowerUpType.Speed:
                // Hız artışı
                if (collector.CompareTag("Player"))
                {
                    Player player = collector.GetComponent<Player>();
                    if (player != null)
                    {
                        player.moveSpeed += speedBonus;
                        Debug.Log($"Hız PowerUp! Oyuncu hızı {speedBonus} arttı!");
                    }
                }
                else if (collector.CompareTag("Zeplin"))
                {
                    Zeplin zeplin = collector.GetComponent<Zeplin>();
                    if (zeplin != null)
                    {
                        zeplin.moveSpeed += speedBonus;
                        Debug.Log($"Hız PowerUp! Zeplin hızı {speedBonus} arttı!");
                    }
                }
                break;
                
            case PowerUpType.Shield:
                // Geçici kalkan
                // Burada kalkan mantığını implemente edebilirsin
                Debug.Log($"Kalkan PowerUp! {shieldDuration} saniyeliğine kalkan aktif!");
                break;
                
            case PowerUpType.MetalPara:
                // Metal para ekle
                playerData.metalPara += metalParaBonus;
                Debug.Log($"Para PowerUp! +{metalParaBonus} metal para eklendi! Toplam: {playerData.metalPara}");
                break;
        }
    }
    
    private void ShowCollectEffect()
    {
        // Toplama efekti
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        // Ses efekti ekleyebilirsiniz
    }
} 