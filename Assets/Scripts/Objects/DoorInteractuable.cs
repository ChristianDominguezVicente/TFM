using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DoorInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private float rotationAngle;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private ObjectManager objectManager;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;

    private bool open = false;
    private Quaternion rotation;

    private bool showingWarning = false;
    private string originalText;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    private void Start()
    {
        // save original text
        originalText = interactText;
    }

    public void Interact(Transform interactorTransform)
    {
        StartCoroutine(InteractCoroutine());
    }

    private IEnumerator InteractCoroutine()
    {
        if (cinematicDialogue != null)
        {
            cinematicDialogue.PlayDialogue();

            while (!cinematicDialogue.End)
            {
                yield return null;
            }

            cinematicDialogue.End = false;
        }

        if ((!open && gameObject.CompareTag("Principal")) || (!open && gameObject.CompareTag("Back") && objectManager.PrincipalDoor))
        {
            // final rotation
            rotation = Quaternion.Euler(0, transform.parent.eulerAngles.y + rotationAngle, 0);
            open = true;
        }
        else
        {
            StartCoroutine(ShowWarning($"<color=red>Open Principal Door first</color>"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (showingWarning) return;

        if (open)
        {
            // if current rotation has not reach final rotation
            if (Quaternion.Angle(transform.parent.rotation, rotation) > 0.1f)
            {
                transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                if (gameObject.CompareTag("Principal"))
                    objectManager.PrincipalDoor = true;

                // destroy script
                Destroy(this);
            }
        }
    }

    private IEnumerator ShowWarning(string warningText)
    {
        showingWarning = true;
        interactText = warningText;
        yield return new WaitForSeconds(2f);
        interactText = originalText;
        showingWarning = false;
    }
}
