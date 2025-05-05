using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChoicesUI : MonoBehaviour
{
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choicePanel;
    [SerializeField] private Image possessImage;

    private List<Button> currentButtons = new List<Button>();
    private DialogueQuestion.Response[] currentChoices;
    private int selectedIndex = 0;

    public bool IsShowing => gameObject.activeSelf;

    public int SelectedIndex { get => selectedIndex; set => selectedIndex = value; }

    public void Show(DialogueQuestion.Response[] choices)
    {
        // remove any previous buttons
        Clear();
        // reset the index
        selectedIndex = 0;
        // save the choices
        currentChoices = choices;

        // creates a button for each choice
        foreach (var choice in choices)
        {
            GameObject button = Instantiate(choiceButtonPrefab, choicePanel);
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            text.text = choice.playerText;
            // add the button to the current list
            currentButtons.Add(button.GetComponent<Button>());
        }

        gameObject.SetActive(true);
        // update the buttons
        UpdatePanel();
    }

    public void MoveSelection(int direction)
    {
        if (currentButtons.Count == 0) return;

        selectedIndex += direction;
        // if you pass the first or last index creates a loop
        if (selectedIndex < 0) 
            selectedIndex = currentButtons.Count - 1;
        else if (selectedIndex >= currentButtons.Count) 
            selectedIndex = 0;
        // update the buttons
        UpdatePanel();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Clear();
    }

    private void Clear()
    {
        foreach (Button button in currentButtons)
        {
            Destroy(button.gameObject);
        }
        currentButtons.Clear();
        possessImage.gameObject.SetActive(false);
    }

    private void UpdatePanel()
    {
        for (int i = 0; i < currentButtons.Count; i++)
        {
            var colors = currentButtons[i].colors;
            // the selected button turns yellow
            if (i == selectedIndex)
            {
                colors.normalColor = Color.yellow;
            }
            else
            {
                colors.normalColor = Color.white;
            }
            currentButtons[i].colors = colors;
        }
        // the selected choice show his image
        possessImage.sprite = currentChoices[selectedIndex].responseImage;
        possessImage.gameObject.SetActive(currentChoices[selectedIndex].responseImage != null);
    }
}
