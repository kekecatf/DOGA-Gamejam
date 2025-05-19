using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections; // Eğer coroutine kullanacaksan

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!pausePanel.activeSelf)
                PauseGame();
            else
                ResumeGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Oyunu durdur
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Oyunu devam ettir
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
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
