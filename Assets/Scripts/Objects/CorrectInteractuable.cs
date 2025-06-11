using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CorrectInteractuable : MonoBehaviour, IInteractuable
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

    public string GetInteractText()
    {
        if (objectManager.Correct || objectManager.Incorrect)
        {
            int firstSpaceIndex = interactText.IndexOf(' ');
            if (firstSpaceIndex != -1 && firstSpaceIndex < interactText.Length - 1)
            {
                string objectName = interactText.Substring(firstSpaceIndex + 1);
                return "Cambiar por " + objectName;
            }

            return "Cambiar Objeto";
        }

        return interactText;
    }
    public Transform GetTransform() => transform;

    public void Interact(Transform interactorTransform)
    {
        // if there is a warning
        if (showingWarning) return;

        StartCoroutine(InteractCoroutine());
    }

    private IEnumerator InteractCoroutine()
    {
        var currentNpc = possessionManager.CurrentNPC;

        if (CompareTag("OriginalChain") && SceneManager.GetActiveScene().name != "Puzzle2")
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
        else if (CompareTag("Letter") && SceneManager.GetActiveScene().name != "Puzzle4")
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
        else if (CompareTag("Draw") && SceneManager.GetActiveScene().name != "Puzzle4")
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
        else if(CompareTag("OriginalChain") && SceneManager.GetActiveScene().name == "Puzzle2" && restrictedNPCs.Contains(currentNpc.NpcName))
        {
            StartCoroutine(ShowWarning("<color=red>No hay nada que hacer con esa bicicleta</color>"));
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

            if (objectManager.CurrentObject != null)
            {
                objectManager.CurrentObject.SetActive(true);
            }

            objectManager.CurrentObject = gameObject;

            // mark it in the ObjectManager
            objectManager.Correct = true;
            objectManager.Incorrect = false;
            Action();
        } 
    }
    public void Action()
    {
        SMSystem smsys = FindAnyObjectByType<SMSystem>();
        smsys.NeedsUIUpdate = true;

        // deactivates the object in the scene when interacted with
        gameObject.SetActive(false);
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
