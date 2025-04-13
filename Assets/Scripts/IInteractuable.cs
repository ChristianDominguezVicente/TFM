using UnityEngine;

// Interface for objects that can be simply interacted with.
// Inherits from IInteractTarget, which means it also has to return its Transform
public interface IInteractuable : IInteractTarget
{
    // logic that executes when interacting
    void Interact(Transform interactorTransform);
    // text to display in UI when the object can be interacted
    string GetInteractText();
}
