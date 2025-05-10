using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInicial : MonoBehaviour
{

    [System.Serializable]
    public class BotonConfig
    {
        [Header("Botones normales")]
        public Button boton;
        public GameObject menuDestino;  // Dejar null para acciones especiales
        public bool esSalir = false;
        public bool esVolver = false;
        public bool esCargarPartida = false;

        [Header("Botón cambiante")]
        public Button botonToggle;
        public bool esToggle = false; // botones con texto deslizante
        public string[] opcionesToggle;
        public int indiceOpcionToggle = 0;

        [Header("Botón Slider")]
        public Slider slider;
        public bool esSlider = false; //boton slider
        public Image fillImage;

        [Header("Configuración Pantalla")]
        public bool esConfiguracionPantalla = false;
        public TipoConfigPantalla tipoConfigPantalla;


    }
    public enum TipoConfigPantalla
    {
        Ninguno,
        Resolucion,
        ModoPantalla
    }

    [Header("Configuración")]
    [SerializeField] private List<BotonConfig> botones;
    [SerializeField] private GameObject menuAnterior; // Solo necesario para esVolver
    private List<Button> currentButtons = new List<Button>();
    public static MenuInicial MenuActivo { get; private set; }

    // Indices 
    private int seleccionBotonIndice = 0;

    //Toggle
    private bool ajustandoToggle = false;

    //Slider 
    private int sliderIndice = 0;
    private float incrementoSlider = 0.1f;
    private bool ajustandoSlider = false;



    private void Start()
    {
        ConfigurarBotones();
    }
    private void OnEnable()
    {
        MenuActivo = this;
        seleccionBotonIndice = 0;

        UpdatePanel();
    }
    private void OnDisable()
    {
        // Si este menú era el activo, limpia la referencia
        if (MenuActivo == this)
        {
            MenuActivo = null;
        }
    }

    private void ConfigurarBotones()
    {
        currentButtons.Clear();
        foreach (var config in botones)
        {
            if (config.esSlider) //slider
            {
                if (config.boton != null)
                    currentButtons.Add(config.boton);
            }
            else //botones normales
            {
                currentButtons.Add(config.boton);
                config.boton.onClick.RemoveAllListeners();
                if (config.esCargarPartida)
                {
                    config.boton.onClick.AddListener(EntrarJuego);
                }
                else if (config.esSalir)
                {
                    config.boton.onClick.AddListener(SalirDelJuego);
                }
                else if (config.esVolver)
                {
                    config.boton.onClick.AddListener(VolverAMenuAnterior);
                }
                else if (config.esToggle)
                {
                    // Inicializa el texto del botón
                    ActualizarTextoBotonToggle(config);

                    // Configura el onClick para el botón principal
                    config.boton.onClick.AddListener(() => {
                        if (!ajustandoToggle) // Solo alterna si no está en modo ajuste
                        {
                            AlternarOpcion(config);
                        }
                    });

                    // Configura el onClick para el botón toggle (si existe)
                    if (config.botonToggle != null)
                    {
                        config.botonToggle.onClick.AddListener(() => AlternarOpcion(config));
                    }

                }
                else if (config.menuDestino != null)
                {
                    config.boton.onClick.AddListener(() => IrAMenu(config.menuDestino));
                }
            }
        }
    }

    private void EntrarJuego()
    {
        SceneManager.LoadScene("Greybox");
    }

    private void SalirDelJuego()
    {
        Application.Quit();

    }

    private void VolverAMenuAnterior()
    {
        if (menuAnterior != null)
        {
            gameObject.SetActive(false);
            menuAnterior.SetActive(true);
        }
    }

    private void IrAMenu(GameObject destino)
    {
        gameObject.SetActive(false);
        destino.SetActive(true);
    }

    public void MoveSelection(int direction)
    {
        if (botones.Count == 0) return;

        // Si estamos ajustando un slider, manejamos movimiento horizontal
        if (ajustandoSlider && Mathf.Abs(direction) == 1)
        {
            AjustarSliderAdjunto(botones[seleccionBotonIndice], direction);
            return;
        }

        // Navegación normal entre botones
        seleccionBotonIndice = (seleccionBotonIndice + direction + botones.Count) % botones.Count;
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        for (int i = 0; i < botones.Count; i++)
        {
            bool isSelected = (i == seleccionBotonIndice);
            var config = botones[i];
            if (config.boton != null)
            {
                var colors = config.boton.colors;
                colors.normalColor = isSelected ?
                    (ajustandoToggle ? Color.yellow : Color.white) :
                    Color.gray;
                config.boton.colors = colors;
            }

            // Resaltado para botón toggle asociado
            if (config.esToggle && config.botonToggle != null)
            {
                var toggleColors = config.botonToggle.colors;
                toggleColors.normalColor = ajustandoToggle ? Color.green : Color.white;
                config.botonToggle.colors = toggleColors;
            }
        

        // Configuración para sliders adjuntos
        if (botones[i].esSlider && botones[i].slider != null)
            {
                Image handle = botones[i].slider.handleRect.GetComponent<Image>();
                handle.color = ajustandoSlider ? Color.red : Color.white;

                // Opcional: Resaltar fill cuando está activo
                if (botones[i].slider.fillRect != null)
                {
                    botones[i].slider.fillRect.GetComponent<Image>().color =
                        ajustandoSlider ? new Color(1, 0.5f, 0) : new Color(1, 1, 1, 0.5f);
                }
            }
        }
    }

    public void ActivateSelectedButton()
    {
        if (currentButtons.Count > 0)
            currentButtons[seleccionBotonIndice].onClick.Invoke();
    }


    private void ActualizarTextoBotonToggle(BotonConfig config)
    {
        if (config.opcionesToggle != null && config.opcionesToggle.Length > 0)
        {
            // Actualiza el texto del botón toggle si existe
            if (config.botonToggle != null)
            {
                TextMeshProUGUI textoBoton = config.botonToggle.GetComponentInChildren<TextMeshProUGUI>();
                if (textoBoton != null)
                {
                    textoBoton.text = config.opcionesToggle[config.indiceOpcionToggle];
                }
            }
        }
    }

    private void AlternarOpcion(BotonConfig config)
    {
        if (config.opcionesToggle != null && config.opcionesToggle.Length > 0)
        {
            config.indiceOpcionToggle = (config.indiceOpcionToggle + 1) % config.opcionesToggle.Length;

            // Actualiza el texto del botón toggle asociado
            if (config.botonToggle != null)
            {
                TextMeshProUGUI textoBoton = config.botonToggle.GetComponentInChildren<TextMeshProUGUI>();
                if (textoBoton != null)
                {
                    textoBoton.text = config.opcionesToggle[config.indiceOpcionToggle];
                }
            }
        }
        ProcesarOpcionToggle(config);
    }


    public bool IsCurrentItemSlider()
    {
        if (sliderIndice >= 0 && seleccionBotonIndice < botones.Count)
        {
            return botones[sliderIndice].esSlider;
        }
        return false;
    }


    private void AjustarSliderAdjunto(BotonConfig config, int direccion)
    {
        if (config.esSlider && config.slider != null)
        {
            config.slider.value = Mathf.Clamp01(
                config.slider.value + (direccion * incrementoSlider));

            // Actualizar valor en tiempo real (ej: volumen)
        }
    }

    public void ToggleAjusteSlider()
    {
        if (botones[seleccionBotonIndice].esSlider)
        {
            ajustandoSlider =!ajustandoSlider;
            UpdatePanel();
        }
    }
    public bool IsAdjustingSlider()
    {
        return botones[seleccionBotonIndice].esSlider && ajustandoSlider;
    }
    public bool CurrentButtonHasAdjacentSlider()
    {
        // Verifica que el índice es válido
        if (seleccionBotonIndice < 0 || seleccionBotonIndice >= botones.Count)
            return false;

        return botones[seleccionBotonIndice].esSlider &&
               botones[seleccionBotonIndice].slider != null;
    }


    public void ToggleModoAjuste()
    {
        if (botones[seleccionBotonIndice].esToggle &&
            botones[seleccionBotonIndice].botonToggle != null)
        {
            ajustandoToggle = !ajustandoToggle;
            UpdatePanel();
        }
    }

    public void CambiarOpcionToggle(int direccion)
    {
        var config = botones[seleccionBotonIndice];
        if (config.esToggle && ajustandoToggle)
        {
            config.indiceOpcionToggle =
                (config.indiceOpcionToggle + direccion + config.opcionesToggle.Length) % config.opcionesToggle.Length;

            // Actualiza el texto del botón toggle
            if (config.botonToggle != null)
            {
                TextMeshProUGUI textoBoton = config.botonToggle.GetComponentInChildren<TextMeshProUGUI>();
                if (textoBoton != null)
                {
                    textoBoton.text = config.opcionesToggle[config.indiceOpcionToggle];
                }
            }
            ProcesarOpcionToggle(config);

        }
    }

    private void ProcesarOpcionToggle(BotonConfig config)
    {
        // pantalla resol y modo 

        PantallaConfig pantallaConfig = FindFirstObjectByType<PantallaConfig>();
        if (pantallaConfig != null)
        {
            string opcionActual = config.opcionesToggle[config.indiceOpcionToggle];

            switch (config.tipoConfigPantalla)
            {
                case TipoConfigPantalla.Resolucion:
                    // Parsea el texto (ej: "1920x1080")
                    pantallaConfig.AplicarResolucionPorNombre(opcionActual);
                    break;

                case TipoConfigPantalla.ModoPantalla:
                    pantallaConfig.AplicarModoPantalla(config.indiceOpcionToggle);
                    break;
            }
        }
        // Aquí puedes añadir más lógica para otros tipos de toggles
    }



    public bool IsAdjustingToggle()
    {
        if (seleccionBotonIndice < 0 || seleccionBotonIndice >= botones.Count)
            return false;

        return botones[seleccionBotonIndice].esToggle && ajustandoToggle;
    }

    public bool CurrentButtonIsToggle()
    {
        if (seleccionBotonIndice < 0 || seleccionBotonIndice >= botones.Count)
            return false;

        return botones[seleccionBotonIndice].esToggle;
    }


    public Button GetToggleAsociado()
    {
        if (IsAdjustingToggle())
            return botones[seleccionBotonIndice].botonToggle;
        return null;
    }


    public string ObtenerTextoBotonSeleccionado()
    {
        if (seleccionBotonIndice >= 0 && seleccionBotonIndice < botones.Count)
        {
            TextMeshProUGUI textoBoton = botones[seleccionBotonIndice].boton.GetComponentInChildren<TextMeshProUGUI>();
            if (textoBoton != null)
            {
                return textoBoton.text;
            }
        }
        return string.Empty;
    }


    public BotonConfig GetCurrentButtonConfig()
    {
        if (seleccionBotonIndice >= 0 && seleccionBotonIndice < botones.Count)
        {
            return botones[seleccionBotonIndice];
        }
        return null;
    }

}
