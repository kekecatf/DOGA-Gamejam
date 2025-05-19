using UnityEngine;

public class CameraZoomAndFollow : MonoBehaviour
{
    public Transform player;
    public Transform zeplin; // Zeplin referansı ekledik
    public Vector3 startOffset = new Vector3(0, 10, -20); // Başlangıçta uzak pozisyon
    public Vector3 followOffset = new Vector3(0, 2, -10); // Takip pozisyonu
    public float zoomDuration = 3.5f;

    private float timer = 0f;
    private bool zooming = true;
    private Transform currentTarget; // Şu anki takip hedefi

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // İlk hedefi player olarak ayarla
        currentTarget = player;
        
        // Player yoksa Zeplin'i bulmayı dene
        if (currentTarget == null)
        {
            FindTargets();
        }
        
        // Hedeflerden biri varsa pozisyonu ayarla
        if (currentTarget != null)
        {
            transform.position = currentTarget.position + startOffset;
        }
        else
        {
            // Hedef yoksa başlangıç pozisyonunu koru
            zooming = false;
            Debug.LogWarning("CameraZoomAndFollow: Hiçbir hedef bulunamadı!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Hedefi kontrol et, kaybolmuşsa alternatif bul
        if (currentTarget == null)
        {
            FindTargets();
            
            // Hala hedef yoksa işlem yapma
            if (currentTarget == null)
            {
                return;
            }
        }
        
        // Player öldüyse Zeplin'e geç
        if (currentTarget == player && Player.isDead)
        {
            if (zeplin != null)
            {
                currentTarget = zeplin;
                Debug.Log("Kamera: Player öldü, hedef Zeplin'e değiştirildi.");
            }
        }
        
        if (zooming && currentTarget != null)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(currentTarget.position + startOffset, currentTarget.position + followOffset, timer / zoomDuration);
            if (timer >= zoomDuration)
            {
                zooming = false;
            }
        }
        else if (currentTarget != null)
        {
            // Normal takip
            transform.position = currentTarget.position + followOffset;
        }
    }
    
    // Hedefleri bul
    private void FindTargets()
    {
        // Player referansı yoksa veya yok edildiyse, yeniden bulmayı dene
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Kamera: Player referansı yeniden bulundu.");
            }
        }
        
        // Zeplin referansı yoksa veya yok edildiyse, yeniden bulmayı dene
        if (zeplin == null)
        {
            GameObject zeplinObj = GameObject.FindGameObjectWithTag("Zeplin");
            if (zeplinObj != null)
            {
                zeplin = zeplinObj.transform;
                Debug.Log("Kamera: Zeplin referansı yeniden bulundu.");
            }
        }
        
        // Player'ı öncelikli hedef olarak ayarla, yoksa Zeplin'i kullan
        if (player != null && !Player.isDead)
        {
            currentTarget = player;
        }
        else if (zeplin != null)
        {
            currentTarget = zeplin;
        }
        else
        {
            currentTarget = null;
            Debug.LogWarning("Kamera: Takip edilecek hedef bulunamadı!");
        }
    }
}
