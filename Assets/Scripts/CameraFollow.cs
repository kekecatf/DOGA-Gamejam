using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // Player transform
    public Transform target2;           // Zeplin transform
    public float smoothSpeed = 0.125f;  // Takip yumuşaklığı
    public Vector3 offset = new Vector3(0, 0, -10); // Kamera offset (z eksenindeki -10 önemli)
    
    [Header("Target Değişim Ayarları")]
    public float targetTransitionSpeed = 0.05f;   // Hedef geçiş hızı (düşük = yavaş, yüksek = hızlı)
    public float delayBeforeTransition = 0.5f;    // Geçiş başlamadan önceki gecikme (saniye)
    
    private Transform originalTarget;     // Orijinal target referansını saklamak için
    private Vector3 transitionPosition;   // Geçiş pozisyonu
    private bool isTransitioning = false; // Hedefler arası geçiş yapılıyor mu?
    private float transitionTimer = 0f;   // Geçiş zamanlayıcısı
    private float transitionProgress = 0f;// Geçiş ilerleme oranı (0-1 arası)
    private bool wasPlayerDead = false;   // Önceki karede player ölü müydü?
    private Transform sourceTarget;       // Geçişin başlangıç hedefi
    private Transform destinationTarget;  // Geçişin hedef noktası
    
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
    
    private void Update()
    {
        // Player'ın ölme durumunun değiştiğini kontrol et
        if (Player.isDead && !wasPlayerDead)
        {
            // Player yeni öldüyse
            wasPlayerDead = true;
            
            if (target2 != null)
            {
                // Geçiş kaynağı ve hedefi ayarla
                sourceTarget = originalTarget;
                destinationTarget = target2;
                
                // Geçiş modunu başlat
                isTransitioning = true;
                transitionTimer = 0f;
                transitionProgress = 0f;
                Debug.Log("Player öldü, kamera Zeplin'e geçiş başlıyor (gecikme: " + delayBeforeTransition + "s)");
            }
        }
        else if (!Player.isDead && wasPlayerDead)
        {
            // Player canlandıysa (test için)
            wasPlayerDead = false;
            
            if (originalTarget != null)
            {
                // Geçiş kaynağı ve hedefi ayarla
                sourceTarget = target2;
                destinationTarget = originalTarget;
                
                // Geçiş modunu başlat
                isTransitioning = true;
                transitionTimer = 0f;
                transitionProgress = 0f;
                Debug.Log("Player canlandı, kamera Player'a geçiş başlıyor");
            }
        }
        
        // Geçiş işlemini yönet
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            
            // Gecikme zamanı dolduysa geçişi başlat
            if (transitionTimer >= delayBeforeTransition)
            {
                // Geçiş hızına göre ilerleme değerini güncelle
                transitionProgress += targetTransitionSpeed * Time.deltaTime;
                
                // İlerleme değerini 0-1 aralığında tut
                transitionProgress = Mathf.Clamp01(transitionProgress);
                
                // Geçiş tamamlandıysa
                if (transitionProgress >= 1.0f)
                {
                    isTransitioning = false;
                    
                    // Eğer Player ölüyse ve Zeplin'e geçtiyse,
                    // bundan sonra kameranın takip mantığını değiştir 
                    if (Player.isDead && destinationTarget == target2)
                    {
                        // Artık Player'ı takip etmeyi tamamen bırak
                        target = target2;
                    }
                    Debug.Log("Kamera hedef geçişi tamamlandı");
                }
            }
        }
    }
    
    private void LateUpdate()
    {
        // Hedef kontrol
        Transform targetToUse = DetermineTargetToUse();
        Vector3 targetPosition = GetTargetPosition(targetToUse);
        
        if (targetToUse == null)
        {
            return; // Geçerli hedef yoksa işlemi sonlandır
        }
        
        // Hedef pozisyonu hesapla
        Vector3 desiredPosition = targetPosition + offset;
        
        // Pozisyona yumuşak geçiş uygula
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Kamera pozisyonunu güncelle, rotasyonu değiştirme
        transform.position = smoothedPosition;
        
        // Kameranın rotasyonunu sabit tut (varsayılan olarak (0,0,0))
        transform.rotation = Quaternion.identity;
    }
    
    // Takip edilecek pozisyonu hesapla - normal durumda veya geçiş sırasında
    private Vector3 GetTargetPosition(Transform targetToUse)
    {
        // Geçiş yapılıyorsa kaynaktan hedefe interpole edilmiş pozisyonu hesapla
        if (isTransitioning && transitionTimer >= delayBeforeTransition && sourceTarget != null && destinationTarget != null)
        {
            // Yumuşak geçiş için SmoothStep kullan
            float t = Mathf.SmoothStep(0, 1, transitionProgress);
            return Vector3.Lerp(sourceTarget.position, destinationTarget.position, t);
        }
        
        // Geçiş yapmıyorsa doğrudan hedef pozisyonunu kullan
        return targetToUse.position;
    }
    
    // Kullanılacak hedefi belirle
    private Transform DetermineTargetToUse()
    {
        // Geçiş yapılıyorsa destinationTarget'ı kullan
        if (isTransitioning && transitionProgress >= 1.0f)
        {
            return destinationTarget;
        }
        
        // Geçiş yoksa normal takip mantığını kullan
        
        // Eğer hedefler yoksa Player'ı bulmaya çalış (sadece başlangıçta)
        if (originalTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                originalTarget = player.transform;
                return originalTarget;
            }
        }
        
        // Player'ın ölü olup olmadığını kontrol et
        if (Player.isDead)
        {
            // Player ölüyse ve Zeplin varsa Zeplin'i takip et
            return (target2 != null) ? target2 : target;
        }
        else 
        {
            // Player yaşıyorsa onu takip et
            return (originalTarget != null) ? originalTarget : target;
        }
    }
} 