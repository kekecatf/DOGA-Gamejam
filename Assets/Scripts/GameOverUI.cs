using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            // GameManager yoksa doğrudan sahneyi yükle
            SceneManager.LoadScene("GameScene");
        }
    }
} 