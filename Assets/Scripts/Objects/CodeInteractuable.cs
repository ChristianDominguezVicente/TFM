using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CodeInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private CodeUI codeUI;
    [SerializeField] private int[] code = new int[4];

    [Header("Desk")]
    [SerializeField] private Transform drawerObject;
    [SerializeField] private float openDistance;
    [SerializeField] private float openDuration;
    [SerializeField] private GameObject masterKey;

    [Header("Diary")]
    [SerializeField] private string nextScene;
    [SerializeField] private CanvasGroup fadeOut;
    public int[] Code { get => code; set => code = value; }

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform.parent;
    
    [Header("BoxLvl2")]
    [SerializeField] private PossessionManager possessionManager;

    public void Interact(Transform interactorTransform)
    {
        
        //If in the puzzle 2 Lia interact with the box that contains the key master, it unlock automatically
        if(CompareTag("Box") && possessionManager.CurrentNPC.NpcName=="Lia")
        {
            UnlockBox();
        }
        // show UI
        else
        {
            codeUI.Show(this);
        }
        
    }

    // when the correct code is entered
    public void OnCorrectCode()
    {
        codeUI.Hide();

        if (CompareTag("Desk"))
        {
            StartCoroutine(MoveDrawer());
        }
        else if (CompareTag("Diary"))
        {
            StartCoroutine(FadeOut());
        }
        else if(CompareTag("Box"))
        {
            UnlockBox();
        }
            
    }


    // drawer opening animation
    private IEnumerator MoveDrawer()
    {
        Vector3 startPos = drawerObject.localPosition;
        Vector3 targetPos = startPos + (-drawerObject.right * openDistance);
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            drawerObject.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / openDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        drawerObject.localPosition = targetPos;

        // activate master key
        masterKey.SetActive(true);
        // destroy script
        Destroy(this);
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

        // load next level
        SceneManager.LoadScene(nextScene);
    }

    private void UnlockBox()
    {
        Destroy(this);
        masterKey.SetActive(true);
    }

}
