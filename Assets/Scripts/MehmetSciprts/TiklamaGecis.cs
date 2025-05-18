using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TiklamaGecis : MonoBehaviour
{
    public string gecilecekSahne = "Oynanis"; // Geçmek istediğin sahnenin adını buraya yaz

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SceneManager.LoadScene(gecilecekSahne);
        }
        // Dokunmatik için:
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            SceneManager.LoadScene(gecilecekSahne);
        }
    }
}
