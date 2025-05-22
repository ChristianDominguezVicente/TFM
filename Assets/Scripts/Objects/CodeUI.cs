using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CodeUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI[] digitTexts;

    private int[] correctCode = new int[4];
    private int[] currentCode = new int[4];
    private int currentIndex = 0;
    private bool active = false;
    private float inputTime = 0f;
    private float inputCoolDown = 0.2f;
    private CodeInteractuable currentInteractuable;
    private bool showingError = false;
    public bool Active { get => active; set => active = value; }

    // show and configure the Code Panel
    public void Show(CodeInteractuable interactuable)
    {
        currentInteractuable = interactuable;
        correctCode = interactuable.Code;
        active = true;
        panel.SetActive(true);
        currentIndex = 0;
        for (int i = 0; i < 4; i++) currentCode[i] = 0;
        // update UI
        UpdateVisual();
    }

    // show and clean the Code Panel
    public void Hide()
    {
        active = false;
        panel.SetActive(false);
        currentIndex = 0;
        currentInteractuable = null;
    }

    // handle the inputs of the player
    public void Inputs(Vector2 ui_move, bool interact, bool cancel)
    {
        // if the panel isn't active, cooldown or showing a error
        if (!active || Time.time - inputTime < inputCoolDown || showingError)
            return;

        // right move
        if (ui_move.x > 0.5f)
        {
            currentIndex = (currentIndex + 1) % 4;
            inputTime = Time.time;
        }
        // left move
        else if (ui_move.x < -0.5f)
        {
            currentIndex = (currentIndex + 3) % 4;
            inputTime = Time.time;
        }
        // up move
        if (ui_move.y > 0.5f)
        {
            currentCode[currentIndex] = (currentCode[currentIndex] + 1) % 10;
            inputTime = Time.time;
        }
        // down move
        else if (ui_move.y < -0.5f)
        {
            currentCode[currentIndex] = (currentCode[currentIndex] + 9) % 10;
            inputTime = Time.time;
        }

        // if the player confirm
        if (interact)
        {
            // verification code
            for (int i = 0; i < 4; i++)
            {
                if (currentCode[i] != correctCode[i])
                {
                    // if the code is incorrect
                    StartCoroutine(IncorrectCode());
                    return;
                }
            }
            // if the code is correct
            currentInteractuable.OpenDrawer();
        }

        // if the player press cancel
        if (cancel)
        {
            Hide();
            return;
        }
        // update UI
        UpdateVisual();
    }

    private IEnumerator IncorrectCode()
    {
        showingError = true;
        // change color
        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].color = Color.red;
        }
        yield return new WaitForSeconds(0.5f);

        showingError = false;
        // update UI
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].text = currentCode[i].ToString();
            // change current digit to yellow
            digitTexts[i].color = (i == currentIndex) ? Color.yellow : Color.white;
        }
    }
}
