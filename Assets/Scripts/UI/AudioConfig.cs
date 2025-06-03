using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioConfig : MonoBehaviour
{
    [Header("Volumen")]
    [SerializeField] private AudioMixer mixer; //AudioMixer



    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;
    [SerializeField] private Slider sliderVoces;

    private string parametroMaster = "VolumenMaster";
    private string parametroMusica = "VolumenMusica";
    private string parametroSFX = "VolumenSFX";
    private string parametroVoces = "VolumenVoces";

    private AudioSource audioSource;

    private void Start()
    {
        // Cargar valores guardados (si existen)
        sliderMaster.value = PlayerPrefs.GetFloat(parametroMaster, 0.75f);
        sliderMusica.value = PlayerPrefs.GetFloat(parametroMusica, 0.75f);
        sliderSFX.value = PlayerPrefs.GetFloat(parametroSFX, 0.75f);
        sliderVoces.value = PlayerPrefs.GetFloat(parametroVoces, 0.75f);


        // Aplicar valores
        SetVolumenMaster(sliderMaster.value);
        SetVolumenMusica(sliderMusica.value);
        SetVolumenSFX(sliderSFX.value);
        SetVolumenVoces(sliderVoces.value);

        // Configurar listeners
        sliderMaster.onValueChanged.AddListener(SetVolumenMaster);
        sliderMusica.onValueChanged.AddListener(SetVolumenMusica);
        sliderSFX.onValueChanged.AddListener(SetVolumenSFX);
        sliderVoces.onValueChanged.AddListener(SetVolumenVoces);
        audioSource = GetComponent<AudioSource>();
    }

    // Configs

    public void SetVolumenMaster(float valor)
    {
        mixer.SetFloat(parametroMaster, Mathf.Log10(valor) * 20); // Conversión a dB
        PlayerPrefs.SetFloat(parametroMaster, valor);
    }


    public void SetVolumenMusica(float valor) {
        mixer.SetFloat(parametroMusica, Mathf.Log10(valor) * 20); // Conversión a dB
        PlayerPrefs.SetFloat(parametroMusica, valor);

    }

    public void SetVolumenSFX(float valor) {
        mixer.SetFloat(parametroSFX, Mathf.Log10(valor) * 20); // Conversión a dB
        PlayerPrefs.SetFloat(parametroSFX, valor);

    }
    public void SetVolumenVoces(float valor) {
        mixer.SetFloat(parametroVoces, Mathf.Log10(valor) * 20); // Conversión a dB
        PlayerPrefs.SetFloat(parametroVoces, valor);
    }




    //Guardado


    public void GuardarConfiguracion()
    {
        PlayerPrefs.Save();
        Debug.Log("Configuración de audio guardada");
    }

    public void SoundEffectSFX(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

}
