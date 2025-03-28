using UnityEngine;

public interface IInteractuable
{
    void Interact(Transform interactorTransform);
    string GetInteractText();
    Transform GetTransform();
}
