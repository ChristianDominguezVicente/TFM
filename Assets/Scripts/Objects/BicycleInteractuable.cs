using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BicycleInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private CanvasGroup fadeOut;
    [SerializeField] private PossessionManager possessionManager;

    [Header("Accepted NPC")]
    [SerializeField] private string acceptedNPC;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;
    [SerializeField] private CinematicDialogue cinematicDialogue2;

    private string originalText;
    private bool showingWarning = false;
    private string nextScene = "Transicion23";
    private AudioConfig audioConfig;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    private void Start()
    {
        // save original text
        originalText = interactText;
        audioConfig = (AudioConfig)FindAnyObjectByType(typeof(AudioConfig));
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
        if (currentNpc != null && !acceptedNPC.Contains(currentNpc.NpcName))
        {
            StartCoroutine(ShowWarning($"<color=red>Esta persona no se atreve a arreglar la bicicleta</color>"));
        }
        // if valve is not activated
        else if (!objectManager.Teddy || !objectManager.ToolBox || objectManager.CurrentObject == null || !objectManager.GiftPaper)
        {
            if (cinematicDialogue2 != null)
            {
                cinematicDialogue2.PlayDialogue();

                while (!cinematicDialogue2.End)
                {
                    yield return null;
                }

                cinematicDialogue2.End = false;
            }
        }
        else
        {
            SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
            float karma = ssm.GetKarma();
            if (karma < 0)
            {
                nextScene = "Transicion4";
                if (objectManager.Incorrect)
                {
                    ssm.SetKarma(-1);
                }
            }
            else if (karma == 0)
            {
                nextScene = "Transicion23";
                if (objectManager.Incorrect)
                {
                    ssm.SetKarma(-1);
                }
            }

            StartCoroutine(FadeOut());

            if (cinematicDialogue != null)
            {
                cinematicDialogue.PlayDialogue();

                while (!cinematicDialogue.End)
                {
                    yield return null;
                }

                cinematicDialogue.End = false;
            }

            // load next level
            SceneManager.LoadScene(nextScene);
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

        //FadeOut the music
        audioConfig.ApplyFadeOut(); 
    }
}
