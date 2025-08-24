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

    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action OnJump;
    public event Action OnGrappleStarted;
    public event Action OnGrappleCanceled;

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
        Debug.Log("InputManager initialized and PlayerControls created.");

        AudioManager.instance.PlayMusic("Menu");
    }

    private void OnEnable()
    {
        _playerControls.Enable();
        _playerControls.Player.Move.performed += HandleMove;
        _playerControls.Player.Move.canceled += HandleMove;
        _playerControls.Player.Look.performed += HandleLook;
        _playerControls.Player.Look.canceled += HandleLook;
        _playerControls.Player.Jump.performed += HandleJump;
        _playerControls.Player.Grapple.performed += HandleGrappleStarted;
        _playerControls.Player.Grapple.canceled += HandleGrappleCanceled;
        Debug.Log("InputManager enabled and controls set up.");
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
    }

    private void HandleMove(InputAction.CallbackContext context) => OnMove?.Invoke(context.ReadValue<Vector2>());
    private void HandleLook(InputAction.CallbackContext context) => OnLook?.Invoke(context.ReadValue<Vector2>());
    private void HandleJump(InputAction.CallbackContext context) => OnJump?.Invoke();
    private void HandleGrappleStarted(InputAction.CallbackContext context) => OnGrappleStarted?.Invoke();
    private void HandleGrappleCanceled(InputAction.CallbackContext context) => OnGrappleCanceled?.Invoke();
}