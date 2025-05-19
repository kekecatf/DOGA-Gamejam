using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class OyunKazanmaManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public float beklemeSuresi = 5f;  // Ana menüye dönmeden önce beklenecek süre
    public string anaMenuSceneName = "AnaMenu";
    
    [Header("UI Elemanları")]
    public Text kazanmaText;
    public Text puanText;
    public Button anaMenuButton;
    
    private void Start()
    {
        // Oyunu normal hızda çalıştır (Pause'dan gelme ihtimaline karşı)
        Time.timeScale = 1f;
        
        // UI elemanlarını ayarla
        if (kazanmaText != null)
        {
            kazanmaText.text = "Tebrikler! Oyunu Kazandınız!";
        }
        
        // Oyuncunun puanını göster (PlayerData'dan al)
        PlayerData playerData = FindObjectOfType<PlayerData>();
        if (playerData != null && puanText != null)
        {
            puanText.text = "Toplam Puan: " + playerData.metalPara.ToString();
        }
        
        // Ana menü butonuna click olayı ata
        if (anaMenuButton != null)
        {
            anaMenuButton.onClick.AddListener(AnaMenuyeDon);
        }
        
        // Otomatik dönüş için zamanlayıcı başlat
        StartCoroutine(OtomatikAnaMenuyeDon());
        
        // Ses efekti çal
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();  // Kazanma sesi yerine geçici olarak buton sesi kullanıldı
        }
    }
    
    private IEnumerator OtomatikAnaMenuyeDon()
    {
        yield return new WaitForSeconds(beklemeSuresi);
        AnaMenuyeDon();
    }
    
    public void AnaMenuyeDon()
    {
        // Ana menüye dön
        SceneManager.LoadScene(anaMenuSceneName);
    }
} 