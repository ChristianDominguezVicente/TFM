using UnityEngine;

public class NPCInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;

    public string GetInteractText()
    {
        return interactText;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact(Transform interactorTransform)
    {
        Debug.Log(gameObject);
    }

}
