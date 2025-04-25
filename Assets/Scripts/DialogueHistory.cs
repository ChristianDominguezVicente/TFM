using System.Collections.Generic;
using UnityEngine;

public class DialogueHistory : MonoBehaviour
{
    private List<string> history = new List<string>();

    public void AddLine(string speaker, string text)
    {
        // add to the history: "Name: Text"
        history.Add($"{speaker}: {text}");
    }

    public void AddSeparator()
    {
        // add a empty line
        history.Add("");
    }

    public List<string> GetHistory()
    {
        // returns the full history
        return history;
    }

    public void Clear()
    {
        // clears the dialogs history
        history.Clear();
    }
}
