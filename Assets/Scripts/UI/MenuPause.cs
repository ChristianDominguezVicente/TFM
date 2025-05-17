using StarterAssets;
using UnityEngine;


public class MenuPause : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject menuPausa;
    [SerializeField] private ThirdPersonController playerController; // mi playerMenu
    [SerializeField] private DetectorUI gameManager;
    [SerializeField] private InputDetector inputDetector;

    [SerializeField] private GameObject menuHUD;

    private bool isPaused = false;
    private bool firstTimePaused = true;
    public bool IsPaused { get => isPaused; set => isPaused = value; }

    // Update is called once per frame
    void Update()
    {
        if (isPaused && firstTimePaused)
        {
            menuHUD.SetActive(false);
            PauseGame();
            inputDetector.enabled = false;
            gameManager.enabled = true;
            menuPausa.SetActive(true);
            firstTimePaused = false;
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        
    }


    public void ResumeGame()
    {
        menuHUD.SetActive(true);
        firstTimePaused = true;
        gameManager.enabled = false;
        inputDetector.enabled = true;
        isPaused = false;
        Time.timeScale = 1f;
    }
}
