// Local: Assets/Scripts/InputManager.cs

using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputStateManager))]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerControls PlayerControls => _playerControls;

    // Eventos de Ações do Jogador
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action OnJumpPerformed;
    public event Action OnJumpCanceled; // NOVO EVENTO
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
        _playerControls.Player.Move.performed += HandleMove;
        _playerControls.Player.Move.canceled += HandleMove;
        _playerControls.Player.Look.performed += HandleLook;
        _playerControls.Player.Look.canceled += HandleLook;

        _playerControls.Player.Jump.performed += HandleJumpPerformed;
        _playerControls.Player.Jump.canceled += HandleJumpCanceled; // NOVA INSCRIÇÃO
        
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
        
        _playerControls.Player.Jump.performed -= HandleJumpPerformed;
        _playerControls.Player.Jump.canceled -= HandleJumpCanceled; // NOVA REMOÇÃO
        
        _playerControls.Player.Grapple.performed -= HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled -= HandleGrappleCanceled;
        _playerControls.Player.SlowTime.performed -= HandleSlowTimeStarted;
        _playerControls.Player.SlowTime.canceled -= HandleSlowTimeCanceled;
        _playerControls.Player.FinishLevel.performed -= HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed -= HandleRestartLevel;
        _playerControls.Player.Pause.performed -= HandlePauseGame;
        _playerControls.UI.Unpause.performed -= HandleResumeGame;
    }

    private void HandleMove(InputAction.CallbackContext context) => OnMove?.Invoke(context.ReadValue<Vector2>());
    private void HandleLook(InputAction.CallbackContext context) => OnLook?.Invoke(context.ReadValue<Vector2>());
    private void HandleJumpPerformed(InputAction.CallbackContext context) => OnJumpPerformed?.Invoke();
    private void HandleJumpCanceled(InputAction.CallbackContext context) => OnJumpCanceled?.Invoke();
    private void HandleGrappleStarted(InputAction.CallbackContext context) => OnGrappleStarted?.Invoke();
    private void HandleGrappleCanceled(InputAction.CallbackContext context) => OnGrappleCanceled?.Invoke();
    private void HandleSlowTimeStarted(InputAction.CallbackContext context) => OnSlowTimeStarted?.Invoke();
    private void HandleSlowTimeCanceled(InputAction.CallbackContext context) => OnSlowTimeCanceled?.Invoke();
    private void HandleFinishLevel(InputAction.CallbackContext context) => OnLevelFinished?.Invoke();
    private void HandleRestartLevel(InputAction.CallbackContext context) => OnLevelRestarted?.Invoke();
    private void HandlePauseGame(InputAction.CallbackContext context) => InputStateManager.Instance.SwitchState(InputState.UI);
    private void HandleResumeGame(InputAction.CallbackContext context) => InputStateManager.Instance.SwitchState(InputState.Gameplay);
}