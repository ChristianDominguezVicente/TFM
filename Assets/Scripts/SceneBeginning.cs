using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBeginning : MonoBehaviour
{
    [SerializeField] private CinematicDialogue cinematicDialogue;
    [SerializeField] private CinematicDialogue cinematicDialogue2;
    [SerializeField] private string scene;
    [SerializeField] private bool nextScene = false;

    private AudioConfig audioConfig;

    private void Start()
    {
        audioConfig = (AudioConfig)FindAnyObjectByType(typeof(AudioConfig));
        StartCoroutine(PlaySceneStart());
    }

    private IEnumerator PlaySceneStart()
    {
        if (SceneManager.GetActiveScene().name == "Final")
        {
            audioConfig.MuteMusic();
            // obtain Karma value of the Save System
            SaveSystemMult ssm = FindFirstObjectByType<SaveSystemMult>();
            float karma = ssm.GetKarma();
            // good Ending
            if (karma >= 0)
            {
                cinematicDialogue.PlayDialogue();

                while (!cinematicDialogue.End)
                {
                    yield return null;
                }
            }
            // bad Ending
            else if (karma < 0)
            {
                cinematicDialogue2.PlayDialogue();

                while (!cinematicDialogue2.End)
                {
                    yield return null;
                }
            }
        }
        // normal introduction to the level
        else
        {
            audioConfig.MuteMusic();
            cinematicDialogue.PlayDialogue();

            while (!cinematicDialogue.End)
            {
                yield return null;
            }
        }

        if (nextScene)
        {
            //FadeIn the music
            SceneManager.LoadScene(scene);
            audioConfig.EnableMusic();
            audioConfig.ApplyFadeIn();
        }
            
    }
}
