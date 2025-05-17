using Cinemachine;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Stairs : MonoBehaviour
{
    [SerializeField] private Vector3 destination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            NPCPossessable npc = other.GetComponent<NPCPossessable>();
            // deactivate the CharacterController to rotate the player
            CharacterController cc = other.GetComponent<CharacterController>();

            bool hadNavAgent = agent != null && agent.enabled;
            
            if (npc != null)
                npc.FlagTP = !npc.FlagTP;

            if (hadNavAgent)
                agent.enabled = false;
            cc.enabled = false;

            // tp the player
            other.transform.position = destination;
            // rotate the player 180
            Vector3 currentEuler = other.transform.eulerAngles;
            currentEuler.y += 180f;
            other.transform.eulerAngles = currentEuler;

            if (hadNavAgent)
                agent.enabled = true;
            // activate the CharacterController
            cc.enabled = true;
        }
    }
}
