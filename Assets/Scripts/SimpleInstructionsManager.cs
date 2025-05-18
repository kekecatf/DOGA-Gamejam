using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SimpleInstructionsManager : MonoBehaviour
{
    [Header("Instrüksiyon Ayarları")]
    public float displayDuration = 5f;  // Yazıların ekranda kalma süresi
    
    [Tooltip("Eğer buraya manuel atama yapamıyorsanız, script otomatik olarak bulacaktır.")]
    public CanvasGroup instructionsCanvasGroup; // Fade efekti için CanvasGroup
    
    [Header("Kontrol Yazıları")]
    [Tooltip("Eğer metin referansları atanmamışsa, script otomatik olarak sahneden bulacaktır.")]
    public TextMeshProUGUI[] instructionTexts; // Tüm kontrol yazıları
    
    [Header("Fade Ayarları")]
    public float fadeOutDuration = 1.0f; // Kaybolma animasyonu süresi
    
    private void Awake()
    {
        // CanvasGroup'u otomatik bul (atanmamışsa)
        if (instructionsCanvasGroup == null)
        {
            // Önce aynı GameObject'te ara
            instructionsCanvasGroup = GetComponent<CanvasGroup>();
            
            // Yoksa "InstructionsCanvas" adlı objede ara
            if (instructionsCanvasGroup == null)
            {
                GameObject canvasObj = GameObject.Find("InstructionsCanvas");
                if (canvasObj != null)
                {
                    instructionsCanvasGroup = canvasObj.GetComponent<CanvasGroup>();
                    
                    // Eğer CanvasGroup yoksa, ekle
                    if (instructionsCanvasGroup == null)
                    {
                        instructionsCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
                        Debug.Log("InstructionsCanvas'a otomatik olarak CanvasGroup eklendi.");
                    }
                }
                else
                {
                    // Canvas bulunamadıysa, yeni bir tane oluştur
                    canvasObj = new GameObject("InstructionsCanvas");
                    Canvas canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = Camera.main;
                    
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                    instructionsCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
                    
                    Debug.Log("InstructionsCanvas otomatik olarak oluşturuldu.");
                }
            }
            
            Debug.Log("CanvasGroup otomatik olarak bulundu/oluşturuldu: " + 
                     (instructionsCanvasGroup != null ? instructionsCanvasGroup.name : "null"));
        }
    }
    
    private void Start()
    {
        // Metin öğelerini otomatik bul (atanmamışsa)
        if (instructionTexts == null || instructionTexts.Length == 0)
        {
            AutoFindTextElements();
        }
        
        // Başlangıçta Canvas Group görünür olsun
        if (instructionsCanvasGroup != null)
        {
            instructionsCanvasGroup.alpha = 1f;
            
            // Belirli süre sonra fade-out animasyonu başlat
            StartCoroutine(HideInstructionsAfterDelay());
        }
        else
        {
            Debug.LogError("Instrüksiyon Canvas Group bulunamadı veya oluşturulamadı!");
        }
        
        // Yazıları ayarla (eğer manuel atanmamışsa)
        SetDefaultInstructions();
    }
    
    // Metin öğelerini otomatik olarak bul
    private void AutoFindTextElements()
    {
        if (instructionsCanvasGroup != null)
        {
            // Canvas altındaki tüm Text ve TextMeshProUGUI öğelerini bul
            TextMeshProUGUI[] tmpTexts = instructionsCanvasGroup.GetComponentsInChildren<TextMeshProUGUI>(true);
            Text[] standardTexts = instructionsCanvasGroup.GetComponentsInChildren<Text>(true);
            
            // TextMeshPro öğeleri varsa onları kullan
            if (tmpTexts != null && tmpTexts.Length > 0)
            {
                instructionTexts = tmpTexts;
                Debug.Log("Sahneden " + tmpTexts.Length + " adet TextMeshProUGUI öğesi bulundu.");
            }
            // Yoksa standart Text öğelerini kullan
            else if (standardTexts != null && standardTexts.Length > 0)
            {
                // Text dizisini TextMeshProUGUI dizisine dönüştür (null olarak)
                instructionTexts = new TextMeshProUGUI[0];
                Debug.Log("Sahneden " + standardTexts.Length + " adet standart Text öğesi bulundu, ancak bu script TextMeshProUGUI kullanıyor.");
            }
            else
            {
                Debug.LogWarning("Sahnede hiç metin öğesi bulunamadı! Basit metin öğeleri otomatik oluşturuluyor...");
                CreateDefaultTextElements();
            }
        }
    }
    
    // Basit metin öğeleri oluştur
    private void CreateDefaultTextElements()
    {
        if (instructionsCanvasGroup == null) return;
        
        string[] defaultTexts = new string[]
        {
            "Z Tuşu / Sol Buton: Ateş etmek için kullanılır (Minigun)",
            "X Tuşu / Sağ Buton: Roket fırlatmak için kullanılır",
            "Yön Tuşları: Gemiyi hareket ettirmek için kullanılır",
            "WASD Tuşları: Gemiyi hareket ettirmek için alternatif tuşlar"
        };
        
        // Metin nesnesi için yeni GameObject'ler oluştur
        instructionTexts = new TextMeshProUGUI[defaultTexts.Length];
        
        for (int i = 0; i < defaultTexts.Length; i++)
        {
            GameObject textObj = new GameObject("InstructionText" + i);
            textObj.transform.SetParent(instructionsCanvasGroup.transform, false);
            
            // TextMeshProUGUI bileşeni var mı kontrol et (gerekli paket yüklü olabilir veya olmayabilir)
            TextMeshProUGUI tmpText = null;
            
            try
            {
                tmpText = textObj.AddComponent<TextMeshProUGUI>();
            }
            catch (System.Exception)
            {
                // TextMeshPro paketi yüklü değilse standart Text kullan
                Debug.LogWarning("TextMeshPro bulunamadı, bunun yerine standart Text kullanılıyor.");
                Destroy(textObj);
                
                textObj = new GameObject("InstructionText" + i);
                textObj.transform.SetParent(instructionsCanvasGroup.transform, false);
                Text text = textObj.AddComponent<Text>();
                text.text = defaultTexts[i];
                text.fontSize = 24;
                text.color = Color.white;
                
                // Metin pozisyonu ayarla
                RectTransform rect = text.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(20, -50 - (i * 40));
                rect.sizeDelta = new Vector2(400, 35);
                
                continue;
            }
            
            // TextMeshProUGUI özelliklerini ayarla
            if (tmpText != null)
            {
                tmpText.text = defaultTexts[i];
                tmpText.fontSize = 24;
                tmpText.color = Color.white;
                
                // Metin pozisyonu ayarla - SOL ÜST KÖŞEYE TAŞINDI
                RectTransform rect = tmpText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(20, -50 - (i * 40));
                rect.sizeDelta = new Vector2(400, 35);
                
                instructionTexts[i] = tmpText;
            }
        }
        
        Debug.Log("Basit metin öğeleri otomatik olarak oluşturuldu.");
    }
    
    // Varsayılan açıklamaları ayarla
    private void SetDefaultInstructions()
    {
        if (instructionTexts == null || instructionTexts.Length == 0)
        {
            Debug.LogWarning("Instrüksiyon textleri atanmamış ve otomatik bulunamadı!");
            return;
        }
        
        string[] defaultTexts = new string[]
        {
            "Z Tuşu / Sol Buton: Ateş etmek için kullanılır (Minigun)",
            "X Tuşu / Sağ Buton: Roket fırlatmak için kullanılır",
            "Yön Tuşları: Gemiyi hareket ettirmek için kullanılır",
            "WASD Tuşları: Gemiyi hareket ettirmek için alternatif tuşlar"
        };
        
        // Atanan text sayısı kadar varsayılan değer ata
        for (int i = 0; i < Mathf.Min(instructionTexts.Length, defaultTexts.Length); i++)
        {
            if (instructionTexts[i] != null && string.IsNullOrEmpty(instructionTexts[i].text))
            {
                instructionTexts[i].text = defaultTexts[i];
            }
        }
    }
    
    // Belirli süre sonra yazıları gizle
    private IEnumerator HideInstructionsAfterDelay()
    {
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out animasyonu
        float startTime = Time.time;
        float endTime = startTime + fadeOutDuration;
        
        while (Time.time < endTime)
        {
            float elapsedTime = Time.time - startTime;
            float normalizedTime = elapsedTime / fadeOutDuration;
            
            // Alpha değerini güncelle (1'den 0'a)
            instructionsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, normalizedTime);
            
            yield return null;
        }
        
        // Animasyon bitince tamamen görünmez yap
        instructionsCanvasGroup.alpha = 0f;
        
        // İsteğe bağlı olarak Canvas'ı devre dışı bırak (performans için)
        instructionsCanvasGroup.gameObject.SetActive(false);
    }
} 