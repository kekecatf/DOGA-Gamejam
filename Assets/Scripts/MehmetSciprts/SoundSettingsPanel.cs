using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundSettingsPanel : MonoBehaviour
{
    [Header("Sliders")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider explosionVolumeSlider;
    public Slider rocketVolumeSlider;
    public Slider kamikazeVolumeSlider;
    public Slider buttonVolumeSlider;
    public Slider lowHealthVolumeSlider;

    [Header("Labels (Optional)")]
    public TextMeshProUGUI masterVolumeLabel;
    public TextMeshProUGUI musicVolumeLabel;
    public TextMeshProUGUI sfxVolumeLabel;
    public TextMeshProUGUI explosionVolumeLabel;
    public TextMeshProUGUI rocketVolumeLabel;
    public TextMeshProUGUI kamikazeVolumeLabel;
    public TextMeshProUGUI buttonVolumeLabel;
    public TextMeshProUGUI lowHealthVolumeLabel;

    private AudioManager audioManager;

    private void Start()
    {
        // AudioManager referansını bul
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("AudioManager bulunamadı! Ses ayarları çalışmayacak.");
                gameObject.SetActive(false);
                return;
            }
        }
        
        // Slider değerlerini AudioManager'dan al
        InitializeSliders();
        
        // Slider olaylarını ayarla
        SetupSliderListeners();
        
        // UI etiketlerini güncelle
        UpdateAllLabels();
    }
    
    private void InitializeSliders()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = audioManager.masterVolume;
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = audioManager.musicVolume;
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = audioManager.sfxVolume;
            
        if (explosionVolumeSlider != null)
            explosionVolumeSlider.value = audioManager.explosionVolume;
            
        if (rocketVolumeSlider != null)
            rocketVolumeSlider.value = audioManager.rocketVolume;
            
        if (kamikazeVolumeSlider != null)
            kamikazeVolumeSlider.value = audioManager.kamikazeVolume;
            
        if (buttonVolumeSlider != null)
            buttonVolumeSlider.value = audioManager.buttonVolume;
            
        if (lowHealthVolumeSlider != null)
            lowHealthVolumeSlider.value = audioManager.lowHealthVolume;
    }
    
    private void SetupSliderListeners()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            
        if (explosionVolumeSlider != null)
            explosionVolumeSlider.onValueChanged.AddListener(OnExplosionVolumeChanged);
            
        if (rocketVolumeSlider != null)
            rocketVolumeSlider.onValueChanged.AddListener(OnRocketVolumeChanged);
            
        if (kamikazeVolumeSlider != null)
            kamikazeVolumeSlider.onValueChanged.AddListener(OnKamikazeVolumeChanged);
            
        if (buttonVolumeSlider != null)
            buttonVolumeSlider.onValueChanged.AddListener(OnButtonVolumeChanged);
            
        if (lowHealthVolumeSlider != null)
            lowHealthVolumeSlider.onValueChanged.AddListener(OnLowHealthVolumeChanged);
    }
    
    private void UpdateAllLabels()
    {
        UpdateVolumeLabel(masterVolumeLabel, audioManager.masterVolume);
        UpdateVolumeLabel(musicVolumeLabel, audioManager.musicVolume);
        UpdateVolumeLabel(sfxVolumeLabel, audioManager.sfxVolume);
        UpdateVolumeLabel(explosionVolumeLabel, audioManager.explosionVolume);
        UpdateVolumeLabel(rocketVolumeLabel, audioManager.rocketVolume);
        UpdateVolumeLabel(kamikazeVolumeLabel, audioManager.kamikazeVolume);
        UpdateVolumeLabel(buttonVolumeLabel, audioManager.buttonVolume);
        UpdateVolumeLabel(lowHealthVolumeLabel, audioManager.lowHealthVolume);
    }
    
    private void UpdateVolumeLabel(TextMeshProUGUI label, float value)
    {
        if (label != null)
        {
            label.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }
    }
    
    // Slider değer değişim olayları
    public void OnMasterVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMasterVolume(value);
            UpdateVolumeLabel(masterVolumeLabel, value);
            
            // Test sesi çal
            PlayTestSound();
        }
    }
    
    public void OnMusicVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(value);
            UpdateVolumeLabel(musicVolumeLabel, value);
        }
    }
    
    public void OnSfxVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(value);
            UpdateVolumeLabel(sfxVolumeLabel, value);
            
            // Test sesi çal
            PlayTestSound();
        }
    }
    
    public void OnExplosionVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetExplosionVolume(value);
            UpdateVolumeLabel(explosionVolumeLabel, value);
            
            // Test sesi çal - patlama
            audioManager.PlayExplosionSound();
        }
    }
    
    public void OnRocketVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetRocketVolume(value);
            UpdateVolumeLabel(rocketVolumeLabel, value);
            
            // Test sesi çal - roket
            audioManager.PlayRocketSound();
        }
    }
    
    public void OnKamikazeVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetKamikazeVolume(value);
            UpdateVolumeLabel(kamikazeVolumeLabel, value);
            
            // Test sesi çal - kamikaze
            audioManager.PlayKamikazeSound();
        }
    }
    
    public void OnButtonVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetButtonVolume(value);
            UpdateVolumeLabel(buttonVolumeLabel, value);
            
            // Test sesi çal - buton
            audioManager.PlayButtonClick();
        }
    }
    
    public void OnLowHealthVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetLowHealthVolume(value);
            UpdateVolumeLabel(lowHealthVolumeLabel, value);
            
            // Test sesi çal - düşük can sesi
            audioManager.PlayLowHealthSound();
            
            // Kısa süre sonra düşük can sesini durdur
            Invoke("StopLowHealthSoundDelayed", 2.0f);
        }
    }
    
    private void StopLowHealthSoundDelayed()
    {
        if (audioManager != null)
        {
            audioManager.StopLowHealthSound();
        }
    }
    
    private void PlayTestSound()
    {
        // Slider değiştiğinde test etmek için buton sesi çal
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }
    
    // Ayarları sıfırla
    public void ResetSettings()
    {
        if (audioManager != null)
        {
            // Varsayılan değerleri ayarla
            audioManager.SetMasterVolume(1.0f);
            audioManager.SetMusicVolume(0.5f);
            audioManager.SetSFXVolume(0.5f);
            audioManager.SetExplosionVolume(0.7f);
            audioManager.SetRocketVolume(0.6f);
            audioManager.SetKamikazeVolume(0.8f);
            audioManager.SetButtonVolume(0.5f);
            audioManager.SetLowHealthVolume(0.5f);
            
            // Slider değerlerini güncelle
            InitializeSliders();
            
            // Etiketleri güncelle
            UpdateAllLabels();
        }
    }
    
    // Paneli aç/kapat
    public void TogglePanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        
        // Panel açıldığında ses seviyelerini güncelle
        if (gameObject.activeSelf)
        {
            InitializeSliders();
            UpdateAllLabels();
        }
    }
} 