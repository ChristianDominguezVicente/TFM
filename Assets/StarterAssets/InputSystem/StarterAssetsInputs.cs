using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
        public Vector2 ui_move;
        public Vector2 look;
		public bool jump;
		public bool sprint;
        public bool interact;
        public bool listen;
        public bool hint;
        public bool history;
        public bool auto;
        public bool skip;
        public bool spectralVision;
		public bool cancel;
		public bool pause;

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnUI_Move(InputValue value)
        {
            UI_MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
                LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            InteractInput(value.isPressed);
        }

        public void OnListen(InputValue value)
        {
            ListenInput(value.isPressed);
        }

        public void OnHint(InputValue value)
        {
            HintInput(value.isPressed);
        }

        public void OnHistory(InputValue value)
        {
            HistoryInput(value.isPressed);
        }

        public void OnAuto(InputValue value)
        {
            AutoInput(value.isPressed);
        }

        public void OnSkip(InputValue value)
        {
            SkipInput(value.isPressed);
        }

        public void OnSpectralVision(InputValue value)
        {
            SpectralVisionInput(value.isPressed);
        }

        public void OnCancel(InputValue value)
        {
            CancelInput(value.isPressed);
        }

        public void OnPause(InputValue value)
        {
            PauseInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void UI_MoveInput(Vector2 newUI_MoveDirection)
        {
            ui_move = newUI_MoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void ListenInput(bool newListenState)
        {
            listen = newListenState;
        }

        public void HintInput(bool newHintState)
        {
            hint = newHintState;
        }

        public void HistoryInput(bool newHistoryState)
        {
            history = newHistoryState;
        }

        public void AutoInput(bool newAutoState)
        {
            auto = newAutoState;
        }

        public void SkipInput(bool newSkipState)
        {
            skip = newSkipState;
        }

        public void SpectralVisionInput(bool newSpectralVisionState)
        {
            spectralVision = newSpectralVisionState;
        }

        public void CancelInput(bool cancelState)
        {
            cancel = cancelState;
        }

        public void PauseInput(bool pauseState)
        {
            pause = pauseState;
        }

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}