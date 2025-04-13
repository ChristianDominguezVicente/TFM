using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

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

    public string GetPossessText() => interactText;
    public Transform GetTransform() => transform;

    public void Possess(Transform interactorTransform)
    {
        if (possessionManager.CanPossess)
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
}
