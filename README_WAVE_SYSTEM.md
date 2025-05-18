# Wave System Kurulum Talimatları

Wave sistemi başarıyla eklendi ve aşağıdaki özellikler bulunmaktadır:

1. Belirli sayıda wave (default: 5) - `EnemySpawner` bileşeninde ayarlanabilir
2. Her wave'de düşman sayısının artması - temel düşman sayısı ve artış oranı ayarlanabilir
3. Wave tamamlanma ve yeni wave başlama mesajları
4. Tüm wave'ler tamamlandığında oyun sonu mesajı

## UI Kurulumu

Wave bilgilerini görüntülemek için aşağıdaki adımları izleyin:

1. GameScene sahnesini açın
2. Hiyerarşide: Sağ tıklayın -> UI -> Canvas seçin
3. Canvas içine: Sağ tıklayın -> UI -> Text - TextMeshPro seçin (TextMeshPro paketi yoksa import edin)
4. Text - TextMeshPro nesnesinin adını "WaveInfoText" olarak değiştirin
5. İstediğiniz konuma yerleştirin (örneğin ekranın üst kısmı)
6. TextMeshPro bileşeninin özelliklerini ayarlayın:
   - Font Size: 24
   - Alignment: Center
   - Color: Beyaz (veya istediğiniz renk)
   - Outline: Ekleyebilirsiniz (görünürlük için)
7. EnemySpawner nesnesini seçin ve Inspector'da `Wave Info Text` alanına oluşturduğunuz "WaveInfoText" nesnesini sürükleyin

## Wave Sistemi Ayarları

`EnemySpawner` bileşenindeki ayarları istediğiniz gibi düzenleyebilirsiniz:

- `Max Waves`: Toplam wave sayısı
- `Base Enemies Per Wave`: İlk wave'deki düşman sayısı
- `Enemy Increase Per Wave`: Her wave'de artacak düşman sayısı
- `Wave Break Duration`: Wave'ler arası bekleme süresi (saniye)
- `Enemy Spawn Interval`: Wave içinde düşmanlar arası spawn aralığı (saniye)

## Test Etme

1. "Play" butonuna tıklayarak oyunu başlatın
2. Wave 1 başlayacak ve Wave bilgileri ekranda görünecek
3. Tüm düşmanları yok ettiğinizde wave tamamlanacak ve kısa bir süre sonra yeni wave başlayacak
4. Son wave'i de tamamladığınızda "Tüm dalgalar tamamlandı! Tebrikler!" mesajı görünecek

Not: UI stilini ve görünümünü kendi oyununuza göre özelleştirebilirsiniz. 