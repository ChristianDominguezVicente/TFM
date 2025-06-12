using System;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SMSystem : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private ObjectManager objectManager; //object manager all puzzles 
    [SerializeField] private InputDetector inputDetector;
    [SerializeField] private DetectorUI gameManager;
    [SerializeField] private GameObject dad;

    [Header("UI")]
    [SerializeField] private GameObject menuMission;
    [SerializeField] private GameObject menuHUD;
    [SerializeField] private GameObject[] listHUD; //el maximo de objetivos en los niveles - 3 
    [SerializeField] private GameObject titleHUD;


    private string currentLevelName;

    [Serializable]
    private class Mission
    {
        public GameObject title;
        public GameObject missionInfoPanel;
        public GameObject missionAltPanel;
        public GameObject[] goalsToCompleteTEXT;
        public bool isCompleted;
    }


    [SerializeField] private Mission[] missions;


    private bool isPaused = false;
    private bool firstTimePaused = true;
    private bool needsUIUpdate = false;

    public bool IsPaused { get => isPaused; set => isPaused = value; }
    public bool NeedsUIUpdate { get => needsUIUpdate; set => needsUIUpdate = value; }

    private void Start()
    {
        //name level-puzle
        currentLevelName = SceneManager.GetActiveScene().name;
        ActualizarHUD();
    }

    private void Update()
    {
        if (isPaused && firstTimePaused)
        {
            menuHUD.SetActive(false);
            PauseGame();
            inputDetector.enabled = false;
            gameManager.enabled = true;
            menuMission.SetActive(true);
            firstTimePaused = false;
            SMObjetivos();

        }
        if (needsUIUpdate)
        {
            SMObjetivos();
            ActualizarHUD();
            needsUIUpdate = false;
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


    public void UpdateMissionMenuEnunciado(int indiceMision)
    {
        LimpiezaEnunciado();
        if (dad.GetComponent<ThirdPersonController>().enabled == true && currentLevelName == "Greybox" && indiceMision == 2) // si es el padre y estamos en ms3 en puzle 1
        {
            missions[indiceMision].missionInfoPanel.SetActive(false);
            missions[indiceMision].missionAltPanel.SetActive(true);
        }
        else
        {
            missions[indiceMision].missionInfoPanel.SetActive(true);
        }

        //  needsUIUpdate = true;

    }

    private void LimpiezaEnunciado()
    {
        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i] != null)
            {
                missions[i].missionInfoPanel.SetActive(false);
                if (missions[i].missionAltPanel != null)
                {
                    missions[i].missionAltPanel.SetActive(false);
                }
            }
        }
    }

    private void SMObjetivos()
    {
        if (currentLevelName == "Greybox") // pz1 
        {
            SMPuzleOne();
        }

        else if (currentLevelName == "Puzzle2")
        {
            SMPuzleTwo();

        }
        // puzle 3 y 4 no tienen nada de sm esencial 
    }

    private void SMPuzleTwo()
    {
        int indiceMsPuzTwo = 0;
        if (missions[indiceMsPuzTwo].isCompleted) //pasar a la siguiente mision
        {
            indiceMsPuzTwo++;

            if (!missions[indiceMsPuzTwo].isCompleted) //ms2
            {
                MSTwoPuzleTwo(indiceMsPuzTwo);

            }
            else //ms3
            {
            }

        }
        else
        {//estoy en mision 1 del puzle 2
            MSOnePuzleTwo(indiceMsPuzTwo);

        }
    }

    private void MSOnePuzleTwo(int indiceMs)
    {
        if (objectManager.Teddy)
        {
            ActualizarTexto(indiceMs, 0);
            missions[indiceMs].isCompleted = true;
        }
    }

    private void MSTwoPuzleTwo(int indiceMs)
    {
        if (objectManager.MasterKeyTaken)
        {
            ActualizarTexto(indiceMs, 0);
        }
        if (objectManager.Incorrect || objectManager.Correct)
        {
            ActualizarTexto(indiceMs, 1);
        }
        if (objectManager.ToolBox)
        {
            ActualizarTexto(indiceMs, 2);
        }
        if (objectManager.MasterKeyTaken && (objectManager.Incorrect || objectManager.Correct) && objectManager.ToolBox)
        {
            missions[indiceMs].isCompleted = true;
        }
    }

    private void SMPuzleOne()
    {
        int indiceMsPuzleOne = 0;
        if (missions[indiceMsPuzleOne].isCompleted) //pasar a la siguiente mision
        {
            indiceMsPuzleOne++;
            //next mission
            if (!missions[indiceMsPuzleOne].isCompleted) //ms2
            {
                MSTwoPuzleOne(indiceMsPuzleOne);

            }
            else //ms3
            {
                indiceMsPuzleOne++;
                MSThreePuzleOne(indiceMsPuzleOne);
            }

        }
        else //estoy en mision 1 del puzle 1
        {
            MSOneePuzleOne(indiceMsPuzleOne);
        }
    }

    private void MSThreePuzleOne(int indiceMs)
    {
        if (dad.GetComponent<ThirdPersonController>().enabled == true) // si es el padre
        {
            missions[indiceMs].missionInfoPanel.SetActive(false);
            missions[indiceMs].missionAltPanel.SetActive(true);
        }
        else
        {
            missions[indiceMs].missionInfoPanel.SetActive(true);
            // no hace falta comprobar si ha cogido las dos recetas pq hay una transicion acto seguido
        }
    }

    private void MSOneePuzleOne(int indiceMs)
    {
        if (objectManager.PrincipalDoor)
        {
            ActualizarTexto(indiceMs, 0);
            missions[indiceMs].isCompleted = true;
         
        }
    }

    private void MSTwoPuzleOne(int indiceMs)
    {
        //encuentra la forma del garaje - llave maestra
        if (objectManager.MasterKeyTaken)
        {
            ActualizarTexto(indiceMs, 0);
        }
        if (objectManager.MasterKeyTaken && objectManager.ValveActive)
        {
            {
                ActualizarTexto(indiceMs, 1);
                missions[indiceMs].isCompleted = true;
            }
        }
    }




    private void ActualizarTexto(int indicems, int indiceTextoMS)
    {
        TextMeshProUGUI textoHijo = missions[indicems].goalsToCompleteTEXT[indiceTextoMS].GetComponent<TextMeshProUGUI>();
        textoHijo.text = "<s>" + textoHijo.text + "</s>";
    }



    private void ActualizarHUD()
    {
        int indiceMsHUD = 0;
        if (!missions[indiceMsHUD].isCompleted) //mision1
        {
            if (currentLevelName == "Greybox") // pz1 
            {
                MSOneePuzleOne(indiceMsHUD);
            }else if( currentLevelName == "Puzzle2")
            {
                MSOnePuzleTwo(indiceMsHUD);
            }
            //all 1 mission act
            ActualizarTitulo(indiceMsHUD);
            ActualizarTextoUI(indiceMsHUD);
        }
        else
        {
            indiceMsHUD++;
            if (!missions[indiceMsHUD].isCompleted) //mision2
            {
                if (currentLevelName == "Greybox") // pz1 
                {
                    MSTwoPuzleOne(indiceMsHUD);
                }
                else if (currentLevelName == "Puzzle2")
                {
                    MSTwoPuzleTwo(indiceMsHUD);
                }
                ActualizarTitulo(indiceMsHUD);
                ActualizarTextoUI(indiceMsHUD);
            }
            else
            {
                indiceMsHUD++; //mision3
                if (currentLevelName == "Greybox") // pz1 
                {
                    MSThreePuzleOne(indiceMsHUD);
                }
                else if (currentLevelName == "Puzzle2")
                {
                }
                ActualizarTitulo(indiceMsHUD);
                ActualizarTextoUI(indiceMsHUD);
            }
        }
    }




    private void ActualizarTextoUI(int indiceMs)
    {
        for (int i = 0; i < missions[indiceMs].goalsToCompleteTEXT.Length; i++)
        {
            TextMeshProUGUI textoObjetivoMs = missions[indiceMs].goalsToCompleteTEXT[i].GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textoObjetivoUI = listHUD[i].GetComponent<TextMeshProUGUI>();
            textoObjetivoUI.text = textoObjetivoMs.text;
            //   Debug.Log(" el texto ahora es: "+ textoObjetivoUI.text);
        }
        if (missions[indiceMs].goalsToCompleteTEXT.Length == 1)
        {
            LimpiezaUI();
        }
    }

    private void LimpiezaUI()
    {
        TextMeshProUGUI textoObjetivoUI1 = listHUD[1].GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI textoObjetivoUI2 = listHUD[2].GetComponent<TextMeshProUGUI>();
        textoObjetivoUI1.text = "";
        textoObjetivoUI2.text = "";
    }

    private void ActualizarTitulo(int indiceMs)
    {
        TextMeshProUGUI titleUI = titleHUD.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI missionTitleUI = missions[indiceMs].title.GetComponent<TextMeshProUGUI>();
        titleUI.text = missionTitleUI.text;
    }






}
