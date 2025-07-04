using StarterAssets;
using TMPro;
using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private TextMeshProUGUI interactText;

    [Header("Sound Control")]
    [SerializeField] private AudioClip showInteractionSound;
    [SerializeField] AudioConfig audioConfig;

    private bool isSounding = false;
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
            if(!isSounding)
            {
                audioConfig.SoundEffectSFX(showInteractionSound);
                isSounding = true;
            }
            
        }
        // if there is nothing interactuable nearby, the text is hidden
        else
        {
            Hide();
            if(isSounding)
            {
                isSounding = false;

            }
        }
    }
    private void Show(object interactable)
    {
        // if it is a IInteractuable object, its interaction text is displayed
        if (interactable is IInteractuable interactuable)
        {
            containerGameObject.SetActive(true);
            interactText.text = interactuable.GetInteractText();
        }
        // if it is a IPossessable object, its possess text is displayed
        else if (interactable is IPossessable possessable)
        {
            
            if (possessable is NPCPossessable npcP)
            {
                containerGameObject.SetActive(true);
                string text = possessable.GetPossessText();
                if (possessionManager.IsPossessing)
                {
                    int firstSpaceIndex = text.IndexOf(' ');
                    if (firstSpaceIndex != -1)
                    {
                        text = "Hablar" + text.Substring(firstSpaceIndex);
                    }
                }
                interactText.text = text;
            }
            else if (possessable is NPCNonPossessable npcNP)
            {
                if (possessionManager.CurrentNPC != null)
                {
                    containerGameObject.SetActive(true);
                    string text = possessable.GetPossessText();
                    if (possessionManager.IsPossessing)
                    {
                        int firstSpaceIndex = text.IndexOf(' ');
                        if (firstSpaceIndex != -1)
                        {
                            text = "Hablar" + text.Substring(firstSpaceIndex);
                        }
                    }
                    interactText.text = text;
                }
            }    
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
