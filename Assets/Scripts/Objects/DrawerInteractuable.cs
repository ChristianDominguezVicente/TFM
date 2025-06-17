using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrawerInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private float rotationAngle;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private GameObject go;

    [SerializeField] private Transform drawerObject;
    [SerializeField] private float openDistance;
    [SerializeField] private float openDuration;

    [Header("Restricted NPCs")]
    [SerializeField] private string[] restrictedNPCs;

    private string originalText;
    private bool showingWarning = false;
    private bool open = false;
    private Quaternion rotation;

    [Header("Refrigerator interact sound")]
    [SerializeField] private AudioClip refrigeratorInteractSound;
    [SerializeField] private AudioClip kitchenCabinetInteractSound;

    private AudioConfig audioConfig;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    private void Start()
    {
        // save original text
        originalText = interactText;
        audioConfig = (AudioConfig)FindAnyObjectByType(typeof(AudioConfig));
    }

    public void Interact(Transform interactorTransform)
    {
        // if there is a warning
        if (showingWarning || open) return;

        StartCoroutine(InteractCoroutine());
    }

    private void Update()
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

    private IEnumerator InteractCoroutine()
    {
        var currentNpc = possessionManager.CurrentNPC;

        if (restrictedNPCs.Contains(currentNpc.NpcName) && CompareTag("Drawer"))
        {
            StartCoroutine(ShowWarning("<color=red>No debería abrir los cajones</color>"));
            yield break;
        }
        else if (restrictedNPCs.Contains(currentNpc.NpcName) && CompareTag("Refrigerator"))
        {
            StartCoroutine(ShowWarning("<color=red>No debería abrir la nevera</color>"));
            yield break;
        }
        else if ((currentNpc.NpcName == "Jane" || currentNpc.NpcName == "Erick") && objectManager.Recipe1 && objectManager.Recipe2 && CompareTag("Refrigerator"))
        {
            StartCoroutine(ShowWarning("<color=red>No tengo nada que hacer en la nevera</color>"));
            yield break;
        }
        else if (CompareTag("DrawerLetter"))
        {
            StartCoroutine(MoveDrawer());
        }
        else
        {
            if(CompareTag("Refrigerator"))
            {
                audioConfig.SoundEffectSFX(refrigeratorInteractSound);
            }
            else if(CompareTag("Drawer"))
            {
                audioConfig.SoundEffectSFX(kitchenCabinetInteractSound);
            }
            // final rotation
            rotation = Quaternion.Euler(0, transform.parent.eulerAngles.y + rotationAngle, 0);
            open = true;
            go.SetActive(true);
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

    private IEnumerator MoveDrawer()
    {
        // activate master key
        go.SetActive(true);

        Vector3 startPos = drawerObject.localPosition;
        Vector3 targetPos = startPos + (drawerObject.forward * openDistance);
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            drawerObject.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / openDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        drawerObject.localPosition = targetPos;

        // destroy script
        Destroy(this);
    }
}
