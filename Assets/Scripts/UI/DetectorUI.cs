using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class DetectorUI : MonoBehaviour
{
    /*
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI listenText;
    [SerializeField] private TextMeshProUGUI nextText;
    [SerializeField] private TextMeshProUGUI autoText;
    [SerializeField] private TextMeshProUGUI historyText;
    [SerializeField] private TextMeshProUGUI skipText;
    [SerializeField] private string pcInteract;
    [SerializeField] private string pcListen;
    [SerializeField] private string pcNext;
    [SerializeField] private string pcAuto;
    [SerializeField] private string pcHistory;
    [SerializeField] private string pcSkip;
    [SerializeField] private string gamepadInteract;
    [SerializeField] private string gamepadListen;
    [SerializeField] private string gamepadNext;
    [SerializeField] private string gamepadAuto;
    [SerializeField] private string gamepadHistory;
    [SerializeField] private string gamepadSkip;
    [SerializeField] private TMP_FontAsset pcFont;
    [SerializeField] private TMP_FontAsset gamepadFont;
    */
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

    private void UpdateUIForKeyboard()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void UpdateUIForGamepad()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
