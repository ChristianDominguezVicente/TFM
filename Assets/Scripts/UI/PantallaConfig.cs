using System;
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
    [SerializeField] private Resolucion[] resoluciones;

    [Header("Modos de Pantalla")]
    [SerializeField] private Button btnModoPantalla;

    private int indiceResolucion = 0;
    private int indiceModoPantalla = 0;
    private bool usandoMando;

    private FullScreenMode[] modosPantalla = {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };

    private void Start()
    {
        CargarConfiguracion();
        ConfigurarBotones();
    }

    private void Update()
    {
        // Detección automática de input
        if (Input.GetJoystickNames().Any(x => !string.IsNullOrEmpty(x)))
            usandoMando = true;
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
    }

    public void CambiarModoPantalla()
    {
        indiceModoPantalla = (indiceModoPantalla + 1) % modosPantalla.Length;
        AplicarConfiguracion();
    }
    private void CargarConfiguracion()
    {
        indiceResolucion = PlayerPrefs.GetInt("ResolucionIndex", 0);
        indiceModoPantalla = PlayerPrefs.GetInt("ModoPantallaIndex", 0);
        AplicarConfiguracion();
    }
    public void CambiarModoPantallaDireccional(int direccion)
    {
        indiceModoPantalla = Mathf.Clamp(indiceModoPantalla + direccion, 0, modosPantalla.Length - 1);
        Debug.Log(indiceModoPantalla);
        AplicarConfiguracion();
    }

    private void AplicarConfiguracion()
    {
        // Aplicar resolución
        Screen.SetResolution(
            resoluciones[indiceResolucion].ancho,
            resoluciones[indiceResolucion].alto,
            modosPantalla[indiceModoPantalla],
            Screen.currentResolution.refreshRateRatio
        );

        // Guardar
        GuardarConfiguracion();
    }

    private void GuardarConfiguracion()
    {
        PlayerPrefs.SetInt("ResolucionIndex", indiceResolucion);
        PlayerPrefs.SetInt("ModoPantallaIndex", indiceModoPantalla);
        PlayerPrefs.Save();
    }
    public void AplicarModoPantalla(int indiceModo)
    {
        if (indiceModo >= 0 && indiceModo < modosPantalla.Length)
        {
            indiceModoPantalla = indiceModo;
            AplicarConfiguracion();
        }
    }

    public bool AplicarResolucionPorNombre(string nombreResolucion)
    {
        foreach (var res in resoluciones)
        {
            if (res.nombre == nombreResolucion)
            {
                Screen.SetResolution(res.ancho, res.alto, Screen.fullScreenMode);
                Debug.Log($"Resolución cambiada a: {res.ancho}x{res.alto}");
                return true;
            }
        }
        Debug.LogError($"Resolución no encontrada: {nombreResolucion}");
        return false;
    }


}
