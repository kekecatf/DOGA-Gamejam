using UnityEngine;
using System.Collections;

public class Boss : Enemy
{
    [Header("Boss Özel Ayarları")]
    public float kamikazeSpawnInterval = 5f;    // Kamikaze düşman spawn aralığı
    public float minigunFireRate = 0.5f;        // Minigun ateş hızı
    public int maxHealth = 500;                 // Boss canı
    public GameObject kamikazePrefab;           // Spawn edilecek kamikaze prefabı
    
    private float nextKamikazeSpawnTime;
    private float nextFireTime = 0f;            // Bir sonraki ateş zamanı
    
    protected override void Start()
    {
        // GameObject'e "Boss" tag'ini ata
        gameObject.tag = "Boss";
        
        // Eğer firePoint atanmamışsa, çocuk objelerde "FirePoint" adlı objeyi ara
        if (firePoint == null)
        {
            // Önce doğrudan çocuklarda ara
            Transform foundFirePoint = transform.Find("FirePoint");
            
            // Bulunamadıysa tüm çocuklarda recursive ara
            if (foundFirePoint == null)
            {
                foundFirePoint = FindFirePointRecursively(transform);
            }
            
            if (foundFirePoint != null)
            {
                firePoint = foundFirePoint;
                Debug.Log("FirePoint objesi otomatik olarak bulundu: " + firePoint.name);
            }
            else
            {
                Debug.LogWarning("FirePoint objesi bulunamadı! Yeni bir tane oluşturuluyor...");
                
                // FirePoint yoksa oluştur
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = new Vector3(0.5f, 0, 0); // Sağa doğru 0.5 birim
                firePoint = firePointObj.transform;
            }
        }
        
        // Enemy sınıfının Start metodunu çağır
        base.Start();
        
        // Boss'un özel ayarlarını yap
        currentHealth = maxHealth;
        nextKamikazeSpawnTime = Time.time + kamikazeSpawnInterval;
        moveSpeed = 2f;                         // Normal düşmanlardan daha yavaş
        fireRate = minigunFireRate;             // Daha hızlı ateş etsin
        scoreValue = 500;                       // Daha fazla puan versin
        enemyType = EnemyType.Minigun;          // Boss minigun tipinde olsun
        
        // Uzaktan saldırı yapması için mesafe değerlerini yüksek ayarla
        minigunAttackRange = 15f;               // Daha uzak mesafeden ateş et (normal düşmanlardan daha uzak)
        minigunSafeDistance = 10f;              // Zeplinden en az 10 birim uzakta dur
        minAttackDistance = 8f;                 // Minimum saldırı mesafesini artır
    }
    
    // Çocuk objelerde recursively (alt alta) FirePoint ara
    private Transform FindFirePointRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "FirePoint")
            {
                return child;
            }
            
            Transform found = FindFirePointRecursively(child);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    protected override void Update()
    {
        // Early check to see if the object is valid
        if (!gameObject || !gameObject.activeInHierarchy)
            return;
            
        // Try-catch to capture any null reference errors
        try
        {
            // Enemy sınıfının Update metodunu çağır
            base.Update();
            
            // Kamikaze düşman spawn etme kontrolü
            if (Time.time >= nextKamikazeSpawnTime)
            {
                SpawnKamikaze();
                nextKamikazeSpawnTime = Time.time + kamikazeSpawnInterval;
            }
            
            // Kendimize özel mesafe kontrolü
            if (targetTransform != null)
            {
                float distanceToTarget = Vector2.Distance(transform.position, targetTransform.position);
                
                // Eğer boss hedefe 10 birimden daha yakınsa, hızlıca uzaklaş
                if (distanceToTarget < minigunSafeDistance)
                {
                    Vector2 direction = ((Vector2)transform.position - (Vector2)targetTransform.position).normalized;
                    transform.position += (Vector3)(direction * moveSpeed * 1.5f * Time.deltaTime);
                }
                
                // Özel ateş etme kontrolü
                if (Time.time >= nextFireTime && 
                    distanceToTarget <= minigunAttackRange && 
                    distanceToTarget >= minAttackDistance)
                {
                    BossFire();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Boss Update hata: {e.Message}");
        }
    }
    
    private void SpawnKamikaze()
    {
        if (kamikazePrefab == null)
            return;
            
        try
        {
            // Boss'un etrafında rastgele bir noktada spawn et
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            Instantiate(kamikazePrefab, spawnPosition, Quaternion.identity);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SpawnKamikaze hata: {e.Message}");
        }
    }
    
    public override void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Hasar efekti
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
        
        // Boss öldü mü kontrol et
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    protected override void Die()
    {
        // Ölüm efektini oynat
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Item düşürme şansı
        if (ItemDropManager.Instance != null)
        {
            ItemDropManager.Instance.TryDropItem(transform.position);
        }
        
        // Puanı ekle
        if (playerData != null)
        {
            playerData.metalPara += scoreValue;
            Debug.Log($"Boss öldürüldü! Para kazanıldı: {scoreValue}");
        }
        
        // Objeyi yok et
        Destroy(gameObject);
    }
    
    // Boss'un özel ateş etme fonksiyonu
    private void BossFire()
    {
        // Null check eklendi
        if (bulletPrefab == null || firePoint == null)
            return;
            
        try
        {
            // Boss daha güçlü ateş etsin
            // Üç mermi ateş et - ortada, sağda ve solda
            for (int i = -1; i <= 1; i++)
            {
                // Mermiyi oluştur
                Quaternion spread = Quaternion.Euler(0, 0, i * 10); // 10 derece açıyla sağa ve sola
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * spread);
                
                // Mermi bileşenini bul ve hasarı ayarla
                Bullet bulletComponent = bullet.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    bulletComponent.damage = minigunBulletDamage * 2; // Normal düşmanın 2 katı hasar
                    bulletComponent.speed *= 1.5f; // Daha hızlı mermi
                    bulletComponent.isEnemyBullet = true;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BossFire hata: {e.Message}");
        }
    }
} 