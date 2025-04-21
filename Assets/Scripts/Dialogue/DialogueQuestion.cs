using System;
using UnityEngine;

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
