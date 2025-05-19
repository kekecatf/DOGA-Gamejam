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
    public AudioSource lowHealthSource; // Düşük can için özel ses kaynağı
    public AudioClip buttonClickClip;
    public AudioClip explosionClip; // Patlama ses efekti
    public AudioClip rocketSfxClip; // Roket ateşleme ses efekti
    public AudioClip gameMusicClip; // Oyun müziği
    public AudioClip kamikazeClip; // Kamikaze ses efekti
    public AudioClip lowHealthClip; // Düşük can sesi (uçak sesi)
    public AudioClip minigunClip; // Minigun ateş sesi

    // Ses seviyesi ayarları
    [Header("Ses Seviyeleri")]
    [Range(0f, 1f)]
    public float masterVolume = 1.0f; // Ana ses seviyesi
    [Range(0f, 1f)]
    public float musicVolume = 0.5f; // Müzik ses seviyesi
    [Range(0f, 1f)]
    public float sfxVolume = 0.5f; // Genel SFX ses seviyesi
    [Range(0f, 1f)]
    public float explosionVolume = 0.7f; // Patlama ses seviyesi
    [Range(0f, 1f)]
    public float rocketVolume = 0.6f; // Roket ses seviyesi
    [Range(0f, 1f)]
    public float kamikazeVolume = 0.8f; // Kamikaze ses seviyesi
    [Range(0f, 1f)]
    public float buttonVolume = 0.5f; // Buton ses seviyesi
    [Range(0f, 1f)]
    public float lowHealthVolume = 0.5f; // Düşük can ses seviyesi
    [Range(0f, 1f)]
    public float minigunVolume = 0.6f; // Minigun ses seviyesi

    // Düşük can ayarları
    [Header("Düşük Can Ayarları")]
    public float lowHealthThreshold = 0.3f; // Sağlık yüzde kaçın altındaysa düşük can sesi çalınsın (0-1 arası)
    public bool isLowHealthSoundPlaying = false; // Düşük can sesi çalıyor mu?

    // Sahne adları
    private const string GAME_SCENE_NAME = "GameScene";
    private const string MENU_SCENE_NAME = "AnaMenu";

    // PlayerPrefs keys
    private const string MASTER_VOLUME_KEY = "masterVolume";
    private const string MUSIC_VOLUME_KEY = "musicVolume";
    private const string SFX_VOLUME_KEY = "sfxVolume";
    private const string EXPLOSION_VOLUME_KEY = "explosionVolume";
    private const string ROCKET_VOLUME_KEY = "rocketVolume";
    private const string KAMIKAZE_VOLUME_KEY = "kamikazeVolume";
    private const string BUTTON_VOLUME_KEY = "buttonVolume";
    private const string LOW_HEALTH_VOLUME_KEY = "lowHealthVolume";
    private const string MINIGUN_VOLUME_KEY = "minigunVolume";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Kayıtlı ses seviyelerini yükle
        LoadSavedVolumes();

        // UI slider değerlerini güncelle
        UpdateSliders();
        
        // UI slider olaylarını ayarla
        SetupSliderListeners();
            
        // Ses efektlerini yükle
        TryLoadExplosionClip();
        TryLoadRocketClip();
        TryLoadGameMusicClip();
        TryLoadKamikazeClip();
        TryLoadLowHealthClip();
        TryLoadMinigunClip();
        
        // Sahne değişim olayını dinle
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Mevcut sahneye göre uygun müziği çal
        PlayMusicForCurrentScene();
        
        // Düşük can ses kaynağını ayarla
        SetupLowHealthSource();
    }
    
    void Update()
    {
        // Oyun sahnesinde ve düşük can sesi aktifse düşük can durumunu kontrol et
        if (SceneManager.GetActiveScene().name == GAME_SCENE_NAME)
        {
            CheckPlayerHealth();
        }
    }
    
    // Düşük can ses kaynağını ayarla
    private void SetupLowHealthSource()
    {
        if (lowHealthSource == null)
        {
            // Eğer düşük can ses kaynağı atanmamışsa oluştur
            GameObject lowHealthSourceObj = new GameObject("LowHealthAudioSource");
            lowHealthSourceObj.transform.parent = transform;
            lowHealthSource = lowHealthSourceObj.AddComponent<AudioSource>();
            
            // Ayarlar
            lowHealthSource.playOnAwake = false;
            lowHealthSource.loop = true;
            lowHealthSource.volume = lowHealthVolume * sfxVolume * masterVolume;
            lowHealthSource.priority = 0; // Yüksek öncelik
        }
    }
    
    // Oyuncu sağlığını kontrol et ve düşük can sesini yönet
    private void CheckPlayerHealth()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null && player.GetComponent<PlayerData>() != null)
        {
            PlayerData playerData = player.GetComponent<PlayerData>();
            
            // Oyuncunun mevcut sağlığını hesapla (yüzde olarak)
            // PlayerData'da anaGemiMaxSaglik doğrudan tanımlı olmadığı için hesaplıyoruz
            int maxHealth = 100 + (playerData.anaGemiSaglikLevel * 10); // Base health + level bonus
            float currentHealth = playerData.anaGemiSaglik;
            float healthPercent = currentHealth / maxHealth;
            
            // Düşük can durumunda ses çal
            if (healthPercent <= lowHealthThreshold && !Player.isDead)
            {
                PlayLowHealthSound();
            }
            else
            {
                StopLowHealthSound();
            }
        }
        else
        {
            // Oyuncu bulunamadıysa ses çalmayı durdur
            StopLowHealthSound();
        }
    }

    private void LoadSavedVolumes()
    {
        // Ana ses seviyesi
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1.0f);
        
        // Müzik ses seviyesi
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        
        // SFX ses seviyesi
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);
        
        // Özel ses seviyeleri
        explosionVolume = PlayerPrefs.GetFloat(EXPLOSION_VOLUME_KEY, 0.7f);
        rocketVolume = PlayerPrefs.GetFloat(ROCKET_VOLUME_KEY, 0.6f);
        kamikazeVolume = PlayerPrefs.GetFloat(KAMIKAZE_VOLUME_KEY, 0.8f);
        buttonVolume = PlayerPrefs.GetFloat(BUTTON_VOLUME_KEY, 0.5f);
        lowHealthVolume = PlayerPrefs.GetFloat(LOW_HEALTH_VOLUME_KEY, 0.5f);
        minigunVolume = PlayerPrefs.GetFloat(MINIGUN_VOLUME_KEY, 0.6f);
        
        // AudioSource ses seviyelerini ayarla
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
            
        if (menuMusicSource != null)
            menuMusicSource.volume = musicVolume * masterVolume;
            
        if (lowHealthSource != null)
            lowHealthSource.volume = lowHealthVolume * sfxVolume * masterVolume;
    }
    
    private void UpdateSliders()
    {
        if (musicSlider != null)
            musicSlider.value = musicVolume;
            
        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;
    }
    
    private void SetupSliderListeners()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    
    private void OnDestroy()
    {
        // Sahne değişim olayını dinlemeyi bırak
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // Sahne değiştiğinde çağrılır
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"AudioManager: Sahne yüklendi: {scene.name}");
        PlayMusicForCurrentScene();
    }
    
    // Mevcut sahneye göre uygun müziği çal
    private void PlayMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"AudioManager: Mevcut sahne: {currentSceneName}");
        
        if (currentSceneName == GAME_SCENE_NAME)
        {
            PlayGameMusic();
        }
        else if (currentSceneName == MENU_SCENE_NAME)
        {
            PlayMenuMusic();
        }
    }
    
    private void TryLoadExplosionClip()
    {
        // Eğer ses zaten yüklüyse işlem yapmaya gerek yok
        if (explosionClip != null)
            return;
        
        // Farklı ses yollarını dene
        string[] pathOptions = {
            "Sounds/patlama",       // Yeni yol: Resources/Sounds/patlama
            "patlama",              // Eski yol: Resources/patlama
            "boosted patlama",      // Alternatif ses
            "kamikaze"              // Diğer bir ses
        };
        
        foreach (string path in pathOptions)
        {
            explosionClip = Resources.Load<AudioClip>(path);
            if (explosionClip != null)
            {
                Debug.Log($"AudioManager: {path} ses efekti başarıyla yüklendi.");
                break;
            }
        }
        
        if (explosionClip == null)
        {
            Debug.LogWarning("AudioManager: Patlama ses efekti yüklenemedi! Unity Editor'da AudioManager prefabı üzerinden manuel olarak atayın.");
        }
    }
    
    private void TryLoadRocketClip()
    {
        // Eğer ses zaten yüklüyse işlem yapmaya gerek yok
        if (rocketSfxClip != null)
            return;
        
        // Farklı ses yollarını dene
        string[] pathOptions = {
            "Sounds/füze",         // Resources/Sounds/füze
            "füze",                // Resources/füze
            "Sounds/rocket_burst_sfx", // Resources/Sounds/rocket_burst_sfx (Düzeltildi)
            "rocket_burst_sfx"     // Resources/rocket_burst_sfx (Düzeltildi)
        };
        
        foreach (string path in pathOptions)
        {
            rocketSfxClip = Resources.Load<AudioClip>(path);
            if (rocketSfxClip != null)
            {
                Debug.Log($"AudioManager: {path} roket ses efekti başarıyla yüklendi.");
                break;
            }
        }
        
        if (rocketSfxClip == null)
        {
            // Son çare: Füze sesi yüklenemedi, patlama sesini veya başka bir sesi kullan
            rocketSfxClip = explosionClip;
            Debug.LogWarning("AudioManager: Roket ses efekti yüklenemedi! Patlama sesi kullanılacak.");
        }
    }
    
    private void TryLoadGameMusicClip()
    {
        // Eğer ses zaten yüklüyse işlem yapmaya gerek yok
        if (gameMusicClip != null)
            return;
        
        // Farklı ses yollarını dene
        string[] pathOptions = {
            "Sounds/oyun müzik",    // Resources/Sounds/oyun müzik
            "oyun müzik",           // Resources/oyun müzik
            "Sounds/Mr. Ultimate - Command Room", // Alternatif müzik
            "Mr. Ultimate - Command Room"         // Alternatif müzik
        };
        
        foreach (string path in pathOptions)
        {
            gameMusicClip = Resources.Load<AudioClip>(path);
            if (gameMusicClip != null)
            {
                Debug.Log($"AudioManager: {path} oyun müziği başarıyla yüklendi.");
                break;
            }
        }
        
        if (gameMusicClip == null)
        {
            Debug.LogWarning("AudioManager: Oyun müziği yüklenemedi! Unity Editor'da AudioManager prefabı üzerinden manuel olarak atayın.");
        }
    }
    
    private void TryLoadKamikazeClip()
    {
        // Eğer ses zaten yüklüyse işlem yapmaya gerek yok
        if (kamikazeClip != null)
            return;
        
        // Farklı ses yollarını dene
        string[] pathOptions = {
            "Sounds/kamikaze",      // Resources/Sounds/kamikaze
            "kamikaze",             // Resources/kamikaze
            "Sounds/patlama",       // Alternatif ses
            "patlama"               // Alternatif ses
        };
        
        foreach (string path in pathOptions)
        {
            kamikazeClip = Resources.Load<AudioClip>(path);
            if (kamikazeClip != null)
            {
                Debug.Log($"AudioManager: {path} kamikaze ses efekti başarıyla yüklendi.");
                break;
            }
        }
        
        if (kamikazeClip == null)
        {
            // Son çare: Kamikaze sesi yüklenemedi, patlama sesini kullan
            kamikazeClip = explosionClip;
            Debug.LogWarning("AudioManager: Kamikaze ses efekti yüklenemedi! Patlama sesi kullanılacak.");
        }
    }
    
    private void TryLoadLowHealthClip()
    {
        // Eğer ses zaten yüklüyse işlem yapmaya gerek yok
        if (lowHealthClip != null)
            return;
        
        // Farklı ses yollarını dene
        string[] pathOptions = {
            "Sounds/ucak_sesi",         // Resources/Sounds/ucak_sesi
            "ucak_sesi",                // Resources/ucak_sesi
            "Sounds/engine_low",         // Alternatif ses
            "engine_low",               // Alternatif ses
            "Sounds/alarm",             // Son çare
            "alarm"                     // Son çare
        };
        
        foreach (string path in pathOptions)
        {
            lowHealthClip = Resources.Load<AudioClip>(path);
            if (lowHealthClip != null)
            {
                Debug.Log($"AudioManager: {path} düşük can ses efekti başarıyla yüklendi.");
                break;
            }
        }
        
        if (lowHealthClip == null)
        {
            Debug.LogWarning("AudioManager: Düşük can ses efekti yüklenemedi! Unity Editor'da AudioManager prefabı üzerinden manuel olarak atayın.");
        }
    }

    private void TryLoadMinigunClip()
    {
        // Eğer ses zaten yüklüyse işlem yapmaya gerek yok
        if (minigunClip != null)
            return;
        
        // Farklı ses yollarını dene
        string[] pathOptions = {
            "Sounds/minigun",        // Yeni yol: Resources/Sounds/minigun
            "minigun"                // Eski yol: Resources/minigun
        };
        
        foreach (string path in pathOptions)
        {
            minigunClip = Resources.Load<AudioClip>(path);
            if (minigunClip != null)
            {
                Debug.Log($"AudioManager: {path} minigun ses efekti başarıyla yüklendi.");
                break;
            }
        }
        
        if (minigunClip == null)
        {
            Debug.LogWarning("AudioManager: Minigun ses efekti bulunamadı!");
        }
    }

    // Ana ses seviyesini ayarlar ve kaydeder
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        
        // Tüm ses kaynaklarının ana ses seviyesine göre güncellenmesi
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
            
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
            
        if (menuMusicSource != null)
            menuMusicSource.volume = musicVolume * masterVolume;
            
        if (lowHealthSource != null)
            lowHealthSource.volume = lowHealthVolume * sfxVolume * masterVolume;
            
        Debug.Log($"Ana ses seviyesi: {masterVolume:F2}");
    }

    // Müzik ses seviyesini ayarlar ve kaydeder
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
            
        if (menuMusicSource != null)
            menuMusicSource.volume = musicVolume * masterVolume;
            
        Debug.Log($"Müzik ses seviyesi: {musicVolume:F2}");
    }

    // SFX ses seviyesini ayarlar ve kaydeder
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
            
        Debug.Log($"SFX ses seviyesi: {sfxVolume:F2}");
    }
    
    // Patlama ses seviyesini ayarlar ve kaydeder
    public void SetExplosionVolume(float volume)
    {
        explosionVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(EXPLOSION_VOLUME_KEY, explosionVolume);
        Debug.Log($"Patlama ses seviyesi: {explosionVolume:F2}");
    }
    
    // Roket ses seviyesini ayarlar ve kaydeder
    public void SetRocketVolume(float volume)
    {
        rocketVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(ROCKET_VOLUME_KEY, rocketVolume);
        Debug.Log($"Roket ses seviyesi: {rocketVolume:F2}");
    }

    // Kamikaze ses seviyesini ayarlar ve kaydeder
    public void SetKamikazeVolume(float volume)
    {
        kamikazeVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(KAMIKAZE_VOLUME_KEY, kamikazeVolume);
        Debug.Log($"Kamikaze ses seviyesi: {kamikazeVolume:F2}");
    }
    
    // Buton ses seviyesini ayarlar ve kaydeder
    public void SetButtonVolume(float volume)
    {
        buttonVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BUTTON_VOLUME_KEY, buttonVolume);
        Debug.Log($"Buton ses seviyesi: {buttonVolume:F2}");
    }

    // Düşük can ses seviyesini ayarlar ve kaydeder
    public void SetLowHealthVolume(float volume)
    {
        lowHealthVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(LOW_HEALTH_VOLUME_KEY, lowHealthVolume);
        
        if (lowHealthSource != null)
            lowHealthSource.volume = lowHealthVolume * sfxVolume * masterVolume;
            
        Debug.Log($"Düşük can ses seviyesi: {lowHealthVolume:F2}");
    }

    // Minigun ses seviyesini ayarla
    public void SetMinigunVolume(float volume)
    {
        minigunVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MINIGUN_VOLUME_KEY, minigunVolume);
        Debug.Log($"Minigun ses seviyesi: {minigunVolume:F2}");
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManager: Instance oluşturuldu");
            
            // Eğer sfxSource null ise yeni oluştur
            if (sfxSource == null)
            {
                Debug.Log("AudioManager.Awake: sfxSource oluşturuluyor");
                GameObject sfxSourceObj = new GameObject("SFXSource");
                sfxSourceObj.transform.parent = transform;
                sfxSource = sfxSourceObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            // Ses kliplerini başlangıçta yüklemeyi dene
            TryLoadExplosionClip();
            TryLoadRocketClip();
            TryLoadMinigunClip();
            
            // Debug bilgisi
            Debug.Log("AudioManager.Awake: explosionClip " + (explosionClip != null ? "yüklendi" : "yüklenemedi"));
            Debug.Log("AudioManager.Awake: rocketSfxClip " + (rocketSfxClip != null ? "yüklendi" : "yüklenemedi"));
            Debug.Log("AudioManager.Awake: minigunClip " + (minigunClip != null ? "yüklendi" : "yüklenemedi"));
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("AudioManager: Başka bir instance var, bu nesne siliniyor");
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
        {
            // Buton sesi için özel ses seviyesi
            sfxSource.PlayOneShot(buttonClickClip, buttonVolume * sfxVolume * masterVolume);
        }
    }
    
    public void PlayExplosionSound()
    {
        Debug.Log("PlayExplosionSound çağrıldı - sfxSource: " + (sfxSource != null ? "var" : "yok") + 
                  ", explosionClip: " + (explosionClip != null ? "var" : "yok"));
                  
        if (sfxSource != null && explosionClip != null)
        {
            // Patlama sesi için özel ses seviyesi
            float volume = explosionVolume * sfxVolume * masterVolume;
            Debug.Log($"Patlama sesi çalınıyor. Ses seviyesi: {volume:F2}");
            sfxSource.PlayOneShot(explosionClip, volume);
        }
        else if (sfxSource != null)
        {
            Debug.Log("explosionClip bulunamadı, Resources'dan yüklemeye çalışılıyor...");
            // Eğer explosionClip null ise, Resources'dan yüklemeyi dene
            string[] pathOptions = {
                "Sounds/patlama",       // Yeni yol: Resources/Sounds/patlama
                "patlama",              // Eski yol: Resources/patlama
                "boosted patlama",      // Alternatif ses
                "kamikaze"              // Diğer bir ses
            };
            
            AudioClip clip = null;
            
            foreach (string path in pathOptions)
            {
                Debug.Log($"Deneniyor: {path}");
                clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    Debug.Log($"AudioManager: {path} ses efekti yüklendi ve çalınıyor.");
                    // Patlama sesi için özel ses seviyesi
                    float volume = explosionVolume * sfxVolume * masterVolume;
                    sfxSource.PlayOneShot(clip, volume);
                    
                    // Bir sonraki kullanım için explosionClip'e atayalım
                    explosionClip = clip;
                    break;
                }
            }
            
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Patlama ses efekti bulunamadı! Hiçbir ses yolu çalışmadı.");
            }
        }
    }
    
    public void PlayRocketSound()
    {
        Debug.Log("PlayRocketSound çağrıldı - sfxSource: " + (sfxSource != null ? "var" : "yok") + 
                  ", rocketSfxClip: " + (rocketSfxClip != null ? "var" : "yok"));
                  
        if (sfxSource != null && rocketSfxClip != null)
        {
            // Roket sesi için özel ses seviyesi
            float volume = rocketVolume * sfxVolume * masterVolume;
            Debug.Log($"Roket sesi çalınıyor. Ses seviyesi: {volume:F2}");
            sfxSource.PlayOneShot(rocketSfxClip, volume);
        }
        else if (sfxSource != null)
        {
            Debug.Log("rocketSfxClip bulunamadı, Resources'dan yüklemeye çalışılıyor...");
            // Eğer rocketSfxClip null ise, Resources'dan yüklemeyi dene
            string[] pathOptions = {
                "Sounds/füze",         // Resources/Sounds/füze
                "füze",                // Resources/füze
                "Sounds/rocket_burst_sfx", // Resources/Sounds/rocket_burst_sfx
                "rocket_burst_sfx"     // Resources/rocket_burst_sfx
            };
            
            AudioClip clip = null;
            
            foreach (string path in pathOptions)
            {
                Debug.Log($"Deneniyor: {path}");
                clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    Debug.Log($"AudioManager: {path} roket ses efekti yüklendi ve çalınıyor.");
                    // Roket sesi için özel ses seviyesi
                    float volume = rocketVolume * sfxVolume * masterVolume;
                    sfxSource.PlayOneShot(clip, volume);
                    
                    // Bir sonraki kullanım için rocketSfxClip'e atayalım
                    rocketSfxClip = clip;
                    break;
                }
            }
            
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Roket ses efekti bulunamadı! Hiçbir ses çalınamadı.");
                // Yedek olarak patlama sesini kullan
                if (explosionClip != null)
                {
                    Debug.Log("Yedek olarak patlama sesi kullanılıyor...");
                    sfxSource.PlayOneShot(explosionClip, rocketVolume * sfxVolume * masterVolume);
                }
            }
        }
    }
    
    public void PlayKamikazeSound()
    {
        if (sfxSource != null && kamikazeClip != null)
        {
            // Kamikaze sesi için özel ses seviyesi
            sfxSource.PlayOneShot(kamikazeClip, kamikazeVolume * sfxVolume * masterVolume);
        }
        else if (sfxSource != null)
        {
            // Eğer kamikazeClip null ise, Resources'dan yüklemeyi dene
            string[] pathOptions = {
                "Sounds/kamikaze",      // Resources/Sounds/kamikaze
                "kamikaze",             // Resources/kamikaze
                "Sounds/patlama",       // Alternatif ses
                "patlama"               // Alternatif ses
            };
            
            AudioClip clip = null;
            
            foreach (string path in pathOptions)
            {
                clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    Debug.Log($"AudioManager: {path} kamikaze ses efekti yüklendi ve çalınıyor.");
                    // Kamikaze sesi için özel ses seviyesi
                    sfxSource.PlayOneShot(clip, kamikazeVolume * sfxVolume * masterVolume);
                    
                    // Bir sonraki kullanım için kamikazeClip'e atayalım
                    kamikazeClip = clip;
                    break;
                }
            }
            
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Kamikaze ses efekti bulunamadı! Hiçbir ses çalınamadı.");
            }
        }
    }
    
    public void PlayGameMusic()
    {
        // Eğer zaten oyun müziği çalıyorsa tekrar başlatma
        if (musicSource != null && musicSource.isPlaying && musicSource.clip == gameMusicClip)
        {
            Debug.Log("AudioManager: Oyun müziği zaten çalıyor.");
            return;
        }
        
        // Diğer müzikleri durdur
        StopMusic();
        
        if (musicSource != null && gameMusicClip != null)
        {
            // Oyun müziğini çal - müzik ses seviyesini uygula (GameScene için %25 azaltma)
            musicSource.clip = gameMusicClip;
            
            // GameScene için ses azaltma
            if (SceneManager.GetActiveScene().name == GAME_SCENE_NAME)
            {
                // GameScene için ses seviyesini %25 olarak ayarla
                musicSource.volume = (musicVolume * masterVolume) * 0.25f;
                Debug.Log("AudioManager: GameScene için oyun müziği ses seviyesi %25 olarak ayarlandı.");
            }
            else
            {
                // Diğer sahneler için normal ses seviyesi
                musicSource.volume = musicVolume * masterVolume;
            }
            
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log("AudioManager: Oyun müziği başlatıldı.");
        }
        else if (musicSource != null)
        {
            // gameMusicClip null ise, Resources'dan yüklemeyi dene
            string[] pathOptions = {
                "Sounds/oyun müzik", 
                "oyun müzik", 
                "Sounds/Mr. Ultimate - Command Room",
                "Mr. Ultimate - Command Room"
            };
            
            AudioClip clip = null;
            
            foreach (string path in pathOptions)
            {
                clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    Debug.Log($"AudioManager: {path} oyun müziği yüklendi ve çalınıyor.");
                    musicSource.clip = clip;
                    
                    // GameScene için ses azaltma
                    if (SceneManager.GetActiveScene().name == GAME_SCENE_NAME)
                    {
                        // GameScene için ses seviyesini %25 olarak ayarla
                        musicSource.volume = (musicVolume * masterVolume) * 0.25f;
                        Debug.Log("AudioManager: GameScene için oyun müziği ses seviyesi %25 olarak ayarlandı.");
                    }
                    else
                    {
                        // Diğer sahneler için normal ses seviyesi
                        musicSource.volume = musicVolume * masterVolume;
                    }
                    
                    musicSource.loop = true;
                    musicSource.Play();
                    
                    // Bir sonraki kullanım için gameMusicClip'e atayalım
                    gameMusicClip = clip;
                    break;
                }
            }
            
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Oyun müziği bulunamadı! Hiçbir müzik çalınamadı.");
            }
        }
    }
    
    public void PlayMenuMusic()
    {
        if (menuMusicSource != null)
        {
            if (!menuMusicSource.isPlaying)
            {
                // Menü müziği ses seviyesini ayarla
                menuMusicSource.volume = musicVolume * masterVolume;
                menuMusicSource.Play();
                Debug.Log("AudioManager: Menü müziği başlatıldı.");
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
    {
        musicSource.Stop();
            Debug.Log("AudioManager: Müzik durduruldu.");
        }
        
        if (menuMusicSource != null && menuMusicSource.isPlaying)
        {
            menuMusicSource.Stop();
            Debug.Log("AudioManager: Menü müziği durduruldu.");
        }
    }

    // Düşük can sesi çal
    public void PlayLowHealthSound()
    {
        if (lowHealthSource != null && lowHealthClip != null && !isLowHealthSoundPlaying)
        {
            // Düşük can sesini ayarla
            lowHealthSource.clip = lowHealthClip;
            lowHealthSource.volume = lowHealthVolume * sfxVolume * masterVolume;
            lowHealthSource.Play();
            isLowHealthSoundPlaying = true;
            Debug.Log("AudioManager: Düşük can sesi başlatıldı.");
        }
        else if (lowHealthSource != null && !isLowHealthSoundPlaying)
        {
            // Eğer lowHealthClip null ise, Resources'dan yüklemeyi dene
            string[] pathOptions = {
                "Sounds/ucak_sesi",         // Resources/Sounds/ucak_sesi
                "ucak_sesi",                // Resources/ucak_sesi
                "Sounds/engine_low",        // Alternatif ses
                "engine_low",               // Alternatif ses
                "Sounds/alarm",             // Son çare
                "alarm"                     // Son çare
            };
            
            AudioClip clip = null;
            
            foreach (string path in pathOptions)
            {
                clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    Debug.Log($"AudioManager: {path} düşük can ses efekti yüklendi ve çalınıyor.");
                    
                    // Düşük can sesini ayarla ve çal
                    lowHealthSource.clip = clip;
                    lowHealthSource.volume = lowHealthVolume * sfxVolume * masterVolume;
                    lowHealthSource.Play();
                    
                    // Bir sonraki kullanım için lowHealthClip'e atayalım
                    lowHealthClip = clip;
                    isLowHealthSoundPlaying = true;
                    break;
                }
            }
            
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Düşük can ses efekti bulunamadı! Hiçbir ses çalınamadı.");
            }
        }
    }
    
    // Düşük can sesini durdur
    public void StopLowHealthSound()
    {
        if (lowHealthSource != null && isLowHealthSoundPlaying)
        {
            lowHealthSource.Stop();
            isLowHealthSoundPlaying = false;
            Debug.Log("AudioManager: Düşük can sesi durduruldu.");
        }
    }
    
    // Tüm sesleri durdur
    public void StopAllSounds()
    {
        StopMusic();
        StopLowHealthSound();
    }

    // Minigun ateş etme sesini çalma metodu
    public void PlayMinigunSound()
    {
        // Minigun sesi devre dışı bırakıldı
        return;
        
        /* Eski kod:
        Debug.Log("PlayMinigunSound çağrıldı - sfxSource: " + (sfxSource != null ? "var" : "yok") + 
                  ", minigunClip: " + (minigunClip != null ? "var" : "yok"));
                  
        if (sfxSource != null && minigunClip != null)
        {
            // Minigun sesi için özel ses seviyesi
            float volume = minigunVolume * sfxVolume * masterVolume;
            Debug.Log($"Minigun sesi çalınıyor. Ses seviyesi: {volume:F2}");
            sfxSource.PlayOneShot(minigunClip, volume);
        }
        else if (sfxSource != null)
        {
            Debug.Log("minigunClip bulunamadı, Resources'dan yüklemeye çalışılıyor...");
            // Eğer minigunClip null ise, Resources'dan yüklemeyi dene
            string[] pathOptions = {
                "Sounds/minigun",        // Resources/Sounds/minigun
                "minigun"                // Resources/minigun
            };
            
            AudioClip clip = null;
            
            foreach (string path in pathOptions)
            {
                Debug.Log($"Deneniyor: {path}");
                clip = Resources.Load<AudioClip>(path);
                if (clip != null)
                {
                    Debug.Log($"AudioManager: {path} minigun ses efekti yüklendi ve çalınıyor.");
                    // Minigun sesi için özel ses seviyesi
                    float volume = minigunVolume * sfxVolume * masterVolume;
                    sfxSource.PlayOneShot(clip, volume);
                    
                    // Bir sonraki kullanım için minigunClip'e atayalım
                    minigunClip = clip;
                    break;
                }
            }
            
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Minigun ses efekti bulunamadı! Hiçbir ses çalınamadı.");
            }
        }
        */
    }
}
