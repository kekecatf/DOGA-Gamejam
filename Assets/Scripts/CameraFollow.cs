using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // Player transform
    public Transform target2;           // Zeplin transform
    public float smoothSpeed = 0.125f;  // Takip yumuşaklığı
    public Vector3 offset = new Vector3(0, 0, -10); // Kamera offset (z eksenindeki -10 önemli)
    
    private Transform originalTarget;   // Orijinal target referansını saklamak için
    
    private void Start()
    {
        // Orijinal target'ı kaydet (Player)
        originalTarget = target;
        
        // Başlangıçta target2 atanmamışsa, zeplini bulup ata
        if (target2 == null)
        {
            // Zeplin'i bulmaya çalış
            GameObject zeplin = GameObject.FindWithTag("Zeplin");
            if (zeplin == null)
            {
                // Tag ile bulunamazsa, doğrudan sınıf ile aramayı dene
                Zeplin zeplinComponent = FindObjectOfType<Zeplin>();
                if (zeplinComponent != null)
                {
                    target2 = zeplinComponent.transform;
                    Debug.Log("Zeplin bulundu ve Target2'ye atandı!");
                }
                else
                {
                    Debug.LogWarning("Zeplin bulunamadı! Target2'yi Inspector'dan atayın.");
                }
            }
            else
            {
                target2 = zeplin.transform;
                Debug.Log("Zeplin tag ile bulundu ve Target2'ye atandı!");
            }
        }
    }
    
    private void LateUpdate()
    {
        // Player'ın yaşayıp yaşamadığına göre hedefi belirle
        if (Player.isDead && target2 != null)
        {
            // Player öldüyse ve Target2 (Zeplin) atanmışsa, onu kullan
            target = target2;
        }
        else if (!Player.isDead && originalTarget != null)
        {
            // Player hayattaysa orijinal target'a (Player) geri dön
            target = originalTarget;
        }
        
        if (target == null)
        {
            // Eğer hedef atanmamışsa, oyuncuyu bulmaya çalış
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                // Orijinal target'ı da güncelle
                originalTarget = target;
            }
            else
            {
                return; // Player bulunamadıysa çık
            }
        }
        
        // Hedef pozisyonu hesapla (rotasyonu yok sayarak)
        Vector3 desiredPosition = target.position + offset;
        
        // Pozisyona yumuşak geçiş uygula
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Kamera pozisyonunu güncelle, rotasyonu değiştirme
        transform.position = smoothedPosition;
        
        // Kameranın rotasyonunu sabit tut (varsayılan olarak (0,0,0))
        transform.rotation = Quaternion.identity;
    }
} 