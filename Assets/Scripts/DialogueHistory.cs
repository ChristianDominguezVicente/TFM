using System.Collections.Generic;
using UnityEngine;

public class DialogueHistory : MonoBehaviour
{
    private List<string> history = new List<string>();

    public void AddLine(string speaker, string text)
    {
        history.Add($"{speaker}: {text}");
    }

    public void AddSeparator()
    {
        history.Add("");
    }

    public List<string> GetHistory()
    {
        return history;
    }

    public void Clear()
    {
        history.Clear();
    }
}
