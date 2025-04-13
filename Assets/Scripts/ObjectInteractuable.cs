using UnityEngine;

public class ObjectInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    public void Interact(Transform interactorTransform)
    {
        // deactivates the object in the scene when interacted with
        gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
