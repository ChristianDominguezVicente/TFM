using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LookingInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private Transform offset;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private GameObject ui;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;

    private bool looking = false;
    private bool hasOriginalTransform = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform.parent;

    public void Interact(Transform interactorTransform)
    {
        StartCoroutine(InteractCoroutine());
    }

    private IEnumerator InteractCoroutine()
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
            if ((CompareTag("Note") || CompareTag("Photo")) && SceneManager.GetActiveScene().name != "Puzzle3")
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
            }
            else
            {
                StartLooking();
            }   
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

        if (CompareTag("Calendar"))
            objectManager.Calendar = true;
        else if (CompareTag("Icons"))
            objectManager.Icons = true;
        else if (CompareTag("Poster"))
            objectManager.Poster = true;
        else if (CompareTag("Tablet"))
            objectManager.Tablet = true;
        else if (CompareTag("Photo"))
            objectManager.Photo = true;
        else if (CompareTag("Note"))
            objectManager.Note = true;

        ui.SetActive(false);
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

        ui.SetActive(true);
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
