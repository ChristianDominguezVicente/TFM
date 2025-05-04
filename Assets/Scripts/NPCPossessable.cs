using Cinemachine;
using StarterAssets;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.Windows;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class NPCPossessable : MonoBehaviour, IPossessable
{
    [SerializeField] private string interactText;
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private GameObject player;
    [SerializeField] private float maxPossessTime = 5.0f;

    [Header("NPC Components")]
    [SerializeField] private PlayerInput npcInput;
    [SerializeField] private ThirdPersonController npcTPC;
    [SerializeField] private BasicRigidBodyPush npcBRBP;
    [SerializeField] private StarterAssetsInputs npcSAI;
    [SerializeField] private Transform npcTarget;

    [Header("Player Components")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private ThirdPersonController playerTPC;
    [SerializeField] private BasicRigidBodyPush playerBRBP;
    [SerializeField] private StarterAssetsInputs playerSAI;
    [SerializeField] private Transform playerTarget;

    [Header("Dialogue System")]
    [SerializeField] private DialogueData dialogueData;
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

    [Header("Camera Effects")]
    [SerializeField] private Volume volume;

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

    private DepthOfField blur;

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
        volume.profile.TryGet<DepthOfField>(out blur);
    }

    public void Possess(Transform interactorTransform)
    {
        interactor = interactorTransform;
        // Cinematic Mode
        if (cinematicFlag)
        {
            if (!talking && !choicePanel.IsShowing && currentQuestion == null)
            {
                NextPhrase();
            }
            else
            {
                CompletedPhrase();
            }
        }
        // if you are already possessing, we interpret this call as an attempt to speak
        else if (possessionManager.IsPossessing)
        {
            possessionManager.CurrentController.DeactivateControl();
            speakerText.text = npcName;
            dialogueBox.SetActive(true);
            cinematicFlag = false;

            if (!talking && !choicePanel.IsShowing && currentQuestion == null)
            {
                NextPhrase();
            }
            else
            {
                CompletedPhrase();
            }
        }
        else if (possessionManager.CanPossess)
        {
            possessionManager.StartPossession(this, maxPossessTime);
        }
    }

    public void EnablePossession()
    {
        // disables the original player components
        playerInput.enabled = false;
        playerTPC.enabled = false;
        playerBRBP.enabled = false;
        playerSAI.enabled = false;

        // activate the NPC components to receive control
        npcInput.enabled = true;
        npcTPC.enabled = true;
        npcBRBP.enabled = true;
        npcSAI.enabled = true;

        // change the camera focus to the NPC
        virtualCamera.Follow = npcTarget;
        virtualCamera.LookAt = npcTarget;
    }

    public void DisablePossession()
    {
        // disables NPC movement
        npcTPC.DeactivateControl();

        // disables all NPC components
        npcInput.enabled = false;
        npcTPC.enabled = false;
        npcBRBP.enabled = false;
        npcSAI.enabled = false;

        // reactivates all components of the original player
        playerInput.enabled = true;
        playerTPC.enabled = true;
        playerBRBP.enabled = true;
        playerSAI.enabled = true;

        // returns the camera to the player
        virtualCamera.Follow = playerTarget;
        virtualCamera.LookAt = playerTarget;
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
            StartCoroutine(SetBlur(true));
        }

        // current node of the dialogue
        DialogueNode node = dialogueData.nodes[currentIndex];

        if (node is DialoguePhrase phrase)
        {
            otherImage.sprite = phrase.image;
            otherImage.gameObject.SetActive(phrase.image != null);
            writePhraseCoroutine = StartCoroutine(WritePhrase(phrase.npcText));
        }
        else if (node is DialogueQuestion question)
        {
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
        interactor = null;
        dialogueHistory.AddSeparator();

        if (blur != null)
        {
            StartCoroutine(SetBlur(false));
        }

        if (autoTalkCoroutine != null)
            StopCoroutine(autoTalkCoroutine);
        autoTalking = false;
        skipTalking = false;

        cinematicFlag = false;
        OnDialogueEnded?.Invoke();
        OnDialogueEnded = null;
        CinematicDialogue.CurrentNPC = null;

        if (originalDialogueData != null)
        {
            dialogueData = originalDialogueData;
            originalDialogueData = null;
        }
        listening = false;

        otherImage.gameObject.SetActive(false);      
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
                speaker = possessionManager.CurrentNPC.npcName + " (Player)";
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
            speaker = possessionManager.CurrentNPC.npcName + " (Player)";
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
        if (talking && interactor != null && !cinematicFlag)
        {
            Vector3 transformLookPos = new Vector3(interactor.position.x, transform.position.y, interactor.position.z);
            Quaternion transformRotation = Quaternion.LookRotation(transformLookPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, transformRotation, Time.deltaTime * 5f);

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
        talking = false;
        currentQuestion = null;

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
            // start looking toward the target
            StartCoroutine(LookAtTarget(lookTarget));
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
}
