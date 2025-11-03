// Assets/Scripts/InputManager.cs

using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputStateManager))]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerControls PlayerControls => _playerControls;

    public event Action<Vector2> OnMove;
    public event Action OnJumpPerformed;
    public event Action OnJumpCanceled;
    public event Action OnGrappleStarted;
    public event Action OnGrappleCanceled;
    public event Action OnSlowTimeToggled;
    public event Action OnLevelRestarted;
    public event Action OnPausePressed;

#if ENABLE_CHEATS
    public event Action OnToggleInfiniteJumps;
    public event Action OnToggleInfiniteGrappleCooldown;
    public event Action OnToggleInfiniteGrappleDuration;
    public event Action OnToggleInfiniteTimeStop;
    public event Action OnToggleAllCheats;
#endif

    private PlayerControls _playerControls;
    private bool _isGameplayInputActive = true;

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
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGamePaused += DisableGameplayInput;
            GameFlowManager.Instance.OnGameResumed += EnableGameplayInput;
        }
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
        _playerControls.Player.SlowTime.performed += HandleSlowTimeToggled;
        _playerControls.Player.FinishLevel.performed += HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed += HandleRestartLevel;
        _playerControls.PostGame.RestartLevel.performed += HandleRestartLevel; 
        _playerControls.Player.Pause.performed += HandlePausePressed;
        _playerControls.UI.Unpause.performed += HandlePausePressed;
        
#if ENABLE_CHEATS
        _playerControls.Debug.Enable();
        _playerControls.Debug.ToggleInfiniteJumps.performed += ctx => OnToggleInfiniteJumps?.Invoke();
        _playerControls.Debug.ToggleInfiniteGrappleCooldown.performed += ctx => OnToggleInfiniteGrappleCooldown?.Invoke();
        _playerControls.Debug.ToggleInfiniteGrappleDuration.performed += ctx => OnToggleInfiniteGrappleDuration?.Invoke();
        _playerControls.Debug.ToggleInfiniteTimeStop.performed += ctx => OnToggleInfiniteTimeStop?.Invoke();
        _playerControls.Debug.ToggleAllCheats.performed += ctx => OnToggleAllCheats?.Invoke();
#endif
    }

    private void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGamePaused -= DisableGameplayInput;
            GameFlowManager.Instance.OnGameResumed -= EnableGameplayInput;
        }
        
        if (_playerControls == null) return;
        
        _playerControls.Player.Move.performed -= HandleMove;
        _playerControls.Player.Move.canceled -= HandleMove;
        _playerControls.Player.Jump.performed -= HandleJumpPerformed;
        _playerControls.Player.Jump.canceled -= HandleJumpCanceled;
        _playerControls.Player.Grapple.performed -= HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled -= HandleGrappleCanceled;
        _playerControls.Player.SlowTime.performed -= HandleSlowTimeToggled;
        _playerControls.Player.FinishLevel.performed -= HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed -= HandleRestartLevel;
        _playerControls.PostGame.RestartLevel.performed -= HandleRestartLevel;
        _playerControls.Player.Pause.performed -= HandlePausePressed;
        _playerControls.UI.Unpause.performed -= HandlePausePressed;

        _playerControls.Disable();
    }

    private void EnableGameplayInput() => _isGameplayInputActive = true;
    private void DisableGameplayInput() => _isGameplayInputActive = false;
    
    private void HandleMove(InputAction.CallbackContext context) { if (!_isGameplayInputActive) return; OnMove?.Invoke(context.ReadValue<Vector2>()); }
    private void HandleJumpPerformed(InputAction.CallbackContext context) { if (!_isGameplayInputActive) return; OnJumpPerformed?.Invoke(); }
    private void HandleJumpCanceled(InputAction.CallbackContext context) { if (!_isGameplayInputActive) return; OnJumpCanceled?.Invoke(); }
    private void HandleGrappleStarted(InputAction.CallbackContext context) { if (!_isGameplayInputActive) return; OnGrappleStarted?.Invoke(); }
    private void HandleGrappleCanceled(InputAction.CallbackContext context) { if (!_isGameplayInputActive) return; OnGrappleCanceled?.Invoke(); }
    private void HandleSlowTimeToggled(InputAction.CallbackContext context) { if (!_isGameplayInputActive) return; OnSlowTimeToggled?.Invoke(); }

    private void HandleFinishLevel(InputAction.CallbackContext context) { if (!_isGameplayInputActive) return; GameFlowManager.Instance.CompleteLevel(); }
    private void HandleRestartLevel(InputAction.CallbackContext context) => OnLevelRestarted?.Invoke();
    private void HandlePausePressed(InputAction.CallbackContext context) => OnPausePressed?.Invoke();
}