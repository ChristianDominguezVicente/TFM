using System;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CodeInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private CodeUI codeUI;
    [SerializeField] private int[] code = new int[4];

    [Header("Desk")]
    [SerializeField] private Transform drawerObject;
    [SerializeField] private float openDistance;
    [SerializeField] private float openDuration;
    [SerializeField] private GameObject masterKey;

    [Header("BoxLvl2")]
    [SerializeField] private float rotationAngle;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private PossessionManager possessionManager;

    [Header("Diary")]
    [SerializeField] private string nextScene;
    [SerializeField] private CanvasGroup fadeOut;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;
    [SerializeField] private CinematicDialogue cinematicDialogue2;
    [SerializeField] private CinematicDialogue cinematicDialogue3;
    [SerializeField] private CinematicDialogue cinematicDialogue4;

    [Header("Restricted NPCs")]
    [SerializeField] private string[] restrictedNPCs;

    private string originalText;
    private bool showingWarning = false;
    private bool open = false;
    private Quaternion rotation;
    private bool first = false;
    private AudioConfig audioConfig;

    [Header("Interaction Sounds")]
    [SerializeField] private AudioClip unlockMechanismSound;
    [SerializeField] private AudioClip openDrawerSound;
    [SerializeField] private AudioClip npcNotAllowedSound;


    public int[] Code { get => code; set => code = value; }

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform.parent;

    private void Start()
    {
        // save original text
        originalText = interactText;
        audioConfig = (AudioConfig)FindAnyObjectByType(typeof(AudioConfig));
    }

    public void Interact(Transform interactorTransform)
    {
        // if there is a warning
        if (showingWarning) return;

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
                Destroy(this);
                //masterKey.SetActive(true);
            }
        }
    }

    private IEnumerator InteractCoroutine()
    {
        var currentNpc = possessionManager.CurrentNPC;

        // if in the puzzle 2 Lia interact with the box that contains the key master, it unlock automatically
        if (CompareTag("Box") && SceneManager.GetActiveScene().name != "Puzzle2")
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
        else if (CompareTag("Box") && restrictedNPCs.Contains(currentNpc.NpcName) && SceneManager.GetActiveScene().name == "Puzzle2")
        {
            audioConfig.SoundEffectSFX(npcNotAllowedSound);
            StartCoroutine(ShowWarning("<color=red>No hay nada de interés en la estantería</color>"));
            yield break;
        }
        else if (CompareTag("Box") && possessionManager.CurrentNPC.NpcName == "Lia" && SceneManager.GetActiveScene().name == "Puzzle2")
        {
            audioConfig.SoundEffectSFX(unlockMechanismSound);
            masterKey.SetActive(true);
            UnlockBox();
        }
        // if player possess a restricted NPC
        else if (CompareTag("Desk") && currentNpc != null && restrictedNPCs.Contains(currentNpc.NpcName))
        {
            audioConfig.SoundEffectSFX(npcNotAllowedSound);
            StartCoroutine(ShowWarning("<color=red>Jane no te permite tocar el cajón</color>"));
            yield break;
        }
        else if (CompareTag("Diary") && !first)
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

            first = true;
            codeUI.Show(this);
        }
        // show UI
        else
        {
            codeUI.Show(this);
        }
    }

    // when the correct code is entered
    public void OnCorrectCode()
    {
        codeUI.Hide();
        audioConfig.SoundEffectSFX(unlockMechanismSound);

        if (CompareTag("Desk"))
        {
            StartCoroutine(MoveDrawer());
        }
        else if (CompareTag("Diary"))
        {
            StartCoroutine(Diary());
        }
        else if(CompareTag("Box"))
        {
            masterKey.SetActive(true);
            UnlockBox();
        }  
    }

    private IEnumerator Diary()
    {
        SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
        float karma = ssm.GetKarma();
        if (karma == 0)
        {
            ssm.SetKarma(1);

            if (cinematicDialogue2 != null)
            {
                cinematicDialogue2.PlayDialogue();

                while (!cinematicDialogue2.End)
                {
                    yield return null;
                }

                cinematicDialogue2.End = false;
            }
        }
        else if (karma == -1)
        {
            ssm.SetKarma(-1);

            if (cinematicDialogue3 != null)
            {
                cinematicDialogue3.PlayDialogue();

                while (!cinematicDialogue3.End)
                {
                    yield return null;
                }

                cinematicDialogue3.End = false;
            }
        }

        if (cinematicDialogue4 != null)
        {
            cinematicDialogue4.PlayDialogue();

            while (!cinematicDialogue4.End)
            {
                yield return null;
            }

            cinematicDialogue4.End = false;
        }

        StartCoroutine(FadeOut());
    }

    // drawer opening animation
    private IEnumerator MoveDrawer()
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

        Vector3 startPos = drawerObject.localPosition;
        Vector3 targetPos = startPos + (-drawerObject.up * openDistance);
        float elapsed = 0f;

        audioConfig.SoundEffectSFX(openDrawerSound);

        while (elapsed < openDuration)
        {
            drawerObject.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / openDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        drawerObject.localPosition = targetPos;

        // activate master key
        masterKey.SetActive(true);
        // destroy script
        Destroy(this);
    }

    private IEnumerator FadeOut()
    {
        fadeOut.gameObject.SetActive(true);

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeOut.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        //FadeOut the music
        audioConfig.ApplyFadeOut();

        // load next level
        SceneManager.LoadScene(nextScene);
    }

    private void UnlockBox()
    {
        if (cinematicDialogue != null)
        {
            cinematicDialogue.PlayDialogue();

            cinematicDialogue.End = false;
        }

        // final rotation
        rotation = Quaternion.Euler(transform.parent.eulerAngles.x + rotationAngle, 0, 0);
        open = true; 
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
