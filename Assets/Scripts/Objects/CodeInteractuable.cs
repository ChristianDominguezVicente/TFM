using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CodeInteractuable : MonoBehaviour, IInteractuable
{
    [SerializeField] private string interactText;
    [SerializeField] private CodeUI codeUI;
    [SerializeField] private int[] code = new int[4];

    [SerializeField] private Transform drawerObject;
    [SerializeField] private float openDistance;
    [SerializeField] private float openDuration;
    [SerializeField] private GameObject masterKey;
    public int[] Code { get => code; set => code = value; }

    public string GetInteractText() => interactText;
    public Transform GetTransform() => transform.parent;

    public void Interact(Transform interactorTransform)
    {
        codeUI.Show(this);
    }

    public void OpenDrawer()
    {
        codeUI.Hide();
        StartCoroutine(MoveDrawer());
    }

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

        masterKey.SetActive(true);
        Destroy(this);
    }
}
