using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Slider musicSlider;
    public Slider sfxSlider;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource menuMusicSource;
    public AudioClip buttonClickClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float savedMusic = PlayerPrefs.GetFloat("musicVolume", 0.5f);
        float savedSFX = PlayerPrefs.GetFloat("sfxVolume", 0.5f);

        if (musicSlider != null)
            musicSlider.value = savedMusic;
        if (sfxSlider != null)
            sfxSlider.value = savedSFX;

        if (musicSource != null)
            musicSource.volume = savedMusic;
        if (sfxSource != null)
            sfxSource.volume = savedSFX;

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
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

    public void StartGame()
    {
        if (menuMusicSource != null)
            menuMusicSource.Stop();
        SceneManager.LoadScene("Oynanis");
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickClip != null)
            sfxSource.PlayOneShot(buttonClickClip);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
