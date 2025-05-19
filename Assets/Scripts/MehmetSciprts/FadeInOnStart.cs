using UnityEngine;

public class FadeInOnStart : MonoBehaviour
{
    private Animator animator;
    
    [Header("Tamamlanma Ayarları")]
    public bool disableAfterAnimation = false;  // Animasyon sonrası panel devre dışı kalsın mı?
    public float checkDelay = 0.1f;             // Kontrol etme gecikmesi
    private float animationDuration;             // Animasyon süresi
    private bool isCheckingCompletion = false;   // Tamamlanma kontrolü yapılıyor mu?
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("FadeIn"); // Animator'da bir "FadeIn" trigger'ı varsa
            
            // Animasyon tamamlandığında devre dışı bırakılacaksa kontrol başlat
            if (disableAfterAnimation)
            {
                // Animasyon bilgisini al
                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    // Animasyon süresini al 
                    animationDuration = clipInfo[0].clip.length;
                    
                    // Animasyon süresinden biraz fazla bekleyelim
                    Invoke("DisableGameObject", animationDuration + checkDelay);
                    isCheckingCompletion = true;
                    
                    Debug.Log($"Panel '{gameObject.name}' animasyon süresi: {animationDuration} saniye. {animationDuration + checkDelay} saniye sonra devre dışı bırakılacak.");
                }
                else
                {
                    Debug.LogWarning($"Panel '{gameObject.name}' için animasyon süresi belirlenemedi!");
                }
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // Eğer animasyon tamamlanma kontrolü yapılıyorsa
        if (isCheckingCompletion && animator != null)
        {
            // Mevcut animasyon durumunu kontrol et
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Animasyon tamamlandıysa (normalizedTime >= 1)
            if (stateInfo.normalizedTime >= 1.0f)
            {
                // İşlemi hemen gerçekleştir ve zamanlanmış çağrıyı iptal et
                CancelInvoke("DisableGameObject");
                DisableGameObject();
                
                isCheckingCompletion = false;
            }
        }
    }
    
    // GameObject'i devre dışı bırak
    void DisableGameObject()
    {
        Debug.Log($"Panel '{gameObject.name}' animasyon sonrası devre dışı bırakılıyor.");
        gameObject.SetActive(false);
    }
    
    // Dışarıdan çağrılabilecek metod (gerekirse)
    public void TriggerDisable()
    {
        DisableGameObject();
    }
}
