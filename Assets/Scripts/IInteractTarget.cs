using UnityEngine;

// Base interface for any object that can be the target interaction or possession
public interface IInteractTarget
{
    // returns the Transform of the interactuable or possessable object
    Transform GetTransform();
}
