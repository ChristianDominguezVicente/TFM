using UnityEngine;

public class TriggerCinematic : MonoBehaviour
{
    [SerializeField] private CinematicDialogue cinematicDialoguePlayer;

    private void OnTriggerEnter(Collider other)
    {
        // if player triggered
        if (other.CompareTag("Player"))
        {
            // starts the cinematic dialogue sequence for the player
            cinematicDialoguePlayer.PlayDialogue();
        }
    }
}
