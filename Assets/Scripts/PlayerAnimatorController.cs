// Local: Assets/Scripts/PlayerAnimationController.cs

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerMovementController playerMovementController;

    private Animator _animator;

    private readonly int _hashHorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
    private readonly int _hashIsGrounded = Animator.StringToHash("IsGrounded");
    private readonly int _hashJump = Animator.StringToHash("JumpTrigger");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (playerMovementController == null)
        {
            Debug.LogError("PlayerMovementController não está atribuído no PlayerAnimationController.");
            enabled = false;
            return;
        }
        
        playerMovementController.OnHorizontalVelocityChanged += HandleVelocityChanged;
        playerMovementController.OnJumped += HandleJump;
        playerMovementController.OnGroundLanded += HandleLand;
        playerMovementController.OnLeftGround += HandleLeftGround;
    }

    private void OnDisable()
    {
        if (playerMovementController == null) return;
        
        playerMovementController.OnHorizontalVelocityChanged -= HandleVelocityChanged;
        playerMovementController.OnJumped -= HandleJump;
        playerMovementController.OnGroundLanded -= HandleLand;
        playerMovementController.OnLeftGround -= HandleLeftGround;
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
    
    private void HandleLand()
    {
        _animator.SetBool(_hashIsGrounded, true);
    }

    private void HandleLeftGround()
    {
        _animator.SetBool(_hashIsGrounded, false);
    }
}