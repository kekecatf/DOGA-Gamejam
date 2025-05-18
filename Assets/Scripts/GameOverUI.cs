using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    // UI elemanları
    public Text killedEnemyText;
    public Text gameTimeText;
    public Button restartButton;
    
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
    }
    
    // Restart butonuna tıklandığında çağrılacak
    public void RestartGame()
    {
        Debug.Log("Oyun yeniden başlatılıyor... Tüm veriler sıfırlanacak.");
        
        // PlayerPrefs üzerinden isteğe bağlı sıfırlama
        PlayerPrefs.DeleteKey("LastScore");
        PlayerPrefs.DeleteKey("LastTime");
        
        // Player ve Enemy gibi statik verileri sıfırla
        Player.isDead = false;
        
        if (GameManager.Instance != null)
        {
            // Oyun verileri sıfırlanıyor
            GameManager.Instance.ResetGameStats();
            
            // GameManager'ı temizle (isteğe bağlı)
            // Eklenmemiş ise, kaldırılabilir, singleton olduğu için kendi kendini temizler
            Destroy(GameManager.Instance.gameObject);
        }
        
        // Tüm aktif DontDestroyOnLoad objelerini temizle (isteğe bağlı)
        GameObject[] persistentObjects = GameObject.FindGameObjectsWithTag("PersistentObject");
        foreach (GameObject obj in persistentObjects)
        {
            Destroy(obj);
        }
        
        // Player nesnesini ve diğer kritik nesneleri her ihtimale karşı temizle
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Destroy(player);
        }
        
        GameObject zeplin = GameObject.FindGameObjectWithTag("Zeplin");
        if (zeplin != null)
        {
            Destroy(zeplin);
        }
        
        // Tam yeniden başlatma için
        Time.timeScale = 1.0f; // Oyun duraklatılmış olabilir, zamanı normale çevir
        
        // Ekranı temizleyip oyunu yeniden başlat
        StartCoroutine(CompleteRestart());
    }
    
    // Yeniden başlatma işlemini biraz geciktirip temiz bir şekilde başlat
    private System.Collections.IEnumerator CompleteRestart()
    {
        // Geçiş animasyonu yapmak veya objelerinin yok edilmesini beklemek için kısa gecikme
        yield return new WaitForSeconds(0.1f);
        
        // Sahneyi tekrar yükle
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        
        // İsteğe bağlı: Sahne yüklendikten sonra bir kontrol daha yapılabilir
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("Oyun tamamen yeniden başlatıldı!");
    }
} 