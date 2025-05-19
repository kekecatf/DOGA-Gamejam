using UnityEngine;

[ExecuteInEditMode]
public class CameraTargetSetup : MonoBehaviour
{
    void Start()
    {
        SetupCameraTargets();
    }

    // Bu metodu Awake'te de çağırıyoruz ki sahne yüklendiğinde hemen çalışsın
    void Awake()
    {
        SetupCameraTargets();
    }

    // Oyun sırasında bir kez daha deneyelim, çünkü nesneler geç yüklenebilir
    void OnEnable()
    {
        Invoke("SetupCameraTargets", 0.5f);
    }

    // Kamera hedeflerini ayarla
    public void SetupCameraTargets()
    {
        Debug.Log("CameraTargetSetup: Kamera hedefleri ayarlanıyor...");
        
        // Kamera bileşenini bul
        CameraZoomAndFollow cameraFollow = GetComponent<CameraZoomAndFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = gameObject.AddComponent<CameraZoomAndFollow>();
            Debug.Log("CameraTargetSetup: Kamera bileşeni eklendi.");
        }
        
        // Player objesini bul
        if (cameraFollow.player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                cameraFollow.player = playerObj.transform;
                Debug.Log("CameraTargetSetup: Player hedefi ayarlandı: " + playerObj.name);
            }
            else
            {
                Debug.LogWarning("CameraTargetSetup: 'Player' etiketli obje bulunamadı!");
            }
        }
        
        // Zeplin objesini bul
        if (cameraFollow.zeplin == null)
        {
            GameObject zeplinObj = GameObject.FindGameObjectWithTag("Zeplin");
            if (zeplinObj != null)
            {
                cameraFollow.zeplin = zeplinObj.transform;
                Debug.Log("CameraTargetSetup: Zeplin hedefi ayarlandı: " + zeplinObj.name);
            }
            else
            {
                Debug.LogWarning("CameraTargetSetup: 'Zeplin' etiketli obje bulunamadı!");
            }
        }
    }
} 