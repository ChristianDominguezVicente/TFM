using System.Collections.Generic;
using UnityEngine;

public class DialogueHistory : MonoBehaviour
{
    private List<string> history = new List<string>();

    public void AddLine(string speaker, string text)
    {
        string line = $"{speaker}: {text}";
        if (!history.Contains(line))
        {
            // add to the history: "Name: Text"
            history.Add($"{speaker}: {text}");
            // save history in the Save System
            SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
            ssm.SetHistory(line);
        }
    }

    public void AddSeparator()
    {
        if (history[history.Count - 1] != "")
        {
            // add a empty line
            history.Add("");
            // save history in the Save System
            SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
            ssm.SetHistory("");
        }
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

    public void OnLoad(string value)
    {
        // load the history saved in the Save System
        history = new List<string>(value.Split("\n"));
        AddSeparator();
    }
}
