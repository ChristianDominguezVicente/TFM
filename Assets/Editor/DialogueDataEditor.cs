using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueData))]
public class DialogueDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DialogueData dialogueData = (DialogueData)target;

        if (dialogueData.nodes == null)
        {
            dialogueData.nodes = new DialogueNode[0];
        }

        if (GUILayout.Button("Add Phrase"))
        {
            AddNode<DialoguePhrase>(dialogueData);
        }

        if (GUILayout.Button("Add Question"))
        {
            AddNode<DialogueQuestion>(dialogueData);
        }

        EditorGUILayout.Space(10);

        for (int i = 0; i < dialogueData.nodes.Length; i++)
        {
            DialogueNode node = dialogueData.nodes[i];
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Node {i} - {node.GetType().Name}", EditorStyles.boldLabel);

            if (node is DialoguePhrase phrase)
            {
                phrase.npcText = EditorGUILayout.TextField("NPC Text", phrase.npcText);
                phrase.nextIndex = EditorGUILayout.IntField("Next Index", phrase.nextIndex);
            }
            else if (node is DialogueQuestion question)
            {
                question.npcText = EditorGUILayout.TextField("NPC Question", question.npcText);

                if (question.responses == null)
                    question.responses = new DialogueQuestion.Response[0];

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Responses:", EditorStyles.boldLabel);

                for (int j = 0; j < question.responses.Length; j++)
                {
                    var response = question.responses[j];
                    EditorGUILayout.BeginHorizontal();
                    response.playerText = EditorGUILayout.TextField($"Text {j}", response.playerText);
                    response.nextIndex = EditorGUILayout.IntField("Next", response.nextIndex);

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        RemoveResponse(question, j);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Response"))
                {
                    AddResponse(question);
                }

                if (GUILayout.Button("Delete Response"))
                {
                    question.responses = new DialogueQuestion.Response[0];
                }
            }

            if (GUILayout.Button("Delete Node"))
            {
                RemoveNode(dialogueData, i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if (GUI.changed)
        {
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