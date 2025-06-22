using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeIn;
    private AudioConfig audioConfig;

    private void Start()
    {
        // configuration of the fade
        fadeIn.alpha = 1f;
        fadeIn.gameObject.SetActive(true);
        // configuration of the audio
        audioConfig = (AudioConfig)FindAnyObjectByType(typeof(AudioConfig));
        audioConfig.EnableMusic();

        StartCoroutine(FadeInCoroutine());
        //FadeIn the music
        audioConfig.ApplyFadeIn();
    }

    private IEnumerator FadeInCoroutine()
    {
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeIn.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }

        fadeIn.gameObject.SetActive(false);
    }
}
