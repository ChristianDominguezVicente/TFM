using StarterAssets;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystemMult : MonoBehaviour
{
    [Header("Configuración player y posesion")]
    [SerializeField] private GameObject player;
    [SerializeField] private PossessionManager possessionManager;


    [Header("Auto Save config")]
    [SerializeField] private AutoSaveHUD autoSaveHUD;
    private float autoSaveIntervalMinutes = 10f; // 10 MINUTES save
    private const int AutoSaveSlotIndex = 4;
    private float autoSaveTimer;
    private bool isAutoSaveActive = false;


    // config 
    public static int CurrentSlot { get; set; } = -1;
    private const string SceneName = "Greybox"; //level one

    // save pos player, scene and play time 
    private static string GetPositionXKey(int slot) => $"Slot{slot}_PosX";
    private static string GetPositionYKey(int slot) => $"Slot{slot}_PosY";
    private static string GetPositionZKey(int slot) => $"Slot{slot}_PosZ";
    private static string GetSceneKey(int slot) => $"Slot{slot}_Scene";
    private static string GetPlayTimeKey(int slot) => $"Slot{slot}_PlayTime";

    // save possesion bar
    private static string GetPossessionCurrentTimeKey(int slot) => $"Slot{slot}_PossessionCurrentTime";
    private static string GetPossessionMaxTimeKey(int slot) => $"Slot{slot}_PossessionMaxTime";


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
        Time.timeScale = 1.0f; // menu pause was active
        if (CurrentSlot >= 0 && scene.name == SceneName)
        {
            StartAutoSaveTimer();
            FindPlayer();
            LoadPlayerPosition();
            LoadPlayTime(); // load time after load game
            LoadPossessionBar();
        }
        else if (scene.name == SceneName)
        {
            FindPlayer();
            StartNewPlayTime(); // init count new game
            StartAutoSaveTimer();
        }
        else // other desactivate autosave
        {
            isAutoSaveActive = false;
        }
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError($"No se encontró el jugador.");
            }
        }
    }

    private void StartAutoSaveTimer()
    {
        autoSaveTimer = autoSaveIntervalMinutes * 60f; // convert min to sec
        isAutoSaveActive = true;
     //   Debug.Log($"Temporizador de guardado automático iniciado. Próximo guardado en {autoSaveIntervalMinutes} minutos.");
    }

    private void StartNewPlayTime()
    {
        currentPlayTime = 0f;
        sessionStartTime = Time.time;
        //   Debug.Log("Nueva partida iniciada. Contador de tiempo reiniciado.");
    }

    void Update()
    {
        // Counting time only if we are in a game(game scene loaded)
        if (SceneManager.GetActiveScene().name == SceneName)
        {
            currentPlayTime += Time.deltaTime;

            // autosave
            if (isAutoSaveActive)
            {
                autoSaveTimer -= Time.deltaTime;
                if (autoSaveTimer <= 0)
                {
                    PerformAutoSave();
                    autoSaveTimer = autoSaveIntervalMinutes * 60f; // retry counter
                }
            }
           
        }
        else // If we are not in the game scene, there is no autosave.
        {
            isAutoSaveActive = false;
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
            PlayerPrefs.SetFloat(GetPlayTimeKey(slotIndex), currentPlayTime); // Save current game time

            if (possessionManager != null) // bar posses
            {
                PlayerPrefs.SetFloat(GetPossessionCurrentTimeKey(slotIndex), possessionManager.GetCurrentTime());
                PlayerPrefs.SetFloat(GetPossessionMaxTimeKey(slotIndex), possessionManager.GetMaxTime());
               // Debug.Log($"Barra de posesión guardada: CurrentTime={possessionManager.GetCurrentTime()}, MaxTime={possessionManager.GetMaxTime()}");
            }
            else
            {
            //    Debug.LogWarning("PossessionManager no está asignado en SaveSystemMult. No se pudo guardar la barra de posesión.");
            }



            PlayerPrefs.Save();
         //   Debug.Log($"Partida guardada en Slot {slotIndex} en la escena {SceneManager.GetActiveScene().name} con tiempo de juego: {FormatPlayTime(currentPlayTime)}");
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
       //     Debug.Log($"Jugador movido a: {position} desde Slot {CurrentSlot}");
        }
        else
        {
       //     Debug.LogError("No se puede cargar la posición porque el jugador no ha sido encontrado.");
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
            string slotLabel = $"Slot {slotIndex + 1}"; // Default label

            if (slotIndex == AutoSaveSlotIndex)
            {
                slotLabel = "Guardado Automático";
            }

            return 
                   $"Capítulo 1" +
                   $" - Tiempo Jugado: {formattedTime}";
        }
        if (slotIndex == AutoSaveSlotIndex) //autosave Show “Empty” for automatic slot if there is no saving
        {
            return "Guardado Automático - (Vacío)";
        }
        return $"Slot {slotIndex + 1} - (Vacío)";
    }

    private string FormatPlayTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{hours:00}:{minutes:00}:{secs:00}";
    }

    void LoadPossessionBar()
    {
        if (possessionManager != null)
        {
            float savedCurrentTime = PlayerPrefs.GetFloat(GetPossessionCurrentTimeKey(CurrentSlot), possessionManager.GetDefaultMaxPossessionTime()); // 0f if it always starts empty in a new game
            float savedMaxTime = PlayerPrefs.GetFloat(GetPossessionMaxTimeKey(CurrentSlot), possessionManager.GetDefaultMaxPossessionTime());

            possessionManager.SetPossessionTimes(savedCurrentTime, savedMaxTime);
    //        Debug.Log($"Barra de posesión cargada: CurrentTime={savedCurrentTime}, MaxTime={savedMaxTime} para Slot {CurrentSlot}");
        }
        else
        {
      //      Debug.LogWarning("PossessionManager no está asignado en SaveSystemMult. No se pudo cargar la barra de posesión.");
        }
    }


    public void PerformAutoSave()
    {
          Debug.Log("Realizando guardado automático...");
        // Calls your existing save function, but uses the special slot
        SaveGame(AutoSaveSlotIndex);

        // If you have the AutoSaveHUD script assigned, show it the icon
        if (autoSaveHUD != null)
        {
            autoSaveHUD.ShowAutoSaveIcon();
        }
        else
        {
      //      Debug.LogWarning("AutoSaveHUD no está asignado en SaveSystemMult. No se mostrará el icono de guardado automático.");
        }
        SlotUpdateLoad slotUpdateLoad = FindFirstObjectByType<SlotUpdateLoad>();
        slotUpdateLoad.UpdateText(AutoSaveSlotIndex, this.GetSaveInfo(AutoSaveSlotIndex));
    }

}
