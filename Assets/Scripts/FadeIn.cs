using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeIn;

    private void Start()
    {
        fadeIn.alpha = 1f;
        fadeIn.gameObject.SetActive(true);

        StartCoroutine(FadeInCoroutine());
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
