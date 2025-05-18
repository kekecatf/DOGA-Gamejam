using UnityEngine;
using System.Collections;

public class DeathEffect : MonoBehaviour
{
    public float lifetime = 1.0f; // Efektin yaşam süresi (saniye)
    public float expandSpeed = 1.5f; // Genişleme hızı
    public bool rotateEffect = true; // Efektin dönmesi
    public float rotateSpeed = 360f; // Saniyede kaç derece dönsün
    public bool fadeOut = true; // Solma efekti
    
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // Sprite renderer yoksa, çocuk objelerden bulalım
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Animasyonu başlat
        StartCoroutine(AnimateEffect());
    }
    
    IEnumerator AnimateEffect()
    {
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;
        Color originalColor = Color.white;
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Animasyon döngüsü
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            
            // Genişleme efekti
            transform.localScale = originalScale * (1 + expandSpeed * t);
            
            // Dönme efekti
            if (rotateEffect)
            {
                transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
            }
            
            // Solma efekti
            if (fadeOut && spriteRenderer != null)
            {
                spriteRenderer.color = new Color(
                    originalColor.r,
                    originalColor.g,
                    originalColor.b,
                    Mathf.Lerp(originalColor.a, 0f, t)
                );
            }
            
            yield return null;
        }
        
        // Animasyon bitiminde yok et
        Destroy(gameObject);
    }
} 