using StarterAssets;
using TMPro;
using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private ThirdPersonController player;
    [SerializeField] private TextMeshProUGUI interactText;
    private void Update()
    {
        if (player.GetInteractuable() != null)
        {
            Show(player.GetInteractuable());
        }
        else
        {
            Hide();
        }
    }
    private void Show(IInteractuable interactuable)
    {
        containerGameObject.SetActive(true);
        interactText.text = interactuable.GetInteractText();
    }

    private void Hide()
    {
        containerGameObject.SetActive(false);
    }
}
