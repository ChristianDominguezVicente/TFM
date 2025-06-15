using StarterAssets;
using System;
using System.Collections;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystemMult : MonoBehaviour
{
    [Header("Configuración player y posesion")]
    [SerializeField] private GameObject player;
    [SerializeField] private PossessionManager possessionManager;
    [SerializeField] private CanvasGroup fadeOut;
    [SerializeField] private ObjectManager objectManager;
    [SerializeField] private SceneBeginning sceneBeginning;
    [SerializeField] private DialogueHistory dialogueHistory;

    [Header("Auto Save config")]
    [SerializeField] private AutoSaveHUD autoSaveHUD;
    private float autoSaveIntervalMinutes = 3f; // 3 MINUTES save
    private const int AutoSaveSlotIndex = 4;
    private float autoSaveTimer;
    private bool isAutoSaveActive = false;


    // config 
    public static int CurrentSlot { get; set; } = -1;
    private const string InitialScenePuzzleOne = "Greybox"; //level one
    private const string InitialMENU = "InicioMenu";
    // save pos player, scene and play time 
    private static string GetPositionXKey(int slot) => $"Slot{slot}_PosX";
    private static string GetPositionYKey(int slot) => $"Slot{slot}_PosY";
    private static string GetPositionZKey(int slot) => $"Slot{slot}_PosZ";
    private static string GetSceneKey(int slot) => $"Slot{slot}_Scene";
    private static string GetKarmaKey(int slot) => $"Slot{slot}_Karma";
    private static string GetPlayTimeKey(int slot) => $"Slot{slot}_PlayTime";
    private static string GetHistoryKey(int slot) => $"Slot{slot}_History";

    // save objectManager
    private static string GetPrincipalDoorKey(int slot) => $"Slot{slot}_PrincipalDoor";
    private static string GetCalendarKey(int slot) => $"Slot{slot}_Calendar";
    private static string GetMasterKeyKey(int slot) => $"Slot{slot}_MasterKey";
    private static string GetValveKey(int slot) => $"Slot{slot}_Valve";
    private static string GetSugarKey(int slot) => $"Slot{slot}_Sugar";
    private static string GetFlourKey(int slot) => $"Slot{slot}_Flour";
    private static string GetEggsKey(int slot) => $"Slot{slot}_Eggs";
    private static string GetRecipe1Key(int slot) => $"Slot{slot}_Recipe1";
    private static string GetRecipe2Key(int slot) => $"Slot{slot}_Recipe2";
    private static string GetTeddyKey(int slot) => $"Slot{slot}_Teddy";
    private static string GetToolBoxKey(int slot) => $"Slot{slot}_ToolBox";
    private static string GetGiftPaperKey(int slot) => $"Slot{slot}_GiftPaper";
    private static string GetIconsKey(int slot) => $"Slot{slot}_Icons";
    private static string GetPosterKey(int slot) => $"Slot{slot}_Poster";
    private static string GetTabletKey(int slot) => $"Slot{slot}_Tablet";
    private static string GetPhotoKey(int slot) => $"Slot{slot}_Photo";
    private static string GetNoteKey(int slot) => $"Slot{slot}_Note";
    private static string GetCorrectKey(int slot) => $"Slot{slot}_Correct";
    private static string GetIncorrectKey(int slot) => $"Slot{slot}_Incorrect";
    private static string GetCurrentObjectKey(int slot) => $"Slot{slot}_CurrentObject";

    // save possesion bar
    private static string GetPossessionCurrentTimeKey(int slot) => $"Slot{slot}_PossessionCurrentTime";
    private static string GetPossessionMaxTimeKey(int slot) => $"Slot{slot}_PossessionMaxTime";


    private float sessionStartTime;
    private float currentPlayTime;
    private float karma;
    private string history;

    private AudioConfig audioConfig;

    public void Start()
    {
        audioConfig = (AudioConfig)FindAnyObjectByType(typeof(AudioConfig));
    }


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
        if (CurrentSlot >= 0)
        {
         //   Debug.Log("PARTIDA CARGAAAAAAAAAAA");
            if (sceneBeginning != null)
                sceneBeginning.gameObject.SetActive(false);

            if (EnterNewScene()) //pasar a otro puzle pero sin guardar 
            {
                // Debug.Log(" no se ha cargado nada de la partida pero se ha pasado");
            }
            else
            {
             //   Debug.Log(" SE HA cargado nada de la partida pero se ha pasado");
                StartAutoSaveTimer();
                FindPlayer();
                LoadPlayerPosition();
                LoadPlayTime(); // load time after load game
                LoadPossessionBar();
                LoadKarma();
                LoadObjects();
                LoadHistory();
            }
        }
        else if (scene.name == InitialScenePuzzleOne)
        {
        //    Debug.Log("PARTIDA NUEVA");
            FindPlayer();
            StartNewPlayTime(); // init count new game
            StartAutoSaveTimer();
        }
        else // other desactivate autosave
        {
            isAutoSaveActive = false;
        }
    }

    private bool EnterNewScene()
    {
        if (player != null)
        {
            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat(GetPositionXKey(CurrentSlot)),
                PlayerPrefs.GetFloat(GetPositionYKey(CurrentSlot)),
                PlayerPrefs.GetFloat(GetPositionZKey(CurrentSlot))
            );
           if( player.transform.position == position)
            {
                return true;
            }
            else {  return false; }
        }
        else
        {
            return false;
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
        if (SceneManager.GetActiveScene().name == InitialScenePuzzleOne || SceneManager.GetActiveScene().name == "Puzzle2" || SceneManager.GetActiveScene().name == "Puzzle3" || SceneManager.GetActiveScene().name == "Puzzle4")
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
            PlayerPrefs.SetFloat(GetKarmaKey(slotIndex), karma);
            PlayerPrefs.SetFloat(GetPlayTimeKey(slotIndex), currentPlayTime); // Save current game time
            PlayerPrefs.SetString(GetHistoryKey(slotIndex), history);
            if (objectManager != null)
                ObjectSave(slotIndex);

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

    private void ObjectSave(int slotIndex)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != InitialMENU)
        {
            // 0 = true; 1= false
            PlayerPrefs.SetInt(GetPrincipalDoorKey(slotIndex), objectManager.PrincipalDoor ? 0 : 1);
            PlayerPrefs.SetInt(GetCalendarKey(slotIndex), objectManager.Calendar ? 0 : 1);
            PlayerPrefs.SetInt(GetMasterKeyKey(slotIndex), objectManager.MasterKeyTaken ? 0 : 1);
            PlayerPrefs.SetInt(GetValveKey(slotIndex), objectManager.ValveActive ? 0 : 1);
            PlayerPrefs.SetInt(GetSugarKey(slotIndex), objectManager.Sugar ? 0 : 1);
            PlayerPrefs.SetInt(GetFlourKey(slotIndex), objectManager.Flour ? 0 : 1);
            PlayerPrefs.SetInt(GetEggsKey(slotIndex), objectManager.Eggs ? 0 : 1);
            PlayerPrefs.SetInt(GetRecipe1Key(slotIndex), objectManager.Recipe1 ? 0 : 1);
            PlayerPrefs.SetInt(GetRecipe2Key(slotIndex), objectManager.Recipe2 ? 0 : 1);
            PlayerPrefs.SetInt(GetTeddyKey(slotIndex), objectManager.Teddy ? 0 : 1);
            PlayerPrefs.SetInt(GetToolBoxKey(slotIndex), objectManager.ToolBox ? 0 : 1);
            PlayerPrefs.SetInt(GetGiftPaperKey(slotIndex), objectManager.GiftPaper ? 0 : 1);
            PlayerPrefs.SetInt(GetIconsKey(slotIndex), objectManager.Icons ? 0 : 1);
            PlayerPrefs.SetInt(GetPosterKey(slotIndex), objectManager.Poster ? 0 : 1);
            PlayerPrefs.SetInt(GetTabletKey(slotIndex), objectManager.Tablet ? 0 : 1);
            PlayerPrefs.SetInt(GetPhotoKey(slotIndex), objectManager.Photo ? 0 : 1);
            PlayerPrefs.SetInt(GetNoteKey(slotIndex), objectManager.Note ? 0 : 1);
            PlayerPrefs.SetInt(GetCorrectKey(slotIndex), objectManager.Correct ? 0 : 1);
            PlayerPrefs.SetInt(GetIncorrectKey(slotIndex), objectManager.Incorrect ? 0 : 1);
            PlayerPrefs.SetString(GetCurrentObjectKey(slotIndex), objectManager.CurrentObject == null ? "" : objectManager.CurrentObject.name);
        }
    }

    public void LoadGame(int slotIndex)
    {
        if (!HasSave(slotIndex)) return;

        CurrentSlot = slotIndex;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        fadeOut.gameObject.SetActive(true);

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeOut.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        //FadeOut the music
        audioConfig.ApplyFadeOut();

        // load next level
        SceneManager.LoadScene(PlayerPrefs.GetString(GetSceneKey(CurrentSlot)));
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
            if (player.GetComponent<CharacterController>() != null)
            {
                player.GetComponent<CharacterController>().enabled = false;
                player.transform.position = position;
                player.GetComponent<CharacterController>().enabled = true;
            }
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
    void LoadKarma()
    {
        karma = PlayerPrefs.GetFloat(GetKarmaKey(CurrentSlot), 0f);
    }
    void LoadObjects()
    {
        Scene scene = SceneManager.GetActiveScene(); 
        if (scene.name != InitialMENU && scene.name != "Transicion12" && scene.name != "Transicion23" && scene.name != "Transicion4" && scene.name != "Final")
        {
            // 0 = true; 1= false
            objectManager.PrincipalDoor = PlayerPrefs.GetInt(GetPrincipalDoorKey(CurrentSlot), 0) == 0;
            objectManager.Calendar = PlayerPrefs.GetInt(GetCalendarKey(CurrentSlot), 0) == 0;
            objectManager.MasterKeyTaken = PlayerPrefs.GetInt(GetMasterKeyKey(CurrentSlot), 0) == 0;
            objectManager.ValveActive = PlayerPrefs.GetInt(GetValveKey(CurrentSlot), 0) == 0;
            objectManager.Sugar = PlayerPrefs.GetInt(GetSugarKey(CurrentSlot), 0) == 0;
            objectManager.Flour = PlayerPrefs.GetInt(GetFlourKey(CurrentSlot), 0) == 0;
            objectManager.Eggs = PlayerPrefs.GetInt(GetEggsKey(CurrentSlot), 0) == 0;
            objectManager.Recipe1 = PlayerPrefs.GetInt(GetRecipe1Key(CurrentSlot), 0) == 0;
            objectManager.Recipe2 = PlayerPrefs.GetInt(GetRecipe2Key(CurrentSlot), 0) == 0;
            objectManager.Teddy = PlayerPrefs.GetInt(GetTeddyKey(CurrentSlot), 0) == 0;
            objectManager.ToolBox = PlayerPrefs.GetInt(GetToolBoxKey(CurrentSlot), 0) == 0;
            objectManager.GiftPaper = PlayerPrefs.GetInt(GetGiftPaperKey(CurrentSlot), 0) == 0;
            objectManager.Icons = PlayerPrefs.GetInt(GetIconsKey(CurrentSlot), 0) == 0;
            objectManager.Poster = PlayerPrefs.GetInt(GetPosterKey(CurrentSlot), 0) == 0;
            objectManager.Tablet = PlayerPrefs.GetInt(GetTabletKey(CurrentSlot), 0) == 0;
            objectManager.Photo = PlayerPrefs.GetInt(GetPhotoKey(CurrentSlot), 0) == 0;
            objectManager.Note = PlayerPrefs.GetInt(GetNoteKey(CurrentSlot), 0) == 0;
            objectManager.Correct = PlayerPrefs.GetInt(GetCorrectKey(CurrentSlot), 0) == 0;
            objectManager.Incorrect = PlayerPrefs.GetInt(GetIncorrectKey(CurrentSlot), 0) == 0;
            objectManager.CurrentObject = GameObject.Find(PlayerPrefs.GetString(GetCurrentObjectKey(CurrentSlot), ""));

            objectManager.OnLoad();
        }
    }

    void LoadHistory()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != InitialMENU)
        {
            dialogueHistory.OnLoad(PlayerPrefs.GetString(GetHistoryKey(CurrentSlot), ""));
        }
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
            string aux;
            float savedPlayTime = PlayerPrefs.GetFloat(GetPlayTimeKey(slotIndex), 0f);
            string formattedTime = FormatPlayTime(savedPlayTime);
            string slotLabel = $"Slot {slotIndex + 1}"; // Default label

            if (slotIndex == AutoSaveSlotIndex)
            {
                slotLabel = "Guardado Automático";
            }
            aux = sceneName;
            if(sceneName == "Greybox")
            {
                aux = "Puzzle1";
            }
            return 
                   $"{aux}" +
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

    public void SetKarma(float value)
    {
        karma = value;
    }

    public void SetHistory(string value)
    {
        if (string.IsNullOrEmpty(history))
        {
            history = value;
        }
        else
        {
            history += "\n" + value;
        }
    }
}
