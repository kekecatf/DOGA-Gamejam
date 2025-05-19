using UnityEngine;

public class HealthBarConnector : MonoBehaviour
{
    [Tooltip("Sağlık barının dolu kısmını gösteren Fill nesnesi")]
    public RectTransform fillRect;
    
    [Tooltip("Otomatik olarak Zeplin'i bul")]
    public bool autoFindZeplin = true;
    
    [Tooltip("Eğer otomatik bulmak istemiyorsanız, Zeplin'i manuel olarak atayın")]
    public Zeplin zeplinReference;
    
    void Start()
    {
        // Fill nesnesi kontrolü
        if (fillRect == null)
        {
            // Fill nesnesini otomatik bulmaya çalış
            Transform fillTransform = transform.Find("Fill");
            if (fillTransform != null)
            {
                fillRect = fillTransform.GetComponent<RectTransform>();
                Debug.Log("Fill nesnesi otomatik olarak bulundu.");
            }
            else
            {
                Debug.LogError("Fill nesnesi bulunamadı ve atanmadı! Sağlık barı çalışmayacak.");
                return;
            }
        }
        
        // Zeplin'i bul veya referansı kullan
        Zeplin zeplin = null;
        
        if (autoFindZeplin)
        {
            zeplin = FindObjectOfType<Zeplin>();
            if (zeplin == null)
            {
                Debug.LogError("Zeplin objesi sahnede bulunamadı!");
                return;
            }
            Debug.Log("Zeplin objesi otomatik olarak bulundu.");
        }
        else if (zeplinReference != null)
        {
            zeplin = zeplinReference;
            Debug.Log("Manuel olarak atanmış Zeplin referansı kullanılıyor.");
        }
        else
        {
            Debug.LogError("Zeplin referansı atanmadı ve otomatik arama kapalı!");
            return;
        }
        
        // Zeplin'e sağlık barı referansını ata
        zeplin.fillRectTransform = fillRect;
        Debug.Log("Fill nesnesi Zeplin'e başarıyla bağlandı.");
        
        // UI'ı hemen güncelle
        if (zeplin.enabled)
        {
            // UpdateUI metodunu doğrudan çağıramayız çünkü private
            // Bu durumda PlayerData sağlık değerini alarak sağlık barını kendimiz güncelleyelim
            PlayerData playerData = FindObjectOfType<PlayerData>();
            if (playerData != null)
            {
                float healthPercent = (float)playerData.zeplinSaglik / 1000; // 1000 maxHealth değeri
                fillRect.localScale = new Vector3(healthPercent, 1, 1);
                Debug.Log($"Sağlık barı manuel olarak güncellendi. Sağlık: {playerData.zeplinSaglik}/1000, Oran: {healthPercent:F2}");
            }
        }
    }
} 