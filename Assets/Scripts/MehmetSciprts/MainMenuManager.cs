using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null)
        AudioManager.Instance.PlayButtonClick();
        AudioManager.Instance.StopMusic();
        SceneManager.LoadScene("Hikaye");
    }
    
    public void LoadMiniGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        SceneManager.LoadScene("MiniOyun");
    }

    public void QuitGame()
    {
        Application.Quit();
        // Editor'de test için:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    

    public Text comingSoonText;

    public GameObject comingSoonTextGameObject; // Inspector'dan atayacaksın

    public void ShowComingSoon()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        if (comingSoonTextGameObject != null)
            comingSoonTextGameObject.SetActive(true);

            if (coopButtonText != null)
            coopButtonText.text = "Yakında Gelecek...";
    }

    public Text coopButtonText; // Eğer TextMeshPro ise: public TMP_Text coopButtonText;

}
