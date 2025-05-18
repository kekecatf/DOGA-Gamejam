using UnityEngine;

// RocketFixHelper - Roket sorunlarını çözmek için yardımcı sınıf
public static class RocketFixHelper
{
    // Roket prefabını Runtime'da düzeltme
    public static GameObject FixRocketPrefab(GameObject rocketPrefab)
    {
        if (rocketPrefab == null) return null;
        
        Debug.Log("RocketFixHelper: Roket prefabı düzeltiliyor: " + rocketPrefab.name);
        
        // RocketPrefab'ı klonla ve düzelt
        GameObject fixedRocket = Object.Instantiate(rocketPrefab);
        fixedRocket.name = "FixedRocketPrefab";
        
        // RocketProjectile bileşenini ekle
        RocketProjectile rocketComp = fixedRocket.GetComponent<RocketProjectile>();
        if (rocketComp == null)
        {
            rocketComp = fixedRocket.AddComponent<RocketProjectile>();
            rocketComp.speed = 8f;
            rocketComp.turnSpeed = 3f;
            rocketComp.lifetime = 5f;
            rocketComp.activationDelay = 0.5f;
            rocketComp.explosionRadius = 3f;
            rocketComp.damage = 30;
            Debug.Log("RocketFixHelper: RocketProjectile bileşeni eklendi");
        }
        
        // Rigidbody kontrolü
        Rigidbody2D rb = fixedRocket.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = fixedRocket.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Debug.Log("RocketFixHelper: Rigidbody2D eklendi");
        }
        
        // Collider kontrolü
        Collider2D collider = fixedRocket.GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = fixedRocket.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = false; // Fiziksel çarpışma için
            Debug.Log("RocketFixHelper: BoxCollider2D eklendi");
        }
        
        // Düzeltilmiş prefab için bilgi yazdır
        Debug.Log($"RocketFixHelper: Düzeltilmiş roket bileşenleri: " +
                 $"RocketProjectile: {fixedRocket.GetComponent<RocketProjectile>() != null}, " +
                 $"Rigidbody2D: {fixedRocket.GetComponent<Rigidbody2D>() != null}, " +
                 $"Collider: {fixedRocket.GetComponent<Collider2D>() != null}");
        
        // Düzeltilmiş prefabı ekranın dışına taşı ve gizle
        fixedRocket.transform.position = new Vector3(-1000, -1000, -1000);
        fixedRocket.SetActive(false);
        
        return fixedRocket;
    }
    
    // Düşman roketini oluşturma ve düzeltme
    public static GameObject CreateEnemyRocket(GameObject rocketPrefab, Vector3 position, Quaternion rotation)
    {
        if (rocketPrefab == null)
        {
            Debug.LogError("RocketFixHelper: rocketPrefab null! Roket oluşturulamadı.");
            return null;
        }
        
        // Rocket prefabını klonla
        GameObject rocket = Object.Instantiate(rocketPrefab, position, rotation);
        if (rocket == null)
        {
            Debug.LogError("RocketFixHelper: Rocket instantiate başarısız!");
            return null;
        }
        
        Debug.Log("RocketFixHelper: Roket oluşturuldu: " + rocket.name);
        
        // "Rocket" tag'i ata
        rocket.tag = "Rocket";
        
        // RocketProjectile bileşenini garantile
        RocketProjectile rocketComp = rocket.GetComponent<RocketProjectile>();
        if (rocketComp == null)
        {
            rocketComp = rocket.AddComponent<RocketProjectile>();
            rocketComp.speed = 8f;
            rocketComp.turnSpeed = 3f;
            rocketComp.lifetime = 5f;
            rocketComp.activationDelay = 0.5f;
            rocketComp.explosionRadius = 3f;
            rocketComp.damage = 30;
            Debug.Log("RocketFixHelper: Rocket'a RocketProjectile bileşeni eklendi");
        }
        
        // Düşman roketi olduğunu belirt
        rocketComp.isEnemyRocket = true;
        
        // Rigidbody kontrolü
        Rigidbody2D rb = rocket.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = rocket.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Debug.Log("RocketFixHelper: Rocket'a Rigidbody2D eklendi");
        }
        
        // Collider kontrolü
        Collider2D collider = rocket.GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = rocket.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = false; // Fiziksel çarpışma için
            Debug.Log("RocketFixHelper: Rocket'a BoxCollider2D eklendi");
        }
        else if (collider.isTrigger)
        {
            // Fiziksel çarpışma için trigger'ı kapat
            collider.isTrigger = false;
            Debug.Log("RocketFixHelper: Rocket Collider'da isTrigger kapatıldı");
        }
        
        // Rocket bileşenlerini kontrol et ve debug bilgisi yazdır
        Debug.Log($"RocketFixHelper: Roket oluşturuldu - Bileşenler: " +
                 $"RocketProjectile: {rocket.GetComponent<RocketProjectile>() != null}, " +
                 $"Rigidbody2D: {rb != null}, " +
                 $"Collider: {collider != null}, " +
                 $"ColliderType: {(collider != null ? collider.GetType().Name : "Yok")}, " +
                 $"IsTrigger: {(collider != null ? collider.isTrigger.ToString() : "Yok")}");
        
        return rocket;
    }
} 