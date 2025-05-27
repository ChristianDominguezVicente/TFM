using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BicycleInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private string nextScene;
    [SerializeField] private CanvasGroup fadeOut;
    [SerializeField] private PossessionManager possessionManager;

    [Header("Accepted NPC")]
    [SerializeField] private string acceptedNPC;

    private string originalText;
    private bool showingWarning = false;
    private bool fixedbycicle = false;

    public string GetInteractText()
    {
        if(fixedbycicle && !showingWarning)
        {
            interactText = "Wrap the bicycle";
        }
        return interactText;
    } 
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
        if (currentNpc != null && !acceptedNPC.Contains(currentNpc.NpcName))
        {
            StartCoroutine(ShowWarning($"<color=red>I dont know how to fix it</color>"));
        }
        // if valve is not activated
        else if (!objectManager.Teddy)
        {
            StartCoroutine(ShowWarning("<color=red>You didnt grab the teddy</color>"));
        }
        // if player hasn't taken the recipes
        else if (!objectManager.ToolBox)
        {
            StartCoroutine(ShowWarning("<color=red>You need the toolbox to fix it</color>"));
        }
        // if player hasn't taken the ingredients
        else if (objectManager.CurrentObject == null)
        {
            StartCoroutine(ShowWarning("<color=red>You need the chain to fix it</color>"));
        }
        // if everything is ok
        else if(objectManager.CurrentObject != null && !fixedbycicle)
        {
            StartCoroutine(ShowFixedBicycleMessage("<color=green>You did it! Now grab a gift paper to wrap</color>"));
            
        }
        else if(!objectManager.GiftPaper) 
        {
            StartCoroutine(ShowWarning("<color=red>You need the gift paper to wrap it</color>"));
        }
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

    private IEnumerator ShowFixedBicycleMessage(string warningText)
    {
        showingWarning = true;
        interactText = warningText;
        yield return new WaitForSeconds(2f);
        interactText = originalText;
        showingWarning = false;
        fixedbycicle = true;
    }
}
