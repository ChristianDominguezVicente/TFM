using System;
using System.Collections;
using System.Linq;
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

    [Header("Diary")]
    [SerializeField] private string nextScene;
    [SerializeField] private CanvasGroup fadeOut;

    [Header("Cinematic")]
    [SerializeField] private CinematicDialogue cinematicDialogue;

    [Header("Restricted NPCs")]
    [SerializeField] private string[] restrictedNPCs;

    private string originalText;
    private bool showingWarning = false;

    public int[] Code { get => code; set => code = value; }

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform.parent;
    
    [Header("BoxLvl2")]
    [SerializeField] private PossessionManager possessionManager;

    private void Start()
    {
        // save original text
        originalText = interactText;
    }

    public void Interact(Transform interactorTransform)
    {
        // if there is a warning
        if (showingWarning) return;

        StartCoroutine(InteractCoroutine());
    }

    private IEnumerator InteractCoroutine()
    {
        var currentNpc = possessionManager.CurrentNPC;

        // if in the puzzle 2 Lia interact with the box that contains the key master, it unlock automatically
        if (CompareTag("Box") && SceneManager.GetActiveScene().name != "Puzzle 2")
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
        else if (CompareTag("Box") && possessionManager.CurrentNPC.NpcName == "Lia" && SceneManager.GetActiveScene().name == "Puzzle 2")
        {
            UnlockBox();
        }
        // if player possess a restricted NPC
        else if (CompareTag("Desk") && currentNpc != null && restrictedNPCs.Contains(currentNpc.NpcName))
        {
            StartCoroutine(ShowWarning("<color=red>Jane no te permite tocar el cajón</color>"));
            yield break;
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

        if (CompareTag("Desk"))
        {
            StartCoroutine(MoveDrawer());
        }
        else if (CompareTag("Diary"))
        {
            SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
            float karma = PlayerPrefs.GetFloat("Karma", 0);
            if (karma == 0)
            {
                karma++;
                ssm.SetKarma(karma);
            }
            else if (karma == -1)
            {
                karma--;
                ssm.SetKarma(karma);
            }
            
            StartCoroutine(FadeOut());
        }
        else if(CompareTag("Box"))
        {
            UnlockBox();
        }
            
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
        Vector3 targetPos = startPos + (-drawerObject.right * openDistance);
        float elapsed = 0f;

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

        // load next level
        SceneManager.LoadScene(nextScene);
    }

    private void UnlockBox()
    {
        Destroy(this);
        masterKey.SetActive(true);
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
