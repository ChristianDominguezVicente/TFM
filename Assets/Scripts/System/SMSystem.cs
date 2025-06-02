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

    public bool IsPaused { get => isPaused; set => isPaused = value; }



    private void Start()
    {
        //name level-puzle
        Scene currentScene = SceneManager.GetActiveScene();
        currentLevelName = currentScene.name;
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
        ActualizarHUD();
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
        // Debug.Log("indice es: " + indiceMision);
        LimpiezaEnunciado();
        missions[indiceMision].missionInfoPanel.SetActive(true);

    }

    private void LimpiezaEnunciado()
    {
        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i] != null)
            {
                missions[i].missionInfoPanel.SetActive(false);
            }
        }
    }

    private void SMObjetivos()
    {
        if (currentLevelName == "Greybox") // pz1 
        {
            int indiceMs = 0;
            if (missions[indiceMs].isCompleted) //pasar a la siguiente mision
            {
                indiceMs++;
                //next mission

                if (indiceMs == 1) //ms2
                {
                    if (objectManager.Flour)
                    {
                        ActualizarTexto(indiceMs, 0);
                    }
                    if (objectManager.Eggs)
                    {
                        ActualizarTexto(indiceMs, 1);
                    }
                    if (objectManager.Sugar)
                    {
                        ActualizarTexto(indiceMs, 2);
                    }
                    if (objectManager.Flour && objectManager.Eggs && objectManager.Sugar)
                    {
                        missions[indiceMs].isCompleted = true;
                    }

                }
                else //ms3
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

            }
            else //estoy en mision 1 del puzle 1
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
        }

        else if (currentLevelName == "Puzzle 2")
        {
            int indiceMs = 0;
            if (missions[indiceMs].isCompleted) //pasar a la siguiente mision
            {
                indiceMs++;

                if (indiceMs == 1) //ms2
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
                else //ms3
                {
                }

            }
            else
            {//estoy en mision 1 del puzle 2

                if (objectManager.Teddy)
                {
                    ActualizarTexto(indiceMs, 0);
                }

            }

        }
        // puzle 3 y 4 no tienen nada de sm esencial 
    }




    private void ActualizarTexto(int indicems, int indiceTextoMS)
    {
        TextMeshProUGUI textoHijo = missions[indicems].goalsToCompleteTEXT[indiceTextoMS].GetComponent<TextMeshProUGUI>();
        textoHijo.text = "<s>" + textoHijo.text + "</s>";
    }



    private void ActualizarHUD()
    {
        int indiceMs = 0;
        SMObjetivos();
        if (!missions[indiceMs].isCompleted)
        {
            ActualizarTitulo(indiceMs);
            ActualizarTextoUI(indiceMs);
        }
        else
        {
            indiceMs++;
            if (!missions[indiceMs].isCompleted)
            {
                ActualizarTitulo(indiceMs);
                ActualizarTextoUI(indiceMs);
            }
            else
            {
                indiceMs++;
                ActualizarTitulo(indiceMs);
                ActualizarTextoUI(indiceMs);
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
