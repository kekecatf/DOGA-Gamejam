using UnityEngine;

// RocketManagerBridge - RocketManager ile doğrudan tip ilişkisi 
// olmadan haberleşmek için aracı sınıf
public class RocketManagerBridge : MonoBehaviour
{
    public GameObject rocketPrefab;
    
    private void Awake()
    {
        // RocketManager ilk başladığında rocketPrefab'ı atar
        // Bu sayede Enemy sınıfı bunu kullanabilir
    }
    
    // Rocket prefabını döndürür
    public GameObject GetRocketPrefab()
    {
        return rocketPrefab;
    }
    
    // Düşmana rocket prefabını atar
    public void AssignRocketPrefabToEnemy(GameObject enemy)
    {
        if (enemy == null || rocketPrefab == null) return;
        
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null && enemyComponent.enemyType == EnemyType.Rocket)
        {
            enemyComponent.rocketPrefab = rocketPrefab;
            Debug.Log("RocketManagerBridge: " + enemy.name + " düşmanına RocketPrefab atandı: " + rocketPrefab.name);
        }
    }
} 