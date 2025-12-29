using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;   

    [Header("Sensibilidade")]
    public Slider sensitivitySlider;
    public static float mouseSensitivity = 1.0f;

    [Header("Video")]
    public Toggle fullscreenToggle;

    void Start()
    {
        // Carregar Volumes
        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // Atualizar posição visual dos sliders
        if (masterSlider) masterSlider.value = savedMaster;
        if (musicSlider) musicSlider.value = savedMusic;
        if (sfxSlider) sfxSlider.value = savedSFX;

        // Aplicar os volumes ao Mixer
        SetMasterVolume(savedMaster);
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);

        // Carregar Sensibilidade 
        if (PlayerPrefs.HasKey("Sensitivity"))
            mouseSensitivity = PlayerPrefs.GetFloat("Sensitivity");

        if (sensitivitySlider) sensitivitySlider.value = mouseSensitivity;

        // Carregar Fullscreen
        if (fullscreenToggle) fullscreenToggle.isOn = Screen.fullScreen;
    }



    public void SetMasterVolume(float sliderValue)
    {
        SetMixerVolume("MasterVolume", sliderValue);
    }

    public void SetMusicVolume(float sliderValue)
    {
        SetMixerVolume("MusicVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        SetMixerVolume("SFXVolume", sliderValue);
    }

    // Função auxiliar para evitar repetir código
    private void SetMixerVolume(string paramName, float sliderValue)
    {

        float volumeDb = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;

        if (mainMixer != null)
            mainMixer.SetFloat(paramName, volumeDb);

        PlayerPrefs.SetFloat(paramName, sliderValue);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        PlayerPrefs.Save();
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}