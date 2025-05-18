using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class GameOverUI : MonoBehaviour
{
    // UI elemanları
    public Text killedEnemyText;
    public Text gameTimeText;
    public Button restartButton;
    public Button anaMenuButton; // Ana menüye dönmek için yeni buton
    
    private void Start()
    {
        // GameManager'a erişim ve bilgileri göster
        if (GameManager.Instance != null)
        {
            // Öldürülen düşman sayısını göster
            if (killedEnemyText != null)
            {
                killedEnemyText.text = "Yok Edilen Düşman: " + GameManager.Instance.killedEnemyCount;
            }
            
            // Oyun süresini göster
            if (gameTimeText != null)
            {
                int minutes = Mathf.FloorToInt(GameManager.Instance.gameTime / 60);
                int seconds = Mathf.FloorToInt(GameManager.Instance.gameTime % 60);
                gameTimeText.text = "Toplam Süre: " + minutes.ToString("00") + ":" + seconds.ToString("00");
            }
        }
        else
        {
            Debug.LogWarning("GameManager bulunamadı! Skorlar gösterilemiyor.");
            
            // GameManager yoksa varsayılan değerleri göster
            if (killedEnemyText != null)
            {
                killedEnemyText.text = "Yok Edilen Düşman: 0";
            }
            
            if (gameTimeText != null)
            {
                gameTimeText.text = "Toplam Süre: 00:00";
            }
        }
        
        // Restart butonuna tıklama olayı ekle
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        // Ana Menü butonuna tıklama olayı ekle
        if (anaMenuButton != null)
        {
            anaMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }
    
    // Ana menüye dönmek için çağrılacak metot
    public void ReturnToMainMenu()
    {
        Debug.Log("Ana menüye dönülüyor...");
        
        // Tüm statik değişkenleri sıfırla
        ResetAllStaticVariables();
        
        // Tüm DontDestroyOnLoad objelerini temizle
        DestroyAllPersistentObjects();
        
        // Zaman ölçeğini normale çevir
        Time.timeScale = 1.0f;
        
        // Ana menü sahnesini yükle
        SceneManager.LoadScene("AnaMenu");
    }
    
    // Restart butonuna tıklandığında çağrılacak
    public void RestartGame()
    {
        Debug.Log("Oyun yeniden başlatılıyor... Tüm veriler sıfırlanacak.");
        
        // PlayerPrefs üzerinden isteğe bağlı sıfırlama
        PlayerPrefs.DeleteKey("LastScore");
        PlayerPrefs.DeleteKey("LastTime");
        
        // Tüm statik değişkenleri sıfırla
        ResetAllStaticVariables();
        
        // Tüm DontDestroyOnLoad objelerini temizle
        DestroyAllPersistentObjects();
        
        // Tam yeniden başlatma için
        Time.timeScale = 1.0f; // Oyun duraklatılmış olabilir, zamanı normale çevir
        
        // Ekranı temizleyip oyunu yeniden başlat
        StartCoroutine(CompleteRestart());
    }
    
    // Statik değişkenleri sıfırlayan metot
    private void ResetAllStaticVariables()
    {
        // Player ve düşman ilgili statik değişkenleri sıfırla
        Player.isDead = false;
        
        // Ek statik değişkenleri de burada sıfırla
        // Örnek: Enemy.spawnCount = 0; gibi
        
        Debug.Log("Tüm statik değişkenler sıfırlandı.");
    }
    
    // Tüm kalıcı nesneleri bulup yok et
    private void DestroyAllPersistentObjects()
    {
        // PersistentObject tag'ine sahip nesneleri bul ve yok et
        GameObject[] persistentObjects = GameObject.FindGameObjectsWithTag("PersistentObject");
        foreach (GameObject obj in persistentObjects)
        {
            Destroy(obj);
            Debug.Log($"Kalıcı nesne yok edildi: {obj.name}");
        }
        
        // GameManager - bu GameOverUI'yi başlatan GameManager'ı yok et
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
            Debug.Log("GameManager yok edildi.");
        }
        
        // DontDestroyOnLoad'a sahip olan, tag'i farklı olan nesneleri de temizle
        DestroySpecificManagers("PlayerData");
        // RocketManager kaldırıldı - düşmanlar artık roket kullanmıyor
        DestroySpecificManagers("AudioManager");
        DestroySpecificManagers("GameInitializer");
        
        // Player nesnesini ve diğer kritik nesneleri her ihtimale karşı temizle
        DestroyGameObjectsWithTag("Player");
        DestroyGameObjectsWithTag("Zeplin");
        DestroyGameObjectsWithTag("Enemy");
        DestroyGameObjectsWithTag("Rocket");
        
        Debug.Log("Tüm DontDestroyOnLoad objeleri temizlendi.");
    }
    
    // Belirli bir tipteki yöneticileri bul ve yok et
    private void DestroySpecificManagers(string managerName)
    {
        var managers = Object.FindObjectsOfType<MonoBehaviour>().Where(
            mono => mono.GetType().Name == managerName || 
                   mono.name.Contains(managerName) || 
                   mono.gameObject.name.Contains(managerName)
        );
        
        foreach (var manager in managers)
        {
            Debug.Log($"{managerName} sınıfı/nesnesi yok edildi: {manager.gameObject.name}");
            Destroy(manager.gameObject);
        }
    }
    
    // Belirli bir tag'e sahip tüm objeleri yok et
    private void DestroyGameObjectsWithTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return;
        
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            Debug.Log($"{tag} tag'li nesne yok edildi: {obj.name}");
            Destroy(obj);
        }
    }
    
    // Yeniden başlatma işlemini biraz geciktirip temiz bir şekilde başlat
    private System.Collections.IEnumerator CompleteRestart()
    {
        // Geçiş animasyonu yapmak veya objelerinin yok edilmesini beklemek için kısa gecikme
        yield return new WaitForSeconds(0.2f);
        
        // Resources.UnloadUnusedAssets çağrılarak hafızayı temizle
        AsyncOperation asyncUnload = Resources.UnloadUnusedAssets();
        yield return asyncUnload;
        
        // Çöp toplayıcıyı çağır
        System.GC.Collect();
        
        // Sahneyi tekrar yükle - eksik sahne ismi kontrolü ekle
        string targetScene = "GameScene";
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.gameSceneName))
        {
            targetScene = GameManager.Instance.gameSceneName;
        }
        
        Debug.Log($"{targetScene} sahnesi tamamen temiz bir şekilde yeniden yükleniyor...");
        SceneManager.LoadScene(targetScene);
        
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Oyun tamamen yeniden başlatıldı!");
    }
} 