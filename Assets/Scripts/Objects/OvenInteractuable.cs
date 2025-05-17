using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OvenInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private string nextScene;
    [SerializeField] private CanvasGroup fadeOut;

    private string originalText;
    private bool showingWarning = false;

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform;

    private void Start()
    {
        originalText = interactText;
    }

    public void Interact(Transform interactorTransform)
    {
        if (showingWarning) return;

        if (!objectManager.ValveActive)
        {
            StartCoroutine(ShowWarning("<color=red>You must turn the valve first</color>"));
        }
        else if (!(objectManager.Recipe1 && objectManager.Recipe2))
        {
            StartCoroutine(ShowWarning("<color=red>You need both recipes</color>"));
        }
        else if (!(objectManager.Flour && objectManager.Eggs && objectManager.Sugar))
        {
            StartCoroutine(ShowWarning("<color=red>Missing ingredients</color>"));
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator ShowWarning(string warningText)
    {
        showingWarning = true;
        interactText = warningText;
        yield return new WaitForSeconds(2f);
        interactText = originalText;
        showingWarning = false;
    }

    private IEnumerator FadeOut()
    {
        fadeOut.gameObject.SetActive(true);

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeOut.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }
}
