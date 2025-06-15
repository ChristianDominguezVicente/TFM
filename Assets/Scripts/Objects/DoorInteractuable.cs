using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteractuable : MonoBehaviour, IInteractuable
{
    [Header("Principal")]
    [SerializeField] private string interactText;
    [SerializeField] private float rotationAngle;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private HintManager hintManager;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private NPCPossessable paul;

    [Header("Back")]
    [SerializeField] private Vector3 movement;
    [SerializeField] private float moveSpeed;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue backCinematicDialogue;
    [SerializeField] private CinematicDialogue principalCinematicDialogue;
    [SerializeField] private DialogueData[] dialogueDataArray;
    [SerializeField] private DialogueData dialogueDataListen;

    private bool open = false;
    private Quaternion rotation;
    private Vector3 targetPosition; 

    private AudioConfig audioConfig;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    public void Start()
    {
        audioConfig = (AudioConfig)FindAnyObjectByType(typeof(AudioConfig));
    }

    public void Interact(Transform interactorTransform)
    {
        // if there is a warning
        if (open) return;

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
            if (Quaternion.Angle(transform.parent.rotation, rotation) > 0.1f && CompareTag("Principal"))
            {
                transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
            else if (Vector3.Distance(transform.parent.position, targetPosition) > 0.01f && CompareTag("Back"))
            {
                transform.parent.position = Vector3.MoveTowards(transform.parent.position, targetPosition, Time.deltaTime * moveSpeed);
            }
            else
            {
                if (gameObject.CompareTag("Principal"))
                {
                    objectManager.PrincipalDoor = true;
                    ActionSM();
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
        open = true;
        fade.gameObject.SetActive(true);

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fade.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        //FadeOut the music
        audioConfig.ApplyFadeOut();

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

        //FadeIn the music
        audioConfig.ApplyFadeIn();
    }

    public void Action()
    {
        if(CompareTag("Principal"))
        {
            // final rotation
            rotation = Quaternion.Euler(0, transform.parent.eulerAngles.y + rotationAngle, 0);
            open = true;
        }
        else if (CompareTag("Back"))
        {
            targetPosition = transform.parent.position + movement;
            open = true;
        }
    }
    public void ActionSM()
    {
        SMSystem smsys = FindAnyObjectByType<SMSystem>();
        smsys.NeedsUIUpdate = true;
        
        
    }
}
