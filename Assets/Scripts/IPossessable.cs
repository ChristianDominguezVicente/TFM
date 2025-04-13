using UnityEngine;

// Interface for objects that can be possessed by the player.
// Inherits from IInteractTarget, which means it also has to return its Transform
public interface IPossessable : IInteractTarget
{
    // logic that runs when the player possesses the NPC
    void Possess(Transform interactorTransform);
    // text to display in UI when the object can be possessed
    string GetPossessText();
}
