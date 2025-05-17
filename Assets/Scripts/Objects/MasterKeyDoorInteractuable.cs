using System.Collections;
using UnityEngine;

public class MasterKeyDoorInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private float rotationAngle;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private ObjectManager objectManager;

    private bool open = false;
    private Quaternion rotation;
    private string originalText;
    private bool showingKeyWarning = false;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    private void Start()
    {
        originalText = interactText;
    }

    public void Interact(Transform interactorTransform)
    {
        if (!open && objectManager.MasterKeyTaken)
        {
            rotation = Quaternion.Euler(0, transform.parent.eulerAngles.y + rotationAngle, 0);
            open = true;
        }
        else if (!open && !showingKeyWarning)
        {
            StartCoroutine(ShowWarning());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (open)
        {
            if (Quaternion.Angle(transform.parent.rotation, rotation) > 0.1f)
            {
                transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                Destroy(this);
            }
        }
    }

    private IEnumerator ShowWarning()
    {
        showingKeyWarning = true;
        interactText = "<color=red>You need the Master Key</color>";
        yield return new WaitForSeconds(2f);
        interactText = originalText;
        showingKeyWarning = false;
    }
}
