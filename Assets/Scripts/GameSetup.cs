using UnityEngine;

public class GameSetup : MonoBehaviour
{
    // This script runs once when the game starts to ensure all required systems are set up
    
    void Awake()
    {
        // Check if ItemDropManager exists, if not, create it
        SetupItemDropManager();
        
        // Disable this script after setup is complete
        this.enabled = false;
    }
    
    void SetupItemDropManager()
    {
        // Check if ItemDropManager already exists
        ItemDropManager existingManager = FindObjectOfType<ItemDropManager>();
        
        if (existingManager == null)
        {
            // Create a new GameObject with ItemDropManager
            GameObject managerObject = new GameObject("ItemDropManager");
            ItemDropManager manager = managerObject.AddComponent<ItemDropManager>();
            
            // Load the ItemPrefab from Resources
            GameObject itemPrefab = Resources.Load<GameObject>("ItemPrefab");
            
            if (itemPrefab != null)
            {
                // Set the prefab on the manager
                manager.itemPrefab = itemPrefab;
                Debug.Log("ItemDropManager created and configured successfully.");
            }
            else
            {
                Debug.LogWarning("ItemPrefab could not be loaded from Resources folder.");
            }
            
            // Don't destroy this manager when scenes change
            DontDestroyOnLoad(managerObject);
        }
        else
        {
            Debug.Log("ItemDropManager already exists in the scene.");
        }
    }
} 