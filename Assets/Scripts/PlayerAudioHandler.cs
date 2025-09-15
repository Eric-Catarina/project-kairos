using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerAudioHandler : MonoBehaviour
{
    private PlayerMovementController movement;
    private GrapplingHookController grapple;
    private Rigidbody rb;

    [Header("Sons - Movimento")]
    [SerializeField] private string jumpSfx = "Jump";
    [SerializeField] private string doubleJumpSfx = "DoubleJump";
    [SerializeField] private string landSfx = "Land";
    [SerializeField] private string footstepSfx = "Step";
    [SerializeField] private string wallHitSfx = "WallHit";

    [Header("Sons - Estado do Player")]
    [SerializeField] private string deathSfx = "Death";
    [SerializeField] private string respawnSfx = "Respawn";
    [SerializeField] private string timeSkillOnSfx = "TimeOn";
    [SerializeField] private string timeSkillOffSfx = "TimeOff";

    [Header("Sons - Grappling Hook")]
    [SerializeField] private string grappleShootSfx = "GrappleShoot";
    [SerializeField] private string grappleAttachSfx = "GrappleAttach";
    [SerializeField] private string grappleReleaseSfx = "GrappleRelease";

    [Header("Parâmetros")]
    [SerializeField] private float stepInterval = 0.35f;
    [SerializeField] private float minStepSpeed = 2f;
    [SerializeField] private float wallHitMinForce = 5f;

    private float stepTimer;
    private bool wasGrounded;
    private bool didDoubleJump;
    private Vector3 lastPosition;

    private bool wasGrappling; // estado anterior do grapple
    private bool grappleJustStarted;

    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
        grapple = GetComponent<GrapplingHookController>();
        rb = movement.Rb;

        wasGrounded = movement.isGrounded;
        stepTimer = stepInterval;
        lastPosition = transform.position;
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
        HandleDoubleJumpDetection();
        HandleFootsteps();
        DetectRespawn();
        HandleGrappleAudio();
    }

    private void HandleJumpDetection()
    {
        if (wasGrounded && !movement.isGrounded)
        {
            AudioManager.instance.PlaySFX(jumpSfx);
            didDoubleJump = false;
        }
        wasGrounded = movement.isGrounded;
    }

    private void HandleDoubleJumpDetection()
    {
        if (!movement.isGrounded && !didDoubleJump)
        {
            if (rb.linearVelocity.y > 1f )
            {
                AudioManager.instance.PlaySFX(doubleJumpSfx);
                didDoubleJump = true;
            }
        }
    }

    private void HandleFootsteps()
    {
        if (!movement.isGrounded || rb.linearVelocity.magnitude < minStepSpeed)
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

    private void DetectRespawn()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);
        if (distance > 10f && wasGrounded && movement.isGrounded)
        {
            AudioManager.instance.PlaySFX(respawnSfx);
        }
        lastPosition = transform.position;
    }

    private void HandleGrappleAudio()
    {
        if (grapple == null) return;

        if (!wasGrappling && grapple.IsGrappling)
        {
            AudioManager.instance.PlaySFX(grappleShootSfx);
            grappleJustStarted = true;
        }

       
        if (grappleJustStarted && grapple.IsGrappling)
        {
            AudioManager.instance.PlaySFX(grappleAttachSfx);
            grappleJustStarted = false;
        }

        if (wasGrappling && !grapple.IsGrappling)
        {
            AudioManager.instance.PlaySFX(grappleReleaseSfx);
        }

        wasGrappling = grapple.IsGrappling;
    }

    private void PlayLand() => AudioManager.instance.PlaySFX(landSfx);

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 normal = collision.contacts[0].normal;
        if (Vector3.Dot(normal, Vector3.up) < 0.5f)
        {
            if (collision.relativeVelocity.magnitude > wallHitMinForce)
            {
                AudioManager.instance.PlaySFX(wallHitSfx);
            }
        }
    }

    // Chamadas externas
    public void PlayDeath() => AudioManager.instance.PlaySFX(deathSfx);
    public void PlayRespawnManual() => AudioManager.instance.PlaySFX(respawnSfx);
    public void PlayTimeSkillOn() => AudioManager.instance.PlaySFX(timeSkillOnSfx);
    public void PlayTimeSkillOff() => AudioManager.instance.PlaySFX(timeSkillOffSfx);
}

