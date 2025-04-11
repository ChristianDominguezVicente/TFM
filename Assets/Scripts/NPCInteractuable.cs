using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class NPCInteractuable : MonoBehaviour, IInteractuable
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

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    public void Interact(Transform interactorTransform)
    {
        if (possessionManager.CanPossess)
        {
            possessionManager.StartPossession(this, maxPossessTime);
        }
    }

    public void EnablePossession()
    {
        playerInput.enabled = false;
        playerTPC.enabled = false;
        playerBRBP.enabled = false;
        playerSAI.enabled = false;

        npcInput.enabled = true;
        npcTPC.enabled = true;
        npcBRBP.enabled = true;
        npcSAI.enabled = true;

        virtualCamera.Follow = npcTarget;
    }

    public void DisablePossession()
    {
        npcInput.enabled = false;
        npcTPC.enabled = false;
        npcBRBP.enabled = false;
        npcSAI.enabled = false;

        playerInput.enabled = true;
        playerTPC.enabled = true;
        playerBRBP.enabled = true;
        playerSAI.enabled = true;

        virtualCamera.Follow = playerTarget;
    }
}
