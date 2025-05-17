using System.Collections;
using UnityEngine;

public class ValveInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private int numberOfTurns;
    [SerializeField] private float rotationSpeed;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;
    

    public void Interact(Transform interactorTransform)
    {
        objectManager.ValveActive = true;
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

        Destroy(this);
    }
}
