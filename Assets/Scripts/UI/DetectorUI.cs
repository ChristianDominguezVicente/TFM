using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class DetectorUI : MonoBehaviour
{
    // Controls Settings 
    [SerializeField] private Sprite psImage;
    [SerializeField] private Sprite pcImage;
    [SerializeField] private Sprite xboxImage;
    [SerializeField] private Image imageGeneral;

    private string controlUsed;
    private Sprite imageControls;

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
            if (device.name.Contains("xbox") || device.name.Contains("xinput") || UnityEngine.InputSystem.Gamepad.current is UnityEngine.InputSystem.XInput.XInputController)
            {
          //      Debug.Log("Mando de Xbox conectado");
                imageControls = xboxImage;
            }
            else
            {
          //      Debug.Log("Mando de PlayStation conectado");
                imageControls = psImage;
            }
            controlUsed = "Gamepad";
            UpdateUIForGamepad();
        }
        // if a Keyboard or Mouse is detected and it was not the previous controller, update to Keyboard
        else if ((device is Keyboard || device is Mouse) && controlUsed != "Keyboard")
        {
            imageControls = pcImage;
            controlUsed = "Keyboard";
            UpdateUIForKeyboard();
        }
        UpdateUiForControls();
    }

    private void UpdateUiForControls()
    {
        if (imageGeneral != null && imageControls != null)
        {
            imageGeneral.sprite = imageControls;
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
