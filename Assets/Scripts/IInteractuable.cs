using UnityEngine;

public interface IInteractuable : IInteractTarget
{
    void Interact(Transform interactorTransform);
    string GetInteractText();
}
