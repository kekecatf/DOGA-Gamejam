using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections; // Eğer coroutine kullanacaksan
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    
    public GameObject pauseMenuUI;
    public GameObject soundSettingsPanel;
    public GameObject pausePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Başlangıçta menü kapalı olsun
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
            
        // Ses ayarları paneli de kapalı olsun
        if (soundSettingsPanel != null)
            soundSettingsPanel.SetActive(false);
            
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        // ESC tuşuna basıldığında menu açılıp kapanır
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        // Önce ses ayarları panelini kapat
        if (soundSettingsPanel != null && soundSettingsPanel.activeSelf)
            soundSettingsPanel.SetActive(false);
            
        // PausePanel'i kapat
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        // Sonra pause menüsünü kapat
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
            
        // Oyunu devam ettir
        Time.timeScale = 1f;
        isPaused = false;
    }
    
    // ResumeGame metodu, UI buttonu buna referans verdiği için
    public void ResumeGame()
    {
        // Resume metodunu çağır
        Resume();
    }

    private void Pause()
    {
        // PauseMenuUI'yi aktif et
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
            
        // PausePanel'i aktif et
        if (pausePanel != null)
            pausePanel.SetActive(true);
            
        // Oyunu durdur
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenSoundSettings()
    {
        if (soundSettingsPanel != null)
    {
            // Ses ayarları panelini aç/kapat
            bool isActive = soundSettingsPanel.activeSelf;
            soundSettingsPanel.SetActive(!isActive);
            
            // SoundSettingsPanel bileşeninde TogglePanel metodunu çağır (varsa)
            SoundSettingsPanel settingsPanel = soundSettingsPanel.GetComponent<SoundSettingsPanel>();
            if (settingsPanel != null && !isActive)
            {
                // Paneli açarken değerleri güncelle
                // Not: TogglePanel metodu içinde zaten bu işlemler yapılıyor, bu nedenle özel bir metod çağırmaya gerek yok
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çıkılıyor...");
        
        // PlayerPrefs değerlerini kaydet
        PlayerPrefs.Save();
        
        // Editör modundaysa oynatmayı durdur, değilse uygulamadan çık
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void AyarlariAc()
    {
        PlayerPrefs.SetString("AyarlarGelis", "Oynanis");
        SceneManager.LoadScene("Ayarlar", LoadSceneMode.Additive);
        Time.timeScale = 0f;
    }

    // Eğer coroutine ile yapmak istersen:
    IEnumerator Duraklat()
    {
        yield return null;
        Time.timeScale = 0f;
    }

    public void DevamEt()
    {
        SceneManager.UnloadSceneAsync("Ayarlar");
        Time.timeScale = 1f;
    }
}
