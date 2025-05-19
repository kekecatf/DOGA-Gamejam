using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class PowerUpCreator : MonoBehaviour
{
    // Bu script sadece oyun başlangıcında prefabın yüklü olmasını sağlar
    // ve ardından kendini devre dışı bırakır.
    
    [Header("PowerUp Prefab Settings")]
    public GameObject powerUpPrefab;
    public Sprite[] powerUpSprites; // Farklı powerup tipleri için
    
    private void Awake()
    {
        // Oyun başlangıcında prefabın yüklü olduğundan emin ol
        CheckAndCreatePrefab();
        
        // Bu scripti devre dışı bırak
        this.enabled = false;
    }
    
    private void CheckAndCreatePrefab()
    {
        // Resources klasöründe ItemPrefab'ı ara
        GameObject itemPrefab = Resources.Load<GameObject>("ItemPrefab");
        
        if (itemPrefab == null)
        {
            Debug.LogWarning("Item prefab bulunamadı! Resources klasöründe ItemPrefab oluşturun.");
            Debug.Log("PowerUp sistemi için gerekli prefab bulunamadı. Düşman öldürme drop sistemini kullanabilmek için bir prefab oluşturmanız gerekiyor.");
            
            // Kullanıcıya prefab oluşturma talimatları göster
#if UNITY_EDITOR
            Debug.Log("UNITY EDITOR: PowerUp prefabı oluşturmak için:");
            Debug.Log("1. Hierarchy'de sağ tıklayıp 'Create Empty' seçin");
            Debug.Log("2. Yeni objeye 'PowerUpItem' ismini verin");
            Debug.Log("3. Bu objeye SpriteRenderer, CircleCollider2D (IsTrigger=true) ve ItemPowerUp script'i ekleyin");
            Debug.Log("4. Bu objeyi Project/Resources klasörüne 'ItemPrefab' adıyla sürükleyip prefab oluşturun");
#endif
        }
        else
        {
            Debug.Log("PowerUp prefabı Resources klasöründe bulundu. Sistem hazır.");
        }
    }
    
#if UNITY_EDITOR
    [ContextMenu("Create ItemPrefab")]
    public void CreateItemPrefab()
    {
        // Resources klasörünün var olduğunu kontrol et
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        // Yeni bir GameObject oluştur
        GameObject powerUpObj = new GameObject("PowerUpItem");
        
        // SpriteRenderer ekle
        SpriteRenderer spriteRenderer = powerUpObj.AddComponent<SpriteRenderer>();
        
        // Eğer sprite mevcutsa ata
        if (powerUpSprites != null && powerUpSprites.Length > 0)
        {
            spriteRenderer.sprite = powerUpSprites[0]; 
        }
        else
        {
            Debug.LogWarning("PowerUp için sprite atanmadı!");
        }
        
        // CircleCollider2D ekle
        CircleCollider2D collider = powerUpObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        // ItemPowerUp scriptini ekle
        powerUpObj.AddComponent<ItemPowerUp>();
        
        // Prefab olarak kaydet
        string path = "Assets/Resources/ItemPrefab.prefab";
        
        // Eğer dosya varsa sil
        if (File.Exists(path))
        {
            AssetDatabase.DeleteAsset(path);
        }
        
        // Prefab oluştur ve kaydet
        PrefabUtility.SaveAsPrefabAsset(powerUpObj, path);
        
        // Sahne objesini sil
        DestroyImmediate(powerUpObj);
        
        Debug.Log("'ItemPrefab' Resources klasörüne oluşturuldu! Bu prefabı düzenleyebilirsiniz.");
        
        // Asset veritabanını güncelle
        AssetDatabase.Refresh();
    }
#endif
} 