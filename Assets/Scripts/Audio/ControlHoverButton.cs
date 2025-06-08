using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlHoverButton : MonoBehaviour, IPointerEnterHandler
{

    [Header("Sound control")]
    [SerializeField] private AudioConfig audioConfig;
    [SerializeField] private AudioClip moveOption;

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = audioConfig.AudioSourceSFX;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioConfig.SoundEffectSFX(moveOption);
    }
}
