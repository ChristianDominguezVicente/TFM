using UnityEngine;

public class MasterKeyInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    public void Interact(Transform interactorTransform)
    {
        objectManager.MasterKeyTaken = true;
        // deactivates the object in the scene when interacted with
        gameObject.SetActive(false);
    }
}
