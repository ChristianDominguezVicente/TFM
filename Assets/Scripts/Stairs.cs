using Cinemachine;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    [SerializeField] private Vector3 destination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // deactivate the CharacterController to rotate the player
            CharacterController cc = other.GetComponent<CharacterController>();
            cc.enabled = false;
            // tp the player
            other.transform.position = destination;
            // rotate the player 180
            Vector3 currentEuler = other.transform.eulerAngles;
            currentEuler.y += 180f;
            other.transform.eulerAngles = currentEuler;
            // activate the CharacterController
            cc.enabled = true;
        }
    }
}
