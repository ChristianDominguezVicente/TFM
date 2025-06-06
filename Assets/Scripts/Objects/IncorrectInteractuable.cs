using System.Collections;
using UnityEngine;

public class IncorrectInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;

    public string GetInteractText()
    {
        if (objectManager.Correct || objectManager.Incorrect)
        {
            int firstSpaceIndex = interactText.IndexOf(' ');
            if (firstSpaceIndex != -1 && firstSpaceIndex < interactText.Length - 1)
            {
                string objectName = interactText.Substring(firstSpaceIndex + 1);
                return "Change with " + objectName;
            }

            return "Change Object";
        }

        return interactText;
    }
    public Transform GetTransform() => transform;

    public void Interact(Transform interactorTransform)
    {
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

        if (objectManager.CurrentObject != null)
        {
            objectManager.CurrentObject.SetActive(true);
        }

        objectManager.CurrentObject = gameObject;

        // mark it in the ObjectManager
        objectManager.Incorrect = true;
        objectManager.Correct = false;
        Action();
    }

    public void Action()
    {
        SMSystem smsys = FindAnyObjectByType<SMSystem>();
        smsys.NeedsUIUpdate = true;
        // deactivates the object in the scene when interacted with
        gameObject.SetActive(false);
    }
}
