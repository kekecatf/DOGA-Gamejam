using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainOyunTimer : MonoBehaviour
{
    public float toplamSure = 10f;
    private float kalanSure;
    public Text timerText;
    public GameObject player;

    void Start()
    {
        kalanSure = toplamSure;
    }

    void Update()
    {
        kalanSure -= Time.deltaTime;
        if (kalanSure <= 0)
        {
            OyunBitti();
        }
        else
        {
            timerText.text = "Süre: " + Mathf.CeilToInt(kalanSure);
        }
    }

    void OyunBitti()
    {
        // Oyun durdurulsun
        Time.timeScale = 0f;

        // Bitiş ekranına geçiş
        UnityEngine.SceneManagement.SceneManager.LoadScene("Oynanis");
    }

    public void MiniOyunSonu(bool kazandi)
    {
        if (kazandi)
        {
            // Ana oyuna devam hakkı ver
            SceneManager.UnloadSceneAsync("MiniOyun");
            // Ana oyunda karakteri canlandıracak bir fonksiyon tetikle
        }
        else
        {
            // Ana oyunda ölüm ekranı veya tekrar deneme
            SceneManager.UnloadSceneAsync("MiniOyun");
            // Ana oyunda ölüm ekranı açılabilir
        }
    }
}
