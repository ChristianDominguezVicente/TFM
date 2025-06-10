using System.Collections;
using System.Linq;
using UnityEngine;

public class EggsInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;


    public string GetInteractText() => interactText;
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

        // mark it in the ObjectManager
        objectManager.Eggs = true;
        // deactivates the object in the scene when interacted with
        gameObject.SetActive(false); 
    }
}
