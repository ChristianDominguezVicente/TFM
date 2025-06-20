using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MenuInicial;
using static UnityEngine.Rendering.STP;

public class MenuInicial : MonoBehaviour
{

    [System.Serializable]
    public class BotonConfig
    {
        [Header("Botones normales")]
        public Button boton;
        public GameObject menuDestino;  // Leave null for special actions
        public bool esSalir = false;
        public bool esVolver = false;
        public bool esNuevaPartida = false;
        public bool esMission = false;
        public bool esVolverMenuInicial = false;
        public bool esFinal1 = false;
        public bool esFinal2 = false;
        public bool esCreditos = false;

        [Header("Botones de Slot de Guardado")]
        public bool esSlotCargandoPartida = false; // cargado partida
        public bool esGuardarPartida = false; //guardado partida
        public int indiceSlot = 0; // 0, 1, 2 para los slots
        public int indiceSlotGuardado = 0;

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
    [SerializeField] private MenuPause menuPausa;
    [SerializeField] private CanvasGroup fadeOut;
    private List<Button> currentButtons = new List<Button>();
    public static MenuInicial menuActivo { get; private set; }

    // Indices 
    private int seleccionBotonIndice = 0;

    //Toggle
    private bool ajustandoToggle = false;

    //Slider 
    private int sliderIndice = 0;
    private float incrementoSlider = 0.1f;
    private bool ajustandoSlider = false;
    
    //Control FadeIn FadeOut
    private AudioConfig audioConfig;


    private Color normal = new Color(1f, 0.9529f, 0.6902f);// FFF3B0


