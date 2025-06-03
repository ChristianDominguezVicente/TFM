using UnityEngine;

public class MasterKeyInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    public void Interact(Transform interactorTransform)
    {
        // mark it in the ObjectManager
        objectManager.MasterKeyTaken = true;
        SMSystem smsys = FindAnyObjectByType<SMSystem>();
        smsys.NeedsUIUpdate = true;
        // deactivates the object in the scene when interacted with
        gameObject.SetActive(false);
    }
}
