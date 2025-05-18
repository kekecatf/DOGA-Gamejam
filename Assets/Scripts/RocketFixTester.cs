using UnityEngine;

public class RocketFixTester : MonoBehaviour
{
    // Rocket prefabı (Inspector'dan atayabilirsiniz)
    public GameObject rocketPrefab;
    
    // Rocket fırlatma aralığı (saniye)
    public float fireInterval = 2f;
    
    // RocketProjectile için parametreler
    public float rocketSpeed = 8f;
    public float rocketTurnSpeed = 3f;
    public float rocketLifetime = 5f;
    public int rocketDamage = 30;
    public bool isEnemyRocket = true;
    
    // Zaman sayacı
    private float nextFireTime;
    
    // Test edilen roketlerin sayısı
    private int rocketCount = 0;
    
    private void Start()
    {
        // Başlangıçta rocket prefabını ara
        if (rocketPrefab == null)
        {
            FindRocketPrefab();
        }
        
        nextFireTime = Time.time + 1f; // İlk roketi 1 saniye sonra fırlat
    }
    
    private void Update()
    {
        // Belirli aralıklarla roket fırlat
        if (Time.time >= nextFireTime)
        {
            FireTestRocket();
            nextFireTime = Time.time + fireInterval;
        }
        
        // 1 tuşuna basılırsa manuel olarak roket fırlat
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FireTestRocket();
        }
    }
    
    // Test roketi fırlatma
    private void FireTestRocket()
    {
        // Eğer prefab yoksa bulmaya çalış
        if (rocketPrefab == null)
        {
            FindRocketPrefab();
            if (rocketPrefab == null)
            {
                Debug.LogError("RocketFixTester: Roket prefabı bulunamadı!");
                return;
            }
        }
        
        // RocketFixHelper ile güvenli şekilde roket oluştur
        GameObject rocket = RocketFixHelper.CreateEnemyRocket(rocketPrefab, transform.position, transform.rotation);
        
        if (rocket == null)
        {
            Debug.LogError("RocketFixTester: Roket oluşturulamadı!");
            return;
        }
        
        rocketCount++;
        Debug.Log($"RocketFixTester: Test roketi #{rocketCount} fırlatıldı!");
        
        // RocketProjectile bileşenini al ve özelleştir
        RocketProjectile rocketComp = rocket.GetComponent<RocketProjectile>();
        if (rocketComp != null)
        {
            rocketComp.speed = rocketSpeed;
            rocketComp.turnSpeed = rocketTurnSpeed;
            rocketComp.lifetime = rocketLifetime;
            rocketComp.damage = rocketDamage;
            rocketComp.isEnemyRocket = isEnemyRocket;
            
            Debug.Log($"RocketFixTester: Roket özelleştirildi - Speed: {rocketSpeed}, TurnSpeed: {rocketTurnSpeed}, " +
                     $"Damage: {rocketDamage}, IsEnemy: {isEnemyRocket}");
        }
        else
        {
            Debug.LogError("RocketFixTester: Oluşturulan rokette RocketProjectile bileşeni bulunamadı!");
        }
    }
    
    // Roket prefabını bulmaya çalış
    private void FindRocketPrefab()
    {
        // Önce Resources klasöründen yüklemeyi dene
        rocketPrefab = Resources.Load<GameObject>("Prefabs/RocketPrefab");
        
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
                    Debug.Log("RocketFixTester: RocketPrefab bulundu: " + obj.name);
                    break;
                }
            }
        }
        
        if (rocketPrefab != null)
        {
            Debug.Log("RocketFixTester: Rocket prefab bulundu: " + rocketPrefab.name);
        }
        else
        {
            Debug.LogWarning("RocketFixTester: Rocket prefab bulunamadı! Manuel olarak atanması gerekiyor.");
        }
    }
    
    // Unity Editor'da bilgi göster
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.right * 2f);
    }
} 