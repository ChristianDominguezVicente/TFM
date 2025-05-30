using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;
using System.Collections.Generic;

public class NPCNonPossessable : MonoBehaviour, IPossessable
{
    [SerializeField] private string interactText;
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private GameObject player;

    [Header("Dialogue System")]
    [SerializeField] private DialogueData[] dialogueDataArray;
    [SerializeField] private string[] speakerNames;

    [SerializeField] private float timeBtwLetters;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private ChoicesUI choicePanel;

    [Header("Dialogue History")]
    [SerializeField] private DialogueHistory dialogueHistory;
    [SerializeField] private string npcName = "NPC";

    [Header("Dialogue Auto")]
    [SerializeField] private float autoTalkDelay;

    [Header("Dialogue Skip")]
    [SerializeField] private float skipTalkDelay;

    [Header("Listening Dialogue")]
    [SerializeField] private DialogueData listeningDialogueData;

    [Header("Dialogue Image")]
    [SerializeField] private Image otherImage;
    [SerializeField] private Image nonSpeakerImage;

    [Header("Camera Effects")]
    [SerializeField] private Volume volume;

    [Header("HUD")]
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject possessBar;
    [SerializeField] private GameObject hud;

    [Header("Cinematic")]
    [SerializeField] private DialogueData cinematicDialogueData;
    [SerializeField] private CinematicDialogue cinematicDialoguePlayer;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI autoFontText;
    [SerializeField] private TextMeshProUGUI autoActionText;
    [SerializeField] private TextMeshProUGUI skipFontText;
    [SerializeField] private TextMeshProUGUI skipActionText;

    private bool talking = false;
    private int currentIndex = 0;
    private DialogueQuestion currentQuestion;
    private Coroutine writePhraseCoroutine;

    private Transform interactor;

    private bool autoTalking = false;
    private Coroutine autoTalkCoroutine;

    private bool skipTalking = false;
    private Coroutine skipTalkCoroutine;

    private bool cinematicFlag = false;
    public System.Action OnDialogueEnded;

    private DialogueData originalDialogueData;
    private bool listening = false;  

    // camera effects
    private DepthOfField blur;

    private Coroutine lookCoroutine;

    private Dictionary<string, DialogueData> dialogueMap;
    private DialogueData dialogueData;

    public bool AutoTalking { get => autoTalking; set => autoTalking = value; }
    public bool SkipTalking { get => skipTalking; set => skipTalking = value; }
    public string NpcName { get => npcName; set => npcName = value; }
    public bool Talking { get => talking; set => talking = value; }
    public GameObject Player { get => player; set => player = value; }
    public bool Listening { get => listening; set => listening = value; }

    public string GetPossessText() => interactText;
    public Transform GetTransform() => transform;

    private void Awake()
    {
        // save the blur
        volume.profile.TryGet<DepthOfField>(out blur);

        dialogueMap = new Dictionary<string, DialogueData>();
        for (int i = 0; i < Mathf.Min(speakerNames.Length, dialogueDataArray.Length); i++)
        {
            dialogueMap[speakerNames[i]] = dialogueDataArray[i];
        }
    }

    private void SetDialogue()
    {
        if (interactor != null && dialogueMap.TryGetValue(interactor.name, out DialogueData data))
        {
            dialogueData = data;
        }
    }

    public void Possess(Transform interactorTransform)
    {
        interactor = interactorTransform;
        if (cinematicDialoguePlayer != null && !cinematicFlag && possessionManager.IsPossessing)
        {
            cinematicDialoguePlayer.PlayDialogue();
        }
        else
        {
            if(!cinematicFlag && !listening)
                SetDialogue();

            if (!talking && !choicePanel.IsShowing && currentQuestion == null)
            {
                speakerText.text = npcName;
                dialogueBox.SetActive(true);
                ui.SetActive(false);
                possessBar.SetActive(false);
                hud.SetActive(false);
                NextPhrase();
            }
            else
            {
                CompletedPhrase();
            }
        }   
    }

