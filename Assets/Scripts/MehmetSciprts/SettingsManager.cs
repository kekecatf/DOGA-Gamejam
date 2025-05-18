using UnityEngine;
using UnityEngine.SceneManagement;
public class SettingsManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Sonra sahne geçişi
        SceneManager.LoadScene("Ayarlar"); // veya ilgili sahne
    }
}
