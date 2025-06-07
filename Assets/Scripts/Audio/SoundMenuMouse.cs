using UnityEngine;
using UnityEngine.Audio;

public class SoundMenuMouse : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Sound control")]
    [SerializeField] private AudioClip moveOption;
    [SerializeField] private AudioClip choseOption;
    [SerializeField] private AudioClip beginPlay;
    [SerializeField] private AudioConfig audioConfig;

    public void SoundSelectOption()
    {
        audioConfig.SoundEffectSFX(moveOption);
    }

    public void SoundChoseOption()
    {
        audioConfig.SoundEffectSFX(choseOption);

    }

    public void SoundBeginPlayOption()
    {
        audioConfig.SoundEffectSFX(beginPlay);

    }
}
