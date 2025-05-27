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
        // save original position and rotation
        if (!hasOriginalTransform)
        {
            originalPosition = transform.parent.position;
            originalRotation = transform.parent.rotation;
            hasOriginalTransform = true;
        }

        // activate looking
        if (!looking)
        {
            StartLooking();
        }
        // deactivate looking
        else
        {
            StopLooking();
        }
    }

    private void StartLooking()
    {
        looking = true;

        originalPosition = transform.parent.position;
        originalRotation = transform.parent.rotation;
        hasOriginalTransform = true;

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
        // if player is looking
        if (objectManager.LookingObject == transform.parent && looking && objectManager.Looking)
        {
            // rotate the object
            if (offset != null)
            {
                transform.parent.position = Vector3.Lerp(transform.parent.position, offset.position, 0.2f);
            }
        }
        // if player is not looking
        else if (looking && !objectManager.Looking)
        {
            StopLooking();
        }
    }
}
