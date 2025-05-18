using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundAutoAdder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null) return;

        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(audioManager.PlayButtonClick);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
