using UnityEngine;

public interface IPossessable : IInteractTarget
{
    void Possess(Transform interactorTransform);
    string GetPossessText();
}
