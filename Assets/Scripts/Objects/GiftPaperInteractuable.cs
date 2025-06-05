using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GiftPaperInteractuable : MonoBehaviour, IInteractuable
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

        if (SceneManager.GetActiveScene().name != "Puzzle 2")
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
        else
        {
            // mark it in the ObjectManager
            objectManager.GiftPaper = true;
            // deactivates the object in the scene when interacted with
            gameObject.SetActive(false);
        }
    }
}
