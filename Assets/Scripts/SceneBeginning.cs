using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBeginning : MonoBehaviour
{
    [SerializeField] private CinematicDialogue cinematicDialogue;
    [SerializeField] private string scene;
    [SerializeField] private bool nextScene = false;

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

        if(nextScene)
            SceneManager.LoadScene(scene);
    }
}
