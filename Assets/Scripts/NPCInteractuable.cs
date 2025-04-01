using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class NPCInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Image possessBarFill;

    [Header("Possess Settings")]
    [SerializeField] private float MaxPossess = 5.0f;
    [SerializeField] private float PossessDecreaseRate = 1.0f;
    [SerializeField] private float PossessRecoveryRate = 1.0f;

    [Header("NPC Components")]
    [SerializeField] private PlayerInput npcPlayerInput;
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

    // possess
    private float _currentPossess;
    private bool _isPossessed = false;
    public bool CanPossess => _currentPossess >= MaxPossess;

    public float CurrentPossess { get => _currentPossess; set => _currentPossess = value; }

    private void Start()
    {
        _currentPossess = MaxPossess;
    }

    private void Update()
    {
        HandlePossess();
    }

    public string GetInteractText()
    {
        return interactText;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact(Transform interactorTransform)
    {
        if (!_isPossessed && CanPossess)
        {
            _isPossessed = true;

            playerInput.enabled = false;
            playerTPC.enabled = false;
            playerBRBP.enabled = false;
            playerSAI.enabled = false;

            npcPlayerInput.enabled = true;
            npcTPC.enabled = true;
            npcBRBP.enabled = true;
            npcSAI.enabled = true;

            virtualCamera.Follow = npcTarget;

            _currentPossess = MaxPossess;
            UpdatePossessUI();
        }
    }

    private void HandlePossess()
    {
        if (_isPossessed)
        {
            _currentPossess -= PossessDecreaseRate * Time.deltaTime;

            if (_currentPossess <= 0)
            {
                _currentPossess = 0;

                npcPlayerInput.enabled = false;
                npcTPC.enabled = false;
                npcBRBP.enabled = false;
                npcSAI.enabled = false;

                playerInput.enabled = true;
                playerTPC.enabled = true;
                playerBRBP.enabled = true;
                playerSAI.enabled = true;

                virtualCamera.Follow = playerTarget;

                _isPossessed = false;
            }
            UpdatePossessUI();
        }
        else
        {
            if (_currentPossess < MaxPossess)
            {
                _currentPossess += PossessRecoveryRate * Time.deltaTime;
                UpdatePossessUI();
            }
        }
    }

    private void UpdatePossessUI()
    {
        if (possessBarFill != null)
        {
            possessBarFill.fillAmount = _currentPossess / MaxPossess;
        }
    }
}
