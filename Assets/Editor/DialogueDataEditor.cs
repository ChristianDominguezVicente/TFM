using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueData))]
public class DialogueDataEditor : Editor
{
    // override the OnInspectorGUI method to customize the Inspector UI
    public override void OnInspectorGUI()
    {
        // DialogueData instance
        DialogueData dialogueData = (DialogueData)target;
        // if there are no nodes in the DialogueData
        if (dialogueData.nodes == null)
        {
            // an empty array is initialized
            dialogueData.nodes = new DialogueNode[0];
        }
        // button to add a new node of type DialoguePhrase
        if (GUILayout.Button("Add Phrase"))
        {
            AddNode<DialoguePhrase>(dialogueData);
        }
        // button to add a new node of type DialogueQuestion
        if (GUILayout.Button("Add Question"))
        {
            AddNode<DialogueQuestion>(dialogueData);
        }

        EditorGUILayout.Space(10);
        // display int the Inspector all the nodes in DialogueData
        for (int i = 0; i < dialogueData.nodes.Length; i++)
        {
            DialogueNode node = dialogueData.nodes[i];
            EditorGUILayout.BeginVertical("box");
            // type of the node
            EditorGUILayout.LabelField($"Node {i} - {node.GetType().Name}", EditorStyles.boldLabel);
            // if the node is DialoguePhrase
            if (node is DialoguePhrase phrase)
            {
                // showing the fields
                phrase.npcText = EditorGUILayout.TextField("NPC Text", phrase.npcText);
                phrase.nextIndex = EditorGUILayout.IntField("Next Index", phrase.nextIndex);
            }
            // if the node is DialogueQuestion
            else if (node is DialogueQuestion question)
            {
                question.npcText = EditorGUILayout.TextField("NPC Question", question.npcText);
                // if there are no responses
                if (question.responses == null)
                    question.responses = new DialogueQuestion.Response[0]; // an empty array is initializated

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Responses:", EditorStyles.boldLabel);
                // iterate over all the responses in that question
                for (int j = 0; j < question.responses.Length; j++)
                {
                    // showing the fields
                    var response = question.responses[j];
                    EditorGUILayout.BeginHorizontal();
                    response.playerText = EditorGUILayout.TextField($"Text {j}", response.playerText);
                    response.nextIndex = EditorGUILayout.IntField("Next", response.nextIndex);
                    // button to remove the response
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        RemoveResponse(question, j);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }
                // button to add a response
                if (GUILayout.Button("Add Response"))
                {
                    AddResponse(question);
                }
                // button to remove the responses
                if (GUILayout.Button("Delete Response"))
                {
                    question.responses = new DialogueQuestion.Response[0];
                }
            }
            // button to delete the current node
            if (GUILayout.Button("Delete Node"))
            {
                RemoveNode(dialogueData, i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        // if any changes were made to the GUI
        if (GUI.changed)
        {
            // mark the object as dirty so Unity saves it
            EditorUtility.SetDirty(dialogueData);
        }
    }

    private void AddNode<T>(DialogueData dialogueData) where T : DialogueNode, new()
    {
        ArrayUtility.Add(ref dialogueData.nodes, new T());
    }

    private void RemoveNode(DialogueData dialogueData, int index)
    {
        ArrayUtility.RemoveAt(ref dialogueData.nodes, index);
    }

    private void AddResponse(DialogueQuestion question)
    {
        var response = new DialogueQuestion.Response();
        ArrayUtility.Add(ref question.responses, response);
    }

    private void RemoveResponse(DialogueQuestion question, int index)
    {
        ArrayUtility.RemoveAt(ref question.responses, index);
    }
}