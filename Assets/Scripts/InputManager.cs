using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputStateManager))]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerControls PlayerControls => _playerControls;

    // *** CORREÇÃO SENIOR: Leitura direta do Action ***
    // Não armazenamos mais em variável. Lemos o valor em tempo real.
    // Isso garante que se o jogador estiver segurando o botão, retornará o valor correto imediatamente.
    public Vector2 CurrentMoveInput => _playerControls != null && _playerControls.Player.enabled 
        ? _playerControls.Player.Move.ReadValue<Vector2>() 
        : Vector2.zero;

    public event Action<Vector2> OnMove;
    public event Action OnJumpPerformed;
    public event Action OnJumpCanceled;
    public event Action OnGrappleStarted;
    public event Action OnGrappleCanceled;
    
    public event Action OnSlowTimeInputStarted;
    public event Action OnSlowTimeInputCanceled;
    
    public event Action OnResetToCheckpoint;
    public event Action OnFullLevelReset;
    public event Action OnPausePressed;

#if ENABLE_CHEATS
    public event Action OnToggleInfiniteJumps;
    public event Action OnToggleInfiniteGrappleCooldown;
    public event Action OnToggleInfiniteGrappleDuration;
    public event Action OnToggleInfiniteTimeStop;
    public event Action OnToggleAllCheats;
#endif

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
        
        GetComponent<InputStateManager>().Initialize(_playerControls);
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
        
        _playerControls.Player.SlowTime.started += HandleSlowTimeInputStarted;
        _playerControls.Player.SlowTime.canceled += HandleSlowTimeInputCanceled;
        
        _playerControls.Player.FinishLevel.performed += HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed += HandleResetToCheckpoint;
        _playerControls.Player.FullReset.performed += HandleFullLevelReset;
        _playerControls.PostGame.RestartLevel.performed += HandleFullLevelReset; 
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
        if (_playerControls == null) return;
        
        _playerControls.Player.Move.performed -= HandleMove;
        _playerControls.Player.Move.canceled -= HandleMove;
        _playerControls.Player.Jump.performed -= HandleJumpPerformed;
        _playerControls.Player.Jump.canceled -= HandleJumpCanceled;
        _playerControls.Player.Grapple.performed -= HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled -= HandleGrappleCanceled;
        
        _playerControls.Player.SlowTime.started -= HandleSlowTimeInputStarted;
        _playerControls.Player.SlowTime.canceled -= HandleSlowTimeInputCanceled;
        
        _playerControls.Player.FinishLevel.performed -= HandleFinishLevel;
        _playerControls.Player.RestartLevel.performed -= HandleResetToCheckpoint;
        _playerControls.Player.FullReset.performed -= HandleFullLevelReset;
        _playerControls.PostGame.RestartLevel.performed -= HandleFullLevelReset;
        _playerControls.Player.Pause.performed -= HandlePausePressed;
        _playerControls.UI.Unpause.performed -= HandlePausePressed;

        _playerControls.Disable();
    }
    
    private void HandleMove(InputAction.CallbackContext context) 
    {
        OnMove?.Invoke(context.ReadValue<Vector2>());
    }

    private void HandleJumpPerformed(InputAction.CallbackContext context) => OnJumpPerformed?.Invoke();
    private void HandleJumpCanceled(InputAction.CallbackContext context) => OnJumpCanceled?.Invoke();
    private void HandleGrappleStarted(InputAction.CallbackContext context) => OnGrappleStarted?.Invoke();
    private void HandleGrappleCanceled(InputAction.CallbackContext context) => OnGrappleCanceled?.Invoke();
    
    private void HandleSlowTimeInputStarted(InputAction.CallbackContext context) => OnSlowTimeInputStarted?.Invoke();
    private void HandleSlowTimeInputCanceled(InputAction.CallbackContext context) => OnSlowTimeInputCanceled?.Invoke();
    
    private void HandleFinishLevel(InputAction.CallbackContext context) => GameFlowManager.Instance.CompleteLevel(true);
    private void HandleResetToCheckpoint(InputAction.CallbackContext context) => OnResetToCheckpoint?.Invoke();
    private void HandleFullLevelReset(InputAction.CallbackContext context) => OnFullLevelReset?.Invoke();
    private void HandlePausePressed(InputAction.CallbackContext context) => OnPausePressed?.Invoke();
}