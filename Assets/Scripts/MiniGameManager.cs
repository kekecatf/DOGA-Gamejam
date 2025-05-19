using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    // Oyunun tamamlanması için gerekli puan
    public int pointsToWin = 100;
    
    // Mevcut puan
    private int currentPoints = 0;
    
    // Singleton yapısı
    public static MiniGameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Debug.Log("MiniGame başladı! Kazanmak için " + pointsToWin + " puan topla.");
        
        // MiniOyun başlangıcında PlayerData'yı kontrol et
        PlayerData playerData = FindObjectOfType<PlayerData>();
        if (playerData != null)
        {
            // MiniOyun sahnesi yüklendiğinde henüz canlanmamış durumda olmalı
            // Bu değer, oyuncu mini oyunu kazanırsa WinMiniGame metodunda true olacak
            Debug.Log("MiniGameManager: PlayerData bulundu. isPlayerRespawned = " + playerData.isPlayerRespawned);
        }
        else
        {
            Debug.LogWarning("MiniGameManager: PlayerData bulunamadı! Yeni bir tane oluşturuluyor.");
            GameObject playerDataObj = new GameObject("PlayerData");
            playerData = playerDataObj.AddComponent<PlayerData>();
            DontDestroyOnLoad(playerDataObj);
            Debug.Log("MiniGameManager: Yeni PlayerData oluşturuldu. isPlayerRespawned = " + playerData.isPlayerRespawned);
        }
    }
    
    // Puan eklemek için kullanılacak method (butonlar veya diğer oyun mekanikleri tarafından çağrılabilir)
    public void AddPoints(int points)
    {
        currentPoints += points;
        Debug.Log("Puan kazanıldı! Mevcut puan: " + currentPoints + " / " + pointsToWin);
        
        // Kazanma şartı sağlandı mı kontrol et
        if (currentPoints >= pointsToWin)
        {
            WinMiniGame();
        }
    }
    
    // Mini oyunu kazanma durumu
    public void WinMiniGame()
    {
        Debug.Log("Mini oyun kazanıldı! Ana oyuna geri dönülüyor...");
        
        // PlayerData'yı bul ve oyuncunun canlandığını işaretle
        PlayerData playerData = FindObjectOfType<PlayerData>();
        if (playerData != null)
        {
            // isPlayerRespawned değerini true yap
            playerData.isPlayerRespawned = true;
            playerData.SaveValues();
            Debug.Log("PlayerData güncellendi, oyuncu canlandı olarak işaretlendi. isPlayerRespawned: " + playerData.isPlayerRespawned);
        }
        else
        {
            // PlayerData bulunamadı, yeni oluşturalım
            GameObject playerDataObj = new GameObject("PlayerData");
            playerData = playerDataObj.AddComponent<PlayerData>();
            playerData.isPlayerRespawned = true;
            DontDestroyOnLoad(playerDataObj);
            playerData.SaveValues();
            Debug.Log("Yeni PlayerData oluşturuldu ve oyuncu canlandı olarak işaretlendi. isPlayerRespawned: " + playerData.isPlayerRespawned);
        }
        
        // PlayerData'nın düzgün ayarlandığını bir kez daha kontrol et
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.isPlayerRespawned = true;
            PlayerData.Instance.SaveValues();
            Debug.Log("PlayerData.Instance güncellendi: " + PlayerData.Instance.isPlayerRespawned);
        }
        
        // Geçiş yapmadan önce tüm referansları temizle (GC'ye yardımcı olması için)
        Debug.Log("Ana oyun sahnesine (GameScene) geçiş için hazırlanıyor...");
        
        // Geçiş yapmadan önce kısa bir bekleme ekle
        StartCoroutine(DelayedSceneLoad("GameScene", 1.0f));
    }
    
    // Gecikmeli sahne yükleme için yardımcı coroutine
    private IEnumerator DelayedSceneLoad(string sceneName, float delay)
    {
        Debug.Log($"DelayedSceneLoad: {delay} saniye sonra {sceneName} sahnesine geçilecek.");
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"{sceneName} sahnesine geçiş yapılıyor...");
        SceneManager.LoadScene(sceneName);
    }
    
    // Mini oyunu kaybetme durumu
    public void LoseMiniGame()
    {
        Debug.Log("Mini oyun kaybedildi! Zeplin kontrolüne geçiliyor...");
        
        // Zeplin kontrolü için ana oyun sahnesine geri dön
        // ve ikinci ölümü simüle et
        PlayerData playerData = FindObjectOfType<PlayerData>();
        if (playerData != null)
        {
            playerData.isPlayerRespawned = true;
            playerData.SaveValues();
            Debug.Log("PlayerData güncellendi: isPlayerRespawned = " + playerData.isPlayerRespawned);
        }
        else
        {
            // PlayerData bulunamadı, yeni oluşturalım
            GameObject playerDataObj = new GameObject("PlayerData");
            playerData = playerDataObj.AddComponent<PlayerData>();
            playerData.isPlayerRespawned = true;
            DontDestroyOnLoad(playerDataObj);
            playerData.SaveValues();
            Debug.Log("Yeni PlayerData oluşturuldu (LoseMiniGame): isPlayerRespawned = true");
        }
        
        // Geçiş yapmadan önce kısa bir bekleme ekle
        Debug.Log("Ana sahneye dönüş için hazırlanıyor. Zeplin kontrolü aktif edilecek.");
        StartCoroutine(DelayedSceneLoad("GameScene", 1.0f));
    }
    
    // Test için: UI butonlarına bağlanabilecek metodlar
    public void TestAddPoints()
    {
        AddPoints(10);
    }
    
    public void TestWinGame()
    {
        WinMiniGame();
    }
    
    public void TestLoseGame()
    {
        LoseMiniGame();
    }
} 