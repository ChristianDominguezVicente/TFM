using System.Collections;
using System.Linq;
using UnityEngine;

public class EggsInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
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

        if (restrictedNPCs.Contains(currentNpc.NpcName))
        {
            StartCoroutine(ShowWarning("<color=red>No debería abrir la nevera</color>"));
            yield break;
        }
        else if ((currentNpc.NpcName == "Jane" || currentNpc.NpcName == "Erick") && objectManager.Recipe1 && objectManager.Recipe2)
        {
            StartCoroutine(ShowWarning("<color=red>No tengo nada que hacer en la nevera</color>"));
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
            objectManager.Eggs = true;
            // deactivates the object in the scene when interacted with
            gameObject.SetActive(false);
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
}
