using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    // The item's properties
    [Header("Item Properties")]
    public float lifetime = 10f;          // How long the item stays in the world before disappearing
    public float floatAmplitude = 0.2f;   // How much the item floats up and down
    public float floatFrequency = 1.0f;   // How quickly the item floats up and down
    
    // Internal variables
    private Vector3 startPos;
    private float timeAlive = 0f;
    
    void Start()
    {
        // Store the initial position
        startPos = transform.position;
        
        // Destroy this item after its lifetime
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Increment the timer
        timeAlive += Time.deltaTime;
        
        // Make the item float up and down
        float newY = startPos.y + Mathf.Sin(timeAlive * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
        
        // Optional: Make the item rotate slowly
        transform.Rotate(0, 0, 30 * Time.deltaTime);
    }
    
    // When the player collects the item
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Zeplin"))
        {
            // Give the player some benefit (health, ammo, etc.)
            GrantItemEffect(other.gameObject);
            
            // Play a collection sound or effect
            PlayCollectionEffect();
            
            // Destroy the item
            Destroy(gameObject);
        }
    }
    
    // Apply the item effect to the player
    private void GrantItemEffect(GameObject collector)
    {
        // Find the PlayerData
        PlayerData playerData = FindObjectOfType<PlayerData>();
        if (playerData != null)
        {
            // Example: Give the player some metal money
            playerData.metalPara += Random.Range(10, 30);
            
            Debug.Log($"Item collected by {collector.name}! +{Random.Range(10, 30)} Metal Para");
        }
    }
    
    // Play a collection effect
    private void PlayCollectionEffect()
    {
        // You could instantiate a particle effect or play a sound here
        Debug.Log("Item collected!");
    }
} 