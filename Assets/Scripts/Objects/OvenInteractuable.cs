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

        var currentNpc = possessionManager.CurrentNPC;

        // if player possess a restricted NPC
        if (currentNpc != null && restrictedNPCs.Contains(currentNpc.NpcName))
        {
            StartCoroutine(ShowWarning($"<color=red>Children should not use it</color>"));
        }
        // if valve is not activated
        else if (!objectManager.ValveActive)
        {
            StartCoroutine(ShowWarning("<color=red>You must turn the valve first</color>"));
        }
        // if player hasn't taken the recipes
        else if (!(objectManager.Recipe1 && objectManager.Recipe2))
        {
            StartCoroutine(ShowWarning("<color=red>You need both recipes</color>"));
        }
        // if player hasn't taken the ingredients
        else if (!(objectManager.Flour && objectManager.Eggs && objectManager.Sugar))
        {
            StartCoroutine(ShowWarning("<color=red>Missing ingredients</color>"));
        }
        // if everything is ok
        else
        {
            StartCoroutine(FadeOut());
        }
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
