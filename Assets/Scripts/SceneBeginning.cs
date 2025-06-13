using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBeginning : MonoBehaviour
{
    [SerializeField] private CinematicDialogue cinematicDialogue;
    [SerializeField] private CinematicDialogue cinematicDialogue2;
    [SerializeField] private CinematicDialogue cinematicDialogue3;
    [SerializeField] private string scene;
    [SerializeField] private bool nextScene = false;

    private void Start()
    {
        StartCoroutine(PlaySceneStart());
    }

    private IEnumerator PlaySceneStart()
    {
        if (SceneManager.GetActiveScene().name == "Final")
        {
            SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
            float karma = PlayerPrefs.GetFloat("Karma", 0);
            if (karma > 0)
            {
                cinematicDialogue.PlayDialogue();

                while (!cinematicDialogue.End)
                {
                    yield return null;
                }
            }
            else if (karma == 0)
            {
                cinematicDialogue2.PlayDialogue();

                while (!cinematicDialogue2.End)
                {
                    yield return null;
                }
            }
            else if (karma < 0)
            {
                cinematicDialogue3.PlayDialogue();

                while (!cinematicDialogue3.End)
                {
                    yield return null;
                }
            }
        }
        else
        {
            cinematicDialogue.PlayDialogue();

            while (!cinematicDialogue.End)
            {
                yield return null;
            }
        }

        if (nextScene)
            SceneManager.LoadScene(scene);
    }
}
