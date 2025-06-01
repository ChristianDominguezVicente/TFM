using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBeginning : MonoBehaviour
{
    [SerializeField] private CinematicDialogue cinematicDialogue;
    [SerializeField] private string scene;

    private void Start()
    {
        StartCoroutine(PlaySceneStart());
    }

    private IEnumerator PlaySceneStart()
    {
        cinematicDialogue.PlayDialogue();

        while (!cinematicDialogue.End)
        {
            yield return null;
        }

        SceneManager.LoadScene(scene);
    }
}
