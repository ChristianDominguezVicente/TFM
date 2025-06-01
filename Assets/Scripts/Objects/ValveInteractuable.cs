using System.Collections;
using UnityEngine;

public class ValveInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private int numberOfTurns;
    [SerializeField] private float rotationSpeed;

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
        objectManager.ValveActive = true;
        // valve rotation
        StartCoroutine(Rotate());
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
}
