using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CinematicDialogue : MonoBehaviour
{
    // structure to store information about each dialog entry
    [System.Serializable]
    public class DialogueEntry
    {
        public MonoBehaviour speaker;
        public int startIndex;
    }

    [SerializeField] private DialogueEntry[] dialogueSequence;
    [SerializeField] private GameObject choicePanel;

    private static NPCPossessable currentNPC;
    private static NPCNonPossessable currentNPCNon;

    private bool end = false;

    public static NPCPossessable CurrentNPC { get => currentNPC; set => currentNPC = value; }
    public static NPCNonPossessable CurrentNPCNon { get => currentNPCNon; set => currentNPCNon = value; }
    public bool End { get => end; set => end = value; }

    public void PlayDialogue()
    {
        // starts the coroutine that plays the dialogue sequence
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // loop through all dialog entries in the sequence
        foreach (var entry in dialogueSequence)
        {
            bool dialogueFinished = false;

            // identify NPC's type
            NPCPossessable possessable = entry.speaker as NPCPossessable;
            NPCNonPossessable nonPossessable = entry.speaker as NPCNonPossessable;

            Transform speakerTransform = entry.speaker.transform;
            // assigns the gaze of other NPCs to the current speaker
            foreach (var otherEntry in dialogueSequence)
            {
                // if the other NPC is not the same as the current speaker
                if (otherEntry.speaker != entry.speaker)
                {
                    NPCPossessable otherPossessable = otherEntry.speaker as NPCPossessable;
                    NPCNonPossessable otherNonPossessable = otherEntry.speaker as NPCNonPossessable;

                    // directs them to look at the speaker
                    if (otherPossessable != null)
                        otherPossessable.SetLookTarget(speakerTransform);
                    else if (otherNonPossessable != null)
                        otherNonPossessable.SetLookTarget(speakerTransform);
                }
            }

            // sets the current NPC for this dialogue
            if (possessable != null)
            { 
                CurrentNPC = possessable;
                possessable.SetDialogueIndex(entry.startIndex);
                // sets a callback function that will be executed when the dialog ends
                possessable.OnDialogueEnded = () => dialogueFinished = true;
                possessable.StartCinematicDialogue(speakerTransform);
            }
            else if (nonPossessable != null)
            {
                CurrentNPCNon = nonPossessable;
                nonPossessable.SetDialogueIndex(entry.startIndex);
                // sets a callback function that will be executed when the dialog ends
                nonPossessable.OnDialogueEnded = () => dialogueFinished = true;
                nonPossessable.StartCinematicDialogue(speakerTransform);
            }

            // wait until the dialog has finished
            while (!dialogueFinished)
            {
                yield return null;
            }
            // wait until the choices panel is hidden
            while (choicePanel.activeSelf)
            {
                yield return null;
            }

            foreach (var otherEntry in dialogueSequence)
            {
                if (otherEntry.speaker != entry.speaker)
                {
                    NPCPossessable otherPossessable = otherEntry.speaker as NPCPossessable;
                    NPCNonPossessable otherNonPossessable = otherEntry.speaker as NPCNonPossessable;

                    if (otherPossessable != null)
                        otherPossessable.ClearLookTarget();
                    else if (otherNonPossessable != null)
                        otherNonPossessable.ClearLookTarget();
                }
            }
        }

        foreach (var entry in dialogueSequence)
        {
            if (entry.speaker is NPCNonPossessable nonPossessable)
            {
                nonPossessable.RemoveBlur();
            }
            else if (entry.speaker is NPCPossessable possessable)
            {
                possessable.RemoveBlur();
            }
        }

        end = true;
    }
}
