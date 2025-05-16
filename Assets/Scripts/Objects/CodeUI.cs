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
    public void Show(CodeInteractuable interactuable)
    {
        currentInteractuable = interactuable;
        correctCode = interactuable.Code;
        active = true;
        panel.SetActive(true);
        currentIndex = 0;
        for (int i = 0; i < 4; i++) currentCode[i] = 0;
        UpdateVisual();
    }
    
    public void Hide()
    {
        active = false;
        panel.SetActive(false);
        currentIndex = 0;
        currentInteractuable = null;
    }

    public void Inputs(Vector2 ui_move, bool interact, bool cancel)
    {
        if (!active || Time.time - inputTime < inputCoolDown || showingError)
            return;

        if (ui_move.x > 0.5f)
        {
            currentIndex = (currentIndex + 1) % 4;
            inputTime = Time.time;
        }
        else if (ui_move.x < -0.5f)
        {
            currentIndex = (currentIndex + 3) % 4;
            inputTime = Time.time;
        }

        if (ui_move.y > 0.5f)
        {
            currentCode[currentIndex] = (currentCode[currentIndex] + 1) % 10;
            inputTime = Time.time;
        }
        else if (ui_move.y < -0.5f)
        {
            currentCode[currentIndex] = (currentCode[currentIndex] + 9) % 10;
            inputTime = Time.time;
        }

        if (interact)
        {
            for (int i = 0; i < 4; i++)
            {
                if (currentCode[i] != correctCode[i])
                {
                    StartCoroutine(IncorrectCode());
                    return;
                }
            }
            currentInteractuable.OpenDrawer();
        }

        if (cancel)
        {
            Hide();
            return;
        }

        UpdateVisual();
    }

    private IEnumerator IncorrectCode()
    {
        showingError = true;

        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].color = Color.red;
        }
        yield return new WaitForSeconds(0.5f);

        showingError = false;
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        for (int i = 0; i < digitTexts.Length; i++)
        {
            digitTexts[i].text = currentCode[i].ToString();
            digitTexts[i].color = (i == currentIndex) ? Color.yellow : Color.white;
        }
    }
}
