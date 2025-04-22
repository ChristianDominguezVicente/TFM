using Cinemachine;
using StarterAssets;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public bool AutoTalking { get => autoTalking; set => autoTalking = value; }
    public bool SkipTalking { get => skipTalking; set => skipTalking = value; }
    public string NpcName { get => npcName; set => npcName = value; }
    public bool Talking { get => talking; set => talking = value; }
    public GameObject Player { get => player; set => player = value; }

    public string GetPossessText() => interactText;
    public Transform GetTransform() => transform;

    public void Possess(Transform interactorTransform)
    {
        interactor = interactorTransform;

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
    }

    private void NextPhrase()
    {
        if (currentIndex < 0 || currentIndex >= dialogueData.nodes.Length)
        {
            EndDialogue();
            return;
        }

        possessionManager.IsTalking = true;

        DialogueNode node = dialogueData.nodes[currentIndex];

        if (node is DialoguePhrase phrase)
        {
            writePhraseCoroutine = StartCoroutine(WritePhrase(phrase.npcText));
        }
        else if (node is DialogueQuestion question)
        {
            ShowChoices(question);
        }
    }
    private void EndDialogue()
    {
        possessionManager.IsTalking = false;
        talking = false;
        dialogueText.text = "";
        currentIndex = 0;
        dialogueBox.SetActive(false);
        interactor = null;
        dialogueHistory.AddSeparator();

        if (autoTalkCoroutine != null)
            StopCoroutine(autoTalkCoroutine);
        autoTalking = false;
        skipTalking = false;

        cinematicFlag = false;
        OnDialogueEnded?.Invoke();
        OnDialogueEnded = null;
        CinematicDialogue.CurrentNPC = null;
    }

    private IEnumerator WritePhrase(string phrase)
    {
        talking = true;
        dialogueText.text = "";

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

        DialogueNode node = dialogueData.nodes[currentIndex];
        if (node is DialoguePhrase p)
        {
            currentIndex = p.nextIndex;
        }
    }

    private void CompletedPhrase()
    {
        DialogueNode node = dialogueData.nodes[currentIndex];

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

        int index = choicePanel.SelectedIndex;

        if (index < 0 || index >= currentQuestion.responses.Length)
        {
            EndDialogue();
            return;
        }

        string speaker;
        if (cinematicFlag)
        {
            if (possessionManager.CurrentNPC != null)
            {
                speaker = possessionManager.CurrentNPC.npcName + " (Player)";
            }
            else
            {
                speaker = "Player";
            }
        }
        else
        {
            speaker = possessionManager.CurrentNPC.npcName + " (Player)";
        }
        dialogueHistory.AddLine(speaker, currentQuestion.responses[index].playerText);

        currentIndex = currentQuestion.responses[index].nextIndex;
        choicePanel.Hide();
        currentQuestion = null;
        NextPhrase();
    }

    public void AutoTalk()
    {
        if (!dialogueBox.activeSelf)
            return;

        autoTalking = !autoTalking;

        if (autoTalking)
        {
            if (autoTalkCoroutine != null)
                StopCoroutine(autoTalkCoroutine);
            autoTalkCoroutine = StartCoroutine(AutoTalkCoroutine());
        }
        else
        {
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
                NextPhrase();
            }

            yield return new WaitForSeconds(autoTalkDelay);
        }
    }

    public void SkipTalk()
    {
        if (!dialogueBox.activeSelf)
            return;

        skipTalking = !skipTalking;

        if (skipTalking)
        {
            if (skipTalkCoroutine != null)
                StopCoroutine(skipTalkCoroutine);
            skipTalkCoroutine = StartCoroutine(SkipTalkCoroutine());
        }
        else
        {
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
        currentIndex = index;
    }
    public void StartCinematicDialogue(Transform cinematicInteractor)
    {
        if (possessionManager.CurrentController != null)
        {
            possessionManager.CurrentController.DeactivateControl();
        }
        else
        {
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
            NextPhrase();
        }
    }

    public void SetLookTarget(Transform lookTarget)
    {
        if (lookTarget == transform) return;

        if (!Talking)
        {
            StartCoroutine(LookAtTarget(lookTarget));
        }
    }

    private IEnumerator LookAtTarget(Transform lookTarget)
    {
        float rotationSpeed = 2f;
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
}
