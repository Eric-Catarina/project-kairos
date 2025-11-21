using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerAudioHandler : MonoBehaviour, IResettable
{
    private PlayerMovementController movement;
    private GrapplingHookController grapple;
    private Rigidbody rb;

    [Header("Sons - Movimento")]
    [SerializeField] private string[] footstepSfxOptions;
    [SerializeField] private string[] jumpSfxOptions;
    [SerializeField] private string[] doubleJumpSfxOptions;
    [SerializeField] private string[] landSfxOptions;
    [SerializeField] private string footstepSfx = "footstep";
    [SerializeField] private string wallHitSfx = "WallHit";
    [SerializeField] private string[] ringSfxOptions;
    [SerializeField] private string[] jumpPadSfxOptions;

    [Header("Sons - Estado do Player")]
    [SerializeField] private string deathSfx = "Death";
    [SerializeField] private string timeSkillOnSfx = "TimeOn";
    [SerializeField] private string timeSkillOffSfx = "TimeOff";
    [SerializeField] private string victorySfx = "Victory";

    [Header("Sons - Grappling Hook")]
    [SerializeField] private string[] grappleShootSfxOptions;
    [SerializeField] private string[] grappleAttachSfxOptions;
    [SerializeField] private string[] grappleReleaseSfxOptions;

    [Header("Parâmetros")]
    [SerializeField] private float stepInterval = 0.35f;
    [SerializeField] private float minStepSpeed = 2f;
    [SerializeField] private float wallHitMinForce = 5f;

    [Header("Som - Vento")]
    [SerializeField] private string windSfx = "Wind";
    [SerializeField] private float windMinSpeed = 20f;
    [SerializeField] private float windMaxSpeed = 60f;
    [SerializeField] private float windFadeSpeed = 5f;

    [Header("Sons - Dano e Laser")]
    [SerializeField] private string[] laserHitSfxOptions;
    [SerializeField] private string[] damageSfxOptions;

    [Header("Som - Laser Barreiras")]
    [SerializeField] private string laserLoopSfx = "LaserLoop";
    [SerializeField] private float laserMaxDistance = 10f;
    [SerializeField] private float laserFadeSpeed = 5f;

    private AudioSource laserLoopSource;
    private LaserBarrier nearestLaser;
    private AudioSource deathSource;
    private bool isResetting = false;
    private bool isPausedManual = false;

    private bool victoryPlayed = false;

    private float stepTimer;
    private bool hasLandedOnce = false;
    private Vector3 lastPosition;

    private bool wasGrappling;
    private bool grappleJustStarted;

    private bool hasJumpedOnce = false;
    private bool canDoubleJump = false;

    private bool doubleJumpAvailable = false;
    private bool doubleJumpSoundPlayed = false;

    private AudioSource windSource;

    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
        grapple = GetComponent<GrapplingHookController>();
        rb = GetComponent<Rigidbody>();

        deathSource = gameObject.AddComponent<AudioSource>();
        deathSource.playOnAwake = false;
        deathSource.loop = false;
        deathSource.ignoreListenerPause = false;

        isResetting = false;
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
            InputManager.Instance.OnResetToCheckpoint += PlayDeathSound;

            BasePowerUpRing.OnPowerRingActivated += HandlePowerRingAudio;

            foreach (var jumpPad in FindObjectsOfType<JumpPad>())
            {
                AddJumpPadListener(jumpPad);
            }
        }

        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.OnTimeStopStarted += PlayTimeSkillOn;
            TimeManipulationManager.Instance.OnTimeStopStopped += PlayTimeSkillOff;
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
            InputManager.Instance.OnResetToCheckpoint -= PlayDeathSound;

            BasePowerUpRing.OnPowerRingActivated -= HandlePowerRingAudio;
        }

        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.OnTimeStopStarted -= PlayTimeSkillOn;
            TimeManipulationManager.Instance.OnTimeStopStopped -= PlayTimeSkillOff;
        }

        movement.OnGroundLanded -= PlayLand;
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (deathSource.isPlaying) { deathSource.Pause(); isPausedManual = true; }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PauseAllSFX();
                AudioManager.instance.PauseAmbient();
            }
            return;
        }
        else
        {
            if (isPausedManual)
            {
                deathSource.UnPause();
                isPausedManual = false;
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.UnpauseAllSFX(); 
                AudioManager.instance.UnpauseAmbient();
            }
        }

        HandleFootsteps();
        HandleGrappleAudio();
        HandleWindAudio();
        HandleLaserAudio();

        if (movement.isGrounded)
        {
            hasJumpedOnce = false;
            canDoubleJump = false;
            doubleJumpSoundPlayed = false;
        }

        if (isResetting && !deathSource.isPlaying)
        {
            isResetting = false;
        }

        lastPosition = transform.position;
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

    private void HandleGrappleStart()
    {
        if (grappleShootSfxOptions != null && grappleShootSfxOptions.Length > 0)
        {
            int idx = Random.Range(0, grappleShootSfxOptions.Length);
            AudioManager.instance.PlaySFX(grappleShootSfxOptions[idx]);
        }
        grappleJustStarted = true;
    }

    private void HandleGrappleEnd()
    {
        if (grappleReleaseSfxOptions != null && grappleReleaseSfxOptions.Length > 0)
        {
            int idx = Random.Range(0, grappleReleaseSfxOptions.Length);
            AudioManager.instance.PlaySFX(grappleReleaseSfxOptions[idx]);
        }

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

        if (grapple.IsGrappling)
        {
            if (grappleAttachSfxOptions != null && grappleAttachSfxOptions.Length > 0)
            {
                int idx = Random.Range(0, grappleAttachSfxOptions.Length);
                AudioManager.instance.PlaySFX(grappleAttachSfxOptions[idx]);
            }

            doubleJumpAvailable = false;
            doubleJumpSoundPlayed = false;

            wasGrappling = true;
            grappleJustStarted = false;
        }
    }

    private void HandleWindAudio()
    {
        if (windSource == null) return;

        float speed = rb.linearVelocity.magnitude;
        float targetVolume = 0f;

        if (speed > windMinSpeed)
            targetVolume = Mathf.InverseLerp(windMinSpeed, windMaxSpeed, speed);

        if (AudioManager.instance != null)
        {
            float sfxVolume = AudioManager.instance.sfxSource.volume;
            float masterVolume = AudioManager.instance.masterSource.volume;
            bool isMuted = AudioManager.instance.sfxSource.mute || AudioManager.instance.masterSource.mute;

            targetVolume = isMuted ? 0f : targetVolume * sfxVolume * masterVolume;
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

        if (!victoryPlayed && collision.gameObject.GetComponent<WinLogic>() != null)
        {
            victoryPlayed = true;
            PlayVictoryAudio();
        }
    }

    private void HandlePowerRingAudio()
    {
        if (ringSfxOptions != null && ringSfxOptions.Length > 0)
        {
            int idx = Random.Range(0, ringSfxOptions.Length);
            AudioManager.instance.PlaySFX(ringSfxOptions[idx]);
        }

        doubleJumpAvailable = true;
        doubleJumpSoundPlayed = false;
    }

    private void AddJumpPadListener(JumpPad pad)
    {
        var trigger = pad.gameObject.AddComponent<JumpPadAudioTrigger>();
        trigger.Setup(pad, this);
    }

    public void PlayJumpPadAudio()
    {
        if (jumpPadSfxOptions != null && jumpPadSfxOptions.Length > 0)
        {
            int idx = Random.Range(0, jumpPadSfxOptions.Length);
            AudioManager.instance.PlaySFX(jumpPadSfxOptions[idx]);
        }

        doubleJumpAvailable = true;
        doubleJumpSoundPlayed = false;
    }

    public void PlayLaserHitAudio()
    {
        if (laserHitSfxOptions != null && laserHitSfxOptions.Length > 0)
        {
            int laserIdx = Random.Range(0, laserHitSfxOptions.Length);
            AudioManager.instance.PlaySFX(laserHitSfxOptions[laserIdx]);
        }

        if (damageSfxOptions != null && damageSfxOptions.Length > 0)
        {
            int damageIdx = Random.Range(0, damageSfxOptions.Length);
            AudioManager.instance.PlaySFX(damageSfxOptions[damageIdx]);
        }
    }

    public void PlayVictoryAudio()
    {
        if (AudioManager.instance == null) return;

        AudioManager.instance.PauseMusic();
        AudioManager.instance.PauseAmbient();
        AudioManager.instance.PlayUnscaledSFX(victorySfx);
    }

    public void ResetState()
    {
        if (isResetting) return;
        PlayDeathSound();
    }

    private void PlayDeathSound()
    {
        if (isResetting) return;
        if ( string.IsNullOrEmpty(deathSfx) || AudioManager.instance == null)
            return;

        if (AudioManager.instance != null)
        {
            if (deathSource.isPlaying) deathSource.Stop();
            AudioManager.instance.PlaySFXInSource(deathSfx, deathSource);
            isResetting = true;
            Debug.Log("Death sound played!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<LaserBarrier>() != null)
        {
            PlayLaserHitAudio();
        }
    }

    private void HandleLaserAudio()
    {
        if (laserLoopSource == null)
            laserLoopSource = AudioManager.instance.PlayLoopingSFX(laserLoopSfx, 0f);

        float targetVolumeLocal = 0f;
        float closestDist = float.MaxValue;

        foreach (var laser in FindObjectsOfType<LaserBarrier>())
        {
            Collider col = laser.GetComponent<Collider>();
            if (!col.enabled) continue;

            Vector3 closestPoint = col.ClosestPoint(transform.position);
            float dist = Vector3.Distance(transform.position, closestPoint);

            if (dist < closestDist)
            {
                closestDist = dist;
                nearestLaser = laser;
            }
        }

        if (nearestLaser != null && closestDist <= laserMaxDistance)
        {
            targetVolumeLocal = 1f - (closestDist / laserMaxDistance);
            targetVolumeLocal = Mathf.Clamp01(targetVolumeLocal);
        }

        float finalVolume = targetVolumeLocal;

        if (AudioManager.instance != null && laserLoopSource != null)
        {
            float sfxVolume = AudioManager.instance.sfxSource.volume;
            float masterVolume = AudioManager.instance.masterSource.volume;

            finalVolume = targetVolumeLocal * sfxVolume * masterVolume;

            bool isMuted = AudioManager.instance.sfxSource.mute || AudioManager.instance.masterSource.mute;
            laserLoopSource.mute = isMuted;
        }
        if (laserLoopSource != null)
            laserLoopSource.volume = Mathf.MoveTowards(laserLoopSource.volume, finalVolume, Time.deltaTime * laserFadeSpeed);
    }

    public void PlayTimeSkillOn() => AudioManager.instance.PlaySFX(timeSkillOnSfx);
    public void PlayTimeSkillOff() => AudioManager.instance.PlaySFX(timeSkillOffSfx);
}