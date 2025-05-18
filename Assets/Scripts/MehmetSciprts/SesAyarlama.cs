using UnityEngine;
using UnityEngine.UI;

public class SesAyarlama : MonoBehaviour
{
    void Start()
    {
        // SliderGameMusic adındaki slider'ı bul
        Slider foundSlider = GameObject.Find("SliderGameMusic").GetComponent<Slider>();
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (foundSlider != null && audioManager != null)
        {
            // Slider'ı AudioManager'a bağla
            audioManager.musicSlider = foundSlider;
            audioManager.musicSlider.value = audioManager.musicSource.volume;
            audioManager.musicSlider.onValueChanged.RemoveAllListeners();
            audioManager.musicSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
        }
    }
}
