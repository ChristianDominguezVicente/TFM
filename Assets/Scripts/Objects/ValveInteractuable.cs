using System.Collections;
using System.Linq;
using UnityEngine;

public class ValveInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private int numberOfTurns;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private PossessionManager possessionManager;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;

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

        StartCoroutine(InteractCoroutine());
    }

    private IEnumerator InteractCoroutine()
    {
        var currentNpc = possessionManager.CurrentNPC;

        // if player possess a restricted NPC
        if (restrictedNPCs.Contains(currentNpc.NpcName))
        {
            StartCoroutine(ShowWarning("<color=red>Los niños no saben encender la válvula</color>"));
            yield break;
        }
        else if (currentNpc.NpcName == "Jane" || currentNpc.NpcName == "Henry")
        {
            StartCoroutine(ShowWarning("<color=red>Este adulto no se atreve a abrir la válvula</color>"));
            yield break;
        }
        else
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

            // mark it in the ObjectManager
            objectManager.ValveActive = true;
            Action();
        } 
    }

    private IEnumerator Rotate()
    {
        float totalRotation = 360f * numberOfTurns;
        float currentRotation = 0f;

        while (currentRotation < totalRotation)
        {
            float step = rotationSpeed * Time.deltaTime;
            transform.parent.Rotate(Vector3.forward, step);
            currentRotation += step;
            yield return null;
        }

        // destroy script
        Destroy(this);
    }

    private IEnumerator ShowWarning(string warningText)
    {
        showingWarning = true;
        interactText = warningText;
        yield return new WaitForSeconds(2f);
        interactText = originalText;
        showingWarning = false;
    }

    public void Action()
    {
        SMSystem smsys = FindAnyObjectByType<SMSystem>();
        smsys.NeedsUIUpdate = true;
        // valve rotation
        StartCoroutine(Rotate());
    }
}
