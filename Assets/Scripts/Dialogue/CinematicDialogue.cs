using System.Collections;
using UnityEngine;

public class CinematicDialogue : MonoBehaviour
{
    // structure to store information about each dialog entry
    [System.Serializable]
    public class DialogueEntry
    {
        public NPCPossessable speaker;
        public int startIndex;
    }

    [SerializeField] private DialogueEntry[] dialogueSequence;
    [SerializeField] private GameObject choicePanel;

    private static NPCPossessable currentNPC;

    public static NPCPossessable CurrentNPC { get => currentNPC; set => currentNPC = value; }

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
            // sets the current NPC for this dialogue
            CurrentNPC = entry.speaker;

            Transform speakerTransform = entry.speaker.transform;
            // assigns the gaze of other NPCs to the current speaker
            foreach (var otherEntry in dialogueSequence)
            {
                // if the other NPC is not the same as the current speaker
                if (otherEntry.speaker != entry.speaker)
                {
                    // directs them to look at the speaker
                    otherEntry.speaker.SetLookTarget(speakerTransform);
                }
            }
            // if the speaker is the player
            if (entry.speaker.Player != null && entry.speaker.Player.transform != speakerTransform)
            {
                Transform playerTransform = entry.speaker.Player.transform;
                Vector3 lookPos = new Vector3(speakerTransform.position.x, playerTransform.position.y, speakerTransform.position.z);
                Quaternion targetRot = Quaternion.LookRotation(lookPos - playerTransform.position);
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRot, 1f);
            }

            entry.speaker.SetDialogueIndex(entry.startIndex);
            // sets a callback function that will be executed when the dialog ends
            entry.speaker.OnDialogueEnded = () => dialogueFinished = true;
            entry.speaker.StartCinematicDialogue(speakerTransform);
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
        }
    }
}
