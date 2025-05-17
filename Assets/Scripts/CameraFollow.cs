using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // Player transform
    public float smoothSpeed = 0.125f;  // Takip yumuşaklığı
    public Vector3 offset = new Vector3(0, 0, -10); // Kamera offset (z eksenindeki -10 önemli)
    
    private void LateUpdate()
    {
        if (target == null)
        {
            // Eğer hedef atanmamışsa, oyuncuyu bulmaya çalış
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
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