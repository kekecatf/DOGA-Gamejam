using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class MiniGamePlayer : MonoBehaviour
{
    public float moveSpeed = 5f; // Aşağıya inme hızı
    public float horizontalSpeed = 5f; // Sağ-sol hareket hızı
    public float takilmaSuresi = 1f; // Engel takılma süresi (saniye)
    public Transform zemin; // Inspector'dan ata

    private bool takildi = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Sadece aşağıya hareketi takılma durumunda engelle
        if (!takildi)
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);
        }

        // Sağ-sol hareket her zaman aktif
        float horizontalInput = 0f;
        if (Keyboard.current.leftArrowKey.isPressed)
            horizontalInput = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed)
            horizontalInput = 1f;

        transform.Translate(Vector2.right * horizontalInput * horizontalSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Zemin"))
        {
            // Direkt olarak Oynanis sahnesine geç
            SceneManager.LoadScene("GameScene");
        }
        else if (other.CompareTag("Engel"))
        {
            StartCoroutine(KaybettinVeBitisEkrani());
        }
    }

    IEnumerator KazandinVeDevamEt()
    {
        Time.timeScale = 0f;
        GameObject kazandinText = GameObject.Find("KazandinText");
        if (kazandinText != null)
            kazandinText.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync("MiniOyun");
    }

    IEnumerator KaybettinVeBitisEkrani()
    {
        Time.timeScale = 0f;
        GameObject kaybettinText = GameObject.Find("KaybettinText"); // Canvas altında bir Text objesi oluşturup adını KaybettinText yap
        if (kaybettinText != null)
            kaybettinText.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("OyunSonu");
    }
}
