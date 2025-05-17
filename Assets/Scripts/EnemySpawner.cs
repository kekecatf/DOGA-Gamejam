using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Düşman Prefabi")]
    public GameObject enemyPrefab;
    
    [Header("Spawn Ayarları")]
    public float spawnRate = 2f;           // Kaç saniyede bir düşman oluşturulacak
    public int maxEnemiesAlive = 10;       // Aynı anda maksimum düşman sayısı
    public float minSpawnDistance = 10f;   // Spawn noktasının merkeze minimum uzaklığı
    public float maxSpawnDistance = 15f;   // Spawn noktasının merkeze maksimum uzaklığı
    
    [Header("Spawn Alanı")]
    public Vector2 spawnAreaCenter = Vector2.zero;  // Spawn alanının merkezi (varsayılan: 0,0)
    public bool visualizeSpawnArea = true;          // Editörde spawn alanını görselleştir
    
    private float nextSpawnTime;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    private void Start()
    {
        nextSpawnTime = Time.time + spawnRate;
    }
    
    private void Update()
    {
        // Ölü düşmanları listeden temizle
        CleanupDestroyedEnemies();
        
        // Eğer yeni düşman oluşturma zamanı geldiyse ve aktif düşman sayısı limiti aşmıyorsa
        if (Time.time > nextSpawnTime && activeEnemies.Count < maxEnemiesAlive)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }
    
    private void SpawnEnemy()
    {
        // Rastgele bir açı seç (0-360 derece)
        float randomAngle = Random.Range(0f, 360f);
        
        // Rastgele bir mesafe seç (min-max arasında)
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        // Polar koordinatları Kartezyen koordinatlara dönüştür
        float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomDistance;
        float y = Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomDistance;
        
        // Spawn pozisyonunu belirle
        Vector2 spawnPosition = spawnAreaCenter + new Vector2(x, y);
        
        // Düşmanı oluştur
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Düşmanın hedefini ayarla
        Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.targetPosition = spawnAreaCenter;
        }
        
        // Aktif düşmanlar listesine ekle
        activeEnemies.Add(newEnemy);
    }
    
    private void CleanupDestroyedEnemies()
    {
        // Yok edilen düşmanları listeden temizle
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (visualizeSpawnArea)
        {
            // Spawn alanını görselleştir (Unity editöründe görülebilir)
            Gizmos.color = Color.yellow;
            
            // Minimum spawn alanı çemberi
            DrawCircle(spawnAreaCenter, minSpawnDistance, 32);
            
            // Maksimum spawn alanı çemberi
            Gizmos.color = Color.red;
            DrawCircle(spawnAreaCenter, maxSpawnDistance, 32);
            
            // Merkez nokta
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnAreaCenter, 0.3f);
        }
    }
    
    private void DrawCircle(Vector2 center, float radius, int segments)
    {
        // Çember çizme yardımcı fonksiyonu
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector2 point1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector2 point2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;
            
            Gizmos.DrawLine(point1, point2);
        }
    }
} 