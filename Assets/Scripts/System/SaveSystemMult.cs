using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystemMult : MonoBehaviour
{
    [Header("Configuración player")]
    [SerializeField] private GameObject player;
    public static int CurrentSlot { get; set; } = -1;
    private const string PlayerTag = "Player";
    private const string SceneName = "Greybox"; // Nombre de la escena del juego

    // Claves para PlayerPrefs
    private static string GetPositionXKey(int slot) => $"Slot{slot}_PosX";
    private static string GetPositionYKey(int slot) => $"Slot{slot}_PosY";
    private static string GetPositionZKey(int slot) => $"Slot{slot}_PosZ";
    private static string GetSceneKey(int slot) => $"Slot{slot}_Scene";
    private static string GetPlayTimeKey(int slot) => $"Slot{slot}_PlayTime";

    private float sessionStartTime;
    private float currentPlayTime;

    void Awake()
    {
        FindPlayer();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1.0f;
        if (CurrentSlot >= 0 && scene.name == SceneName)
        {
            FindPlayer();
            LoadPlayerPosition();
            Debug.Log($"Posición del jugador DESPUÉS de cargar: {player.transform.position}");
            LoadPlayTime(); // Cargar el tiempo de juego al cargar una partida
        }
        else if (scene.name == SceneName)
        {
            FindPlayer();
            StartNewPlayTime(); // Iniciar el contador de tiempo para una nueva partida
        }
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError($"No se encontró el jugador. Asegúrate de que tiene el tag '{PlayerTag}' en la escena '{SceneManager.GetActiveScene().name}'.");
            }
        }
    }

    private void StartNewPlayTime()
    {
        currentPlayTime = 0f;
        sessionStartTime = Time.time;
     //   Debug.Log("Nueva partida iniciada. Contador de tiempo reiniciado.");
    }

    void Update()
    {
        // Contar el tiempo solo si estamos en una partida (escena del juego cargada)
        if (SceneManager.GetActiveScene().name == SceneName)
        {
            currentPlayTime += Time.deltaTime;
        }
    }

    public void SaveGame(int slotIndex)
    {
        CurrentSlot = slotIndex;

        if (player != null)
        {
            PlayerPrefs.SetFloat(GetPositionXKey(slotIndex), player.transform.position.x);
            PlayerPrefs.SetFloat(GetPositionYKey(slotIndex), player.transform.position.y);
            PlayerPrefs.SetFloat(GetPositionZKey(slotIndex), player.transform.position.z);
            PlayerPrefs.SetString(GetSceneKey(slotIndex), SceneManager.GetActiveScene().name);
            PlayerPrefs.SetFloat(GetPlayTimeKey(slotIndex), currentPlayTime); // Guardar el tiempo actual de la partida
            PlayerPrefs.Save();
            Debug.Log($"Partida guardada en Slot {slotIndex} en la escena {SceneManager.GetActiveScene().name} con tiempo de juego: {FormatPlayTime(currentPlayTime)}");
        }
    }

    public void LoadGame(int slotIndex)
    {
        if (!HasSave(slotIndex)) return;

        CurrentSlot = slotIndex;
        SceneManager.LoadScene(SceneName);
    }

    void LoadPlayerPosition()
    {
        if (player != null)
        {
            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat(GetPositionXKey(CurrentSlot)),
                PlayerPrefs.GetFloat(GetPositionYKey(CurrentSlot)),
                PlayerPrefs.GetFloat(GetPositionZKey(CurrentSlot))
            );
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = position;
            player.GetComponent<CharacterController>().enabled = true;
            Debug.Log($"Jugador movido a: {position} desde Slot {CurrentSlot}");
        }
        else
        {
            Debug.LogError("No se puede cargar la posición porque el jugador no ha sido encontrado.");
        }
    }

    void LoadPlayTime()
    {
        currentPlayTime = PlayerPrefs.GetFloat(GetPlayTimeKey(CurrentSlot), 0f);
        sessionStartTime = Time.time - currentPlayTime; // Ajusta el inicio de la sesión para continuar contando
       // Debug.Log($"Tiempo de juego cargado para Slot {CurrentSlot}: {FormatPlayTime(currentPlayTime)}");
    }

    public bool HasSave(int slotIndex)
    {
        return PlayerPrefs.HasKey(GetPositionXKey(slotIndex));
    }

    public string GetSaveInfo(int slotIndex)
    {
        if (HasSave(slotIndex))
        {
            string sceneName = PlayerPrefs.GetString(GetSceneKey(slotIndex));
            float savedPlayTime = PlayerPrefs.GetFloat(GetPlayTimeKey(slotIndex), 0f);
            string formattedTime = FormatPlayTime(savedPlayTime);
            return $"Slot {slotIndex + 1}\n" +
                   $"Escena: {sceneName}\n" +
                   $"Tiempo Jugado: {formattedTime}";
        }
        return $"Slot {slotIndex + 1}\n(Vacío)";
    }

    private string FormatPlayTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{hours:00}:{minutes:00}:{secs:00}";
    }

}
