using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualUI_MoveInput(Vector2 virtualUI_MoveDirection)
        {
            starterAssetsInputs.UI_MoveInput(virtualUI_MoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs.SprintInput(virtualSprintState);
        }

        public void VirtualInteractInput(bool virtualInteractState)
        {
            starterAssetsInputs.InteractInput(virtualInteractState);
        }

        public void VirtualListenInput(bool virtualListenState)
        {
            starterAssetsInputs.ListenInput(virtualListenState);
        }

        public void VirtualHintInput(bool virtualHintState)
        {
            starterAssetsInputs.HintInput(virtualHintState);
        }

        public void VirtualHistoryInput(bool virtualHistoryState)
        {
            starterAssetsInputs.HistoryInput(virtualHistoryState);
        }

        public void VirtualAutoInput(bool virtualAutoState)
        {
            starterAssetsInputs.AutoInput(virtualAutoState);
        }

        public void VirtualSkipInput(bool virtualSkipState)
        {
            starterAssetsInputs.SkipInput(virtualSkipState);
        }

        public void VirtualSpectralVisionInput(bool virtualSpectralVisionState)
        {
            starterAssetsInputs.SpectralVisionInput(virtualSpectralVisionState);
        }

        public void VirtualCancelInput(bool virtualCancelState)
        {
            starterAssetsInputs.CancelInput(virtualCancelState);
        }

        public void VirtualPauseInput(bool virtualPauseState)
        {
            starterAssetsInputs.PauseInput(virtualPauseState);
        }
        public void VirtualSMeInput(bool virtualSMState)
        {
            starterAssetsInputs.SMInput(virtualSMState);
        }
    }

}
