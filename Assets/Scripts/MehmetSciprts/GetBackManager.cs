using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GetBackManager : MonoBehaviour
{
    public GameObject geriDonButonu;
    public GameObject oyunaDevamEtButonu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string gelisYeri = PlayerPrefs.GetString("AyarlarGelis", "AnaMenu");
        if (gelisYeri == "Oynanis")
        {
            oyunaDevamEtButonu.SetActive(true);
            geriDonButonu.SetActive(false);
        }
        else
        {
            oyunaDevamEtButonu.SetActive(false);
            geriDonButonu.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        StartCoroutine(LoadSceneWithDelay());
    }

    IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // 0.1 saniye bekle
        SceneManager.LoadScene("AnaMenu");
    }

    void OnEnable()
    {
        // Sahnedeki slider'ı bul
        Slider foundSlider = FindObjectOfType<Slider>(); // Gerekirse tag ile bul
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (foundSlider != null && audioManager != null)
        {
            audioManager.musicSlider = foundSlider;
            audioManager.musicSlider.value = audioManager.musicSource.volume;
            audioManager.musicSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
        }
    }

    public void GeriDon()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("AnaMenu");
    }

    public void OyunaDevamEt()
    {
        SceneManager.UnloadSceneAsync("Ayarlar");
        Time.timeScale = 1f;
    }
}
