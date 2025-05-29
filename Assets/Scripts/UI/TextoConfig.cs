using UnityEngine;
using UnityEngine.UI;

public class TextoConfig : MonoBehaviour
{
    [SerializeField] private Image dialogueBox; // Imagen del cuadro de diálogo
    [SerializeField] private Slider transparencySlider; // Slider para transparencia
    [SerializeField] private Slider speedSlider; // Slider para velocidad

    [Header("Afectados")]
    [SerializeField] private NPCNonPossessable[] nonPossessableNPCs;
    [SerializeField] private NPCPossessable[] possessableNPCs;
    [SerializeField] private HintManager[] hintManagers;


    [Header("Configuración")]
    [Range(0.1f, 1f)] private float defaultTransparency = 1f;
    [Range(0.01f, 0.1f)] private float defaultSpeed = 0.03f;
    private const string TransparencyKey = "DialogueTransparency";
    private const string SpeedKey = "DialogueSpeed";

    private void Start()
    {
        // Cargar valores guardados o usar defaults
        float savedTransparency = PlayerPrefs.GetFloat(TransparencyKey, defaultTransparency);
        float savedSpeed = PlayerPrefs.GetFloat(SpeedKey, defaultSpeed);

        // Configurar sliders
        transparencySlider.value = savedTransparency;
        speedSlider.value = NormalizeSpeed(savedSpeed);

        // Aplicar valores iniciales
        ApplyTransparency(savedTransparency);
        ApplySpeed(savedSpeed);

        // Configurar listeners
        transparencySlider.onValueChanged.AddListener(ApplyTransparency);
        speedSlider.onValueChanged.AddListener(ApplySpeedFromSlider);
    }

    
    public void ApplyTransparency(float transparencyValue)
    {
        if (dialogueBox != null)
        {
            Color color = dialogueBox.color;
            color.a = transparencyValue;
            dialogueBox.color = color;

            PlayerPrefs.SetFloat(TransparencyKey, transparencyValue);
        }
    }

    
    private void ApplySpeedFromSlider(float sliderValue)
    {
        float actualSpeed = Mathf.Lerp(0.1f, 0.01f, sliderValue);
        ApplySpeed(actualSpeed);
    }

    public void ApplySpeed(float speedValue)
    {

        if (nonPossessableNPCs != null)
        {
            foreach (NPCNonPossessable npc in nonPossessableNPCs)
            {
                if (npc != null)
                {
                    npc.SetTimeBtLetters(speedValue);
                }
            }
        }

        if (possessableNPCs != null)
        {
            foreach (NPCPossessable npc in possessableNPCs)
            {
                if (npc != null)
                {
                    npc.SetTimeBtLetters(speedValue);
                }
            }
        }

        if (hintManagers != null)
        {
            foreach (HintManager hintManager in hintManagers)
            {
                if (hintManager != null)
                {
                    hintManager.SetTimeBtLetters(speedValue);
                }
            }
        }

        PlayerPrefs.SetFloat(SpeedKey, speedValue);
    }




    // Convierte velocidad real (0.1-5) a valor de slider (0-1)
    private float NormalizeSpeed(float speed)
    {
        return Mathf.InverseLerp(0.1f, 0.01f, speed);
    }



    // Resetear a valores por defecto
    public void ResetConfiguracion()
    {
        transparencySlider.value = defaultTransparency;
        speedSlider.value = NormalizeSpeed(defaultSpeed);

        ApplyTransparency(defaultTransparency);
        ApplySpeed(defaultSpeed);
    }

}
