using System;
using UnityEngine;

// Represents a question that the NPC asks the player,
// with answer options available.
[Serializable]
public class DialogueQuestion : DialogueNode
{
    [Serializable]
    public class Response
    {
        public string playerText;
        public int nextIndex = -1;
    }

    public Response[] responses;
}
