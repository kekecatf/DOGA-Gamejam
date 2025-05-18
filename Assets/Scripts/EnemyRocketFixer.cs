using UnityEngine;
using System.Collections;

// EnemyRocketFixer - Enemy sınıfındaki FireRocket metodunu düzeltmek için
// Bu sınıfı bir GameObject'e ekleyerek çalıştırabilirsiniz
public class EnemyRocketFixer : MonoBehaviour
{
    // Enable/disable flags
    public bool enablePatching = true;
    public bool logDebugInfo = true;
    
    // Uygulama durumu
    private bool patchApplied = false;
    
    private void Start()
    {
        if (enablePatching)
        {
            StartCoroutine(PatchEnemyRockets());
        }
    }
    
    // Gecikmeli olarak tüm Enemy objelerini tara ve düzelt
    private IEnumerator PatchEnemyRockets()
    {
        // Bir kare bekle
        yield return null;
        
        // Tüm düşmanları bul
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        
        if (logDebugInfo)
        {
            Debug.Log($"EnemyRocketFixer: {allEnemies.Length} adet düşman bulundu. Patch işlemi başlatılıyor...");
        }
        
        // Rocket tipi düşmanları düzelt
        int patchCount = 0;
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.enemyType == EnemyType.Rocket)
            {
                PatchRocketEnemy(enemy);
                patchCount++;
            }
        }
        
        patchApplied = true;
        
        if (logDebugInfo)
        {
            Debug.Log($"EnemyRocketFixer: Toplam {patchCount} adet Rocket düşmanı düzeltildi.");
        }
        
        // Düzenli olarak yeni düşmanları kontrol et
        StartCoroutine(PeriodicCheck());
    }
    
    // Belirli aralıklarla yeni düşmanları kontrol et
    private IEnumerator PeriodicCheck()
    {
        while (true)
        {
            // 5 saniyede bir kontrol et
            yield return new WaitForSeconds(5f);
            
            // Tüm düşmanları bul
            Enemy[] allEnemies = FindObjectsOfType<Enemy>();
            
            // Rocket tipi düşmanları düzelt
            int patchCount = 0;
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy.enemyType == EnemyType.Rocket && !enemy.gameObject.GetComponent<PatchedEnemyTag>())
                {
                    PatchRocketEnemy(enemy);
                    patchCount++;
                }
            }
            
            if (logDebugInfo && patchCount > 0)
            {
                Debug.Log($"EnemyRocketFixer: Periyodik kontrolde {patchCount} adet yeni Rocket düşmanı düzeltildi.");
            }
        }
    }
    
    // Belirli bir düşmanı düzelt
    private void PatchRocketEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        
        // RocketFixHelper için prefab referansını kontrol et
        if (enemy.rocketPrefab == null)
        {
            // Rocket prefabını ara ve ata
            GameObject rocketPrefab = FindRocketPrefab();
            if (rocketPrefab != null)
            {
                enemy.rocketPrefab = rocketPrefab;
                if (logDebugInfo)
                {
                    Debug.Log($"EnemyRocketFixer: {enemy.name} düşmanına RocketPrefab atandı: {rocketPrefab.name}");
                }
            }
            else
            {
                Debug.LogWarning($"EnemyRocketFixer: {enemy.name} düşmanı için RocketPrefab bulunamadı!");
            }
        }
        
        // Patch etiketi ekle (tekrar patch etmemek için)
        if (!enemy.gameObject.GetComponent<PatchedEnemyTag>())
        {
            enemy.gameObject.AddComponent<PatchedEnemyTag>();
            if (logDebugInfo)
            {
                Debug.Log($"EnemyRocketFixer: {enemy.name} düşmanına PatchedEnemyTag eklendi.");
            }
        }
    }
    
    // Rocket prefabını bul
    private GameObject FindRocketPrefab()
    {
        // Önce Resources klasöründen yüklemeyi dene
        GameObject rocketPrefab = Resources.Load<GameObject>("Prefabs/RocketPrefab");
        
        if (rocketPrefab == null)
        {
            rocketPrefab = Resources.Load<GameObject>("RocketPrefab");
        }
        
        // Bulunamazsa, sahnede prefabları ara
        if (rocketPrefab == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("RocketPrefab"))
                {
                    rocketPrefab = obj;
                    break;
                }
            }
        }
        
        return rocketPrefab;
    }
}

// Patch edilmiş düşmanları işaretlemek için (tekrar düzeltmemek için)
public class PatchedEnemyTag : MonoBehaviour
{
    // Bu sınıf sadece bir işaretleyici olarak kullanılır
} 