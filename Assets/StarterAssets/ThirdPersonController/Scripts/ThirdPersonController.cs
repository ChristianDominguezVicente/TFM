using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
//using Unity.Android.Gradle.Manifest;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
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

        [Header("Dialogue History")]
        [SerializeField] private DialogueHistory dialogueHistory;
        [SerializeField] private GameObject historyPanel;
        [SerializeField] private TextMeshProUGUI historyText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float scrollSpeed;
        [SerializeField] private ChoicesUI choicePanel;

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
            if (possesionManager.IsTalking || CinematicDialogue.CurrentNPC != null)
            {
                Interact();
                History();
                UI_Move();
                Auto();
                Skip();
                return;
            }

            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
            Interact();
            SpectralVision();
            Cancel();
        }

        private void LateUpdate()
        {
            if (!possesionManager.IsTalking)
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
            if (CinematicDialogue.CurrentNPC != null)
            {
                if (_input.interact && !showingHistory)
                {
                    if (choicePanel.IsShowing)
                    {
                        CinematicDialogue.CurrentNPC.SelectCurrentChoice();
                    }
                    else if (!CinematicDialogue.CurrentNPC.AutoTalking && !CinematicDialogue.CurrentNPC.SkipTalking)
                    {
                        CinematicDialogue.CurrentNPC.Possess(transform);
                    }
                }

                _input.interact = false;
                return;
            }

            if (choicePanel.IsShowing && _input.interact)
            {
                if (GetInteractuables() is NPCPossessable npcP)
                {
                    npcP.SelectCurrentChoice();
                }

                _input.interact = false;
                return;
            }

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
            }

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
                    possessable.Possess(transform);
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
                        float distance = Vector3.Distance(transform.position, interactuable.GetTransform().position);
                        if (distance < closestDistance)
                        {
                            closest = interactuable;
                            closestDistance = distance;
                        }
                    }
                    else if (collider.TryGetComponent(out IPossessable possessable))
                    {
                        // calculate the distance to the possessable object
                        float distance = Vector3.Distance(transform.position, possessable.GetTransform().position);
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
                        float distance = Vector3.Distance(transform.position, possessable.GetTransform().position);
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

        private void History()
        {
            if (CinematicDialogue.CurrentNPC != null)
            {
                if (_input.history && !choicePanel.IsShowing && !CinematicDialogue.CurrentNPC.AutoTalking && !CinematicDialogue.CurrentNPC.SkipTalking)
                {
                    showingHistory = !showingHistory;
                    historyPanel.SetActive(showingHistory);

                    if (showingHistory)
                    {
                        List<string> lines = dialogueHistory.GetHistory();
                        historyText.text = string.Join("\n", lines);

                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    _input.history = false;
                }
                return;
            }

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
            }

            if (!_input.history || !possesionManager.IsTalking || auto || skip || choicePanel.IsShowing)
            {
                _input.history = false;
                return;
            }

            showingHistory = !showingHistory;
            historyPanel.SetActive(showingHistory);

            if (showingHistory)
            {
                List<string> lines = dialogueHistory.GetHistory();
                historyText.text = string.Join("\n", lines);

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            _input.history = false;
        }

        private void UI_Move()
        {
            if (CinematicDialogue.CurrentNPC != null)
            {
                if (choicePanel.IsShowing)
                {
                    scrollInput = _input.ui_move;

                    if (Time.time - lastInputTime > inputCooldown)
                    {
                        if (Mathf.Abs(scrollInput.y) > 0.5f)
                        {
                            int direction = scrollInput.y > 0 ? -1 : 1;
                            choicePanel.MoveSelection(direction);
                            lastInputTime = Time.time;
                        }
                    }

                    return;
                }

                if (!showingHistory)
                    return;

                scrollInput = _input.ui_move;

                if (Mathf.Abs(scrollInput.y) > 0.1f)
                {
                    scrollRect.verticalNormalizedPosition += scrollInput.y * scrollSpeed * Time.deltaTime;
                    scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
                }

                return;
            }

            if (choicePanel.IsShowing)
            {
                scrollInput = _input.ui_move;

                if (Time.time - lastInputTime > inputCooldown)
                {
                    if (Mathf.Abs(scrollInput.y) > 0.5f) 
                    {
                        int direction = scrollInput.y > 0 ? -1 : 1;
                        choicePanel.MoveSelection(direction);
                        lastInputTime = Time.time;
                    }
                }

                return;
            }

            if (!showingHistory || !possesionManager.IsTalking)
                return;

            scrollInput = _input.ui_move;

            if (Mathf.Abs(scrollInput.y) > 0.1f)
            {
                scrollRect.verticalNormalizedPosition += scrollInput.y * scrollSpeed * Time.deltaTime;
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
            }
        }

        private void Auto()
        {
            if (CinematicDialogue.CurrentNPC != null)
            {
                if (_input.auto && !showingHistory && !choicePanel.IsShowing && !CinematicDialogue.CurrentNPC.SkipTalking)
                {
                    CinematicDialogue.CurrentNPC.AutoTalk();
                }

                _input.auto = false;
                return;
            }

            var t = GetInteractuables();
            bool skip = false;

            if (t is IPossessable p)
            {
                if (p is NPCPossessable npcP)
                {
                    skip = npcP.SkipTalking;
                }
            }

            if (showingHistory || skip || choicePanel.IsShowing)
            {
                _input.auto = false;
                return;
            }

            if (_input.auto)
            {
                var target = GetInteractuables();

                if (target is IPossessable possessable)
                {
                    if (possessable is NPCPossessable npcPossessable)
                    {
                        npcPossessable.AutoTalk();
                    }
                }

                _input.auto = false;
            }
        }

        private void Skip()
        {
            if (CinematicDialogue.CurrentNPC != null)
            {
                if (_input.skip && !showingHistory && !choicePanel.IsShowing && !CinematicDialogue.CurrentNPC.AutoTalking)
                {
                    CinematicDialogue.CurrentNPC.SkipTalk();
                }

                _input.skip = false;
                return;
            }

            var t = GetInteractuables();
            bool auto = false;

            if (t is IPossessable p)
            {
                if (p is NPCPossessable npcP)
                {
                    auto = npcP.AutoTalking;
                }
            }

            if (showingHistory || auto || choicePanel.IsShowing)
            {
                _input.skip = false;
                return;
            }

            if (_input.skip)
            {
                var target = GetInteractuables();

                if (target is IPossessable possessable)
                {
                    if (possessable is NPCPossessable npcPossessable)
                    {
                        npcPossessable.SkipTalk();
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

        private void Cancel()
        {
            if (_input.cancel)
            {
                // if the player is possessing something, cancel the possession
                if (possesionManager.IsPossessing)
                {
                    possesionManager.CancelPossession();
                }

                _input.cancel = false;
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
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
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