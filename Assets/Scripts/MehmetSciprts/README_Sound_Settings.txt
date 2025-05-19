# Ses Ayarları Sistemi Kullanım Kılavuzu

Bu belge, oyundaki ses ayarları sisteminin nasıl kurulacağını ve kullanılacağını açıklar.

## 1. AudioManager Bileşeni

AudioManager, tüm ses efektlerini ve müziği yöneten ana bileşendir. Şu ses seviyelerini kontrol eder:

- Master Volume: Tüm sesleri etkileyen ana ses seviyesi
- Music Volume: Müzik ses seviyesi
- SFX Volume: Tüm ses efektleri için genel ses seviyesi
- Explosion Volume: Patlama ses efektleri için özel ses seviyesi
- Rocket Volume: Roket ses efektleri için özel ses seviyesi
- Kamikaze Volume: Kamikaze ses efektleri için özel ses seviyesi
- Button Volume: Buton ses efektleri için özel ses seviyesi
- Low Health Volume: Düşük can durumunda çalan uçak sesi için özel ses seviyesi

## 2. Düşük Can Uçak Sesi Özelliği

Oyundaki "Düşük Can Uçak Sesi" özelliği, oyuncunun sağlığı belirli bir eşiğin altına düştüğünde otomatik olarak çalışan bir uyarı sesidir. Bu özellikle ilgili ayarlar:

- lowHealthThreshold: Oyuncu sağlığının yüzde kaç olduğunda sesin çalacağını belirler (varsayılan: 0.3 = %30)
- lowHealthVolume: Uçak sesinin ses düzeyi (varsayılan: 0.5 = %50)
- Ses dosyası: "ucak_sesi.wav" Resources/Sounds klasöründe bulunmalıdır

Bu ses, oyuncuya düşük sağlık durumunu sesli olarak hatırlatmak için kullanılır. Oyuncunun sağlığı yeniden yükseldiğinde veya oyuncu öldüğünde ses otomatik olarak durur.

## 3. SoundSettingsPanel Bileşeni

SoundSettingsPanel, oyuncunun ses seviyelerini ayarlayabileceği bir kullanıcı arayüzü sağlar.

### Kurulum Adımları:

1. Hiyerarşide "SoundSettings" adında yeni bir Canvas oluşturun
2. Canvas'a SoundSettingsPanel.cs script'ini ekleyin
3. Canvas içine şu UI elemanlarını ekleyin:
   - Her ses tipi için bir Slider
   - İsteğe bağlı olarak, her ses seviyesi için bir TextMeshProUGUI etiketi
   - "Sıfırla" butonu
   - "Kapat" butonu

4. SoundSettingsPanel bileşeni üzerinde şu atamaları yapın:
   - masterVolumeSlider: Ana ses seviyesi slider'ı
   - musicVolumeSlider: Müzik ses seviyesi slider'ı
   - sfxVolumeSlider: SFX ses seviyesi slider'ı
   - explosionVolumeSlider: Patlama ses seviyesi slider'ı
   - rocketVolumeSlider: Roket ses seviyesi slider'ı
   - kamikazeVolumeSlider: Kamikaze ses seviyesi slider'ı
   - buttonVolumeSlider: Buton ses seviyesi slider'ı
   - lowHealthVolumeSlider: Düşük can uçak sesi seviyesi slider'ı
   
   - masterVolumeLabel: Ana ses seviyesi etiketi (TextMeshProUGUI)
   - musicVolumeLabel: Müzik ses seviyesi etiketi (TextMeshProUGUI)
   - sfxVolumeLabel: SFX ses seviyesi etiketi (TextMeshProUGUI)
   - explosionVolumeLabel: Patlama ses seviyesi etiketi (TextMeshProUGUI)
   - rocketVolumeLabel: Roket ses seviyesi etiketi (TextMeshProUGUI)
   - kamikazeVolumeLabel: Kamikaze ses seviyesi etiketi (TextMeshProUGUI)
   - buttonVolumeLabel: Buton ses seviyesi etiketi (TextMeshProUGUI)
   - lowHealthVolumeLabel: Düşük can uçak sesi seviyesi etiketi (TextMeshProUGUI)

5. "Sıfırla" butonunun OnClick olayına SoundSettingsPanel.ResetSettings metodunu ekleyin.
6. "Kapat" butonunun OnClick olayına SoundSettingsPanel.TogglePanel metodunu ekleyin.
7. Panelin başlangıçta kapalı olmasını sağlamak için Canvas'ı kapalı olarak ayarlayın.

## 4. Pause Menu ile Entegrasyon

Pause menüsü üzerinden ses ayarları paneline erişim sağlamak için:

1. PauseMenu.cs script'inde soundSettingsPanel değişkenine SoundSettings Canvas'ını atayın.
2. Pause menüsüne "Ses Ayarları" butonu ekleyin.
3. "Ses Ayarları" butonunun OnClick olayına PauseMenu.OpenSoundSettings metodunu ekleyin.

## 5. Kullanım

- Ana ses seviyesi, tüm ses seviyelerini etkiler (master volume).
- Müzik ve SFX ses seviyeleri, kendi kategorilerindeki tüm sesleri etkiler.
- Özel ses seviyeleri (patlama, roket, kamikaze, buton, düşük can), belirli ses efektlerinin ses seviyelerini ayarlar.
- Slider'ları hareket ettirdiğinizde ilgili ses çalınarak test edebilirsiniz.
- Ayarlar PlayerPrefs'e kaydedilir ve oyun yeniden başlatıldığında korunur.
- Düşük can sesi, oyuncunun sağlığı lowHealthThreshold değerinin altına düştüğünde otomatik olarak çalar.

## 6. Programlama API'si

Ses seviyelerini kod üzerinden ayarlamak için AudioManager'ın şu metodlarını kullanabilirsiniz:

```csharp
// Audio Manager'a erişim
AudioManager audioManager = AudioManager.Instance;

// Ses seviyelerini ayarlama
audioManager.SetMasterVolume(1.0f);  // 0.0f - 1.0f arası değer
audioManager.SetMusicVolume(0.5f);
audioManager.SetSFXVolume(0.5f);
audioManager.SetExplosionVolume(0.7f);
audioManager.SetRocketVolume(0.6f);
audioManager.SetKamikazeVolume(0.8f);
audioManager.SetButtonVolume(0.5f);
audioManager.SetLowHealthVolume(0.5f);

// Sesleri çalma
audioManager.PlayExplosionSound();
audioManager.PlayRocketSound();
audioManager.PlayKamikazeSound();
audioManager.PlayButtonClick();
audioManager.PlayLowHealthSound();
audioManager.StopLowHealthSound();

// Düşük can sesi eşiğini ayarlama
audioManager.lowHealthThreshold = 0.3f; // %30 sağlık altında çalmaya başlar
```

## 7. Sorun Giderme

- Ses çalmıyorsa AudioManager'ın sahnede olduğundan emin olun.
- AudioManager prefabı üzerinde müzik ve ses efekti AudioSource bileşenlerinin atanmış olduğunu kontrol edin.
- Ses dosyalarının doğru konumlarda (Resources/Sounds/ klasöründe) olduğunu kontrol edin.
- Düşük can sesi çalmıyorsa, lowHealthThreshold değerini kontrol edin ve daha yüksek bir değere (örn. 0.5 = %50) ayarlayarak test edin.
- PlayerPrefs.DeleteAll() çağrısı ile tüm kayıtlı ayarları sıfırlayabilirsiniz. 