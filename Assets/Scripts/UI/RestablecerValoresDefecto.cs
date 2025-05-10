using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class RestablecerValoresDefecto : MonoBehaviour
{
    // --- Audio ---
    [Header("Configuración de Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;


    // Parámetros
    private const string MASTER_VOL = "VolumenMaster";
    private const string MUSIC_VOL = "VolumenMusica";
    private const string SFX_VOL = "VolumenSFX";
    private const string VOICE_VOL = "VolumenVoces";

    // Valores por defecto
    private const float DEFAULT_MASTER = 0.75f;
    private const float DEFAULT_MUSIC = 0.75f;
    private const float DEFAULT_SFX = 0.75f;
    private const float DEFAULT_VOICE = 0.75f;



    // --- Futuras Configs (ejemplo) ---
    [Header("Otras Configuraciones")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;








    void Start()
    {
        CargarConfiguracion();
    }

    public void ResetearTodo()
    {
        // 1. Resetear Audio
        ResetearAudio();

        // 2. Resetear otras configuraciones
        ResetearGraficos();

        // 3. Guardar cambios
        GuardarConfiguracion();

        Debug.Log("¡Configuración restablecida a valores predeterminados!");
    }

    private void ResetearAudio()
    {
        // Aplicar valores a sliders
        masterSlider.value = DEFAULT_MASTER;
        musicSlider.value = DEFAULT_MUSIC;
        sfxSlider.value = DEFAULT_SFX;
        voiceSlider.value = DEFAULT_VOICE;

        // Aplicar al AudioMixer
        SetVolume(MASTER_VOL, DEFAULT_MASTER);
        SetVolume(MUSIC_VOL, DEFAULT_MUSIC);
        SetVolume(SFX_VOL, DEFAULT_SFX);
        SetVolume(VOICE_VOL, DEFAULT_VOICE);
    }

    private void ResetearGraficos()
    {
        // Ejemplo para futuras configuraciones
        fullscreenToggle.isOn = true;
        resolutionDropdown.value = 0; // Primera opción
    }

    private void SetVolume(string param, float value)
    {
        float dB = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(param, dB);
        PlayerPrefs.SetFloat(param, value);
    }

    private void CargarConfiguracion()
    {
        // Audio
        masterSlider.value = PlayerPrefs.GetFloat(MASTER_VOL, DEFAULT_MASTER);
        musicSlider.value = PlayerPrefs.GetFloat(MUSIC_VOL, DEFAULT_MUSIC);
        sfxSlider.value = PlayerPrefs.GetFloat(SFX_VOL, DEFAULT_SFX);
        voiceSlider.value = PlayerPrefs.GetFloat(VOICE_VOL, DEFAULT_VOICE);

        // Gráficos (ejemplo)
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }

    public void GuardarConfiguracion()
    {
        PlayerPrefs.Save();
    }
}
