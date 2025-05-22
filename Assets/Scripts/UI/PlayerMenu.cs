using System;
using StarterAssets;
using UnityEngine;
using TMPro;

using UnityEngine.InputSystem;
using UnityEngine.UI;



public class PlayerMenu : MonoBehaviour
{
    // [SerializeField] private MenuInicial botonesUI;
    private PlayerInput _playerInput;
    private StarterAssetsInputs _input;


    private Vector2 scrollInput;
    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created



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
        }
        // Modo ajuste de slider (mantén tu código existente)
        else if (MenuInicial.menuActivo.IsAdjustingSlider() && Mathf.Abs(input.x) > deadzone)
        {
            int direction = input.x > 0 ? 1 : -1;
            MenuInicial.menuActivo.MoveSelection(direction);
            lastInputTime = Time.time;
        }
        // Navegación normal (vertical)
        else if (Mathf.Abs(input.y) > deadzone)
        {
            int direction = input.y > 0 ? -1 : 1;
            MenuInicial.menuActivo.MoveSelection(direction);
            lastInputTime = Time.time;
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
            }
            else if (MenuInicial.menuActivo.IsAdjustingSlider())
            {
                MenuInicial.menuActivo.ToggleAjusteSlider();
            }
            else if (MenuInicial.menuActivo.CurrentButtonIsToggle())
            {
                // Entrar en modo ajuste
                MenuInicial.menuActivo.ToggleModoAjuste();
            }
            else if (MenuInicial.menuActivo.CurrentButtonHasAdjacentSlider())
            {
                MenuInicial.menuActivo.ToggleAjusteSlider();
            }
            else
            {
                // Acción normal del botón
                MenuInicial.menuActivo.ActivateSelectedButton();
            }
            _input.interact = false;
        }
    }
}
