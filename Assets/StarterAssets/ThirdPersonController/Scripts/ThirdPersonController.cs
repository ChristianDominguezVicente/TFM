using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using TMPro;
using UnityEditor;

//using Unity.Android.Gradle.Manifest;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [UnityEngine.Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [UnityEngine.Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;
        public GameObject PlayerFollowCamera;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Interact")]
        [SerializeField] private float interactRange = 2f;

        [Header("Spectral Vsion")]
        [SerializeField] private UniversalRendererData rendererData;
        [SerializeField] private Volume volume;
        [SerializeField] private float transitionSpeed = 2f;

        [Header("GameManager")]
        [SerializeField] private PossessionManager possesionManager;
        [SerializeField] private HintManager hintManager;
        [SerializeField] private MenuPause pauseManager;
       
        [SerializeField] private ObjectManager objectManager;

        [Header("Dialogue History")]
        [SerializeField] private DialogueHistory dialogueHistory;
        [SerializeField] private GameObject historyPanel;
        [SerializeField] private TextMeshProUGUI historyText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float scrollSpeed;
        [SerializeField] private ChoicesUI choicePanel;

        [Header("CodeUI")]
        [SerializeField] private CodeUI codeUI;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI historyFontText;
        [SerializeField] private TextMeshProUGUI historyActionText;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        // spectral vision
        private bool isSenseActive = false;
        private ScriptableRendererFeature spectralVisionInteractuable;
        private ScriptableRendererFeature spectralVisionPossessable;
        private float targetWeight = 0f;
        private ColorAdjustments colorAdjustments;

        // dialogue history
        private bool showingHistory = false;
        private Vector2 scrollInput;

        // dialogue choice
        private float inputCooldown = 0.2f;
        private float lastInputTime = 0f;

        // input detector
        private string controlUsed;
        private float rotationSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void OnEnable()
        {
            // subscribe to the global input system event when this script is triggered
            InputSystem.onEvent += OnInputEvent;
        }

        private void OnDisable()
        {
            // unsubscribe from the event when this script is deactivated
            InputSystem.onEvent -= OnInputEvent;
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // ignore events that are not of type "state" or "state delta" 
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;

            // if a Gamepad is detected and it was not the previous controller, update to Gamepad
            if (device is Gamepad && controlUsed != "Gamepad")
            {
                rotationSpeed = 0.005f;
            }
            // if a Keyboard or Mouse is detected and it was not the previous controller, update to Keyboard
            else if ((device is Keyboard || device is Mouse) && controlUsed != "Keyboard")
            {
                rotationSpeed = 0.5f;
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            if (rendererData != null)
            {
                // cycle through the available rendering features in rendererData
                foreach (var feature in rendererData.rendererFeatures)
                {
                    if (feature.name == "RenderOutlines")
                    {
                        spectralVisionInteractuable = feature;
                    }
                    else if (feature.name == "RenderObjects")
                    {
                        spectralVisionPossessable = feature;
                    }
                }
            }

            // attempt to get the ColorAdjustments effect from the post-processing profile
            if (volume.profile.TryGet<ColorAdjustments>(out var ca))
            {
                colorAdjustments = ca;
                colorAdjustments.active = false;
            }
        }

        private void Update()
        {
            if (pauseManager.IsPaused) {
                if (MenuInicial.menuActivo != null)
                {
                    PlayerFollowCamera.SetActive(false);
                    UI_Move_Paused();
                    UI_Interact();
                    UI_CancelPauseMenu();
                }
            }
            else
            {
                PlayerFollowCamera.SetActive(true);

                if (codeUI != null && codeUI.Active)
                {
                    // reset animations
                    possesionManager.CurrentController.DeactivateControl();
                    // detect inputs available
                    codeUI.Inputs(_input.ui_move, _input.interact, _input.cancel);

                    // reset buttons that had the same input
                    ResetInputs();

                    return;
                } 
                if (possesionManager.IsTalking || (CinematicDialogue.CurrentNPC != null || CinematicDialogue.CurrentNPCNon != null) || HintManager.CurrentHint != null)
                    {
                        Interact();
                        History();
                        UI_Move();
                        Auto();
                        Skip();

                    // reset buttons that had the same input
                    ResetInputs();

                    return;
                }
                // if the player is looking a object
                else if (objectManager.Looking)
                {
                    LookingObject();
                    Cancel();

                    // reset buttons that had the same input
                    ResetInputs();

                    return;
                }

                _hasAnimator = TryGetComponent(out _animator);

                JumpAndGravity();
                GroundedCheck();
                Move();
                Interact();
                Listen();
                Hint();
                SpectralVision();
                Cancel();
                Pause();

                // reset buttons that had the same input
                ResetInputs();
            }
        }

        private void UI_CancelPauseMenu()
        {
            if (_input.cancel)
            {
             //   UnityEngine.Debug.Log("echa pa tras");
                MenuInicial.menuActivo.VolverAMenuAnterior();

                _input.cancel = false;
                return;

            }
        }

        private void ResetInputs()
        {
            _input.pause = false;
            _input.jump = false;
            _input.interact = false;
            _input.auto = false;
            _input.skip = false;
            _input.listen = false;
            _input.hint = false;
            _input.cancel = false;
        }
   
        private void UI_Interact()
        {
            if (_input.interact)
            {
                if (MenuInicial.menuActivo.IsAdjustingToggle())
                {
                    // Confirmar selección y salir del modo ajuste
                    MenuInicial.menuActivo.ToggleModoAjuste();
                }
                else if (MenuInicial.menuActivo.IsAdjustingSlider())
                {
                    MenuInicial.menuActivo.ToggleAjusteSlider();
                }
                else if (MenuInicial.menuActivo.CurrentButtonIsToggle())
                {
                    // Entrar en modo ajuste
                    MenuInicial.menuActivo.ToggleModoAjuste();
                }
                else if (MenuInicial.menuActivo.CurrentButtonHasAdjacentSlider())
                {
                    MenuInicial.menuActivo.ToggleAjusteSlider();
                }
                else
                {
                    // Acción normal del botón
                    MenuInicial.menuActivo.ActivateSelectedButton();
                }
                _input.interact = false;
            }
        }

        private void UI_Move_Paused()
        {
            //Time.unscaledTime en vez de Time.time
            if (MenuInicial.menuActivo == null) return;

            Vector2 input = _input.ui_move;
            float deadzone = 0.7f;

            if (Time.unscaledTime - lastInputTime < inputCooldown) return;

            // Modo ajuste de toggle
            if (MenuInicial.menuActivo.IsAdjustingToggle() && Mathf.Abs(input.x) > deadzone)
            {
                int direction = input.x > 0 ? 1 : -1;
                MenuInicial.menuActivo.CambiarOpcionToggle(direction);
                lastInputTime = Time.unscaledTime;
            }
            // Modo ajuste de slider (mantén tu código existente)
            else if (MenuInicial.menuActivo.IsAdjustingSlider() && Mathf.Abs(input.x) > deadzone)
            {
                int direction = input.x > 0 ? 1 : -1;
                MenuInicial.menuActivo.MoveSelection(direction);
                lastInputTime = Time.unscaledTime;
            }
            // Navegación normal (vertical)
            else if (Mathf.Abs(input.y) > deadzone)
            {
                int direction = input.y > 0 ? -1 : 1;
                MenuInicial.menuActivo.MoveSelection(direction);
                lastInputTime = Time.unscaledTime;
            }
        }




        private void LateUpdate()
        {
            if (!possesionManager.IsTalking && !objectManager.Looking)
            {
                CameraRotation();
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                /*
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }
                */
                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void Interact()
        {
            // Cinematic Mode
            if (CinematicDialogue.CurrentNPC != null || CinematicDialogue.CurrentNPCNon != null)
            {
                // if the player presses the interact button and the history is not being displayed
                if (_input.interact && !showingHistory)
                {
                    if (CinematicDialogue.CurrentNPC != null)
                    {
                        // if the choices are being displayed
                        if (choicePanel.IsShowing)
                        {
                            // selects the current choice
                            CinematicDialogue.CurrentNPC.SelectCurrentChoice();
                        }
                        // if no auto or skip
                        else if (!CinematicDialogue.CurrentNPC.AutoTalking && !CinematicDialogue.CurrentNPC.SkipTalking)
                        {
                            // next dialogue
                            CinematicDialogue.CurrentNPC.Possess(transform);
                        }
                    }
                    else if (CinematicDialogue.CurrentNPCNon != null)
                    {
                        // if the choices are being displayed
                        if (choicePanel.IsShowing)
                        {
                            // selects the current choice
                            CinematicDialogue.CurrentNPCNon.SelectCurrentChoice();
                        }
                        // if no auto or skip
                        else if (!CinematicDialogue.CurrentNPCNon.AutoTalking && !CinematicDialogue.CurrentNPCNon.SkipTalking)
                        {
                            // next dialogue
                            CinematicDialogue.CurrentNPCNon.Possess(transform);
                        }
                    }
                }

                _input.interact = false;
                return;
            }
            // Hint Mode
            else if (HintManager.CurrentHint != null)
            {
                // if the player presses the interact button and the history is not being displayed
                if (_input.interact && !showingHistory)
                {
                    // if no auto or skip
                    if (!HintManager.CurrentHint.AutoTalking && !HintManager.CurrentHint.SkipTalking)
                    {
                        // next dialogue
                        HintManager.CurrentHint.Possess(transform);
                    }
                }
            }


            // if the player presses the interact button and the choices are being displayed
            if (choicePanel.IsShowing && _input.interact)
            {
                if (GetInteractuables() is NPCPossessable npcP)
                {
                    // selects the current choice
                    npcP.SelectCurrentChoice();
                }
                else if (GetInteractuables() is NPCNonPossessable npcNP)
                {
                    // selects the current choice
                    npcNP.SelectCurrentChoice();
                }

                _input.interact = false;
                return;
            }
            // get the closest interactuable object
            var t = GetInteractuables();
            bool auto = false;
            bool skip = false;

            if (t is IPossessable p)
            {
                if (p is NPCPossessable npcP)
                {
                    auto = npcP.AutoTalking;
                    skip = npcP.SkipTalking;
                }
                else if (p is NPCNonPossessable npcNP)
                {
                    auto = npcNP.AutoTalking;
                    skip = npcNP.SkipTalking;
                }
            }
            // if history is being displayed or auto-talking is active or skip-talking is active
            if (showingHistory || auto || skip)
            {
                _input.interact = false;
                return;
            }       

            // detect the input button
            if (_input.interact)
            {
                // find the nearest interactuable object
                var target = GetInteractuables();

                // if the object implements the IInteractuable interface
                if (target is IInteractuable interactuable)
                {
                    interactuable.Interact(transform);
                }
                // if the object implements the IPossessable interface
                else if (target is IPossessable possessable)
                {
                    if (possessable is NPCPossessable npcP)
                    {
                        possessable.Possess(transform);
                    }
                    else if (possessable is NPCNonPossessable npcNP)
                    {
                        if (possesionManager.CurrentNPC != null || npcNP.Listening)
                            possessable.Possess(transform);
                    }
                }

                _input.interact = false;
            }
        }

        public object GetInteractuables()
        {
            object closest = null;
            float closestDistance = float.MaxValue;

            // get all colliders within a defined radius around the player
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                // if the player is already possessing something, they can only interact or talk to other NPCs
                if (possesionManager.IsPossessing)
                {
                    if (collider.gameObject == gameObject)
                        continue;

                    if (collider.TryGetComponent(out IInteractuable interactuable))
                    {
                        // calculate the distance to the interactuable object
                        Vector3 point = collider.ClosestPoint(transform.position);
                        float distance = Vector3.Distance(transform.position, point);
                        if (distance < closestDistance)
                        {
                            closest = interactuable;
                            closestDistance = distance;
                        }
                    }
                    else if (collider.TryGetComponent(out IPossessable possessable))
                    {
                        // calculate the distance to the possessable object
                        Vector3 point = collider.ClosestPoint(transform.position);
                        float distance = Vector3.Distance(transform.position, point);
                        if (distance < closestDistance)
                        {
                            closest = possessable;
                            closestDistance = distance;
                        }
                    }
                }
                // if you are not possessing, you can only possess
                else
                {
                    if (collider.TryGetComponent(out IPossessable possessable))
                    {
                        // calculate the distance to the possessable object
                        Vector3 point = collider.ClosestPoint(transform.position);
                        float distance = Vector3.Distance(transform.position, point);
                        if (distance < closestDistance)
                        {
                            closest = possessable;
                            closestDistance = distance;
                        }
                    }
                }
            }
            // return the closest object found 
            return closest;
        }

        private void Listen()
        {
            // get the closest interactuable object
            var t = GetInteractuables();

            if (t is NPCPossessable n)
            {
                if (n.Talking || n.AutoTalking || n.SkipTalking || n.Listening)
                {
                    _input.listen = false;
                    return;
                }
            }
            else if (t is NPCNonPossessable nn)
            {
                if (nn.Talking || nn.AutoTalking || nn.SkipTalking || nn.Listening)
                {
                    _input.listen = false;
                    return;
                }
            }

            if (_input.listen)
            {
                // get the closest interactuable object
                var target = GetInteractuables();

                if (target is NPCPossessable npc)
                {
                    // start listening
                    npc.StartListeningDialogue(transform);    
                }
                else if (target is NPCNonPossessable npcNon)
                {
                    // start listening
                    npcNon.StartListeningDialogue(transform);
                }

                _input.listen = false;
            }
        }

        private void Hint()
        {
            if (_input.hint)
            {
                // if there is no dialogue active an there is no cinematic
                if (!possesionManager.IsTalking && (CinematicDialogue.CurrentNPC == null || CinematicDialogue.CurrentNPCNon == null))
                {
                    // hints appear
                    hintManager.Possess(transform);
                }

                _input.hint = false;
            }
        }

        private void History()
        {
            // Cinematic Mode
            if (CinematicDialogue.CurrentNPC != null)
            {
                // if the history button is pressed, choices panel, auto-talking mode and skip-talking mode are not available
                if (_input.history && !choicePanel.IsShowing && (!CinematicDialogue.CurrentNPC.AutoTalking && !CinematicDialogue.CurrentNPC.SkipTalking))
                {
                    // activates or desactivates the history
                    showingHistory = !showingHistory;
                    historyFontText.color = showingHistory ? Color.yellow : Color.white;
                    historyActionText.color = showingHistory ? Color.yellow : Color.white;
                    historyPanel.SetActive(showingHistory);
                    // if history is active
                    if (showingHistory)
                    {
                        // get the dialog history and display it
                        List<string> lines = dialogueHistory.GetHistory();
                        historyText.text = string.Join("\n", lines);
                        // cursor visible
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        // hide the cursor
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    _input.history = false;
                }
                return;
            }
            else if (CinematicDialogue.CurrentNPCNon != null)
            {
                // if the history button is pressed, choices panel, auto-talking mode and skip-talking mode are not available
                if (_input.history && !choicePanel.IsShowing && (!CinematicDialogue.CurrentNPCNon.AutoTalking && !CinematicDialogue.CurrentNPCNon.SkipTalking))
                {
                    // activates or desactivates the history
                    showingHistory = !showingHistory;
                    historyFontText.color = showingHistory ? Color.yellow : Color.white;
                    historyActionText.color = showingHistory ? Color.yellow : Color.white;
                    historyPanel.SetActive(showingHistory);
                    // if history is active
                    if (showingHistory)
                    {
                        // get the dialog history and display it
                        List<string> lines = dialogueHistory.GetHistory();
                        historyText.text = string.Join("\n", lines);
                        // cursor visible
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        // hide the cursor
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    _input.history = false;
                }
                return;
            }
            // Hint Mode
            else if (HintManager.CurrentHint != null)
            {
                // if the history button is pressed, choices panel, auto-talking mode and skip-talking mode are not available
                if (_input.history && !choicePanel.IsShowing && !HintManager.CurrentHint.AutoTalking && !HintManager.CurrentHint.SkipTalking)
                {
                    // activates or desactivates the history
                    showingHistory = !showingHistory;
                    historyFontText.color = showingHistory ? Color.yellow : Color.white;
                    historyActionText.color = showingHistory ? Color.yellow : Color.white;
                    historyPanel.SetActive(showingHistory);
                    // if history is active
                    if (showingHistory)
                    {
                        // get the dialog history and display it
                        List<string> lines = dialogueHistory.GetHistory();
                        historyText.text = string.Join("\n", lines);
                        // cursor visible
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        // hide the cursor
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    _input.history = false;
                }
                return;
            }

            // get the closest interactuable object
            var t = GetInteractuables();
            bool auto = false;
            bool skip = false;

            if (t is IPossessable p)
            {
                if (p is NPCPossessable npcP)
                {
                    auto = npcP.AutoTalking;
                    skip = npcP.SkipTalking;
                }
                else if (p is NPCNonPossessable npcNP)
                {
                    auto = npcNP.AutoTalking;
                    skip = npcNP.SkipTalking;
                }
            }
            // if auto-talking, skip-talking or choice panel is active the history will not be displayed
            if (!_input.history || !possesionManager.IsTalking || auto || skip || choicePanel.IsShowing)
            {
                _input.history = false;
                return;
            }
            // activates or desactivates the history
            showingHistory = !showingHistory;
            historyFontText.color = showingHistory ? Color.yellow : Color.white;
            historyActionText.color = showingHistory ? Color.yellow : Color.white;
            historyPanel.SetActive(showingHistory);
            // if history is active
            if (showingHistory)
            {
                // get the dialog history and display it
                List<string> lines = dialogueHistory.GetHistory();
                historyText.text = string.Join("\n", lines);
                // cursor visible
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                // hide the cursor
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            _input.history = false;
        }

        private void UI_Move()
        {
            // Cinematic Mode || Hint Mode
            if ((CinematicDialogue.CurrentNPC != null || CinematicDialogue.CurrentNPCNon != null) || HintManager.CurrentHint != null)
            {
                // if choices are being displayed
                if (choicePanel.IsShowing)
                {
                    // gets the scroll movement
                    scrollInput = _input.ui_move;
                    // only process input if enough time has passed
                    if (Time.time - lastInputTime > inputCooldown)
                    {
                        // if the movement is on the Y
                        if (Mathf.Abs(scrollInput.y) > 0.5f)
                        {
                            // up or down
                            int direction = scrollInput.y > 0 ? -1 : 1;
                            choicePanel.MoveSelection(direction);
                            lastInputTime = Time.time;
                        }
                    }

                    return;
                }

                if (!showingHistory)
                    return;
                // gets the scroll movement
                scrollInput = _input.ui_move;
                // if vertical movement is sufficient
                if (Mathf.Abs(scrollInput.y) > 0.1f)
                {
                    // update the scroll position
                    scrollRect.verticalNormalizedPosition += scrollInput.y * scrollSpeed * Time.deltaTime;
                    // scroll position is within the limits
                    scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
                }

                return;
            }

            // choices are beign displayed
            if (choicePanel.IsShowing)
            {
                // gets the scroll movement
                scrollInput = _input.ui_move;
                // only process input if enough time has passed
                if (Time.time - lastInputTime > inputCooldown)
                {
                    // if the movement is on the Y
                    if (Mathf.Abs(scrollInput.y) > 0.5f)
                    {
                        // up or down
                        int direction = scrollInput.y > 0 ? -1 : 1;
                        choicePanel.MoveSelection(direction);
                        lastInputTime = Time.time;
                    }
                }

                return;
            }

            if (!showingHistory || !possesionManager.IsTalking)
                return;
            // gets the scroll movement
            scrollInput = _input.ui_move;
            // if vertical movement is sufficient
            if (Mathf.Abs(scrollInput.y) > 0.1f)
            {
                // update the scroll position
                scrollRect.verticalNormalizedPosition += scrollInput.y * scrollSpeed * Time.deltaTime;
                // scroll position is within the limits
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
            }
        }

        private void Auto()
        {
            // Cinematic Mode
            if (CinematicDialogue.CurrentNPC != null)
            {
                // if the auto button is pressed, history, choices panel and skip-talking mode are not available
                if (_input.auto && !showingHistory && !choicePanel.IsShowing && !CinematicDialogue.CurrentNPC.SkipTalking)
                {
                    // start auto-talking
                    CinematicDialogue.CurrentNPC.AutoTalk();
                }

                _input.auto = false;
                return;
            }
            else if (CinematicDialogue.CurrentNPCNon != null)
            {
                // if the auto button is pressed, history, choices panel and skip-talking mode are not available
                if (_input.auto && !showingHistory && !choicePanel.IsShowing && !CinematicDialogue.CurrentNPCNon.SkipTalking)
                {
                    // start auto-talking
                    CinematicDialogue.CurrentNPCNon.AutoTalk();
                }

                _input.auto = false;
                return;
            }
            // Hint Mode
            else if (HintManager.CurrentHint != null)
            {
                // if the auto button is pressed, history, choices panel and skip-talking mode are not available
                if (_input.auto && !showingHistory && !choicePanel.IsShowing && !HintManager.CurrentHint.SkipTalking)
                {
                    // start auto-talking
                    HintManager.CurrentHint.AutoTalk();
                }

                _input.auto = false;
                return;
            }

            // get the closest interactuable object
            var t = GetInteractuables();
            bool skip = false;

            if (t is IPossessable p)
            {
                if (p is NPCPossessable npcP)
                {
                    skip = npcP.SkipTalking;
                }
                else if (p is NPCNonPossessable npcNP)
                {
                    skip = npcNP.SkipTalking;
                }
            }
            // if history, skip-talking or choice panel is active auto-talking will not activate
            if (showingHistory || skip || choicePanel.IsShowing)
            {
                _input.auto = false;
                return;
            }

            if (_input.auto)
            {
                // get the closest interactuable object
                var target = GetInteractuables();

                if (target is IPossessable possessable)
                {
                    if (possessable is NPCPossessable npcPossessable)
                    {
                        // start auto-talking
                        npcPossessable.AutoTalk();
                    }
                    else if (possessable is NPCNonPossessable npcNonPossessable)
                    {
                        // start auto-talking
                        npcNonPossessable.AutoTalk();
                    }
                }

                _input.auto = false;
            }
        }

        private void Skip()
        {
            // Cinematic Mode
            if (CinematicDialogue.CurrentNPC != null)
            {
                // if the skip button is pressed, history, choices panel and auto-talking mode are not available
                if (_input.skip && !showingHistory && !choicePanel.IsShowing && !CinematicDialogue.CurrentNPC.AutoTalking)
                {
                    // start skip-talking
                    CinematicDialogue.CurrentNPC.SkipTalk();
                }

                _input.skip = false;
                return;
            }
            else if (CinematicDialogue.CurrentNPCNon != null)
            {
                // if the skip button is pressed, history, choices panel and auto-talking mode are not available
                if (_input.skip && !showingHistory && !choicePanel.IsShowing && !CinematicDialogue.CurrentNPCNon.AutoTalking)
                {
                    // start skip-talking
                    CinematicDialogue.CurrentNPCNon.SkipTalk();
                }

                _input.skip = false;
                return;
            }
            // Hint Mode
            else if (HintManager.CurrentHint != null)
            {
                // if the skip button is pressed, history, choices panel and auto-talking mode are not available
                if (_input.skip && !showingHistory && !choicePanel.IsShowing && !HintManager.CurrentHint.AutoTalking)
                {
                    // start skip-talking
                    HintManager.CurrentHint.SkipTalk();
                }

                _input.skip = false;
                return;
            }

            // get the closest interactuable object
            var t = GetInteractuables();
            bool auto = false;

            if (t is IPossessable p)
            {
                if (p is NPCPossessable npcP)
                {
                    auto = npcP.AutoTalking;
                }
                else if (p is NPCNonPossessable npcNP)
                {
                    auto = npcNP.AutoTalking;
                }
            }
            // if history, auto-talking or choice panel is active skip-talking will not activate
            if (showingHistory || auto || choicePanel.IsShowing)
            {
                _input.skip = false;
                return;
            }

            if (_input.skip)
            {
                // get the closest interactuable object
                var target = GetInteractuables();

                if (target is IPossessable possessable)
                {
                    if (possessable is NPCPossessable npcPossessable)
                    {
                        // start skip-talking
                        npcPossessable.SkipTalk();
                    }
                    else if (possessable is NPCNonPossessable npcNonPossessable)
                    {
                        // start skip-talking
                        npcNonPossessable.SkipTalk();
                    }
                }

                _input.skip = false;
            }
        }

        private void SpectralVision()
        {
            if (volume != null)
            {
                // smooth transition of volume weight towards the target
                volume.weight = Mathf.Lerp(volume.weight, targetWeight, Time.deltaTime * transitionSpeed);
                
                // if it was completely disabled and the target was 0, disables color adjustments
                if (volume.weight <= 0.01f && targetWeight == 0f)
                {
                    if (colorAdjustments != null)
                        colorAdjustments.active = false;
                }
            }
            
            // if the spectral vision button is pressed and it is not yet active
            if (_input.spectralVision && !isSenseActive)
            {
                isSenseActive = true;
                // 100% of the visual effect
                targetWeight = 1f;

                // activate color settings
                if (colorAdjustments != null)
                    colorAdjustments.active = true;
                // enables highlighting of interactuable objects
                if (spectralVisionInteractuable != null)
                    spectralVisionInteractuable.SetActive(true);
                // enables highlighting of possessable objects
                if (spectralVisionPossessable != null)
                    spectralVisionPossessable.SetActive(true);

            }
            // if the button is released and was active, it is deactivated
            else if (!_input.spectralVision && isSenseActive)
            {
                isSenseActive = false;
                // 100% of the visual effect
                targetWeight = 0f;

                // disables highlighting of interactuable objects
                if (spectralVisionInteractuable != null)
                    spectralVisionInteractuable.SetActive(false);
                // disables highlighting of possessable objects
                if (spectralVisionPossessable != null)
                    spectralVisionPossessable.SetActive(false);
            }
        }

        private void LookingObject()
        {
            // reset animations
            possesionManager.CurrentController.DeactivateControl();
            Vector2 lookInput = _input.look;
            // minimum threshold
            float threshold = 0.1f;
            if (lookInput.sqrMagnitude < threshold * threshold)
                return;
            // rotate object
            objectManager.LookingObject.Rotate(Vector3.up, lookInput.x * rotationSpeed, Space.World);
            objectManager.LookingObject.Rotate(Vector3.left, lookInput.y * rotationSpeed, Space.World);
        }

        private void Cancel()
        {
            if (_input.cancel)
            {
                // if the player is looking a object
                if (objectManager != null && objectManager.Looking)
                {
                    objectManager.Looking = false;

                    _input.cancel = false;

                    return;
                }

                // if the player is possessing something, cancel the possession
                if (possesionManager.IsPossessing)
                {
                    possesionManager.CancelPossession();
                }

                _input.cancel = false;
            }
        }

        private void Pause()
        {
            if (_input.pause)
            {
             //   UnityEngine.Debug.Log(_input.pause);
                pauseManager.IsPaused = true;
                Cursor.visible = true;
                _input.pause = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);

            // interact sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            var index = 0;
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    foreach (Collider collider in colliderArray)
                    {
                        if(collider.CompareTag("Jardin"))
                        {
                            index = Random.Range(0, 4);
                            UnityEngine.Debug.Log("Pisas en Jardin");
                        }
                        else if(collider.CompareTag("Casa"))
                        {
                            index = Random.Range(5, FootstepAudioClips.Length);
                            UnityEngine.Debug.Log("Pisas en Casa");

                        }
                    }
                    UnityEngine.Debug.Log(index);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
        public void DeactivateControl()
        {
            // if the object has an animator, reset the speed values in the animations
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetFloat(_animIDMotionSpeed, 0f);
            }

            // reset vertical speed
            _verticalVelocity = 0f;

            // ensures that animation flags are the corrects
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
                _animator.SetBool(_animIDGrounded, true);
            }

            // start the coroutine that waits for the character to touch the ground
            StartCoroutine(HandleDeactivationDelay());
        }

        private IEnumerator HandleDeactivationDelay()
        {
            // reset vertical speed
            _verticalVelocity = 0f;

            // as long as the character is not touching the ground
            while (!Grounded)
            {
                // simulate the gravity to push it towards the ground
                _verticalVelocity -= -Gravity * Time.deltaTime;
                // applies vertical movement downwards
                _controller.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                yield return null;
            }

            // reset vertical speed
            _verticalVelocity = 0f;

            // ensures that animation flags are the corrects
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
                _animator.SetBool(_animIDGrounded, true);
            }

            yield return new WaitForSeconds(0.5f);

            // if the object has an animator, reset the speed values in the animations
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetFloat(_animIDMotionSpeed, 0f);
            }
        }
    }
}