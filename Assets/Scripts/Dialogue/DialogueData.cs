using UnityEngine;

// ScriptableObject that acts as a data container for the dialog system.
// [CreateAssetMenu] allows creating a new dialog asset.
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue")]
public class DialogueData : ScriptableObject
{ 
    // [SerializeReference] can store references to different types of objects.
    [SerializeReference]
    public DialogueNode[] nodes; // this contains all the dialog nodes in the sequence
}
