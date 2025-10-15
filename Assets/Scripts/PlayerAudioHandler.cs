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
    [SerializeField] private string footstepSfx = "Footstep";
    [SerializeField] private string wallHitSfx = "WallHit";

    [Header("Sons - Estado do Player")]
    [SerializeField] private string deathSfx = "Death";
    [SerializeField] private string deathFallSFX = "DeathFall";
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

    [Header("Som - Vento")]
    [SerializeField] private string windSfx = "Wind";
    [SerializeField] private float windMinSpeed = 20f;
    [SerializeField] private float windMaxSpeed = 60f;
    [SerializeField] private float windFadeSpeed = 5f;

    private float stepTimer;
    private bool wasGrounded;
    private bool canDoubleJumpSound;
    private Vector3 lastPosition;

    private bool wasGrappling;
    private bool grappleJustStarted;

    private bool hasLandedOnce = false;

    private AudioSource windSource;


    private void Awake()
    {

        movement = GetComponent<PlayerMovementController>();
        grapple = GetComponent<GrapplingHookController>();
        rb = GetComponent<Rigidbody>();

        wasGrounded = movement.isGrounded;
        stepTimer = stepInterval;
        lastPosition = transform.position;

        if (AudioManager.instance != null)
        {
            windSource = AudioManager.instance.PlayLoopingSFX(windSfx, 0f);
        }
        else
        {
            Debug.LogWarning("AudioManager não encontrado na cena ao iniciar PlayerAudioHandler!");
        }
    }

    private void OnEnable()
    {
        movement.OnGroundLanded += PlayLand;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSlowTimeStarted += PlayTimeSkillOn;
            InputManager.Instance.OnSlowTimeCanceled += PlayTimeSkillOff;

            InputManager.Instance.OnJumpPerformed += HandleJumpAudio; // novo
        }
    }

    private void OnDisable()
    {
        movement.OnGroundLanded -= PlayLand;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSlowTimeStarted -= PlayTimeSkillOn;
            InputManager.Instance.OnSlowTimeCanceled -= PlayTimeSkillOff;

            InputManager.Instance.OnJumpPerformed -= HandleJumpAudio; // novo
        }
    }

    private void Update()
    {
        HandleFootsteps();
        DetectRespawn();
        HandleGrappleAudio();
        HandleWindAudio();
    }



    private void HandleJumpAudio()
    {
        if (movement.isGrounded)
        {
            // Som do pulo normal
            AudioManager.instance.PlaySFX(jumpSfx);
        }
        else if (!movement.isGrounded && movement.GetType()
                 .GetField("canDoubleJump", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 ?.GetValue(movement) is bool canDoubleJump && canDoubleJump)
        {
            // Som do double jump (apenas se ainda pode dar double jump)
            AudioManager.instance.PlaySFX(doubleJumpSfx);
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

            if (windSource != null)
            {
                windSource.Stop();
                windSource.Play(); // reinicia o loop, com volume zerado
            }
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
            canDoubleJumpSound = true;
        }

        wasGrappling = grapple.IsGrappling;
    }


    private void HandleWindAudio()
    {
        if (windSource == null || rb == null) return;

        float speed = rb.linearVelocity.magnitude;
        float targetVolume = 0f;

        if (speed > windMinSpeed)
        {
            targetVolume = Mathf.InverseLerp(windMinSpeed, windMaxSpeed, speed);
        }

        windSource.volume = Mathf.MoveTowards(windSource.volume, targetVolume, Time.deltaTime * windFadeSpeed);
    }




    private void PlayLand()
    {
        if (!hasLandedOnce)
        {
            hasLandedOnce = true;
            return;
        }

        AudioManager.instance.PlaySFX(landSfx);
    }

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

    public void PlayDeath() => AudioManager.instance.PlaySFX(deathSfx);
    public void PlayRespawnManual()
    {
        AudioManager.instance.PlaySFX(respawnSfx);

        if (windSource != null)
            windSource.volume = 0f; // resetar o vento
    }
    public void PlayTimeSkillOn() => AudioManager.instance.PlaySFX(timeSkillOnSfx);
    public void PlayTimeSkillOff() => AudioManager.instance.PlaySFX(timeSkillOffSfx);
}


