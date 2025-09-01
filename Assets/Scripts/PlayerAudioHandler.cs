using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerAudioHandler : MonoBehaviour
{
    private PlayerMovementController movement;

    [Header("Sons")]
    [SerializeField] private string jumpSfx = "Jump";
    [SerializeField] private string landSfx = "Land";
    [SerializeField] private string footstepSfx = "Step";

    [Header("Parâmetros")]
    [SerializeField] private float stepInterval = 0.35f;
    [SerializeField] private float minStepSpeed = 2f;

    private float stepTimer;
    private bool wasGrounded;

    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
        wasGrounded = movement.isGrounded;
        stepTimer = stepInterval;
    }

    private void OnEnable()
    {
        movement.OnGroundLanded += PlayLand;
    }

    private void OnDisable()
    {
        movement.OnGroundLanded -= PlayLand;
    }

    private void Update()
    {
        HandleJumpDetection();
        HandleFootsteps();
    }

    private void HandleJumpDetection()
    {
        if (wasGrounded && !movement.isGrounded)
        {
            AudioManager.instance.PlaySFX(jumpSfx);
        }

        wasGrounded = movement.isGrounded;
    }

    private void HandleFootsteps()
    {
        if (!movement.isGrounded || movement.Rb.linearVelocity.magnitude < minStepSpeed)
        {
            stepTimer = stepInterval;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            AudioManager.instance.PlaySFX(footstepSfx);
            stepTimer = stepInterval;
        }
    }

    private void PlayLand() => AudioManager.instance.PlaySFX(landSfx);
}
