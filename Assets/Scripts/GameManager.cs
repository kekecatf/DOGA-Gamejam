using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton yapısı
    public static GameManager Instance { get; private set; }
    
    // Oyun istatistikleri
    public int killedEnemyCount = 0;
    public float gameTime = 0f;
    public bool isGameOver = false;
    
    // Sahne adları
    public string gameSceneName = "GameScene";
    public string gameOverSceneName = "OyunSonu";
    
    private void Awake()
    {
        // Singleton yapısı
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
    
    private void Start()
    {
        // Oyun başlangıç ayarları
        ResetGameStats();
    }
    
    private void Update()
    {
        if (!isGameOver)
        {
            // Oyun devam ederken zamanı say
            gameTime += Time.deltaTime;
        }
    }
    
    // Düşman öldürüldüğünde çağrılacak
    public void EnemyKilled(EnemyType enemyType = EnemyType.Kamikaze)
    {
        killedEnemyCount++;
        Debug.Log($"Düşman öldürüldü! Toplam öldürülen düşman sayısı: {killedEnemyCount}");
    }
    
    // Zeplin öldüğünde çağrılacak
    public void GameOver()
    {
        isGameOver = true;
        Debug.Log("Oyun Bitti! Öldürülen düşman sayısı: " + killedEnemyCount);
        
        // Oyun sonu sahnesine geçiş
        SceneManager.LoadScene(gameOverSceneName);
    }
    
    // Oyun istatistiklerini sıfırla
    public void ResetGameStats()
    {
        killedEnemyCount = 0;
        gameTime = 0f;
        isGameOver = false;
    }
    
    // Oyunu yeniden başlat
    public void RestartGame()
    {
        ResetGameStats();
        SceneManager.LoadScene(gameSceneName);
    }
    
    // Ana menüye dön
    public void GoToMainMenu()
    {
        ResetGameStats();
        SceneManager.LoadScene("MainMenu");
    }
} 