using UnityEngine;

public class ItemDropManager : MonoBehaviour
{
    // Singleton instance
    public static ItemDropManager Instance { get; private set; }
    
    [Header("Item Drop Settings")]
    public GameObject itemPrefab;             // The item prefab to spawn
    [Range(0, 100)]
    public float dropChance = 20f;            // 20% chance by default
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Called when an enemy dies to potentially drop an item
    public void TryDropItem(Vector3 position)
    {
        // Check if we have a valid prefab
        if (itemPrefab == null)
        {
            Debug.LogWarning("ItemDropManager: No item prefab assigned!");
            return;
        }
        
        // Roll for chance to drop (20% by default)
        float roll = Random.Range(0f, 100f);
        if (roll <= dropChance)
        {
            // Create the item at the position where the enemy died
            Instantiate(itemPrefab, position, Quaternion.identity);
            Debug.Log($"Item dropped at {position}! (Roll: {roll}, Needed: {dropChance} or less)");
        }
        else
        {
            Debug.Log($"No item dropped. (Roll: {roll}, Needed: {dropChance} or less)");
        }
    }
} 