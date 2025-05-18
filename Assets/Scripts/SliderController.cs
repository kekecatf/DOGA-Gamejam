using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private Slider[] gameSliders; // Tüm slider'ları içeren dizi
    
    private void Start()
    {
        // Başlangıçta tüm slider'ları etkileşimsiz yap
        DisableSliderInteraction();
    }
    
    // Tüm slider'ları etkileşimsiz yapan metod
    public void DisableSliderInteraction()
    {
        if (gameSliders == null || gameSliders.Length == 0)
        {
            // Eğer slider dizisi atanmamışsa, tüm slider'ları otomatik bul
            gameSliders = FindObjectsOfType<Slider>();
        }
        
        foreach (Slider slider in gameSliders)
        {
            // Slider görünümünü koruyup etkileşimi engelle
            
            // 1. Yöntem: Interactable özelliğini kapat
            // slider.interactable = false;
            
            // 2. Yöntem: Raycast hedefini devre dışı bırak (görünüm değişmez)
            Image[] sliderImages = slider.GetComponentsInChildren<Image>();
            foreach (Image img in sliderImages)
            {
                img.raycastTarget = false;
            }
            
            // Handle (tutamaç) etkileşimini de engelle
            if (slider.handleRect != null)
            {
                Image handleImage = slider.handleRect.GetComponent<Image>();
                if (handleImage != null)
                {
                    handleImage.raycastTarget = false;
                }
            }
            
            Debug.Log($"Slider '{slider.name}' etkileşimsiz hale getirildi.");
        }
    }
    
    // İhtiyaç halinde etkileşimi tekrar aktif etmek için
    public void EnableSliderInteraction()
    {
        if (gameSliders == null || gameSliders.Length == 0) return;
        
        foreach (Slider slider in gameSliders)
        {
            // 1. Yöntem: Interactable özelliğini aç
            // slider.interactable = true;
            
            // 2. Yöntem: Raycast hedefini aktif et
            Image[] sliderImages = slider.GetComponentsInChildren<Image>();
            foreach (Image img in sliderImages)
            {
                img.raycastTarget = true;
            }
            
            // Handle (tutamaç) etkileşimini de aktif et
            if (slider.handleRect != null)
            {
                Image handleImage = slider.handleRect.GetComponent<Image>();
                if (handleImage != null)
                {
                    handleImage.raycastTarget = true;
                }
            }
        }
    }
} 