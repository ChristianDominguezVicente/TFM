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

    private string controlUsed;

    private void OnEnable()
    {
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        if (device is Gamepad && controlUsed != "Gamepad")
        {
            controlUsed = "Gamepad";
            UpdateUIForGamepad();
        }
        else if ((device is Keyboard || device is Mouse) && controlUsed != "Keyboard")
        {
            controlUsed = "Keyboard";
            UpdateUIForKeyboard();
        }
    }

    private void UpdateUIForGamepad()
    {
        interactText.text = gamepad;
    }

    private void UpdateUIForKeyboard()
    {
        interactText.text = pc;
    }
}
