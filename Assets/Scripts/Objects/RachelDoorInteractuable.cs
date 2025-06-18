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
    [SerializeField] private string nextScene = "Final";

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;
    [SerializeField] private CinematicDialogue cinematicDialogue2;

    private string originalText;
    private bool showingWarning = false;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    private AudioConfig audioConfig;

    private void Start()
    {
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
        SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
        float karma = ssm.GetKarma();
        if (SceneManager.GetActiveScene().name == "Puzzle2" && karma < 0)
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
        else if (SceneManager.GetActiveScene().name == "Puzzle2" && karma == 0)
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
            if (!objectManager.Incorrect && !objectManager.Correct)
            {
                StartCoroutine(ShowWarning("<color=red>Necesitas traer un objeto</color>"));
            }
            else if (objectManager.Incorrect)
            {
                StartCoroutine(FadeOut());

                if (cinematicDialogue2 != null)
                {
                    cinematicDialogue2.PlayDialogue();

                    while (!cinematicDialogue2.End)
                    {
                        yield return null;
                    }

                    cinematicDialogue2.End = false;
                }

                ssm.SetKarma(-1); 

                // load next level
                SceneManager.LoadScene(nextScene);

            }
            else if (objectManager.Correct)
            {
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

                ssm.SetKarma(1);

                // load next level
                SceneManager.LoadScene(nextScene);
            }
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
