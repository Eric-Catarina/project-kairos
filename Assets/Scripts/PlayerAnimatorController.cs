// Local: Assets/Scripts/PlayerAnimationController.cs

using UnityEngine;
using System; // Adicionado para Action (embora não seja usado diretamente aqui, é bom para consistência)

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [SerializeField] private GrapplingHookController grapplingHookController;

    private Animator _animator;

    // Hashes existentes
    private readonly int _hashHorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
    private readonly int _hashIsGrounded = Animator.StringToHash("IsGrounded");
    private readonly int _hashJump = Animator.StringToHash("JumpTrigger");

    // Hashes do gancho
    private readonly int _hashGrappleStart = Animator.StringToHash("GrappleStartTrigger");
    private readonly int _hashGrappleStop = Animator.StringToHash("GrappleStopTrigger");
    private readonly int _hashIsGrappling = Animator.StringToHash("IsGrappling");

    // NOVO Hash para o Double Jump
    private readonly int _hashDoubleJump = Animator.StringToHash("DoubleJumpTrigger");


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // Checagem existente
        if (playerMovementController == null)
        {
            Debug.LogError("PlayerMovementController não está atribuído no PlayerAnimationController.");
            enabled = false;
            return;
        }

        // Checagem para o gancho
        if (grapplingHookController == null)
        {
            Debug.LogWarning("GrapplingHookController não está atribuído. Animações de gancho não funcionarão.");
        }

        // Inscrições existentes
        playerMovementController.OnHorizontalVelocityChanged += HandleVelocityChanged;
        playerMovementController.OnJumped += HandleJump;
        playerMovementController.OnGroundLanded += HandleLand;
        playerMovementController.OnLeftGround += HandleLeftGround;

        // NOVO: Inscrição para o Double Jump
        playerMovementController.OnDoubleJumpUsed += HandleDoubleJump;

        // Inscrições do gancho
        if (grapplingHookController != null)
        {
            grapplingHookController.OnGrappleStarted += HandleGrappleStarted;
            grapplingHookController.OnGrappleStopped += HandleGrappleStopped;
        }
    }

    private void OnDisable()
    {
        // Desinscrições existentes
        if (playerMovementController != null)
        {
            playerMovementController.OnHorizontalVelocityChanged -= HandleVelocityChanged;
            playerMovementController.OnJumped -= HandleJump;
            playerMovementController.OnGroundLanded -= HandleLand;
            playerMovementController.OnLeftGround -= HandleLeftGround;

            // NOVO: Desinscrição para o Double Jump
            playerMovementController.OnDoubleJumpUsed -= HandleDoubleJump;
        }

        // Desinscrições do gancho
        if (grapplingHookController != null)
        {
            grapplingHookController.OnGrappleStarted -= HandleGrappleStarted;
            grapplingHookController.OnGrappleStopped -= HandleGrappleStopped;
        }

        // Garante que o estado seja resetado se este script for desabilitado
        if (_animator != null && _animator.isInitialized)
        {
            _animator.SetBool(_hashIsGrappling, false);
        }
    }

    // Handlers existentes
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

    // NOVO Handler para o Double Jump
    private void HandleDoubleJump()
    {
        // Não reseta IsGrounded aqui, pois o jogador já estava no ar.
        _animator.SetTrigger(_hashDoubleJump);
    }


    // Handlers do gancho 
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