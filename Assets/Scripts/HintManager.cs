using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HintManager : MonoBehaviour, IPossessable
{
    [Header("Hints Settings")]
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private DialogueData[] dialogueDataArray;
    [SerializeField] private float timeBtwLetters;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private ChoicesUI choicePanel;
    [SerializeField] private DialogueHistory dialogueHistory;
    [SerializeField] private string nameHints = "Enmsis";
    [SerializeField] private float autoTalkDelay;
    [SerializeField] private float skipTalkDelay;
    [SerializeField] private Image otherImage;
    [SerializeField] private Image nonSpeakerImage;
    [SerializeField] private Volume volume;

    [Header("HUD")]
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject possessBar;
    [SerializeField] private GameObject hud;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI autoFontText;
    [SerializeField] private TextMeshProUGUI autoActionText;
    [SerializeField] private TextMeshProUGUI skipFontText;
    [SerializeField] private TextMeshProUGUI skipActionText;

    private bool talking = false;
    private int currentIndex = 0;
    private DialogueQuestion currentQuestion;
    private Coroutine writePhraseCoroutine;

    private bool autoTalking = false;
    private Coroutine autoTalkCoroutine;

    private bool skipTalking = false;
    private Coroutine skipTalkCoroutine;

    private static HintManager hintManager;

    private DepthOfField blur;

    private int currentDialogueIndex = -1;

    public static HintManager CurrentHint { get => hintManager; set => hintManager = value; }

    public bool IsTalking { get => talking; set => talking = value; }
    public bool AutoTalking { get => autoTalking; set => autoTalking = value; }
    public bool SkipTalking { get => skipTalking; set => skipTalking = value; }

    public string GetPossessText() => "Enmsis";
    public Transform GetTransform() => transform;

    private void Awake()
    {
        // save the blur
        volume.profile.TryGet<DepthOfField>(out blur);
    }

    public void Possess(Transform interactorTransform)
    {
        hintManager = this;

        if (!talking && !choicePanel.IsShowing && currentQuestion == null)
        {
            UpdateDialogue();

            if (currentDialogueIndex == -1)
            {
                EndDialogue();
                return;
            }

            speakerText.text = nameHints;
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

    private bool IsDialogueActive(int index)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Puzzle 1":
                switch (index)
                {
                    case 0: return objectManager.MasterKeyTaken;
                    case 1: return objectManager.ValveActive;
                    case 2: return objectManager.Sugar;
                    case 3: return objectManager.Flour;
                    case 4: return objectManager.Eggs;
                    case 5: return objectManager.Recipe1 && objectManager.Recipe2;
                    default: return false;
                }
            case "Puzzle 2":
                switch (index)
                {
                    case 0: return objectManager.Teddy;
                    case 1: return objectManager.ToolBox;
                    case 2: return objectManager.GiftPaper;
                    case 3: return objectManager.Correct || objectManager.Incorrect;
                    case 4: return objectManager.Incorrect;
                    default: return false;
                }
            case "Puzzle 3":
                switch (index)
                {
                    default: return false;
                }
            case "Puzzle 4":
                switch (index)
                {
                    case 0: return objectManager.Correct || objectManager.Incorrect;
                    default: return false;
                }
            default:
                return false;
        }
    }

    private int[] GetPendingDialogues()
    {
        List<int> pending = new List<int>();
        for (int i = 0; i < dialogueDataArray.Length; i++)
        {
            if (!IsDialogueActive(i))
                pending.Add(i);
        }
        return pending.ToArray();
    }


    private void UpdateDialogue()
    {
        int previousIndex = currentDialogueIndex;

        var pendingDialogues = GetPendingDialogues();

        if (pendingDialogues.Length == 0)
        {
            currentDialogueIndex = -1;
            EndDialogue();
            return;
        }

        currentDialogueIndex = pendingDialogues[0];

        if (previousIndex != currentDialogueIndex)
        {
            currentIndex = 0;
        }
    }

    private void NextPhrase()
    {
        // if the current index is out of bounds end the dialog
        if (currentDialogueIndex == -1 || currentIndex < 0 || currentIndex >= dialogueDataArray[currentDialogueIndex].nodes.Length)
        {
            var pendingDialogues = GetPendingDialogues();

            int nextDialogueIndex = -1;
            foreach (int idx in pendingDialogues)
            {
                if (idx > currentDialogueIndex)
                {
                    nextDialogueIndex = idx;
                    break;
                }
            }

            if (nextDialogueIndex == -1)
            {
                EndDialogue();
                return;
            }
            else
            {
                currentDialogueIndex = nextDialogueIndex;
                currentIndex = 0;
            }
        }

        possessionManager.IsTalking = true;

        if (blur != null)
        {
            // set the blur
            StartCoroutine(SetBlur(true));
        }

        // current node of the dialogue
        DialogueNode node = dialogueDataArray[currentDialogueIndex].nodes[currentIndex];

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
        currentDialogueIndex = 0;
        dialogueBox.SetActive(false);
        ui.SetActive(true);
        possessBar.SetActive(true);
        hud.SetActive(true);
        dialogueHistory.AddSeparator();

        if (blur != null)
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

        hintManager = null;

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
        dialogueHistory.AddLine(nameHints, phrase);
        // current node of the dialogue
        DialogueNode node = dialogueDataArray[currentDialogueIndex].nodes[currentIndex];
        // update the index to advance the dialogue
        if (node is DialoguePhrase p)
        {
            currentIndex = p.nextIndex;
        }
    }

    private void CompletedPhrase()
    {
        // current node of the dialogue
        DialogueNode node = dialogueDataArray[currentDialogueIndex].nodes[currentIndex];
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
            dialogueHistory.AddLine(nameHints, phrase.npcText);
            talking = false;
            writePhraseCoroutine = null;
        }
    }

    private void ShowChoices(DialogueQuestion question)
    {
        talking = false;
        currentQuestion = question;
        dialogueText.text = question.npcText;
        dialogueHistory.AddLine(nameHints, question.npcText);
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

        dialogueHistory.AddLine(nameHints, currentQuestion.responses[index].playerText);

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
