using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class CalendarInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private Transform offset;
    [SerializeField] private ObjectManager objectManager;

    private bool looking = false;
    private bool hasOriginalTransform = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform.parent;

    public void Interact(Transform interactorTransform)
    {
        if (!hasOriginalTransform)
        {
            originalPosition = transform.parent.position;
            originalRotation = transform.parent.rotation;
            hasOriginalTransform = true;
        }

        if (!looking)
        {
            StartLooking();
        }
        else
        {
            StopLooking();
        }
    }

    private void StartLooking()
    {
        looking = true;

        if (objectManager != null)
        {
            objectManager.Looking = true;
            objectManager.LookingObject = transform.parent;
        }
            
    }

    private void StopLooking()
    {
        looking = false;

        transform.parent.position = originalPosition;
        transform.parent.rotation = originalRotation;

        if (objectManager != null)
        {
            objectManager.Looking = false;
            objectManager.LookingObject = null;
        }
    }

    private void Update()
    {
        if (looking && objectManager.Looking)
        {
            if (offset != null)
            {
                transform.parent.position = Vector3.Lerp(transform.parent.position, offset.position, 0.2f);
            }
        }
        else if (objectManager.LookingObject != null && !objectManager.Looking)
        {
            StopLooking();
        }
    }
}
