using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerAudioHandler : MonoBehaviour, IResettable
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
    [SerializeField] private float laserMaxDistance = 10f; // distância máxima pra ouvir o laser
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
            InputManager.Instance.OnSlowTimeToggled += PlayTimeSkillOn;
            InputManager.Instance.OnSlowTimeToggled += PlayTimeSkillOff;
            InputManager.Instance.OnResetToCheckpoint += PlayDeathSound;

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
            InputManager.Instance.OnResetToCheckpoint -= PlayDeathSound;

            BasePowerUpRing.OnPowerRingActivated -= HandlePowerRingAudio;
        }

        movement.OnGroundLanded -= PlayLand;
    }

    private void Update()
    {

        if (Time.timeScale == 0f)
        {
            if (deathSource.isPlaying)
            {
                deathSource.Pause();
                isPausedManual = true; // Marca que fomos nós que pausamos
            }
            return;
        }
        else
        {
            // Se o jogo despausou e nós tínhamos pausado o som, solta o play
            if (isPausedManual)
            {
                deathSource.UnPause();
                isPausedManual = false;
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

        // Som e estado só se realmente conectar
        if (grapple.IsGrappling)
        {
            if (grappleAttachSfxOptions != null && grappleAttachSfxOptions.Length > 0)
            {
                int idx = Random.Range(0, grappleAttachSfxOptions.Length);
                AudioManager.instance.PlaySFX(grappleAttachSfxOptions[idx]);
            }

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

        AudioManager.instance.PauseMusic(); // pausa a música
        AudioManager.instance.PlaySFX(victorySfx); // toca som de vitória
        Debug.Log("Música pausada? " + !AudioManager.instance.musicSource.isPlaying);
    }


    public void ResetState()
    {
        // Toca o som instantaneamente quando o Manager manda resetar
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
            // Chama a função que toca o SFX na fonte dedicada
            AudioManager.instance.PlaySFXInSource(deathSfx, deathSource);
            isResetting = true;
            Debug.Log("Death sound played!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Se encostar em um laser
        if (other.GetComponent<LaserBarrier>() != null)
        {
            PlayLaserHitAudio();
        }

    }


    private void HandleLaserAudio()
    {
        // Cria a fonte de áudio do laser se ainda não existir
        if (laserLoopSource == null)
            laserLoopSource = AudioManager.instance.PlayLoopingSFX(laserLoopSfx, 0f);

        float targetVolume = 0f;

        if (Time.timeScale == 0f)
        {
            if (laserLoopSource != null)
                laserLoopSource.volume = 0f;
            return;
        }

        // Procura o laser mais próximo com collider ativo
        LaserBarrier closest = null;
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
                closest = laser;
            }
        }

        nearestLaser = closest;

        // Se tem laser próximo dentro da distância, volume máximo
        if (nearestLaser != null && closestDist <= laserMaxDistance)
        {
            // Calcula volume baseado na distância: perto = 1, longe = 0
            targetVolume = 1f - (closestDist / laserMaxDistance);
            targetVolume = Mathf.Clamp01(targetVolume); // garante entre 0 e 1
        }

        // Atualiza o volume da fonte diretamente
        if (laserLoopSource != null)
            laserLoopSource.volume = Mathf.MoveTowards(laserLoopSource.volume, targetVolume, Time.deltaTime * laserFadeSpeed);

    }





    public void PlayTimeSkillOn() => AudioManager.instance.PlaySFX(timeSkillOnSfx);
    public void PlayTimeSkillOff() => AudioManager.instance.PlaySFX(timeSkillOffSfx);
}

