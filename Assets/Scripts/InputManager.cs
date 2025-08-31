// Local: Assets/Scripts/InputManager.cs

using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gerencia todas as entradas do jogador usando o novo Input System.
/// Implementa um padrão Singleton para fácil acesso e desacopla a lógica do jogo.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Eventos de Ações do Jogador
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action OnJump;
    public event Action OnGrappleStarted;
    public event Action OnGrappleCanceled;

    public event Action OnLevelFinished;
    public event Action OnLevelRestarted;



    // --- NOVO ---
    public event Action OnSlowTimeStarted;
    public event Action OnSlowTimeCanceled;
    // --- FIM NOVO ---

    private PlayerControls _playerControls;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        _playerControls.Enable();

        // Movimento e Câmera
        _playerControls.Player.Move.performed += HandleMove;
        _playerControls.Player.Move.canceled += HandleMove;
        _playerControls.Player.Look.performed += HandleLook;
        _playerControls.Player.Look.canceled += HandleLook;

        // Ações
        _playerControls.Player.Jump.performed += HandleJump;
        _playerControls.Player.Grapple.performed += HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled += HandleGrappleCanceled;

        // --- NOVO ---
        // Certifique-se de ter criado uma ação chamada "SlowTime" no seu Input Actions Asset.
        _playerControls.Player.SlowTime.performed += HandleSlowTimeStarted;
        _playerControls.Player.SlowTime.canceled += HandleSlowTimeCanceled;

        _playerControls.Player.FinishLevel.performed += HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed += HandleRestartLevel;

    }

    private void OnDisable()
    {
        if (_playerControls == null) return;
        _playerControls.Disable();

        _playerControls.Player.Move.performed -= HandleMove;
        _playerControls.Player.Move.canceled -= HandleMove;
        _playerControls.Player.Look.performed -= HandleLook;
        _playerControls.Player.Look.canceled -= HandleLook;
        _playerControls.Player.Jump.performed -= HandleJump;
        _playerControls.Player.Grapple.performed -= HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled -= HandleGrappleCanceled;

        // --- NOVO ---
        _playerControls.Player.SlowTime.performed -= HandleSlowTimeStarted;
        _playerControls.Player.SlowTime.canceled -= HandleSlowTimeCanceled;
        // --- FIM NOVO ---
    }

    // Métodos de manipulação de eventos
    private void HandleMove(InputAction.CallbackContext context) => OnMove?.Invoke(context.ReadValue<Vector2>());
    private void HandleLook(InputAction.CallbackContext context) => OnLook?.Invoke(context.ReadValue<Vector2>());
    private void HandleJump(InputAction.CallbackContext context) => OnJump?.Invoke();
    private void HandleGrappleStarted(InputAction.CallbackContext context) => OnGrappleStarted?.Invoke();
    private void HandleGrappleCanceled(InputAction.CallbackContext context) => OnGrappleCanceled?.Invoke();

    private void HandleSlowTimeStarted(InputAction.CallbackContext context) => OnSlowTimeStarted?.Invoke();
    private void HandleSlowTimeCanceled(InputAction.CallbackContext context) => OnSlowTimeCanceled?.Invoke();
    
    private void HandleFinishLevel(InputAction.CallbackContext context) => OnLevelFinished?.Invoke();
    private void HandleRestartLevel(InputAction.CallbackContext context) => OnLevelRestarted?.Invoke();

}