    private void NextPhrase()
    {
        // if the current index is out of bounds end the dialog
        if (currentIndex < 0 || currentIndex >= dialogueData.nodes.Length)
        {
            EndDialogue();
            return;
        }

        possessionManager.IsTalking = true;

        if (blur != null)
        {
            // set the blur
            StartCoroutine(SetBlur(true));
        }

        // current node of the dialogue
        DialogueNode node = dialogueData.nodes[currentIndex];

        if (node is DialoguePhrase phrase)
        {
            // show the expression image
            otherImage.sprite = phrase.image;
            otherImage.gameObject.SetActive(phrase.image != null);
            nonSpeakerImage.sprite = phrase.nonSpeakerImage;
            nonSpeakerImage.gameObject.SetActive(phrase.nonSpeakerImage != null);
            writePhraseCoroutine = StartCoroutine(WritePhrase(phrase.npcText));
        }
        else if (node is DialogueQuestion question)
        {
            // show the expression image
            otherImage.sprite = question.image;
            otherImage.gameObject.SetActive(question.image != null);
            ShowChoices(question);
        }
    }
    private void EndDialogue()
    {
        // end the current dialogue resetting everything
        possessionManager.IsTalking = false;
        talking = false;
        dialogueText.text = "";
        currentIndex = 0;
        dialogueBox.SetActive(false);
        ui.SetActive(true);
        possessBar.SetActive(true);
        hud.SetActive(true);
        interactor = null;
        dialogueHistory.AddSeparator();

        if (blur != null && (!cinematicFlag || listening))
        {
            // remove the blur
            StartCoroutine(SetBlur(false));
        }

        if (autoTalkCoroutine != null)
            StopCoroutine(autoTalkCoroutine);
        autoTalking = false;
        autoFontText.color = Color.white;
        autoActionText.color = Color.white;

        skipTalking = false;
        skipFontText.color = Color.white;
        skipActionText.color = Color.white;

        cinematicFlag = false;
        OnDialogueEnded?.Invoke();
        OnDialogueEnded = null;
        CinematicDialogue.CurrentNPCNon = null;

        if (originalDialogueData != null)
        {
            dialogueData = originalDialogueData;
            originalDialogueData = null;
        }
        listening = false;

        otherImage.gameObject.SetActive(false);
        nonSpeakerImage.gameObject.SetActive(false);
    }

    private IEnumerator WritePhrase(string phrase)
    {
        talking = true;
        dialogueText.text = "";
        // add each character in the provided phrase
        foreach (char c in phrase)
        {
            dialogueText.text += c;
            if (skipTalking)
            {
                yield return new WaitForSeconds(timeBtwLetters * 0.1f);
            }
            else
            {
                yield return new WaitForSeconds(timeBtwLetters);
            }
        }

        talking = false;
        dialogueHistory.AddLine(npcName, phrase);
        // current node of the dialogue
        DialogueNode node = dialogueData.nodes[currentIndex];
        // update the index to advance the dialogue
        if (node is DialoguePhrase p)
        {
            currentIndex = p.nextIndex;
        }
    }

    private void CompletedPhrase()
    {
        // current node of the dialogue
        DialogueNode node = dialogueData.nodes[currentIndex];
        // if the coroutine is running, it stops
        if (node is DialoguePhrase phrase)
        {
            if (writePhraseCoroutine != null)
            {
                StopCoroutine(writePhraseCoroutine);
                writePhraseCoroutine = null;
                currentIndex = phrase.nextIndex;
            }

            dialogueText.text = phrase.npcText;
            dialogueHistory.AddLine(npcName, phrase.npcText);
            talking = false;
            writePhraseCoroutine = null;
        }
    }

    private void ShowChoices(DialogueQuestion question)
    {
        talking = false;
        currentQuestion = question;
        dialogueText.text = question.npcText;
        dialogueHistory.AddLine(npcName, question.npcText);
        choicePanel.Show(question.responses);
    }

    public void SelectCurrentChoice()
    {
        if (currentQuestion == null || choicePanel == null)
            return;
        // index of the option selected by the player
        int index = choicePanel.SelectedIndex;
        // if the current index is out of bounds end the dialog
        if (index < 0 || index >= currentQuestion.responses.Length)
        {
            EndDialogue();
            return;
        }
        // determine who is talking
        string speaker;
        // Cinematic Mode
        if (cinematicFlag)
        {
            // if there is a possessed NPC
            if (possessionManager.CurrentNPC != null)
            {
                // the player talk as the speaker
                speaker = possessionManager.CurrentNPC.NpcName + " (Player)";
            }
            // if there is not a possessed NPC
            else
            {
                // the player is talking
                speaker = "Player";
            }
        }
        else
        {
            // the player talk as the speaker
            speaker = possessionManager.CurrentNPC.NpcName + " (Player)";
        }
        dialogueHistory.AddLine(speaker, currentQuestion.responses[index].playerText);

        currentIndex = currentQuestion.responses[index].nextIndex;
        choicePanel.Hide();
        currentQuestion = null;
        // advance to the next node in the dialog
        NextPhrase();
    }

    public void AutoTalk()
    {
        if (!dialogueBox.activeSelf)
            return;
        // change the state of the automatic conversation
        autoTalking = !autoTalking;

        autoFontText.color = autoTalking ? Color.yellow : Color.white;
        autoActionText.color = autoTalking ? Color.yellow : Color.white;

        if (autoTalking)
        {
            if (autoTalkCoroutine != null)
                StopCoroutine(autoTalkCoroutine);
            // starts the auto coroutine
            autoTalkCoroutine = StartCoroutine(AutoTalkCoroutine());
        }
        else
        {
            // stops the auto coroutine
            if (autoTalkCoroutine != null)
                StopCoroutine(autoTalkCoroutine);
        }
    }

    private IEnumerator AutoTalkCoroutine()
    {
        while (autoTalking)
        {
            if (!talking && !choicePanel.IsShowing)
            {
                // advance to the next node in the dialog
                NextPhrase();
            }

            yield return new WaitForSeconds(autoTalkDelay);
        }
    }

