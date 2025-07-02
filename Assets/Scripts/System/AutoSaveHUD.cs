using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AutoSaveHUD : MonoBehaviour
{
    [SerializeField] private GameObject autoSaveIcon;
    [SerializeField] private float displayDurationSeconds = 2f; // How long the icon is displayed in seconds

    private Coroutine currentDisplayCoroutine; // To control the icon's hiding routine

    void Awake()
    {
        if (autoSaveIcon == null)
        {
          //  Debug.LogError("AutoSave Icon (Image) no asignado en AutoSaveHUD. Desactivando script.");
            this.enabled = false; // Disable the script if there is no icon to avoid errors
            return;
        }
        // hidden at the beginning
        autoSaveIcon.gameObject.SetActive(false);
    }

    public void ShowAutoSaveIcon()
    {
        if (autoSaveIcon == null) return;

        // If a hide routine is already active, we stop it to restart the timer.
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
        }


        autoSaveIcon.gameObject.SetActive(true);
        // Start a new routine to hide it after a while.
        currentDisplayCoroutine = StartCoroutine(HideAutoSaveIconAfterDelay());
    }

    private IEnumerator HideAutoSaveIconAfterDelay()
    {
        yield return new WaitForSeconds(displayDurationSeconds); // Espera el tiempo especificado
        autoSaveIcon.gameObject.SetActive(false); // Oculta el icono
        currentDisplayCoroutine = null; // Reinicia la referencia a la corrutina
    }
}
