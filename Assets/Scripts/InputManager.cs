// Local: Assets/Scripts/InputManager.cs

using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputStateManager))]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // --- NOVA LINHA ---
    // Propriedade pública para que outros sistemas possam acessar a instância gerenciada.
    public PlayerControls PlayerControls => _playerControls;
    // --- FIM DA NOVA LINHA ---

    // Eventos de Ações do Jogador
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action OnJump;
    public event Action OnGrappleStarted;
    public event Action OnGrappleCanceled;
    public event Action OnLevelFinished;
    public event Action OnLevelRestarted;
    public event Action OnSlowTimeStarted;
    public event Action OnSlowTimeCanceled;

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
        
        InputStateManager stateManager = GetComponent<InputStateManager>();
        stateManager.Initialize(_playerControls);
    }

    private void Start()
    {
        InputStateManager.Instance.SwitchState(InputState.Gameplay);
    }

    private void OnEnable()
    {
        // _playerControls.Enable() não é mais necessário aqui, o StateManager cuida disso.

        // Movimento e Câmera
        _playerControls.Player.Move.performed += HandleMove;
        _playerControls.Player.Move.canceled += HandleMove;
        _playerControls.Player.Look.performed += HandleLook;
        _playerControls.Player.Look.canceled += HandleLook;

        // Ações
        _playerControls.Player.Jump.performed += HandleJump;
        _playerControls.Player.Grapple.performed += HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled += HandleGrappleCanceled;
        _playerControls.Player.SlowTime.performed += HandleSlowTimeStarted;
        _playerControls.Player.SlowTime.canceled += HandleSlowTimeCanceled;
        _playerControls.Player.FinishLevel.performed += HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed += HandleRestartLevel;
        _playerControls.Player.Pause.performed += HandlePauseGame;
        _playerControls.UI.Unpause.performed += HandleResumeGame;
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
        _playerControls.Player.SlowTime.performed -= HandleSlowTimeStarted;
        _playerControls.Player.SlowTime.canceled -= HandleSlowTimeCanceled;
        _playerControls.Player.FinishLevel.performed -= HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed -= HandleRestartLevel;
        _playerControls.Player.Pause.performed -= HandlePauseGame;
        _playerControls.UI.Unpause.performed -= HandleResumeGame;
    }

    // Métodos de manipulação de eventos (sem alterações)
    private void HandleMove(InputAction.CallbackContext context) => OnMove?.Invoke(context.ReadValue<Vector2>());
    private void HandleLook(InputAction.CallbackContext context) => OnLook?.Invoke(context.ReadValue<Vector2>());
    private void HandleJump(InputAction.CallbackContext context) => OnJump?.Invoke();
    private void HandleGrappleStarted(InputAction.CallbackContext context) => OnGrappleStarted?.Invoke();
    private void HandleGrappleCanceled(InputAction.CallbackContext context) => OnGrappleCanceled?.Invoke();
    private void HandleSlowTimeStarted(InputAction.CallbackContext context) => OnSlowTimeStarted?.Invoke();
    private void HandleSlowTimeCanceled(InputAction.CallbackContext context) => OnSlowTimeCanceled?.Invoke();
    private void HandleFinishLevel(InputAction.CallbackContext context) => OnLevelFinished?.Invoke();
    private void HandleRestartLevel(InputAction.CallbackContext context) => OnLevelRestarted?.Invoke();
    private void HandlePauseGame(InputAction.CallbackContext context) => InputStateManager.Instance.SwitchState(InputState.UI);
    private void HandleResumeGame(InputAction.CallbackContext context) => InputStateManager.Instance.SwitchState(InputState.Gameplay);
}