    public void SkipTalk()
    {
        if (!dialogueBox.activeSelf)
            return;
        // change the state of the skip conversation
        skipTalking = !skipTalking;

        skipFontText.color = skipTalking ? Color.yellow : Color.white;
        skipActionText.color = skipTalking ? Color.yellow : Color.white;

        if (skipTalking)
        {
            if (skipTalkCoroutine != null)
                StopCoroutine(skipTalkCoroutine);
            // starts the skip coroutine
            skipTalkCoroutine = StartCoroutine(SkipTalkCoroutine());
        }
        else
        {
            // stops the skip coroutine
            if (skipTalkCoroutine != null)
                StopCoroutine(skipTalkCoroutine);
        }
    }

    private IEnumerator SkipTalkCoroutine()
    {
        while (skipTalking)
        {
            if (!talking && !choicePanel.IsShowing)
            {
                // advance to the next node in the dialog
                NextPhrase();
            }

            yield return new WaitForSeconds(skipTalkDelay);
        }
    }

    private void Update()
    {
        // if player is talking and not in a cinematic
        if (talking && interactor != null && !cinematicFlag)
        {
            // NPC look interactor
            Vector3 transformLookPos = new Vector3(interactor.position.x, transform.position.y, interactor.position.z);
            Quaternion transformRotation = Quaternion.LookRotation(transformLookPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, transformRotation, Time.deltaTime * 5f);

            // interactor look NPC
            Vector3 interactorLookPos = new Vector3(transform.position.x, interactor.position.y, transform.position.z);
            Quaternion interactorRotation = Quaternion.LookRotation(interactorLookPos - interactor.position);
            interactor.rotation = Quaternion.Slerp(interactor.rotation, interactorRotation, Time.deltaTime * 5f);
        }
    }

    public void SetDialogueIndex(int index)
    {
        // assigns the provided index to the current index of the dialog
        currentIndex = index;
    }
    public void StartCinematicDialogue(Transform cinematicInteractor)
    {
        // if there is already a possessed NPC
        if (possessionManager.CurrentController != null)
        {
            // deactivate possessed NPC
            possessionManager.CurrentController.DeactivateControl();
        }
        else
        {
            // deactivate player
            player.GetComponent<ThirdPersonController>().DeactivateControl();
        }

        interactor = cinematicInteractor;
        cinematicFlag = true;
        speakerText.text = npcName;
        dialogueBox.SetActive(true);
        ui.SetActive(false);
        possessBar.SetActive(false);
        hud.SetActive(false);
        talking = false;
        currentQuestion = null;

        // original dialoqueData
        if (originalDialogueData == null)
        {
            originalDialogueData = dialogueData;
        }
        // change dialogueData
        dialogueData = cinematicDialogueData;

        if (!choicePanel.IsShowing)
        {
            // advance to the next node in the dialog
            NextPhrase();
        }
    }

    public void SetLookTarget(Transform lookTarget)
    {
        // if the target is the itself, doesn't do nothing
        if (lookTarget == transform) return;
        // if the NPC is not talking
        if (!Talking)
        {
            if (lookCoroutine != null)
                StopCoroutine(lookCoroutine);

            // start looking toward the target
            lookCoroutine = StartCoroutine(LookAtTarget(lookTarget));
        }
    }

    public void ClearLookTarget()
    {
        if (lookCoroutine != null)
        {
            StopCoroutine(lookCoroutine);
            lookCoroutine = null;
        }
    }

    private IEnumerator LookAtTarget(Transform lookTarget)
    {
        float rotationSpeed = 2f;
        // loop that runs as long as the NPC has a target to look at
        while (true)
        {
            if (lookTarget == null) yield break;

            Vector3 lookPos = new Vector3(lookTarget.position.x, transform.position.y, lookTarget.position.z);
            Quaternion targetRot = Quaternion.LookRotation(lookPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            //if (Quaternion.Angle(transform.rotation, targetRot) < 1f)
            //break;

            yield return null;
        }
    }

    public void StartListeningDialogue(Transform listener)
    {
        // if there is already a possessed NPC
        if (possessionManager.CurrentController != null)
        {
            // deactivate possessed NPC
            possessionManager.CurrentController.DeactivateControl();
        }
        else
        {
            // deactivate player
            player.GetComponent<ThirdPersonController>().DeactivateControl();
        }

        interactor = listener;
        cinematicFlag = true;
        speakerText.text = npcName;
        dialogueBox.SetActive(true);
        ui.SetActive(false);
        possessBar.SetActive(false);
        hud.SetActive(false);
        talking = false;
        currentQuestion = null;
        listening = true;

        // original dialoqueData
        if (originalDialogueData == null)
        {
            originalDialogueData = dialogueData;
        }
        // change dialogueData
        dialogueData = listeningDialogueData;

        if (!choicePanel.IsShowing)
        {
            // advance to the next node in the dialog
            NextPhrase();
        }
    }

    private IEnumerator SetBlur(bool flag)
    {
        if (blur == null) yield break;

        float focus;
        if (flag)
            focus = 0.1f;
        else
            focus = 10f;
        float duration = 0.5f;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            blur.focusDistance.value = Mathf.Lerp(blur.focusDistance.value, focus, t);
            yield return null;
        }
    }

    public void RemoveBlur()
    {
        StartCoroutine(SetBlur(false));
    }
}
