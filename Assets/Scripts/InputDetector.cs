using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputDetector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private string pc;
    [SerializeField] private string gamepad;
    [SerializeField] private TMP_FontAsset pcFont;
    [SerializeField] private TMP_FontAsset gamepadFont;

    private string controlUsed;

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
            controlUsed = "Gamepad";
            UpdateUIForGamepad();
        }
        // if a Keyboard or Mouse is detected and it was not the previous controller, update to Keyboard
        else if ((device is Keyboard || device is Mouse) && controlUsed != "Keyboard")
        {
            controlUsed = "Keyboard";
            UpdateUIForKeyboard();
        }
    }

    private void UpdateUIForGamepad()
    {
        interactText.text = gamepad;
        interactText.font = gamepadFont;
    }

    private void UpdateUIForKeyboard()
    {
        interactText.text = pc;
        interactText.font = pcFont;
    }
}
