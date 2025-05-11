using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PantallaConfig : MonoBehaviour
{
    [System.Serializable]
    public struct Resolucion
    {
        public int ancho;
        public int alto;
        public string nombre;
    }

    [Header("UI Resolución")]
    [SerializeField] private Button btnResolucion;
    [SerializeField] private TextMeshProUGUI textoResolucion;
    private Resolucion[] resoluciones = new Resolucion[]
    {
        new Resolucion { ancho = 1920, alto = 1080, nombre = "1920x1080" }, // Índice 0
        new Resolucion { ancho = 2560, alto = 1440, nombre = "2560x1440" }, // Índice 1
        new Resolucion { ancho = 3840, alto = 2160, nombre = "3840x2160" }, // Índice 2
        new Resolucion { ancho = 960, alto = 540, nombre = "960x540" },     // Índice 3
        new Resolucion { ancho = 1280, alto = 720, nombre = "1280x720" }    // Índice 4
    };

    [Header("Modos de Pantalla")]
    [SerializeField] private Button btnModoPantalla;

    private int indiceResolucion = 0;
    private int indiceModoPantalla = 0;
    private bool usandoMando;

    private readonly FullScreenMode[] modosPantalla = {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };

    private void Awake()
    {
        // Establecer 1920x1080 como valor por defecto (índice 0)
        indiceResolucion = 0;

        PlayerPrefs.SetInt("ResolucionIndex", 0); // 1920x1080
        PlayerPrefs.SetInt("ModoPantallaIndex", 0); // FullScreenWindow
    //    Debug.Log($"PlayerPrefs Resolución: {PlayerPrefs.GetInt("ResolucionIndex", -1)}");
        AplicarConfiguracionInmediata(true);
    }

    private void Start()
    {
        // Cargar configuración guardada (sobrescribe el valor por defecto si existe)
        CargarConfiguracion();
        ConfigurarBotones();

        // Actualizar UI para reflejar la configuración actual
        ActualizarUI();
    }

    private void CargarConfiguracion()
    {
        indiceResolucion = PlayerPrefs.GetInt("ResolucionIndex", 0); // 0 = 1920x1080 por defecto
        indiceModoPantalla = PlayerPrefs.GetInt("ModoPantallaIndex", 0);
        AplicarConfiguracion();
    }

    private void ActualizarUI()
    {
        if (textoResolucion != null)
        {
            textoResolucion.text = resoluciones[indiceResolucion].nombre;
        }
    }

    private void ConfigurarBotones()
    {
        btnResolucion.onClick.RemoveAllListeners();
        btnModoPantalla.onClick.RemoveAllListeners();

        btnResolucion.onClick.AddListener(() =>
        {
            if (!usandoMando) CambiarResolucion();
        });

        btnModoPantalla.onClick.AddListener(() =>
        {
            if (!usandoMando) CambiarModoPantalla();
        });
    }

    public void CambiarResolucion()
    {
        indiceResolucion = (indiceResolucion + 1) % resoluciones.Length;
        AplicarConfiguracion();
    //    Debug.Log($"Cambiada resolución a índice: {indiceResolucion} ({resoluciones[indiceResolucion].nombre})");
    }

    public void CambiarModoPantalla()
    {
        indiceModoPantalla = (indiceModoPantalla + 1) % modosPantalla.Length;
        AplicarConfiguracion();
    }

    private void AplicarConfiguracion()
    {
        try
        {
            Screen.SetResolution(
                resoluciones[indiceResolucion].ancho,
                resoluciones[indiceResolucion].alto,
                modosPantalla[indiceModoPantalla],
                Screen.currentResolution.refreshRateRatio
            );

            if (modosPantalla[indiceModoPantalla] == FullScreenMode.Windowed)
            {
                StartCoroutine(AjustarVentana(resoluciones[indiceResolucion].ancho, resoluciones[indiceResolucion].alto));
            }

            GuardarConfiguracion();
            ActualizarUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error aplicando configuración: {e.Message}");
            ResetearAValoresPorDefecto();
        }
    }

    public bool AplicarResolucionPorNombre(int indiceResolucion)
    {
        for (int i = 0; i < resoluciones.Length; i++)
        {
            if (i == indiceResolucion)
            {
                indiceResolucion = i; // Actualizar el índice para mantener sincronización
                Screen.SetResolution(resoluciones[i].ancho, resoluciones[i].alto, modosPantalla[indiceModoPantalla]);
                Debug.Log($"Resolución cambiada por nombre: {indiceResolucion} (Índice: {i})");
                return true;
            }
        }
        Debug.LogError($"Resolución no encontrada: {indiceResolucion}");
        return false;
    }

    // Resto de los métodos permanecen igual...
    private void GuardarConfiguracion()
    {
        PlayerPrefs.SetInt("ResolucionIndex", indiceResolucion);
        PlayerPrefs.SetInt("ModoPantallaIndex", indiceModoPantalla);
        PlayerPrefs.Save();
    }

    private IEnumerator AjustarVentana(int ancho, int alto)
    {
        yield return new WaitForEndOfFrame();
        Screen.SetResolution(ancho, alto, FullScreenMode.Windowed);
    }

    private void ResetearAValoresPorDefecto()
    {
        indiceResolucion = 0; // Volver a 1920x1080
        indiceModoPantalla = 0;
        AplicarConfiguracion();
    }

    private void AplicarConfiguracionInmediata(bool esPorDefecto)
    {
        try
        {
            int indice = PlayerPrefs.GetInt("ResolucionIndex", 0);
            int ancho = resoluciones[indice].ancho; // Siempre usa 1920x1080 (índice 0)
            int alto = resoluciones[indice].alto;
            var modo = modosPantalla[0];

            Screen.SetResolution(ancho, alto, modo);

            if (modo == FullScreenMode.Windowed)
            {
                StartCoroutine(AjustarVentana(ancho, alto));
            }

            Debug.Log($"Configuración inmediata aplicada: {ancho}x{alto}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error aplicando configuración inmediata: {e.Message}");
        }
    }

    public void AplicarModoPantalla(int indiceModo)
    {
        if (indiceModo >= 0 && indiceModo < modosPantalla.Length)
        {
            indiceModoPantalla = indiceModo;
            AplicarConfiguracion();
        }
    }


    public void AplicarPorDefectoTodo(bool forzar = false)
    {
        try
        {
            //  forzando 1280x720
            Screen.SetResolution(
                resoluciones[0].ancho,
                resoluciones[0].alto,
                modosPantalla[0]
            );

            //       Debug.Log($"Resolución aplicada: {ancho}x{alto}");

            GuardarConfiguracion();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error aplicando configuración: {e.Message}");

            // Fallback a la primera resolución
            indiceResolucion = 0;
            indiceModoPantalla = 0;
        }
    }
}
