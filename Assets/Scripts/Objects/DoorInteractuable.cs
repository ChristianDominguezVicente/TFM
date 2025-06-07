using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private float rotationAngle;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private HintManager hintManager;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private NPCPossessable paul;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue backCinematicDialogue;
    [SerializeField] private CinematicDialogue principalCinematicDialogue;
    [SerializeField] private DialogueData[] dialogueDataArray;
    [SerializeField] private DialogueData dialogueDataListen;

    private bool open = false;
    private Quaternion rotation;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    public void Interact(Transform interactorTransform)
    {
        StartCoroutine(InteractCoroutine());
    }

    private IEnumerator InteractCoroutine()
    {
        if (!open && gameObject.CompareTag("Principal"))
        {
            StartCoroutine(FadeOut());
        }
        else if (!open && gameObject.CompareTag("Back") && objectManager.PrincipalDoor)
        {
            Action();
        }
        else
        {
            if (backCinematicDialogue != null)
            {
                backCinematicDialogue.PlayDialogue();

                while (!backCinematicDialogue.End)
                {
                    yield return null;
                }

                backCinematicDialogue.End = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (open)
        {
            // if current rotation has not reach final rotation
            if (Quaternion.Angle(transform.parent.rotation, rotation) > 0.1f)
            {
                transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                if (gameObject.CompareTag("Principal"))
                {
                    objectManager.PrincipalDoor = true;
                    hintManager.DialogueDataArray = dialogueDataArray;
                    paul.ListeningDialogueData = dialogueDataListen;
                }
                    
                // destroy script
                Destroy(this);
            }
        }
    }

    private IEnumerator FadeOut()
    {
        fade.gameObject.SetActive(true);

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fade.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        StartCoroutine(PlayStart());
    }

    private IEnumerator FadeInCoroutine()
    {
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fade.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }
    }

    private IEnumerator PlayStart()
    {
        principalCinematicDialogue.PlayDialogue();

        while (!principalCinematicDialogue.End)
        {
            yield return null;
        }

        Action();

        StartCoroutine(FadeInCoroutine());
    }

    public void Action()
    {
        // final rotation
        rotation = Quaternion.Euler(0, transform.parent.eulerAngles.y + rotationAngle, 0);
        open = true;
    }
}
