using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerAudioHandler : MonoBehaviour
{
    private PlayerMovementController movement;
    private GrapplingHookController grapple;
    private Rigidbody rb;

    private bool jumpSoundPlayed = false;

    [Header("Sons - Movimento")]
    [SerializeField] private string[] footstepSfxOptions;
    [SerializeField] private string[] jumpSfxOptions;
    [SerializeField] private string[] doubleJumpSfxOptions;
    [SerializeField] private string[] landSfxOptions;
    [SerializeField] private string footstepSfx = "Footstep";
    [SerializeField] private string wallHitSfx = "WallHit";
    [SerializeField] private string ringSfx = "Ring";
    [SerializeField] private string jumpPadSfx = "JumpPad";


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

    private bool doubleJumpAvailable = false;
    private bool doubleJumpSoundPlayed = false;


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
            InputManager.Instance.OnSlowTimeToggled += PlayTimeSkillOn;
            InputManager.Instance.OnSlowTimeToggled += PlayTimeSkillOff;

            BasePowerUpRing.OnPowerRingActivated += HandlePowerRingAudio;

            // Escuta manualmente JumpPads que existirem na cena
            foreach (var jumpPad in FindObjectsOfType<JumpPad>())
            {
                AddJumpPadListener(jumpPad);
            }
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
            InputManager.Instance.OnSlowTimeToggled -= PlayTimeSkillOn;
            InputManager.Instance.OnSlowTimeToggled -= PlayTimeSkillOff;
            BasePowerUpRing.OnPowerRingActivated -= HandlePowerRingAudio;
        }

        movement.OnGroundLanded -= PlayLand;
    }

    private void Update()
    {
        HandleFootsteps();
        DetectRespawn();
        HandleGrappleAudio();
        HandleWindAudio();
        //HandleDoubleJumpAudio();

        if (movement.isGrounded)
        {
            hasJumpedOnce = false;
            canDoubleJump = false;
            doubleJumpSoundPlayed = false;
        }
    }


    private void HandleJumpAudio()
    {
        if (movement.isGrounded)
        {
            if (jumpSfxOptions != null && jumpSfxOptions.Length > 0)
            {
                int idx = Random.Range(0, jumpSfxOptions.Length);
                AudioManager.instance.PlaySFX(jumpSfxOptions[idx]);
            }

            doubleJumpAvailable = true;
            doubleJumpSoundPlayed = false;
        }
        else
        {
            // Double jump: toca som apenas se ainda estiver disponível
            if (doubleJumpAvailable && !doubleJumpSoundPlayed)
            {
                if (doubleJumpAvailable && !doubleJumpSoundPlayed)
                {
                    if (doubleJumpSfxOptions != null && doubleJumpSfxOptions.Length > 0)
                    {
                        int idx = Random.Range(0, doubleJumpSfxOptions.Length);
                        AudioManager.instance.PlaySFX(doubleJumpSfxOptions[idx]);
                    }

                    doubleJumpSoundPlayed = true;
                    doubleJumpAvailable = false;
                }
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
            string sfxToPlay = footstepSfx;

            if (footstepSfxOptions != null && footstepSfxOptions.Length > 0)
            {
                int idx = Random.Range(0, footstepSfxOptions.Length);


                if (!string.IsNullOrEmpty(footstepSfxOptions[idx]))
                    sfxToPlay = footstepSfxOptions[idx];
            }

            AudioManager.instance.PlaySFX(sfxToPlay);
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

    private void HandleGrappleStart()
    {
        AudioManager.instance.PlaySFX(grappleShootSfx);
        grappleJustStarted = true;
    }

    private void HandleGrappleEnd()
    {

        AudioManager.instance.PlaySFX(grappleReleaseSfx);

        // Libera double jump só se tinha realmente se agarrado
        if (wasGrappling)
        {
            doubleJumpAvailable = true;
            doubleJumpSoundPlayed = false;
        }

        grappleJustStarted = false;
        wasGrappling = false;
    }

    private void HandleGrappleAudio()
    {
        if (grapple == null || !grappleJustStarted) return;

        // Som e estado só se realmente conectar
        if (grapple.IsGrappling)
        {
            AudioManager.instance.PlaySFX(grappleAttachSfx);

            // Bloqueia double jump enquanto estiver agarrado
            doubleJumpAvailable = false;
            doubleJumpSoundPlayed = false;

            wasGrappling = true;
            grappleJustStarted = false;
        }

    }


    private void HandleWindAudio()
    {
        if (windSource == null) return;

        // Se o jogo está pausado, zera o volume
        if (Time.timeScale == 0f)
        {
            windSource.volume = 0f;
            return;
        }

        // Calcula volume base pelo movimento do player
        float speed = rb.linearVelocity.magnitude;
        float targetVolume = 0f;

        if (speed > windMinSpeed)
            targetVolume = Mathf.InverseLerp(windMinSpeed, windMaxSpeed, speed);

        // Aplica SFX e Master volume, e considera muting
        if (AudioManager.instance != null)
        {
            float sfxVolume = AudioManager.instance.sfxSource.volume;
            float masterVolume = AudioManager.instance.masterSource.volume;
            bool isMuted = AudioManager.instance.sfxSource.mute || AudioManager.instance.masterSource.mute;

            targetVolume = isMuted ? 0f : targetVolume * sfxVolume * masterVolume;
        }

        // Suaviza a transição do volume
        windSource.volume = Mathf.MoveTowards(windSource.volume, targetVolume, Time.deltaTime * windFadeSpeed);
    }



    private void PlayLand()
    {
        if (!hasLandedOnce)
        {
            hasLandedOnce = true;
            return;
        }

        if (landSfxOptions != null && landSfxOptions.Length > 0)
        {
            int idx = Random.Range(0, landSfxOptions.Length);
            AudioManager.instance.PlaySFX(landSfxOptions[idx]);
        }
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


    private void HandlePowerRingAudio()
    {
        AudioManager.instance.PlaySFX(ringSfx);
    }

    private void AddJumpPadListener(JumpPad pad)
    {
        // Essa parte é uma gambiarra temporária, mas funciona bem
        var trigger = pad.gameObject.AddComponent<JumpPadAudioTrigger>();
        trigger.Setup(pad, this);
    }

    public void PlayJumpPadAudio()
    {
        AudioManager.instance.PlaySFX(jumpPadSfx);
    }

}

