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
        // get the current controller (can be the player or a possessed NPC)
        var player = possessionManager.CurrentController;
        if (player == null) return;

        // get the closest interactuable object
        var target = player.GetInteractuables();
        // if there is an interactuable object, the UI is displayed with the corresponding text
        if (target != null)
        {
            Show(target);
        }
        // if there is nothing interactuable nearby, the text is hidden
        else
        {
            Hide();
        }
    }
    private void Show(object interactable)
    {
        containerGameObject.SetActive(true);

        // if it is a IInteractuable object, its interaction text is displayed
        if (interactable is IInteractuable interactuable)
        {
            interactText.text = interactuable.GetInteractText();
        }
        // if it is a IPossessable object, its possess text is displayed
        else if (interactable is IPossessable possessable)
        {
            interactText.text = possessable.GetPossessText();
        }
        // otherwise, the text is cleared
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
