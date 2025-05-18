using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimasyonSahneGeçiş : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
public void GecisYap()
    {
        SceneManager.LoadScene("Oynanis"); // Oyun sahnesinin adını doğru yaz!
    }
     public void AnimasyonBitti()
    {
        SceneManager.LoadScene("Oynanis"); // Oyun sahnesinin adını buraya yaz
    }
}


