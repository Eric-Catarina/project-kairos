using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerAudioHandler : MonoBehaviour
{
    private PlayerMovementController movement;
    private GrapplingHookController grapple;
    private Rigidbody rb;

    private bool jumpSoundPlayed = false;

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
    private bool hasLandedOnce = false;
    private Vector3 lastPosition;

    private bool wasGrappling;
    private bool grappleJustStarted;

    private bool hasJumpedOnce = false; // Já deu o primeiro pulo
    private bool canDoubleJump = false; // Se double jump pode tocar


    private AudioSource windSource;

    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
        grapple = GetComponent<GrapplingHookController>();
        rb = GetComponent<Rigidbody>();

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
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPerformed += HandleJumpAudio;
            InputManager.Instance.OnGrappleStarted += HandleGrappleStart;
            InputManager.Instance.OnGrappleCanceled += HandleGrappleEnd;
            InputManager.Instance.OnSlowTimeStarted += PlayTimeSkillOn;
            InputManager.Instance.OnSlowTimeCanceled += PlayTimeSkillOff;
        }

        movement.OnGroundLanded += PlayLand;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPerformed -= HandleJumpAudio;
            InputManager.Instance.OnGrappleStarted -= HandleGrappleStart;
            InputManager.Instance.OnGrappleCanceled -= HandleGrappleEnd;
            InputManager.Instance.OnSlowTimeStarted -= PlayTimeSkillOn;
            InputManager.Instance.OnSlowTimeCanceled -= PlayTimeSkillOff;
        }

        movement.OnGroundLanded -= PlayLand;
    }

    private void Update()
    {
        HandleFootsteps();
        DetectRespawn();
        HandleGrappleAudio();
        HandleWindAudio();

        if (movement.isGrounded)
        {
            hasJumpedOnce = false;
            canDoubleJump = false;
        }
    }


    private void HandleJumpAudio()
    {
        if (movement.isGrounded)
        {
            // Pulo normal
            AudioManager.instance.PlaySFX(jumpSfx);
            hasJumpedOnce = true;
            canDoubleJump = false;
        }
        else
        {
            if (canDoubleJump)
            {
                AudioManager.instance.PlaySFX(doubleJumpSfx);
                canDoubleJump = false; // só toca uma vez
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
        if (distance > 10f && movement.isGrounded)
        {
            AudioManager.instance.PlaySFX(respawnSfx);
            if (windSource != null)
            {
                windSource.Stop();
                windSource.Play();
            }
        }
        lastPosition = transform.position;
    }

    private void HandleGrappleStart() => grappleJustStarted = true;

    // Quando o grappling termina, libera double jump
    private void HandleGrappleEnd()
    {
        AudioManager.instance.PlaySFX(grappleReleaseSfx);
        grappleJustStarted = false;
        canDoubleJump = true; // agora o player pode dar double jump
    }

    private void HandleGrappleAudio()
    {
        if (grapple == null) return;

        if (grappleJustStarted)
        {
            AudioManager.instance.PlaySFX(grappleShootSfx);
            AudioManager.instance.PlaySFX(grappleAttachSfx);
            grappleJustStarted = false;
            wasGrappling = true;
        }
    }

    private void HandleWindAudio()
    {
        if (windSource == null) return;

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
        if (Vector3.Dot(normal, Vector3.up) < 0.5f && collision.relativeVelocity.magnitude > wallHitMinForce)
        {
            AudioManager.instance.PlaySFX(wallHitSfx);
        }
    }

    // Funções públicas de áudio
    public void PlayDeath() => AudioManager.instance.PlaySFX(deathSfx);
    public void PlayRespawnManual()
    {
        AudioManager.instance.PlaySFX(respawnSfx);
        if (windSource != null)
            windSource.volume = 0f;
    }
    public void PlayTimeSkillOn() => AudioManager.instance.PlaySFX(timeSkillOnSfx);
    public void PlayTimeSkillOff() => AudioManager.instance.PlaySFX(timeSkillOffSfx);
}

