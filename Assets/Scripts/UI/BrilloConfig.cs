using UnityEngine;
using UnityEngine.UI;

public class BrilloConfig : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image brightnessOverlay; // El Image negro UI
    [SerializeField] private Slider brightnessSlider; //Slider de UI


    [Range(0, 1)] private float defaultBrightness = 0.5f;
    private void Start()
    {
        // Carga el valor guardado o usa el valor por defecto
        float savedBrightness = PlayerPrefs.GetFloat("Brightness", defaultBrightness);
        brightnessSlider.value = savedBrightness;
        ApplyBrightness(savedBrightness);

        // Configura el evento del Slider
        brightnessSlider.onValueChanged.AddListener(ApplyBrightness);
    }

    // Aplica el brillo cambiando la opacidad del overlay
    public void ApplyBrightness(float brightnessValue)
    {
        // Mapear el slider (0-1) a un rango de opacidad personalizado (ej: 0.7 a 0)
        float minAlpha = 0.7f; // Alpha mínimo (30% de visibilidad)
        float alpha = Mathf.Lerp(minAlpha, 0f, brightnessValue); // brightnessValue = 0 (mínimo brillo) aplica minAlpha

        Color overlayColor = brightnessOverlay.color;
        overlayColor.a = alpha;
        brightnessOverlay.color = overlayColor;

        PlayerPrefs.SetFloat("Brightness", brightnessValue);
    }

    public void ResetBrillo()
    {
        brightnessSlider.value = defaultBrightness; // Usa el valor por defecto
        ApplyBrightness(defaultBrightness); 
        PlayerPrefs.SetFloat("Brightness", defaultBrightness); // Guarda
    }
}
