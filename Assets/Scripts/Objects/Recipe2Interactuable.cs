using System.Collections;
using System.Linq;
using UnityEngine;

public class Recipe2Interactuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private PossessionManager possessionManager;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue correctCinematicDialogue;
    [SerializeField] private CinematicDialogue incorrectCinematicDialogue;

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
            StartCoroutine(ShowWarning("<color=red>Los niños no se atreven a hablar con Rachel</color>"));
            yield break;
        }
        else if ((currentNpc.NpcName == "Henry" || currentNpc.NpcName == "Erick"))
        {
            if (incorrectCinematicDialogue != null)
            {
                incorrectCinematicDialogue.PlayDialogue();

                while (!incorrectCinematicDialogue.End)
                {
                    yield return null;
                }

                incorrectCinematicDialogue.End = false;
            }
        }
        else
        {
            if (correctCinematicDialogue != null)
            {
                correctCinematicDialogue.PlayDialogue();

                while (!correctCinematicDialogue.End)
                {
                    yield return null;
                }

                correctCinematicDialogue.End = false;
            }

            // mark it in the ObjectManager
            objectManager.Recipe2 = true;
            Destroy(this);
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
