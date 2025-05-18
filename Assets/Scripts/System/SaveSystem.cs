using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    // Claves para PlayerPrefs
    private const string POS_X_KEY = "PlayerPosX";
    private const string POS_Y_KEY = "PlayerPosY";
    private const string POS_Z_KEY = "PlayerPosZ";
    private const string SCENE_KEY = "PlayerScene";
    private const string GAME_MODE_KEY = "GameMode";
    private string levelOne = "Greybox";

    public static bool IsNewGame { get; private set; }
    void Start()
    {
        // Determinar el modo de juego (SOLO si estamos en Greybox)
        if (SceneManager.GetActiveScene().name == levelOne)
        {
            IsNewGame = !PlayerPrefs.HasKey(GAME_MODE_KEY) || PlayerPrefs.GetInt(GAME_MODE_KEY) == 0;

            if (!IsNewGame) // Modo "Cargar Partida"
            {
                LoadPosition();
            }
            else // Modo "Nueva Partida"
            {
                ResetPosition();
            }

            // Marcar que ahora hay una partida en curso
            PlayerPrefs.SetInt(GAME_MODE_KEY, 1);
            PlayerPrefs.Save();
        }
    }

    void OnDestroy()
    {
        if (SceneManager.GetActiveScene().name == levelOne)
        {
            SavePosition();
        }
    }

    private void SavePosition()
    {
        PlayerPrefs.SetFloat(POS_X_KEY, transform.position.x);
        PlayerPrefs.SetFloat(POS_Y_KEY, transform.position.y);
        PlayerPrefs.SetFloat(POS_Z_KEY, transform.position.z);
        PlayerPrefs.SetString(SCENE_KEY, "Greybox");
        PlayerPrefs.Save();
    }

    private void LoadPosition()
    {
        transform.position = new Vector3(
            PlayerPrefs.GetFloat(POS_X_KEY, transform.position.x),
            PlayerPrefs.GetFloat(POS_Y_KEY, transform.position.y),
            PlayerPrefs.GetFloat(POS_Z_KEY, transform.position.z)
        );
    }

    private void ResetPosition()
    {
        // Posición inicial por defecto 
        transform.position = new Vector3(-9.08f, 1, -2.66f);

        // Opcional: Borrar datos antiguos
        PlayerPrefs.DeleteKey(POS_X_KEY);
        PlayerPrefs.DeleteKey(POS_Y_KEY);
        PlayerPrefs.DeleteKey(POS_Z_KEY);
    }

    public static void SetNewGameMode(bool isNewGame)
    {
        PlayerPrefs.SetInt(GAME_MODE_KEY, isNewGame ? 0 : 1);
        PlayerPrefs.Save();
    }
}
