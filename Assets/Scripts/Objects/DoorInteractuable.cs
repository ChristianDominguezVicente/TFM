using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DoorInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private float rotationAngle;
    [SerializeField] private float rotationSpeed;

    private bool open = false;
    private Quaternion rotation;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    public void Interact(Transform interactorTransform)
    {
        if (!open)
        {
            // final rotation
            rotation = Quaternion.Euler(0, transform.parent.eulerAngles.y + rotationAngle, 0);
            open = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (open)
        {
            // if current rotation has not reach final rotation
            if (Quaternion.Angle(transform.parent.rotation, rotation) > 0.1f)
            {
                transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                // destroy script
                Destroy(this);
            }
        }
    }
}
