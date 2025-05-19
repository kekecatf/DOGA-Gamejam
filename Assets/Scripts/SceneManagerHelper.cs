using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneManagerHelper : MonoBehaviour
{
    private static SceneManagerHelper _instance;
    
    public static SceneManagerHelper Instance
    {
        get { return _instance; }
    }

    // Sahnelerin durumunu tutacak değişkenler
    private bool isMiniOyunSceneInBuildSettings = false;
    private int miniOyunBuildIndex = -1;
    
    // MiniOyun sahne adı - sabit string olarak tanımladık
    public string miniOyunSceneName = "MiniOyun";
    
    void Awake()
    {
        // Singleton yapısı
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SceneManagerHelper: Instance oluşturuldu ve DontDestroyOnLoad yapıldı.");
            
            // Sahne durumunu kontrol et
            CheckScenes();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void CheckScenes()
    {
        Debug.Log("SceneManagerHelper: Sahne durumu kontrol ediliyor...");
        
        // Build Settings'deki sahne sayısını al
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        Debug.Log("Build Settings'deki sahne sayısı: " + sceneCount);
        
        // Tüm sahneleri logla
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            Debug.Log("Sahne " + i + ": " + scenePath);
            
            // MiniOyun sahnesini ara
            if (scenePath.Contains(miniOyunSceneName))
            {
                isMiniOyunSceneInBuildSettings = true;
                miniOyunBuildIndex = i;
                Debug.Log("MiniOyun sahnesi Build Settings'de bulundu! Index: " + miniOyunBuildIndex);
            }
        }
        
        // MiniOyun sahnesi yoksa uyarı ver
        if (!isMiniOyunSceneInBuildSettings)
        {
            Debug.LogError("MiniOyun sahnesi Build Settings'de bulunamadı! " +
                          "Lütfen Edit > Build Settings menüsünden sahneyi ekleyin.");
        }
    }
    
    // MiniOyun sahnesini yüklemek için public metot
    public void LoadMiniOyunScene()
    {
        Debug.Log("SceneManagerHelper: MiniOyun sahnesine geçiş yapılıyor...");
        
        if (isMiniOyunSceneInBuildSettings)
        {
            try
            {
                // Build index ile yüklemeyi dene
                Debug.Log("MiniOyun sahnesi yükleniyor... BuildIndex: " + miniOyunBuildIndex);
                SceneManager.LoadScene(miniOyunBuildIndex);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Build index ile sahne yükleme hatası: " + e.Message);
                
                try
                {
                    // İsim ile yüklemeyi dene
                    Debug.Log("MiniOyun sahnesi isimle yükleniyor...");
                    SceneManager.LoadScene(miniOyunSceneName);
                }
                catch (System.Exception e2)
                {
                    Debug.LogError("İsim ile sahne yükleme hatası: " + e2.Message);
                    
                    // Hata durumunda varsayılan sahneye dön
                    Debug.LogWarning("Hata olduğu için varsayılan sahneye dönülüyor.");
                    SceneManager.LoadScene(0); // İlk sahneye dön
                }
            }
        }
        else
        {
            Debug.LogError("MiniOyun sahnesi Build Settings'de olmadığı için yüklenemiyor!");
            
            // Acil durum çözümü: GameScene'e dön
            Debug.LogWarning("Acil durum çözümü: GameScene yükleniyor.");
            SceneManager.LoadScene("GameScene");
        }
    }
    
    // Ana sahneye dönmek için metot
    public void LoadMainGameScene()
    {
        Debug.Log("SceneManagerHelper: Ana oyun sahnesine dönülüyor...");
        SceneManager.LoadScene("GameScene");
    }
} 