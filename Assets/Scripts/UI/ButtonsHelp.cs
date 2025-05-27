using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class ButtonsHelp : MonoBehaviour
{
    //text
    [SerializeField] private TextMeshProUGUI moveText;
    [SerializeField] private TextMeshProUGUI selectText;
    [SerializeField] private TextMeshProUGUI backText;

    [Header("PC Inputs")]
    [SerializeField] private string pcMove;
    [SerializeField] private string pcSelect;
    [SerializeField] private string pcBack;

    [Header("PS Inputs")]
    [SerializeField] private string gamepadMove;
    [SerializeField] private string gamepadSelect;
    [SerializeField] private string gamepadBack;

    [Header("XBOX Inputs")]
    [SerializeField] private string gamepadXBOXMove;
    [SerializeField] private string gamepadXBOXSelect;
    [SerializeField] private string gamepadXBOXBack;



    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset pcFont;
    [SerializeField] private TMP_FontAsset gamepadFont;
    [SerializeField] private TMP_FontAsset pcMouseFont;
    private string controlUsed;

    private void OnEnable()
    {
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
            //xbox
            if (device.name.Contains("xbox") || device.name.Contains("xinput") || UnityEngine.InputSystem.Gamepad.current is UnityEngine.InputSystem.XInput.XInputController)
            {
                UpdateUIForGamepadXBOX();
            }
            else
            {

                UpdateUIForGamepadPS();
            }
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
        moveText.text = pcMove;
        selectText.text = pcSelect;
        backText.text = pcBack;

        moveText.font = pcFont;
        selectText.font = pcFont;
        backText.font = pcFont;

    }

    private void UpdateUIForGamepadPS()
    {
        moveText.text = gamepadMove;
        selectText.text = gamepadSelect;
        backText.text = gamepadBack;

        moveText.font = gamepadFont;
        selectText.font = gamepadFont;
        backText.font = gamepadFont;
    }

    private void UpdateUIForGamepadXBOX()
    {
        moveText.text = gamepadXBOXMove;
        selectText.text = gamepadXBOXSelect;
        backText.text = gamepadXBOXBack;

        moveText.font = gamepadFont;
        selectText.font = pcFont;
        backText.font = pcFont;
    }




}
