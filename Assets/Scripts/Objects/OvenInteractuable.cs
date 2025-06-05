using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OvenInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private string nextScene;
    [SerializeField] private CanvasGroup fadeOut;
    [SerializeField] private PossessionManager possessionManager;

    [Header("Restricted NPCs")]
    [SerializeField] private string[] restrictedNPCs;

    [Header("Acepted NPCs")]
    [SerializeField] private string[] acceptedNPCs;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;

    private string originalText;
    private bool showingWarning = false;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    private void Start()
    {
        // save original text
        originalText = interactText;
    }

    public void Interact(Transform interactorTransform)
    {
        // if there is a warning
        if (showingWarning) return;

        StartCoroutine(InteractCoroutine());
    }
    private IEnumerator InteractCoroutine()
    {
        var currentNpc = possessionManager.CurrentNPC;

        // if player possess a restricted NPC
        if (currentNpc != null && restrictedNPCs.Contains(currentNpc.NpcName))
        {
            StartCoroutine(ShowWarning("<color=red>Los niños no pueden usar el horno</color>"));
            yield break;
        }

        // if valve is not activated
        if (!objectManager.ValveActive)
        {
            if (cinematicDialogue != null)
            {
                cinematicDialogue.PlayDialogue();

                while (!cinematicDialogue.End)
                {
                    yield return null;
                }

                cinematicDialogue.End = false;
            }
        }

        // if player hasn't taken the ingredients
        if (!objectManager.Flour || !objectManager.Eggs || !objectManager.Sugar)
        {
            StartCoroutine(ShowWarning("<color=red>Faltan los ingredientes</color>"));
            yield break;
        }

        if (currentNpc != null && acceptedNPCs.Contains(currentNpc.NpcName))
        {
            StartCoroutine(FadeOut());
            yield break;
        }

        // if player hasn't taken the recipes
        if (!objectManager.Recipe1 || !objectManager.Recipe2)
        {
            
            StartCoroutine(ShowWarning("<color=red>Necesitas las dos partes de la receta</color>"));
            yield break;
        }

        SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
        float karma = PlayerPrefs.GetFloat("Karma", 0);
        karma--;
        ssm.SetKarma(karma);
        
        // if everything is ok
        StartCoroutine(FadeOut());
    }

    private IEnumerator ShowWarning(string warningText)
    {
        showingWarning = true;
        interactText = warningText;
        yield return new WaitForSeconds(2f);
        interactText = originalText;
        showingWarning = false;
    }

    private IEnumerator FadeOut()
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

        // load next level
        SceneManager.LoadScene(nextScene);
    }
}
