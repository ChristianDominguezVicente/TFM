using System;
using StarterAssets;
using UnityEngine;
using TMPro;

using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class PlayerMenu : MonoBehaviour
{
    // [SerializeField] private MenuInicial botonesUI;
    private PlayerInput _playerInput;
    private StarterAssetsInputs _input;


    private Vector2 scrollInput;
    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Effects SFX")]
    [SerializeField] private AudioClip startGame;
    [SerializeField] private AudioClip selectOptionMenuSound;
    [SerializeField] private AudioClip chosedOptionMenuSound;
    [SerializeField] private AudioClip beginPlaySound;
    [SerializeField] AudioConfig audioConfig; 

    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if (MenuInicial.menuActivo != null)
        {
            UI_Move();
            UI_Interact();
            UI_CancelPauseMenu();
        }
    }

    private void UI_CancelPauseMenu()
    {
        if (_input.cancel)
        {
            
            MenuInicial.menuActivo.VolverAMenuAnterior();
            audioConfig.SoundEffectSFX(chosedOptionMenuSound);
            _input.cancel = false;
            return;

        }
    }

    private void UI_Move()
    {
        if (MenuInicial.menuActivo == null) return;

        Vector2 input = _input.ui_move;
        float deadzone = 0.7f;

        if (Time.time - lastInputTime < inputCooldown) return;

        // Modo ajuste de toggle
        if (MenuInicial.menuActivo.IsAdjustingToggle() && Mathf.Abs(input.x) > deadzone)
        {
            int direction = input.x > 0 ? 1 : -1;
            MenuInicial.menuActivo.CambiarOpcionToggle(direction);
            lastInputTime = Time.time;
            audioConfig.SoundEffectSFX(selectOptionMenuSound);
        }
        // Modo ajuste de slider (mantén tu código existente)
        else if (MenuInicial.menuActivo.IsAdjustingSlider() && Mathf.Abs(input.x) > deadzone)
        {
            int direction = input.x > 0 ? 1 : -1;
            MenuInicial.menuActivo.MoveSelection(direction);
            lastInputTime = Time.time;
            audioConfig.SoundEffectSFX(selectOptionMenuSound);
        }
        // Navegación normal (vertical)
        else if (Mathf.Abs(input.y) > deadzone)
        {
            int direction = input.y > 0 ? -1 : 1;
            MenuInicial.menuActivo.MoveSelection(direction);
            lastInputTime = Time.time;
            audioConfig.SoundEffectSFX(selectOptionMenuSound);
        }
    }

    private void UI_Interact()
    {
        if (_input.interact)
        {
            if (MenuInicial.menuActivo.IsAdjustingToggle())
            {
                // Confirmar selección y salir del modo ajuste
                MenuInicial.menuActivo.ToggleModoAjuste();
                audioConfig.SoundEffectSFX(chosedOptionMenuSound);
            }
            else if (MenuInicial.menuActivo.IsAdjustingSlider())
            {
                MenuInicial.menuActivo.ToggleAjusteSlider();
                audioConfig.SoundEffectSFX(chosedOptionMenuSound);

            }
            else if (MenuInicial.menuActivo.CurrentButtonIsToggle())
            {
                // Entrar en modo ajuste
                MenuInicial.menuActivo.ToggleModoAjuste();
                audioConfig.SoundEffectSFX(chosedOptionMenuSound);
            }
            else if (MenuInicial.menuActivo.CurrentButtonHasAdjacentSlider())
            {
                MenuInicial.menuActivo.ToggleAjusteSlider();
                audioConfig.SoundEffectSFX(chosedOptionMenuSound);
            }
            else
            {
                // Acción normal del botón
                MenuInicial.menuActivo.ActivateSelectedButton();
                if(MenuInicial.menuActivo.GetCurrentButtonConfig().esNuevaPartida || MenuInicial.menuActivo.GetCurrentButtonConfig().esSlotCargandoPartida)
                {
                    audioConfig.SoundEffectSFX(beginPlaySound);
                }
                else
                {
                    audioConfig.SoundEffectSFX(chosedOptionMenuSound);
                }
            }
            _input.interact = false;
        }
    }





}
