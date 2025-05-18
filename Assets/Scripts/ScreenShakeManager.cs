using System.Collections;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance { get; private set; }

    [Header("Titreşim Ayarları")]
    [Tooltip("Maksimum titreşim şiddeti")]
    public float shakeIntensity = 0.05f;
    
    [Tooltip("Titreşim sönümlenme hızı")]
    public float shakeDampingSpeed = 5f;
    
    [Tooltip("Titreşim süresini çarpan (1.0 = normal süre)")]
    public float shakeDurationMultiplier = 1.0f;

    // Kamera transform referansı
    private Transform cameraTransform;
    private Vector3 originalPosition;
    
    // Mevcut titreşim değerleri
    private float currentShakeAmount = 0f;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ana kamerayı referans al
        cameraTransform = Camera.main.transform;
        originalPosition = cameraTransform.localPosition;
    }

    private void LateUpdate()
    {
        // Eğer titreşim miktarı varsa, kamerayı titret
        if (currentShakeAmount > 0)
        {
            // Rastgele bir yön hesapla
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeAmount;
            
            // Z koordinatını koruyalım (2D oyunlar için)
            shakeOffset.z = 0;
            
            // Kamera pozisyonunu orijinal pozisyona titreşim ekleyerek ayarla
            cameraTransform.localPosition = originalPosition + shakeOffset;
            
            // Titreşimi zamanla azalt
            currentShakeAmount = Mathf.Lerp(currentShakeAmount, 0, Time.deltaTime * shakeDampingSpeed);
            
            // Çok küçük bir değere ulaştığında sıfırla
            if (currentShakeAmount < 0.01f)
            {
                currentShakeAmount = 0;
                cameraTransform.localPosition = originalPosition;
            }
        }
    }

    /// <summary>
    /// Ekrana tek seferlik titreşim uygular
    /// </summary>
    /// <param name="intensity">Titreşim şiddeti (0-1 arası, 1 = maksimum), null ise varsayılan değer kullanılır</param>
    /// <param name="duration">Titreşim süresi (saniye), null ise varsayılan değer kullanılır</param>
    public void ShakeOnce(float? intensity = null, float duration = 0.2f)
    {
        // Titreşim şiddetini azaltmak için 0.5 ile çarpalım (daha hafif titreşim)
        float shakeAmount = (intensity ?? shakeIntensity) * 0.5f;
        
        // Önceki titreşimi durdur
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        // Yeni titreşim başlat
        shakeCoroutine = StartCoroutine(ShakeRoutine(shakeAmount, duration * shakeDurationMultiplier));
    }

    /// <summary>
    /// Sürekli titreşimi başlatır/ayarlar (örneğin minigun ateş ederken)
    /// </summary>
    /// <param name="intensity">Titreşim şiddeti (0-1 arası)</param>
    public void StartContinuousShake(float intensity)
    {
        // Titreşim şiddetini azalt ve mevcut titreşim miktarını ayarla
        float reducedIntensity = intensity * shakeIntensity;
        
        // Şiddet daha yüksekse güncelle
        if (reducedIntensity > currentShakeAmount)
        {
            currentShakeAmount = reducedIntensity;
        }
    }

    /// <summary>
    /// Sürekli titreşimi durdurur
    /// </summary>
    public void StopShake()
    {
        currentShakeAmount = 0;
        
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalPosition;
        }
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Zaman geçtikçe azalan bir titreşim şiddeti hesapla
            float currentIntensity = Mathf.Lerp(intensity, 0, elapsed / duration);
            currentShakeAmount = currentIntensity;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentShakeAmount = 0;
        cameraTransform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
} 