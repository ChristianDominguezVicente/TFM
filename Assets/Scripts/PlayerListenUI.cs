using StarterAssets;
using TMPro;
using UnityEngine;

public class PlayerListenUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private TextMeshProUGUI listenText;
    private void Update()
    {
        // get the current controller (can be the player or a possessed NPC)
        var player = possessionManager.CurrentController;
        if (player == null) return;

        // get the closest interactuable object
        var target = player.GetInteractuables();
        // if there is an interactuable object, the UI is displayed with the corresponding text
        if (target != null && possessionManager.CurrentNPC == null)
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
        if (interactable is IPossessable possessable)
        {
            containerGameObject.SetActive(true);
            string text = possessable.GetPossessText();
            int firstSpaceIndex = text.IndexOf(' ');
            if (firstSpaceIndex != -1)
            {
                text = "Listen" + text.Substring(firstSpaceIndex);
            }
            listenText.text = text;
        }
        else
        {
            Hide();
        }
    }

    private void Hide()
    {
        containerGameObject.SetActive(false);
    }
}
