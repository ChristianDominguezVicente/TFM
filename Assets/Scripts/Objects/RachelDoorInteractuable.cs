using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RachelDoorInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private float rotationAngle;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private CanvasGroup fadeOut;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;

    private string originalText;
    private bool showingWarning = false;
    private string nextScene = "Final";

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    private void Start()
    {
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
        if (cinematicDialogue != null)
        {
            cinematicDialogue.PlayDialogue();

            while (!cinematicDialogue.End)
            {
                yield return null;
            }

            cinematicDialogue.End = false;
        }

        if (!objectManager.Incorrect && !objectManager.Correct)
        {
            StartCoroutine(ShowWarning("<color=red>You need a object</color>"));
        }
        else if (objectManager.Incorrect)
        {
            SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
            float karma = PlayerPrefs.GetFloat("Karma", 0);
            nextScene = "Final Bueno";
            karma++;
            ssm.SetKarma(karma);
            StartCoroutine(FadeOut());
            
        }
        else if (objectManager.Correct)
        {
            SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
            float karma = PlayerPrefs.GetFloat("Karma", 0);
            nextScene = "Final Malo";
            karma--;
            ssm.SetKarma(karma);
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
