using System.Collections;
using UnityEngine;

public class CinematicDialogue : MonoBehaviour
{
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
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        foreach (var entry in dialogueSequence)
        {
            bool dialogueFinished = false;
            CurrentNPC = entry.speaker;

            Transform speakerTransform = entry.speaker.transform;
            foreach (var otherEntry in dialogueSequence)
            {
                if (otherEntry.speaker != entry.speaker)
                {
                    otherEntry.speaker.SetLookTarget(speakerTransform);
                }
            }
            if (entry.speaker.Player != null && entry.speaker.Player.transform != speakerTransform)
            {
                Transform playerTransform = entry.speaker.Player.transform;
                Vector3 lookPos = new Vector3(speakerTransform.position.x, playerTransform.position.y, speakerTransform.position.z);
                Quaternion targetRot = Quaternion.LookRotation(lookPos - playerTransform.position);
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRot, 1f);
            }


            entry.speaker.SetDialogueIndex(entry.startIndex);
            entry.speaker.OnDialogueEnded = () => dialogueFinished = true;
            entry.speaker.StartCinematicDialogue(speakerTransform);

            while (!dialogueFinished)
            {
                yield return null;
            }

            while (choicePanel.activeSelf)
            {
                yield return null;
            }
        }
    }
}
