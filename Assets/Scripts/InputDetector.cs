using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputDetector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI listenText;
    [SerializeField] private TextMeshProUGUI nextText;
    [SerializeField] private TextMeshProUGUI autoText;
    [SerializeField] private TextMeshProUGUI historyText;
    [SerializeField] private TextMeshProUGUI skipText;

    //Button help HUD
    [SerializeField] private TextMeshProUGUI clue;
    [SerializeField] private TextMeshProUGUI mission;
    [SerializeField] private TextMeshProUGUI pause;
    [SerializeField] private TextMeshProUGUI vision; // computer mouse


    [SerializeField] private string pcInteract;
    [SerializeField] private string pcListen;
    [SerializeField] private string pcNext;
    [SerializeField] private string pcAuto;
    [SerializeField] private string pcHistory;
    [SerializeField] private string pcSkip;
    [SerializeField] private string pcClue;
    [SerializeField] private string pcMission;
    [SerializeField] private string pcPause;
    [SerializeField] private string pcVision;


    [SerializeField] private string gamepadInteract;
    [SerializeField] private string gamepadListen;
    [SerializeField] private string gamepadNext;
    [SerializeField] private string gamepadAuto;
    [SerializeField] private string gamepadHistory;
    [SerializeField] private string gamepadSkip;
    [SerializeField] private string gamepadClue;
    [SerializeField] private string gamepadMission;
    [SerializeField] private string gamepadPause;
    [SerializeField] private string gamepadVision;


    [SerializeField] private TMP_FontAsset pcFont;
    [SerializeField] private TMP_FontAsset gamepadFont;
    [SerializeField] private TMP_FontAsset pcMouseFont;
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
        interactText.text = gamepadInteract;
        listenText.text = gamepadListen;
        nextText.text = gamepadNext;
        autoText.text = gamepadAuto;
        historyText.text = gamepadHistory;
        skipText.text = gamepadSkip;
        clue.text = gamepadClue;
        mission.text = gamepadMission;
        pause.text = gamepadPause;
        vision.text = gamepadVision;

        interactText.font = gamepadFont;
        listenText.font = gamepadFont;
        nextText.font = gamepadFont;
        autoText.font = gamepadFont;
        historyText.font = gamepadFont;
        skipText.font = gamepadFont;
        clue.font = gamepadFont;
        mission.font = gamepadFont;
        pause.font = gamepadFont;
        vision.font = gamepadFont;
    }

    private void UpdateUIForKeyboard()
    {
        interactText.text = pcInteract;
        listenText.text = pcListen;
        nextText.text = pcNext;
        autoText.text = pcAuto;
        historyText.text = pcHistory;
        skipText.text = pcSkip;
        clue.text = pcClue;
        mission.text = pcMission;
        pause.text = pcPause;
        vision.text = pcVision;



        interactText.font = pcFont;
        listenText.font = pcFont;
        nextText.font = pcFont;
        autoText.font = pcFont;
        historyText.font = pcFont;
        skipText.font = pcFont;
        clue.font = pcFont;
        mission.font = pcFont;
        pause.font = pcFont;
        vision.font = pcMouseFont;
    }
}
