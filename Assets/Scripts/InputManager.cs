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
    public event Action OnJumpPerformed;
    public event Action OnJumpCanceled;
    public event Action OnGrappleStarted;
    public event Action OnGrappleCanceled;
    public event Action OnSlowTimeStarted;
    public event Action OnSlowTimeCanceled;
    
    // Eventos de Jogo (Debug/Sistema)
    public event Action OnLevelFinished;
    public event Action OnLevelRestarted;
    public event Action OnPausePressed;

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
        _playerControls.Enable();
        
        _playerControls.Player.Move.performed += HandleMove;
        _playerControls.Player.Move.canceled += HandleMove;

        _playerControls.Player.Jump.performed += HandleJumpPerformed;
        _playerControls.Player.Jump.canceled += HandleJumpCanceled;
        
        _playerControls.Player.Grapple.performed += HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled += HandleGrappleCanceled;
        
        _playerControls.Player.SlowTime.performed += HandleSlowTimeStarted;
        _playerControls.Player.SlowTime.canceled += HandleSlowTimeCanceled;
        
        _playerControls.Player.FinishLevel.performed += HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed += HandleRestartLevel;
        
        _playerControls.Player.Pause.performed += HandlePausePressed;
        _playerControls.UI.Unpause.performed += HandlePausePressed;
    }

    private void OnDisable()
    {
        if (_playerControls == null) return;

        _playerControls.Player.Move.performed -= HandleMove;
        _playerControls.Player.Move.canceled -= HandleMove;
        
        _playerControls.Player.Jump.performed -= HandleJumpPerformed;
        _playerControls.Player.Jump.canceled -= HandleJumpCanceled;
        
        _playerControls.Player.Grapple.performed -= HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled -= HandleGrappleCanceled;
        
        _playerControls.Player.SlowTime.performed -= HandleSlowTimeStarted;
        _playerControls.Player.SlowTime.canceled -= HandleSlowTimeCanceled;
        
        _playerControls.Player.FinishLevel.performed -= HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed -= HandleRestartLevel;
        
        _playerControls.Player.Pause.performed -= HandlePausePressed;
        _playerControls.UI.Unpause.performed -= HandlePausePressed;
        
        _playerControls.Disable();
    }

    private void HandleMove(InputAction.CallbackContext context) => OnMove?.Invoke(context.ReadValue<Vector2>());
    private void HandleJumpPerformed(InputAction.CallbackContext context) => OnJumpPerformed?.Invoke();
    private void HandleJumpCanceled(InputAction.CallbackContext context) => OnJumpCanceled?.Invoke();
    private void HandleGrappleStarted(InputAction.CallbackContext context) => OnGrappleStarted?.Invoke();
    private void HandleGrappleCanceled(InputAction.CallbackContext context) => OnGrappleCanceled?.Invoke();
    private void HandleSlowTimeStarted(InputAction.CallbackContext context) => OnSlowTimeStarted?.Invoke();
    private void HandleSlowTimeCanceled(InputAction.CallbackContext context) => OnSlowTimeCanceled?.Invoke();
    private void HandleFinishLevel(InputAction.CallbackContext context) => OnLevelFinished?.Invoke();
    private void HandleRestartLevel(InputAction.CallbackContext context) => OnLevelRestarted?.Invoke();
    private void HandlePausePressed(InputAction.CallbackContext context) => OnPausePressed?.Invoke();
}