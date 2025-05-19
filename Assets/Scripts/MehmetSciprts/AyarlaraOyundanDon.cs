using UnityEngine;
using UnityEngine.SceneManagement;

public class AyarlaraOyundanDon : MonoBehaviour
{
    public GameObject devamEtButton;
    public GameObject geriDonButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string gelis = PlayerPrefs.GetString("AyarlarGelis", "");
        if (gelis == "GameScene")
        {
            devamEtButton.SetActive(true);
            geriDonButton.SetActive(false);
        }
        else if (gelis == "AnaMenu")
        {
            devamEtButton.SetActive(false);
            geriDonButton.SetActive(true);
        }
        else
        {
            // Hiçbiri değilse ikisini de gizle
            devamEtButton.SetActive(false);
            geriDonButton.SetActive(false);
        }
    }

    public void DevamEt()
    {
        // Ayarlar sahnesini kapat
        SceneManager.UnloadSceneAsync("Ayarlar");
        // Oyunu devam ettir
        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
