using System.Collections;
using UnityEngine;

public class PlayerRespawner : MonoBehaviour
{
    // Oyuncu prefabı (Inspector'dan atanmalı)
    public GameObject playerPrefab;
    
    // Oyuncunun yeniden canlanacağı pozisyon
    public Transform respawnPoint;
    
    // Oyuncuyu canlandırırken efekt gösterilsin mi?
    public bool showRespawnEffect = true;
    
    // Canlanma efekti (optional)
    public GameObject respawnEffectPrefab;
    
    // Canlandıktan sonra verilecek can miktarı
    public int respawnHealthAmount = 50;
    
    void Start()
    {
        Debug.Log("PlayerRespawner: Start metodu çağrıldı");
        
        // Singleton PlayerData'yı bul
        PlayerData playerData = FindObjectOfType<PlayerData>();
        
        // PlayerData bulunduysa durumunu logla
        if (playerData != null)
        {
            Debug.Log($"PlayerRespawner: PlayerData bulundu. isPlayerRespawned: {playerData.isPlayerRespawned}");
        }
        else
        {
            Debug.LogError("PlayerRespawner: PlayerData bulunamadı!");
        }
        
        // Oyuncu prefabını kontrol et
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerRespawner: playerPrefab atanmamış! Resources klasöründe Player prefabını kontrol edin.");
            // Otomatik olarak yüklemeyi dene
            playerPrefab = Resources.Load<GameObject>("Player");
            if (playerPrefab != null)
            {
                Debug.Log("PlayerRespawner: Player prefabı otomatik olarak yüklendi.");
            }
        }
        else
        {
            Debug.Log("PlayerRespawner: playerPrefab hazır: " + playerPrefab.name);
        }
        
        // Eğer oyuncu canlandıysa ve sahnede player yoksa
        if (playerData != null && playerData.isPlayerRespawned && FindObjectOfType<Player>() == null)
        {
            Debug.Log("PlayerRespawner: Oyuncu MiniGame'den sonra canlandırılacak.");
            StartCoroutine(RespawnPlayerAfterDelay(1.0f));
        }
        else if (playerData != null && !playerData.isPlayerRespawned)
        {
            Debug.Log("PlayerRespawner: Oyuncu henüz canlanmadı, respawn yapılmayacak.");
        }
        else if (FindObjectOfType<Player>() != null)
        {
            Debug.Log("PlayerRespawner: Sahnede zaten bir Player var, respawn yapılmayacak.");
        }
    }
    
    // Kısa bir gecikmeden sonra oyuncuyu canlandır
    private IEnumerator RespawnPlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RespawnPlayer();
    }
    
    // Oyuncuyu canlandır
    public void RespawnPlayer()
    {
        Debug.Log("PlayerRespawner: RespawnPlayer metodu çağrıldı");
        
        // Player prefabını kontrol et
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerRespawner: playerPrefab atanmamış! Oyuncu canlandırılamıyor.");
            return;
        }
        
        // Canlanma noktasını kontrol et, yoksa kendi pozisyonunu kullan
        Vector3 spawnPosition = respawnPoint != null ? respawnPoint.position : transform.position;
        Debug.Log($"PlayerRespawner: Canlanma pozisyonu: {spawnPosition}");
        
        // Efekt göster
        if (showRespawnEffect && respawnEffectPrefab != null)
        {
            Instantiate(respawnEffectPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("PlayerRespawner: Canlanma efekti gösterildi");
        }
        
        // Oyuncuyu oluştur
        GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"PlayerRespawner: Yeni oyuncu oluşturuldu: {newPlayer.name}");
        
        // Player bileşenini bul ve canını ayarla
        Player player = newPlayer.GetComponent<Player>();
        if (player != null)
        {
            // Player.isDead değişkenini sıfırla
            Player.isDead = false;
            
            // İlgili PlayerData referansını ayarla
            PlayerData playerData = FindObjectOfType<PlayerData>();
            if (playerData != null)
            {
                // Oyuncuya yeni can ver
                playerData.anaGemiSaglik = respawnHealthAmount;
                
                // Player.Update() metodunda kullanılacak referanslar güncelleniyor
                Debug.Log($"Oyuncu canlandırıldı! Yeni can: {respawnHealthAmount}");
            }
            else
            {
                Debug.LogError("PlayerRespawner: PlayerData bulunamadı, can değeri ayarlanamadı!");
            }
        }
        else
        {
            Debug.LogError("PlayerRespawner: Oluşturulan objede Player bileşeni bulunamadı!");
        }
        
        Debug.Log("Oyuncu başarıyla canlandırıldı!");
    }
} 