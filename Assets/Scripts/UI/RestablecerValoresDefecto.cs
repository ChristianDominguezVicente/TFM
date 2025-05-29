using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RestablecerValoresDefecto : MonoBehaviour
{
    [SerializeField] private Button btnRestablecer; // Botón que activa el reset
    // --- Audio 
    [Header("Configuración de Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;


    // Parámetros audio
    private const string MASTER_VOL = "VolumenMaster";
    private const string MUSIC_VOL = "VolumenMusica";
    private const string SFX_VOL = "VolumenSFX";
    private const string VOICE_VOL = "VolumenVoces";

    // Valores por defecto audio
    private const float DEFAULT_MASTER = 0.75f;
    private const float DEFAULT_MUSIC = 0.75f;
    private const float DEFAULT_SFX = 0.75f;
    private const float DEFAULT_VOICE = 0.75f;


    // ---- Demás
    [Header("Demás")]
    [SerializeField] private PantallaConfig pantallaConfig;
    [SerializeField] private MenuInicial menuInicial;
    [SerializeField] private BrilloConfig brilloConfig;
    [SerializeField] private TextoConfig textoConfig;

    private bool primeraActivacion = true;

    private void OnEnable()
    {
        if (primeraActivacion)
        {
            primeraActivacion = false;
            StartCoroutine(ConfigurarPrimeraVez());
        }
        else
        {
            ConfigurarBoton();
        }
    }

    private IEnumerator ConfigurarPrimeraVez()
    {
        yield return new WaitForEndOfFrame(); // Espera crítica

        // Obtener referencias si son nulas

        if (menuInicial == null)
            menuInicial = FindFirstObjectByType<MenuInicial>(FindObjectsInactive.Include);

        if (brilloConfig == null)
            brilloConfig = FindFirstObjectByType<BrilloConfig>(FindObjectsInactive.Include);
        if ((textoConfig == null))
        {
            textoConfig = FindFirstObjectByType<TextoConfig>(FindObjectsInactive.Include);
        }

        if (pantallaConfig == null)
            pantallaConfig = FindFirstObjectByType<PantallaConfig>(FindObjectsInactive.Include);

        ConfigurarBoton();
    }

    private void ConfigurarBoton()
    {
        btnRestablecer.onClick.RemoveAllListeners();
        btnRestablecer.onClick.AddListener(() => StartCoroutine(ResetCompleto()));
    }

    private IEnumerator ResetCompleto()
    {
        yield return null; // Frame adicional de seguridad

        Debug.Log("Iniciando reset completo...");

        // Resetear menús
        if (MenuInicial.menuActivo != null)
            MenuInicial.menuActivo.ResetearTodo();

        // Resetear brillo
        if (brilloConfig != null)
            brilloConfig.ResetBrillo();

        if(textoConfig != null)
            textoConfig.ResetConfiguracion();

        // Resetear audio
        ResetearAudio();

        // Resetear gráficos
        if (pantallaConfig != null)
            pantallaConfig.AplicarPorDefectoTodo();

        // Guardar cambios
        GuardarConfiguracion();

        Debug.Log("Reset completado correctamente");
    }

    private void ResetearAudio()
    {
        // Aplicar valores a sliders
        if (masterSlider != null) masterSlider.value = DEFAULT_MASTER;
        if (musicSlider != null) musicSlider.value = DEFAULT_MUSIC;
        if (sfxSlider != null) sfxSlider.value = DEFAULT_SFX;
        if (voiceSlider != null) voiceSlider.value = DEFAULT_VOICE;

        // Aplicar al AudioMixer
        SetVolume(MASTER_VOL, DEFAULT_MASTER);
        SetVolume(MUSIC_VOL, DEFAULT_MUSIC);
        SetVolume(SFX_VOL, DEFAULT_SFX);
        SetVolume(VOICE_VOL, DEFAULT_VOICE);
    }

    private void SetVolume(string param, float value)
    {
        if (audioMixer != null)
        {
            float dB = value > 0 ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(param, dB);
        }
        PlayerPrefs.SetFloat(param, value);
    }

    public void GuardarConfiguracion()
    {
        PlayerPrefs.Save();
        Debug.Log("Configuración guardada");
    }

}
