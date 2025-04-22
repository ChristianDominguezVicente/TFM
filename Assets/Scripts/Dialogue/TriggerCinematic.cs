using UnityEngine;

public class TriggerCinematic : MonoBehaviour
{
    [SerializeField] private CinematicDialogue cinematicDialoguePlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cinematicDialoguePlayer.PlayDialogue();
        }
    }
}
