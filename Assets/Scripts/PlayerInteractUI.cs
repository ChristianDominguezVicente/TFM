using StarterAssets;
using TMPro;
using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private TextMeshProUGUI interactText;
    private void Update()
    {
        var player = possessionManager.CurrentController;
        if (player == null) return;

        var target = player.GetInteractuables();
        if (target != null)
        {
            Show(target);
        }
        else
        {
            Hide();
        }
    }
    private void Show(object interactable)
    {
        containerGameObject.SetActive(true);

        if (interactable is IInteractuable interactuable)
        {
            interactText.text = interactuable.GetInteractText();
        }
        else if (interactable is IPossessable possessable)
        {
            interactText.text = possessable.GetPossessText();
        }
        else
        {
            interactText.text = "";
        }
    }

    private void Hide()
    {
        containerGameObject.SetActive(false);
    }
}
