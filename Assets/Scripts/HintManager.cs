using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HintManager : MonoBehaviour, IPossessable
{
    [Header("Hints Settings")]
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private float timeBtwLetters;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private ChoicesUI choicePanel;
    [SerializeField] private DialogueHistory dialogueHistory;
    [SerializeField] private string nameHints = "Enmsis";
    [SerializeField] private float autoTalkDelay;
    [SerializeField] private float skipTalkDelay;

    private bool talking = false;
    private int currentIndex = 0;
    private DialogueQuestion currentQuestion;
    private Coroutine writePhraseCoroutine;

    private bool autoTalking = false;
    private Coroutine autoTalkCoroutine;

    private bool skipTalking = false;
    private Coroutine skipTalkCoroutine;

    private static HintManager hintManager;

    public static HintManager CurrentHint { get => hintManager; set => hintManager = value; }

    public bool IsTalking { get => talking; set => talking = value; }
    public bool AutoTalking { get => autoTalking; set => autoTalking = value; }
    public bool SkipTalking { get => skipTalking; set => skipTalking = value; }

    public string GetPossessText() => "Enmsis";
    public Transform GetTransform() => transform;

    public void Possess(Transform interactorTransform)
    {
        hintManager = this;
        if (!talking && !choicePanel.IsShowing && currentQuestion == null)
        {
            speakerText.text = nameHints;
            dialogueBox.SetActive(true);
            NextPhrase();
        }
        else
        {
            CompletedPhrase();
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
        // current node of the dialogue
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
        // end the current dialogue resetting everything
        possessionManager.IsTalking = false;
        talking = false;
        dialogueText.text = "";
        currentIndex = 0;
        dialogueBox.SetActive(false);
        dialogueHistory.AddSeparator();

        if (autoTalkCoroutine != null)
            StopCoroutine(autoTalkCoroutine);
        autoTalking = false;
        skipTalking = false;

        CinematicDialogue.CurrentNPC = null;
        hintManager = null;
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
}
