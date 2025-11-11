using UnityEngine;
using System;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private GrapplingHookController grapplingHookController;

    [Header("Configuração de Rotação")]
    [Tooltip("A velocidade angular (graus por segundo) que corresponde à inclinação máxima da animação.")]
    [SerializeField] private float maxTurnSpeedForAnimation = 180f;
    [Tooltip("Tempo de suavização para a animação de inclinação. Valores menores são mais rápidos, maiores são mais suaves.")]
    private float turnAnimationSmoothTime = 0.1f;

    private Animator _animator;
    private float _lastYRotation;
    private float _currentTurnAmount;
    private float _smoothTurnVelocity;

    private readonly int _hashHorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
    private readonly int _hashIsGrounded = Animator.StringToHash("IsGrounded");
    private readonly int _hashJump = Animator.StringToHash("JumpTrigger");
    private readonly int _hashDoubleJump = Animator.StringToHash("DoubleJumpTrigger");
    private readonly int _hashGrappleStart = Animator.StringToHash("GrappleStartTrigger");
    private readonly int _hashGrappleStop = Animator.StringToHash("GrappleStopTrigger");
    private readonly int _hashIsGrappling = Animator.StringToHash("IsGrappling");
    private readonly int _hashTurnDirection = Animator.StringToHash("TurnDirection");


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _lastYRotation = transform.eulerAngles.y;
    }

    private void OnEnable()
    {
        if (playerMovementController == null)
        {
            Debug.LogError("PlayerMovementController não está atribuído no PlayerAnimationController.");
            enabled = false;
            return;
        }

        if (grapplingHookController == null)
        {
            Debug.LogWarning("GrapplingHookController não está atribuído. Animações de gancho não funcionarão.");
        }

        playerMovementController.OnHorizontalVelocityChanged += HandleVelocityChanged;
        playerMovementController.OnJumped += HandleJump;
        playerMovementController.OnDoubleJumpUsed += HandleDoubleJump;
        playerMovementController.OnGroundLanded += HandleLand;
        playerMovementController.OnLeftGround += HandleLeftGround;

        if (grapplingHookController != null)
        {
            grapplingHookController.OnGrappleStarted += HandleGrappleStarted;
            grapplingHookController.OnGrappleStopped += HandleGrappleStopped;
        }
    }

    private void OnDisable()
    {
        if (playerMovementController != null)
        {
            playerMovementController.OnHorizontalVelocityChanged -= HandleVelocityChanged;
            playerMovementController.OnJumped -= HandleJump;
            playerMovementController.OnDoubleJumpUsed -= HandleDoubleJump;
            playerMovementController.OnGroundLanded -= HandleLand;
            playerMovementController.OnLeftGround -= HandleLeftGround;
        }

        if (grapplingHookController != null)
        {
            grapplingHookController.OnGrappleStarted -= HandleGrappleStarted;
            grapplingHookController.OnGrappleStopped -= HandleGrappleStopped;
        }

        if (_animator != null && _animator.isInitialized)
        {
            _animator.SetBool(_hashIsGrappling, false);
            _animator.SetFloat(_hashTurnDirection, 0f);
        }
    }

    private void Update()
    {
        HandleTurningAnimation();
    }
    
    private void HandleTurningAnimation()
    {
        float targetTurnAmount = 0f;
        
        if (playerMovementController.isGrounded)
        {
            float currentYRotation = transform.eulerAngles.y;
            float deltaAngle = Mathf.DeltaAngle(_lastYRotation, currentYRotation);
            
            if (Time.deltaTime > 0)
            {
                float angularVelocity = deltaAngle / Time.deltaTime;
                targetTurnAmount = Mathf.Clamp(angularVelocity / maxTurnSpeedForAnimation, -1f, 1f);
            }
            
            _lastYRotation = currentYRotation;
        }
        else
        {
            _lastYRotation = transform.eulerAngles.y;
            targetTurnAmount = 0f;
        }

        _currentTurnAmount = Mathf.SmoothDamp(
            _currentTurnAmount, 
            targetTurnAmount, 
            ref _smoothTurnVelocity, 
            turnAnimationSmoothTime
        );

        _animator.SetFloat(_hashTurnDirection, _currentTurnAmount);
    }

    private void HandleVelocityChanged(float horizontalSpeed)
    {
        _animator.SetFloat(_hashHorizontalSpeed, horizontalSpeed);
    }

    private void HandleJump()
    {
        _animator.SetBool(_hashIsGrounded, false);
        _animator.SetTrigger(_hashJump);
    }
    
    private void HandleDoubleJump()
    {
        _animator.SetTrigger(_hashDoubleJump);
    }

    private void HandleLand()
    {
        _animator.SetBool(_hashIsGrounded, true);
    }

    private void HandleLeftGround()
    {
        _animator.SetBool(_hashIsGrounded, false);
    }

    private void HandleGrappleStarted()
    {
        _animator.SetTrigger(_hashGrappleStart);
        _animator.SetBool(_hashIsGrappling, true);
    }

    private void HandleGrappleStopped()
    {
        _animator.SetTrigger(_hashGrappleStop);
        _animator.SetBool(_hashIsGrappling, false);
    }
}