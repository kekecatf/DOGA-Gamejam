using UnityEngine;

// This script ensures Zeplin health is always 1000 by forcing it in both
// PlayerPrefs and in the PlayerData object. This runs before any other scripts.
public class ZeplinHealthFixer : MonoBehaviour 
{
    // Use Awake to execute this before other scripts
    private void Awake()
    {
        Debug.Log("ZeplinHealthFixer: Forcing zeplin health to 1000");
        
        // Force zeplin health to 1000 in PlayerPrefs
        if (PlayerPrefs.HasKey("zeplinSaglik"))
        {
            int currentHealth = PlayerPrefs.GetInt("zeplinSaglik");
            Debug.Log($"ZeplinHealthFixer: Current zeplinSaglik in PlayerPrefs is {currentHealth}, setting to 1000");
            PlayerPrefs.DeleteKey("zeplinSaglik");
        }
        
        PlayerPrefs.SetInt("zeplinSaglik", 1000);
        PlayerPrefs.Save();
        
        // Try to find PlayerData and force its value too
        PlayerData playerData = FindObjectOfType<PlayerData>();
        if (playerData != null)
        {
            playerData.zeplinSaglik = 1000;
            Debug.Log("ZeplinHealthFixer: Directly set PlayerData.zeplinSaglik to 1000");
        }
        else
        {
            Debug.Log("ZeplinHealthFixer: PlayerData not found yet, will be set from PlayerPrefs");
        }
        
        // Self-destruct after running
        Destroy(this);
    }
} 