    private void Start()
    {
        ConfigurarBotones();
        audioConfig = (AudioConfig)FindAnyObjectByType(typeof(AudioConfig));

    }
    private void OnEnable()
    {
        menuActivo = this;
        
        seleccionBotonIndice = 0;
        UpdatePanel();

    }
    private void OnDisable()
    {
        // Si este menú era el activo, limpia la referencia
        if (menuActivo == this)
        {
            menuActivo = null;
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
                if (config.esNuevaPartida)
                {
                    config.boton.onClick.AddListener(EntrarJuego);
                }else if (config.esSlotCargandoPartida) // load game
                {
                    ConfigureForLoadData(config);
                } else if (config.esGuardarPartida) // save game
                {
                    ConfigureSaveButton(config);
                }else if (config.esFinal1)
                {
                    config.boton.onClick.AddListener(IrFinalUno);
                }
                else if (config.esFinal2)
                {
                    config.boton.onClick.AddListener(IrFinalDos);
                }else if (config.esCreditos)
                {
                    config.boton.onClick.AddListener(IrCreditos);
                }
                else if (config.esVolverMenuInicial) // back to menu initial
                {
                    config.boton.onClick.AddListener(VolverMenuInicio);
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
                    if(config.esConfiguracionPantalla) // si estoy en las graficos
                    {
                        ProcesarCargaToggle(config);
                    }
                    // Inicializa el texto del botón
                    ActualizarTextoBotonToggle(config);

                    
                    config.boton.onClick.AddListener(() =>
                    {
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
                }else if (config.esMission)
                {
                    config.boton.onClick.AddListener(() => MostrarObjetivosSM(config));
                }
            }
        }
    }

    private void IrCreditos()
    {
        StartCoroutine(FadeOut("Creditos"));
    }

    private void IrFinalDos()
    {
        StartCoroutine(FadeOut("Final2"));
    }

    private void IrFinalUno()
    {
        StartCoroutine(FadeOut("Final1"));
    }

    private void MostrarObjetivosSM(BotonConfig config)
    {
        SMSystem sMSystem = FindAnyObjectByType<SMSystem>();
        if (sMSystem != null)
        {
            sMSystem.UpdateMissionMenuEnunciado(config.indiceSlot);
        }
    }

    private void ConfigureSaveButton(BotonConfig config)
    {
        TextMeshProUGUI textoBoton = config.boton.GetComponentInChildren<TextMeshProUGUI>();
        textoBoton.text = $"Guardar en Slot {config.indiceSlotGuardado + 1}";

        config.boton.onClick.AddListener(() => {
            SaveSystemMult saveSystem = FindFirstObjectByType<SaveSystemMult>();
            saveSystem.SaveGame(config.indiceSlotGuardado);
            UpdateSingleSaveSlotUI(config.indiceSlotGuardado);
            VolverAMenuAnterior();
        });
    }

    private void ConfigureForLoadData(BotonConfig config)
    {
        SaveSystemMult saveSystem = FindFirstObjectByType<SaveSystemMult>();
        TextMeshProUGUI textoBoton = config.boton.GetComponentInChildren<TextMeshProUGUI>();
        textoBoton.text = saveSystem.GetSaveInfo(config.indiceSlot);

        config.boton.onClick.AddListener(() => {
            SaveSystemMult.CurrentSlot = config.indiceSlot;
            if (saveSystem.HasSave(config.indiceSlot))
            {

                saveSystem.LoadGame(config.indiceSlot); // cargo partida
            }
            else
            {
                StartCoroutine(FadeOut("Intro")); // Nueva partida
            }
        });
    }

    private IEnumerator FadeOut(string sceneName)
    {
        fadeOut.gameObject.SetActive(true);

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeOut.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        audioConfig.ApplyFadeOut();

        SceneManager.LoadScene(sceneName);
    }

    private void UpdateSingleSaveSlotUI(int slotIndex)
    {
        SaveSystemMult saveSystem = FindFirstObjectByType<SaveSystemMult>();
        SlotUpdateLoad slotUpdateLoad = FindFirstObjectByType<SlotUpdateLoad>();
        slotUpdateLoad.UpdateText(slotIndex, saveSystem.GetSaveInfo(slotIndex));
      //  Debug.Log("Guardado en el texto: "+ slotIndex);
    }


    private void EntrarJuego()
    {
        StartCoroutine(FadeOut("Intro"));
        // Estaba Greybox anteriormente

        audioConfig.ApplyFadeOut();
    }

    private void VolverMenuInicio()
    {
        //StartCoroutine(FadeOut("InicioMenu"));
        SceneManager.LoadScene(0);
    }

    private void SalirDelJuego()
    {
        Application.Quit();

    }

    internal void VolverAMenuAnterior()
    {
        if (menuAnterior != null)
        {
            if(menuPausa != null && menuPausa.IsPaused) //paused game
            {
                menuPausa.ResumeGame();
            }
            SMSystem sMSystem = FindAnyObjectByType<SMSystem>();
            if ((this.gameObject.name == "MenuSM1"|| this.gameObject.name == "MenuSM2" || this.gameObject.name == "MenuSM3" || this.gameObject.name == "MenuSM4") && sMSystem.IsPaused) //mission system
            {
                sMSystem.ResumeGame();
            }
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
                    (ajustandoToggle ? Color.white : Color.white) : normal;
                config.boton.colors = colors;
            }

            // Resaltado para botón toggle asociado
            if (config.esToggle && config.botonToggle != null)
            {
                var toggleColors = config.botonToggle.colors;
                toggleColors.normalColor = ajustandoToggle ? Color.white : normal;
                config.botonToggle.colors = toggleColors;
            }


            // Configuración para sliders adjuntos
            if (botones[i].esSlider && botones[i].slider != null)
            {
                Image handle = botones[i].slider.handleRect.GetComponent<Image>();
                handle.color = ajustandoSlider ? normal : Color.white;

                // Opcional: Resaltar fill cuando está activo
                if (botones[i].slider.fillRect != null)
                {
                    botones[i].slider.fillRect.GetComponent<Image>().color =
                        ajustandoSlider ? normal : normal;
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
             //       Debug.Log("Menu inicial ACTUALIZARBOTONTOGGLE : " + textoBoton.text);
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
                Debug.Log("Menu inicial alternraropcion : " + textoBoton.text);
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
            ajustandoSlider = !ajustandoSlider;
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
                Debug.Log("Menu inicial cambiaropciontoggle : " + textoBoton.text);
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
        PantallaConfig pantallaConfig = FindFirstObjectByType<PantallaConfig>();
        if (pantallaConfig != null)
        {

            if (config.tipoConfigPantalla == TipoConfigPantalla.Resolucion)
            {
                pantallaConfig.AplicarResolucionPorNombre(config.indiceOpcionToggle);
            }
            else if (config.tipoConfigPantalla == TipoConfigPantalla.ModoPantalla)
            {
                pantallaConfig.AplicarModoPantalla(config.indiceOpcionToggle);
            }
        }
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
                            Debug.Log("Menu inicial ObtenertextoBotonSele : "+ textoBoton.text);
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

    public void ResetearTodo()
    {
        // 1. Resetear índices del menú actual
        seleccionBotonIndice = 0;
        sliderIndice = 0;
        ajustandoToggle = false;
        ajustandoSlider = false;

        // 2. Resetear todos los BotonConfig en este menú
        foreach (var config in botones)
        {
            if (config.esToggle)
            {
                config.indiceOpcionToggle = 0; // Resetear toggle a la primera opción
                ActualizarTextoBotonToggle(config); // Actualizar UI
            }

            if (config.esSlider && config.slider != null)
            {
                config.slider.value = 0; // Resetear sliders a 0
            }
        }

        // 3. Opcional: Resetear también en otros menús (recursivamente)
        ResetearMenusAnidados();

        UpdatePanel(); // Actualizar visualización

    }

    private void ResetearMenusAnidados()
    {
        MenuInicial[] todosLosMenus = FindObjectsByType<MenuInicial>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var menu in todosLosMenus)
        {
            if (menu != this) // Evitar resetear este menú dos veces
            {
                menu.seleccionBotonIndice = 0;
                menu.sliderIndice = 0;

                foreach (var config in menu.botones)
                {
                    if (config.esToggle)
                    {
                        config.indiceOpcionToggle = 0;
                        menu.ActualizarTextoBotonToggle(config);
                    }

                    if (config.esSlider && config.slider != null)
                    {
                        config.slider.value = 0;
                    }
                }
            }
        }
    }


    private void ProcesarCargaToggle(BotonConfig config)
    {
        PantallaConfig pantallaConfig = FindFirstObjectByType<PantallaConfig>();
        if (pantallaConfig != null)
        {

            if (config.tipoConfigPantalla == TipoConfigPantalla.Resolucion)
            {
                config.indiceOpcionToggle = PlayerPrefs.GetInt("ResolucionIndex", 0);
                ActualizarTextoBotonToggle(config);
                pantallaConfig.AplicarResolucionPorNombre(config.indiceOpcionToggle);
            }
            else if (config.tipoConfigPantalla == TipoConfigPantalla.ModoPantalla)
            {
                config.indiceOpcionToggle = PlayerPrefs.GetInt("ModoPantallaIndex", 0);
                ActualizarTextoBotonToggle(config);
                pantallaConfig.AplicarModoPantalla(config.indiceOpcionToggle);
            }
        }
    }
}
