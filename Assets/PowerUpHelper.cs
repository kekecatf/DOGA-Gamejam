/*
* PowerUp Ekleme Sistemi
* =====================
* 
* Bu sistem, düşman öldürüldüğünde %20 şansla güçlendirme
* (power-up) öğelerinin düşmesini sağlar.
* 
* Nasıl Kurulur?
* -------------
* 1. PowerUpCreator scriptini bir GameManager veya boş bir objeye ekleyin
* 2. Unity Editöründe, PowerUpCreator bileşenine sağ tıklayın ve 
*    "Create ItemPrefab" seçeneğine tıklayın
* 3. Eğer hiçbir sprite atanmamışsa, PowerUpCreator'a sprite ekleyin
*    (varsayılan olarak beyaz kare görünecektir)
* 4. İsterseniz oluşturulan prefabı Resources/ItemPrefab.prefab adresinden
*    düzenleyebilirsiniz
* 
* NOT: Bu sistemin çalışması için Resources klasöründe "ItemPrefab" adında
* bir prefabın olması gerekiyor. PowerUpCreator, bu prefabı otomatik olarak
* oluşturmanıza yardımcı olur.
*/

// Bu dosya sadece bilgi içindir ve herhangi bir kod içermez.
// Asıl işlevsellik PowerUpCreator ve ItemPowerUp scriptlerinde bulunur. 